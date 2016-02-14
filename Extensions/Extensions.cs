using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Extensions
{
    class Extensions
    {
        // Extension container
        public static ICollection<IExtension> Loaded;

        // Unload all loaded extensions
        public static void Unload()
        {
            foreach (var ext in Loaded)
            {
                ext.Destruct();
            }
        }

        // Load extension directory
        public static ICollection<IExtension> Load(string path)
        {
            string[] files = null;

            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*.dll");

                ICollection<Assembly> assemblies = new List<Assembly>(files.Length);

                foreach (string file in files)
                {
                    AssemblyName name = AssemblyName.GetAssemblyName(file);
                    Assembly assembly = Assembly.Load(name);
                    assemblies.Add(assembly);
                }

                Type extType = typeof(IExtension);

                ICollection<Type> extTypes = new List<Type>();

                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }

                            if (type.GetInterface(extType.FullName) != null)
                            {
                                extTypes.Add(type);
                            }
                        }
                    }
                }

                ICollection<IExtension> extensions = new List<IExtension>(extTypes.Count());

                foreach (Type type in extTypes)
                {
                    IExtension extension = (IExtension)Activator.CreateInstance(type);
                    extension.Initialise();
                    extensions.Add(extension);
                }

                return extensions;
            }

            return null;
        }
    }
}
