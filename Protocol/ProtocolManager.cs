using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Protocol
{
    public static class ProtocolManager
    {
        // Contains the list of loaded protocol wrappers with the name provided by the Name() function as the key
        public static Dictionary<string, IProtocol> Loaded { get; private set; } = null;

        // Gets the list of files to load.
        private static Assembly[] GetAssemblies(string dir)
        {
            // Create a container list
            List<Assembly> asms = new List<Assembly>();

            // Check if the given directory exists
            if (Directory.Exists(dir))
            {
                // Build a string array of the files with the dll extension
                string[] files = Directory.GetFiles(dir, "*.dll");

                // Iterate over the files
                foreach (string file in files)
                {
                    // Get the internal name
                    AssemblyName name = AssemblyName.GetAssemblyName(file);

                    // Load the assembly
                    Assembly assembly = Assembly.Load(name);

                    // Add it to the list
                    asms.Add(assembly);
                }
            }

            // Return the list as an Assembly array
            return asms.ToArray();
        }

        // Validate the protocol types
        private static Type[] GetTypes(Assembly[] asms)
        {
            // Get the type of IProtocol
            Type extType = typeof(IProtocol);

            // Create a list for the validated assemblies
            List<Type> extTypes = new List<Type>();

            // Iterate over the assemblies
            foreach (Assembly asm in asms)
            {
                // Discard the assembly if it's null
                if (asm == null)
                {
                    continue;
                }

                // Get the types
                Type[] types = asm.GetTypes();

                // Iterate over the types
                foreach (Type type in types)
                {
                    // Discard if the type is an interface or abstract class or if the interface type doesn't match
                    if (type.IsInterface || type.IsAbstract || type.GetInterface(extType.FullName) == null)
                    {
                        continue;
                    }

                    // Add the protocol wrapper to the type list
                    extTypes.Add(type);
                }
            }

            // Return the list as a Type array
            return extTypes.ToArray();
        }

        // Returns all the keys in a regular string array
        public static string[] GetLoaded()
        {
            ICollection<string> keys = Loaded.Keys;
            string[] names = new string[keys.Count];

            keys.CopyTo(names, 0);

            return names;
        }

        // Load all protocol wrappers in the given directory
        public static void BuildList(string dir)
        {
            // Prevent this command from being executed if Loaded isn't null
            if (Loaded != null)
            {
                return;
            }

            // Get all assemblies
            Assembly[] assemblies = GetAssemblies(dir);

            // Get all types
            Type[] types = GetTypes(assemblies);

            // Create a dictionary for the loaded protocol wrappers
            Dictionary<string, IProtocol> protocols = new Dictionary<string, IProtocol>(types.Length);

            // Create an instance of the protocol wrappers
            foreach (Type type in types)
            {
                IProtocol ext = (IProtocol)Activator.CreateInstance(type);
                protocols.Add(ext.Name(), ext);
            }

            // Commit the protocol wrappers list to the loaded array
            Loaded = protocols;
        }
    }
}
