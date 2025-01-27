using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.StreamLibrary
{
    public class UnsafeStreamCodec : IDisposable
    {
        internal Size Resolution { get; private set; }
        internal Size CheckBlock { get; private set; }
        
        internal int ImageQuality
        {
            get => _imageQuality;
            private set
            {
                lock (_imageProcessLock)
                {
                    _imageQuality = value;

                    // Dispose and reinitialize compression only if quality changes
                    _jpgCompression?.Dispose();
                    _jpgCompression = new JpgCompression(_imageQuality);
                }
            }
        }

        private int _imageQuality;
        private byte[] _encodeBuffer;
        private Bitmap _decodedBitmap;
        private PixelFormat _encodedFormat;
        private int _encodedWidth;
        private int _encodedHeight;
        private readonly object _imageProcessLock = new object();
        private JpgCompression _jpgCompression;

        public UnsafeStreamCodec(int imageQuality)
        {
            this.ImageQuality = imageQuality;
            this.CheckBlock = new Size(50, 1);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _decodedBitmap?.Dispose();
                _jpgCompression?.Dispose();
            }
        }

        internal unsafe void CodeImage(IntPtr scan0, Rectangle scanArea, Size imageSize, PixelFormat format, Stream outStream)
        {
            lock (_imageProcessLock)
            {
                byte* pScan0 = GetScan0(scan0);

                if (!outStream.CanWrite)
                    throw new Exception("Stream must be writable.");

                int stride = imageSize.Width * GetPixelSize(format);
                int rawLength = stride * imageSize.Height;

                if (_encodeBuffer == null)
                {
                    InitializeEncodeBuffer(format, imageSize, scan0, stride, rawLength, outStream);
                    return;
                }

                if (_encodedFormat != format || _encodedWidth != imageSize.Width || _encodedHeight != imageSize.Height)
                    throw new Exception("Inconsistent image format or dimensions.");

                long oldPos = outStream.Position;
                outStream.Write(new byte[4], 0, 4); // Reserve space for total data length
                long totalDataLength = 0;

                List<Rectangle> blocks = FindChangedBlocks(scan0, scanArea, imageSize, format, stride, pScan0);

                foreach (var rect in blocks)
                {
                    CompressAndWriteBlock(rect, scanArea, format, stride, pScan0, outStream, ref totalDataLength);
                }

                outStream.Position = oldPos;
                outStream.Write(BitConverter.GetBytes(totalDataLength), 0, 4); // Update total length
            }
        }

        private static byte* GetScan0(IntPtr scan0)
        {
            return IntPtr.Size == 8 ? (byte*)scan0.ToInt64() : (byte*)scan0.ToInt32();
        }

        private static int GetPixelSize(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.Format24bppRgb or PixelFormat.Format32bppRgb => 3,
                PixelFormat.Format32bppArgb or PixelFormat.Format32bppPArgb => 4,
                _ => throw new NotSupportedException($"Unsupported PixelFormat: {format}")
            };
        }

        private void InitializeEncodeBuffer(PixelFormat format, Size imageSize, IntPtr scan0, int stride, int rawLength, Stream outStream)
        {
            _encodedFormat = format;
            _encodedWidth = imageSize.Width;
            _encodedHeight = imageSize.Height;
            _encodeBuffer = new byte[rawLength];

            fixed (byte* ptr = _encodeBuffer)
            {
                using (Bitmap tmpBmp = new Bitmap(imageSize.Width, imageSize.Height, stride, format, scan0))
                {
                    byte[] compressedData = _jpgCompression.Compress(tmpBmp);
                    outStream.Write(BitConverter.GetBytes(compressedData.Length), 0, 4);
                    outStream.Write(compressedData, 0, compressedData.Length);
                    NativeMethods.memcpy(new IntPtr(ptr), scan0, (uint)rawLength);
                }
            }
        }

        private List<Rectangle> FindChangedBlocks(IntPtr scan0, Rectangle scanArea, Size imageSize, PixelFormat format, int stride, byte* pScan0)
        {
            var blocks = new List<Rectangle>();
            var s = new Size(scanArea.Width, CheckBlock.Height);
            var lastSize = new Size(scanArea.Width % CheckBlock.Width, scanArea.Height % CheckBlock.Height);

            int lasty = scanArea.Height - lastSize.Height;
            int lastx = scanArea.Width - lastSize.Width;

            for (int y = scanArea.Y; y < scanArea.Height; y += s.Height)
            {
                if (y == lasty)
                {
                    s = new Size(scanArea.Width, lastSize.Height);
                }

                var cBlock = new Rectangle(scanArea.X, y, scanArea.Width, s.Height);
                int offset = (y * stride) + (scanArea.X * format.ToPixelSize());

                if (NativeMethods.memcmp(pScan0 + offset, scan0 + offset, (uint)stride) != 0)
                {
                    blocks.Add(cBlock);
                }
            }

            return blocks;
        }

        private void CompressAndWriteBlock(Rectangle rect, Rectangle scanArea, PixelFormat format, int stride, byte* pScan0, Stream outStream, ref long totalDataLength)
        {
            using (Bitmap tmpBmp = new Bitmap(rect.Width, rect.Height, format))
            {
                using (BitmapData tmpData = tmpBmp.LockBits(new Rectangle(0, 0, tmpBmp.Width, tmpBmp.Height),
                    ImageLockMode.ReadWrite, tmpBmp.PixelFormat))
                {
                    int blockStride = format.ToPixelSize() * rect.Width;

                    for (int j = 0; j < rect.Height; j++)
                    {
                        int blockOffset = (stride * (rect.Y + j)) + (format.ToPixelSize() * rect.X);
                        NativeMethods.memcpy((byte*)tmpData.Scan0.ToPointer() + (j * blockStride), pScan0 + blockOffset, (uint)blockStride);
                    }

                    outStream.Write(BitConverter.GetBytes(rect.X), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Y), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Width), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Height), 0, 4);
                    outStream.Write(new byte[4], 0, 4); // Reserved space for length

                    long lengthBefore = outStream.Position;
                    _jpgCompression.Compress(tmpBmp, ref outStream);
                    long blockLength = outStream.Position - lengthBefore;

                    outStream.Position -= 4;
                    outStream.Write(BitConverter.GetBytes(blockLength), 0, 4); // Write actual block length

                    totalDataLength += blockLength + 20; // 4 for each of X, Y, Width, Height
                }
            }
        }

        internal unsafe Bitmap DecodeData(IntPtr codecBuffer, uint length)
        {
            if (length < 4)
                return _decodedBitmap;

            int dataSize = *(int*)codecBuffer;
            if (_decodedBitmap == null)
            {
                byte[] temp = new byte[dataSize];

                fixed (byte* tempPtr = temp)
                {
                    NativeMethods.memcpy(new IntPtr(tempPtr), new IntPtr(codecBuffer.ToInt32() + 4), (uint)dataSize);
                }

                _decodedBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
            }

            return _decodedBitmap;
        }

        internal Bitmap DecodeData(Stream inStream)
        {
            byte[] temp = new byte[4];
            inStream.Read(temp, 0, 4);
            int dataSize = BitConverter.ToInt32(temp, 0);

            if (_decodedBitmap == null)
            {
                temp = new byte[dataSize];
                inStream.Read(temp, 0, temp.Length);
                _decodedBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
            }
            else
            {
                using (Graphics g = Graphics.FromImage(_decodedBitmap))
                {
                    while (dataSize > 0)
                    {
                        byte[] tempData = new byte[4 * 5];
                        inStream.Read(tempData, 0, tempData.Length);

                        var rect = new Rectangle(BitConverter.ToInt32(tempData, 0), BitConverter.ToInt32(tempData, 4),
                            BitConverter.ToInt32(tempData, 8), BitConverter.ToInt32(tempData, 12));

                        int updateLen = BitConverter.ToInt32(tempData, 16);
                        byte[] buffer = new byte[updateLen];
                        inStream.Read(buffer, 0, buffer.Length);

                        using (MemoryStream m = new MemoryStream(buffer))
                        {
                            using (Bitmap tmp = (Bitmap)Image.FromStream(m))
                            {
                                g.DrawImage(tmp, rect.Location);
                            }
                        }

                        dataSize -= updateLen + (4 * 5);
                    }
                }
            }

            Resolution = _decodedBitmap.Size;
            return _decodedBitmap;
        }
    }
}
