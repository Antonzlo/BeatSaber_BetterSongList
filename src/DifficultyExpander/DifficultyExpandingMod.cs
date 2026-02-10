using System;
using System.Linq;
using System.Reflection;
using System.Threading;

// Ищет HookLevelCollectionTableSet и подставляет туда наш sorter через reflection.
// Этот класс выполнится при загрузке сборки (статический конструктор).
public static class DifficultyExpandingMod {
    static DifficultyExpandingMod() {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
            TryInitWithAssembly(asm);
        }
    }

    static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args) {
        TryInitWithAssembly(args.LoadedAssembly);
    }

    static void Log(string message) {
        try {
            // Try to use UnityEngine.Debug if available, otherwise fall back to Console
            var unityEngineAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "UnityEngine" || a.GetName().Name == "UnityEngine.CoreModule");
            if (unityEngineAsm != null) {
                var debugType = unityEngineAsm.GetType("UnityEngine.Debug");
                if (debugType != null) {
                    var logMethod = debugType.GetMethod("Log", new[] { typeof(object) });
                    if (logMethod != null) {
                        logMethod.Invoke(null, new object[] { message });
                        return;
                    }
                }
            }
            Console.WriteLine(message);
        } catch {
            Console.WriteLine(message);
        }
    }

    static void TryInitWithAssembly(Assembly asm) {
        try {
            if (asm == null) return;
            var t = asm.GetType("BetterSongList.HarmonyPatches.HookLevelCollectionTableSet");
            if (t == null) return;

            var sorter = new DifficultyExpandingSorter("Easy","Normal","Hard","Expert","ExpertPlus");

            var f = t.GetField("sorter", BindingFlags.Public | BindingFlags.Static)
                 ?? t.GetField("sorter", BindingFlags.NonPublic | BindingFlags.Static);
            if (f == null) {
                Log("[DifficultyExpander] Can't find HookLevelCollectionTableSet.sorter field.");
                return;
            }

            f.SetValue(null, sorter);

            var refreshM = t.GetMethod("Refresh", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                        ?? t.GetMethod("Refresh", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (refreshM != null) {
                try {
                    // Попытка вызвать Refresh(true, true)
                    refreshM.Invoke(null, new object[] { true, true });
                } catch { /* ignore */ }
            }

            Log("[DifficultyExpander] Installed sorter into BetterSongList.");
        } catch (Exception ex) {
            Log("[DifficultyExpander] Exception: " + ex);
        }
    }
}
