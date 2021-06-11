using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace YasinDLLLoader
{
    public class DLLLoaderSubModule : MBSubModuleBase
    {
        List<DLL> dlls = new List<DLL>();

        // These strings may be customizable in the future
        const string ModuleLoadMethod = "BannerlordModuleLoad";
        const string GameStartMethod = "BannerlordStart";
        const string GameLoadMethod = "BannerlordGameLoad";
        const string TickMethod = "BannerlordTick";
        const string ModuleUnloadMethod = "BannerlordModuleUnload";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            LoadDlls();
            InvokeMethodForAllDlls(ModuleLoadMethod);
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            InvokeMethodForAllDlls(GameStartMethod, game);
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            base.OnGameLoaded(game, initializerObject);
            InvokeMethodForAllDlls(GameLoadMethod, game, initializerObject);
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            InvokeMethodForAllDlls(TickMethod, dt);
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            InvokeMethodForAllDlls(ModuleUnloadMethod);
        }


        private void InvokeMethodForAllDlls(string methodName, params object[] parameters) // Public static methods only
        {
            foreach (DLL dll in dlls)
            {
                dll.InvokeMethod(methodName, BindingFlags.Public | BindingFlags.Static, parameters);
            }
        }

        private void LoadDlls()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Environment.CurrentDirectory); // Bannerlord exe file location
            DLL.Directory = Path.Combine(directoryInfo.Parent.Parent.FullName, "Scripts");

            // TODO: Add support for custom extensions
            foreach (string dllPath in Directory.EnumerateFiles(DLL.Directory, "*.dll", SearchOption.AllDirectories))
            {
                string dllName = Path.GetFileName(dllPath);

                DLL dll = new DLL(dllPath);
                if (dll.isValid)
                {
                    dlls.Add(dll);
                }

            }
        }

        public static void SendMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg));
        }

        public static void SendMessage(string msg, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg, color));
        }

        public static void ShowMessageBox(string text, MessageBoxIcon icon)
        {
            MessageBox.Show(text, "Yasin's DLL Loader", MessageBoxButtons.OK, icon);
        }
    }

    public static class Extensions
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dirInfo, params string[] extensions)
        {
            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

            return dirInfo.EnumerateFiles()
                          .Where(f => allowedExtensions.Contains(f.Extension));
        }
    }
}
