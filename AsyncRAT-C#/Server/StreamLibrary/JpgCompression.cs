using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Server.StreamLibrary
{
    public class JpgCompression : IDisposable
    {
        private readonly ImageCodecInfo? _encoderInfo; // Nullable for safety
        private readonly EncoderParameters _encoderParams;

        // Constructor with quality parameter
        public JpgCompression(long quality)
        {
            // Setting the encoder parameters
            var parameter = new EncoderParameter(Encoder.Quality, quality);
            _encoderInfo = GetEncoderInfo("image/jpeg");
            _encoderParams = new EncoderParameters(2)
            {
                Param = {
                    [0] = parameter,
                    [1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionRle)
                }
            };
        }

        // Dispose pattern with suppression of finalization
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Actual dispose logic
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _encoderParams?.Dispose(); // Safe disposal with null-conditional operator
            }
        }

        // Method to compress the bitmap and return byte array
        public byte[] Compress(Bitmap bmp)
        {
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, _encoderInfo, _encoderParams);
                return stream.ToArray();
            }
        }

        // Method to compress the bitmap into an existing stream
        public void Compress(Bitmap bmp, ref Stream targetStream)
        {
            bmp.Save(targetStream, _encoderInfo, _encoderParams);
        }

        // Method to retrieve the correct encoder for a given mime type
        private ImageCodecInfo? GetEncoderInfo(string mimeType)
        {
            return ImageCodecInfo.GetImageEncoders()
                                  .FirstOrDefault(encoder => encoder.MimeType == mimeType);
        }
    }
}
