using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace StreamLibrary.src
{
    public class LzwCompression
    {
        private readonly EncoderParameter qualityParameter;
        private readonly ImageCodecInfo jpegEncoderInfo;
        private readonly EncoderParameters encoderParams;

        public LzwCompression(int quality)
        {
            // Ensure quality is within the acceptable range (0-100)
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 100.");

            this.qualityParameter = new EncoderParameter(Encoder.Quality, (long)quality);
            this.jpegEncoderInfo = GetEncoderInfo("image/jpeg");
            this.encoderParams = new EncoderParameters(2);
            this.encoderParams.Param[0] = qualityParameter;
            this.encoderParams.Param[1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
        }

        /// <summary>
        /// Compresses a bitmap to a byte array in LZW format, with optional additional information.
        /// </summary>
        /// <param name="bmp">The bitmap to compress.</param>
        /// <param name="additionInfo">Optional additional information to prepend to the compressed data.</param>
        /// <returns>A byte array containing the compressed image data.</returns>
        public byte[] Compress(Bitmap bmp, byte[] additionInfo = null)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp), "Bitmap cannot be null.");

            using (MemoryStream stream = new MemoryStream())
            {
                if (additionInfo != null)
                {
                    stream.Write(additionInfo, 0, additionInfo.Length);
                }

                bmp.Save(stream, jpegEncoderInfo, encoderParams);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Compresses a bitmap and writes the result directly to the provided stream, with optional additional information.
        /// </summary>
        /// <param name="bmp">The bitmap to compress.</param>
        /// <param name="targetStream">The stream to write the compressed image data to.</param>
        /// <param name="additionInfo">Optional additional information to prepend to the compressed data.</param>
        public void Compress(Bitmap bmp, Stream targetStream, byte[] additionInfo = null)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp), "Bitmap cannot be null.");
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream), "Target stream cannot be null.");

            if (additionInfo != null)
            {
                targetStream.Write(additionInfo, 0, additionInfo.Length);
            }

            bmp.Save(targetStream, jpegEncoderInfo, encoderParams);
        }

        /// <summary>
        /// Retrieves the encoder information for a given MIME type.
        /// </summary>
        /// <param name="mimeType">The MIME type of the image format.</param>
        /// <returns>The <see cref="ImageCodecInfo"/> for the given MIME type.</returns>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }

            throw new InvalidOperationException($"No encoder found for MIME type: {mimeType}");
        }
    }
}
