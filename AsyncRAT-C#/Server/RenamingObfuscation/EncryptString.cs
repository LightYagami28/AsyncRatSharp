using Server.RenamingObfuscation.Classes;
using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace Server.RenamingObfuscation
{
    public static class EncryptString
    {
        private static MethodDef InjectMethod(ModuleDef module, string methodName)
        {
            // Load the DecryptionHelper class from the module and resolve its method
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(DecryptionHelper).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(DecryptionHelper).MetadataToken));
            var injectedMethods = InjectHelper.Inject(typeDef, module.GlobalType, module);
            var injectedMethodDef = injectedMethods.SingleOrDefault(method => method.Name == methodName);

            // Removing default constructor if it exists in the global type
            var ctorMethod = module.GlobalType.Methods.SingleOrDefault(md => md.Name == ".ctor");
            if (ctorMethod != null)
            {
                module.GlobalType.Remove(ctorMethod);
            }

            return injectedMethodDef as MethodDef;
        }

        public static void DoEncrypt(ModuleDef inPath)
        {
            EncryptStrings(inPath);
        }

        private static ModuleDef EncryptStrings(ModuleDef inModule)
        {
            var module = inModule;
            ICrypto crypto = new Base64();  // Assuming Base64 is a valid ICrypto implementation

            // Inject Decrypt method into the module
            MethodDef decryptMethod = InjectMethod(module, "Decrypt_Base64");

            // Iterate over all types and methods in the module for encryption
            foreach (var type in module.Types)
            {
                // Skip global module types, Resources, and Settings classes
                if (type.IsGlobalModuleType || type.Name == "Resources" || type.Name == "Settings")
                    continue;

                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method == decryptMethod)
                        continue;

                    method.Body.KeepOldMaxStack = true;

                    // Loop through method body instructions and replace strings with encrypted versions
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)  // Identifying string literals
                        {
                            string originalString = method.Body.Instructions[i].Operand.ToString();  // Original String

                            // Encrypt the string and insert decryption call
                            method.Body.Instructions[i].Operand = crypto.Encrypt(originalString);
                            method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, decryptMethod));
                        }
                    }

                    method.Body.SimplifyBranches();
                    method.Body.OptimizeBranches();
                }
            }

            return module;
        }
    }
}
