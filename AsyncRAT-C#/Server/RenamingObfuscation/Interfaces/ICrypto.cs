namespace Server.RenamingObfuscation.Interfaces
{
    /// <summary>
    /// Defines methods for encrypting data.
    /// </summary>
    public interface ICrypto
    {
        /// <summary>
        /// Encrypts the specified plain text data.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <returns>The encrypted data as a string.</returns>
        Task<string> EncryptAsync(string plainText);
    }
}