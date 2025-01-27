using Server.RenamingObfuscation.Classes;
using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;

// Credit github.com/srn-g/RenamingObfuscation
// Fixed by nyan cat

namespace Server.RenamingObfuscation
{
    public class Renaming
    {
        public static ModuleDefMD DoRenaming(ModuleDefMD inPath)
        {
            // Directly calling the RenamingObfuscation method to apply renaming strategies
            return RenamingObfuscation(inPath);
        }

        private static ModuleDefMD RenamingObfuscation(ModuleDefMD inModule)
        {
            // Set up the module for renaming
            ModuleDefMD module = inModule;

            // List of renaming strategies to apply
            var renamingStrategies = new IRenaming[]
            {
                new NamespacesRenaming(),
                new ClassesRenaming(),
                new MethodsRenaming(),
                new PropertiesRenaming(),
                new FieldsRenaming()
            };

            // Apply each renaming strategy to the module
            foreach (var strategy in renamingStrategies)
            {
                module = strategy.Rename(module);
            }

            return module;
        }
    }
}