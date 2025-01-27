using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace StreamLibrary
{
    public abstract class IVideoCodec
    {
        // Delegate declarations
        public delegate void VideoCodeProgress(Stream stream, Rectangle[] motionChanges);
        public delegate void VideoDecodeProgress(Bitmap bitmap);
        public delegate void VideoDebugScanningDelegate(Rectangle scanArea);

        // Abstract events for video progress tracking
        public abstract event VideoCodeProgress onVideoStreamCoding;
        public abstract event VideoDecodeProgress onVideoStreamDecoding;
        public abstract event VideoDebugScanningDelegate onCodeDebugScan;
        public abstract event VideoDebugScanningDelegate onDecodeDebugScan;

        // Protected field for compression logic
        protected JpgCompression? jpgCompression;  // Nullable to indicate potential null value
        public abstract ulong CachedSize { get; internal set; }

        // Auto-implemented property for image quality
        public int ImageQuality { get; set; }

        // Constructor with default value for ImageQuality
        public IVideoCodec(int imageQuality = 100)
        {
            this.ImageQuality = imageQuality;
            this.jpgCompression = new JpgCompression(imageQuality);
        }

        // Abstract properties for codec specifics
        public abstract int BufferCount { get; }
        public abstract CodecOption CodecOptions { get; }

        // Abstract methods for video encoding and decoding
        public abstract void CodeImage(Bitmap bitmap, Stream outStream);
        public abstract Bitmap DecodeData(Stream inStream);
    }
}