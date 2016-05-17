using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Extensions
{
    public static class ExtensionManager
    {
        // Contains the list of loaded extensions with the name provided by the Name() function as the key
        public static Dictionary<string, IExtension> Loaded { get; private set; } = null;

        // Contains the source directory for the reload function
        public static string ExtensionsDir { get; private set; } = null;

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

        // Validate the extension types
        private static Type[] GetTypes(Assembly[] asms)
        {
            // Get the type of IExtension
            Type extType = typeof(IExtension);

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

                    // Add the extension to the type list
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

        // Load all extensions in the given directory and fire the Init command off
        public static void Load(string dir)
        {
            // Prevent this command from being executed if Loaded isn't null
            if (Loaded != null)
            {
                return;
            }

            // Set dir
            ExtensionsDir = dir;

            // Get all assemblies
            Assembly[] assemblies = GetAssemblies(dir);

            // Get all types
            Type[] types = GetTypes(assemblies);

            // Create a dictionary for the loaded extensions
            Dictionary<string, IExtension> extensions = new Dictionary<string, IExtension> (types.Length);

            // Create an instance of the extensions and run the Init commands
            foreach (Type type in types)
            {
                IExtension ext = (IExtension)Activator.CreateInstance(type);
                ext.Init();
                extensions.Add(ext.Name(), ext);
            }

            // Commit the extensions list to the loaded array
            Loaded = extensions;
        }

        // Unload all loaded extensions
        public static void Unload()
        {
            if (Loaded == null)
            {
                return;
            }

            foreach (KeyValuePair<string, IExtension> ext in Loaded)
            {
                ext.Value.Kill();
                Loaded.Remove(ext.Key);
            }

            Loaded = null;
        }

        // Reload all extensions
        public static void Reload()
        {
            if (ExtensionsDir == null)
            {
                return;
            }

            Unload();
            Load(ExtensionsDir);
        }
    }
}
