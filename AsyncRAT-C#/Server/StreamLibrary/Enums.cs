using System;

namespace StreamLibrary
{
    public enum CodecOption
    {
        /// <summary>
        /// The Previous and next image size must be equal
        /// </summary>
        RequireSameSize = 1,
        
        /// <summary>
        /// If the codec is having a stream buffer
        /// </summary>
        HasBuffers = 2,
        
        /// <summary>
        /// The image will be disposed by the codec and shall not be disposed by the user
        /// </summary>
        AutoDispose = 3,
        
        /// <summary>
        /// No codec options were used
        /// </summary>
        None = 0
    };
}