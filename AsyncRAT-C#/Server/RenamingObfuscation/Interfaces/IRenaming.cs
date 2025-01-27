namespace Server.RenamingObfuscation.Interfaces
{
    /// <summary>
    /// Defines a method for renaming elements within a module.
    /// </summary>
    public interface IRenaming
    {
        /// <summary>
        /// Renames elements within the provided module.
        /// </summary>
        /// <param name="module">The module to rename elements within.</param>
        /// <returns>The updated module with renamed elements.</returns>
        Task<ModuleDefMD> RenameAsync(ModuleDefMD module);
    }
}