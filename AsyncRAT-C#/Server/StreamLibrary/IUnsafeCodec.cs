using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary
{
    public abstract class IUnsafeCodec
    {
        protected JpgCompression? jpgCompression;  // Nullable, considering it's reinitialized in the property setter
        protected LzwCompression? lzwCompression;  // Nullable, same reason
        public abstract ulong CachedSize { get; internal set; }
        protected object ImageProcessLock { get; private set; }

        private int _imageQuality;
        public int ImageQuality
        {
            get => _imageQuality;
            set
            {
                _imageQuality = value;
                // Ensure these are not null
                jpgCompression = new JpgCompression(value);
                lzwCompression = new LzwCompression(value);
            }
        }

        public abstract event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public abstract event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public IUnsafeCodec(int imageQuality = 100)  // Default parameter in constructor
        {
            this.ImageQuality = imageQuality;
            ImageProcessLock = new object();
        }

        // Abstract properties for codec specifics
        public abstract int BufferCount { get; }
        public abstract CodecOption CodecOptions { get; }

        // Abstract methods for image coding/decoding
        public abstract unsafe void CodeImage(IntPtr scan0, Rectangle scanArea, Size imageSize, PixelFormat format, Stream outStream);
        public abstract unsafe Bitmap DecodeData(Stream inStream);
        public abstract unsafe Bitmap DecodeData(IntPtr codecBuffer, uint length);
    }
}