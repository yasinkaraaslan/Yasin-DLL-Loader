using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace YasinDLLLoader
{
    class DLL
    {
        public static string Directory; // Directory where dlls are stored

        public object instance { get; private set; }
        public Type classType { get; private set; }


        public string name { get; private set; }

        public bool isValid { get; private set; } = true;
        
        public string className => Path.GetFileNameWithoutExtension(name);


        public DLL(string dllPath)
        {

            try
            {
                name = Path.GetFileName(dllPath);
                Assembly assembly = Assembly.LoadFile(dllPath);
                if (assembly == null)
                {
                    isValid = false;
                    return;
                }

                object obj = assembly.CreateInstance(className);
                if (obj == null)
                {
                    DLLLoaderSubModule.ShowMessageBox("Make sure the class name is same as dll name. " + name, MessageBoxIcon.Error);
                    isValid = false;
                    return;
                }
                instance = obj;
                classType = instance.GetType();
            }
            catch (FileLoadException)
            {
                isValid = false;
                DLLLoaderSubModule.SendMessage(name + " has already been loaded.");
            } // The Assembly has already been loaded.
            catch (BadImageFormatException)
            {
                isValid = false;
                string msg = name + " couldn't be loaded! Make sure the file is a .NET assembly.";
                DLLLoaderSubModule.ShowMessageBox(msg, MessageBoxIcon.Error);
            } // If a BadImageFormatException exception is thrown, the file is not an assembly.
            catch (Exception e)
            {
                isValid = false;
                DLLLoaderSubModule.ShowMessageBox(e.Message + " in " + name, MessageBoxIcon.Error);
            } // Other exceptions

        }

        // TODO: Add support for parameters
        public void InvokeMethod(string methodName, BindingFlags bindingFlags, object[] parameters)
        {
            MethodInfo mi = classType.GetMethod(methodName, bindingFlags);
            if (mi != null)
            {
                mi.Invoke(instance, parameters);
            }
        }

    }
}
