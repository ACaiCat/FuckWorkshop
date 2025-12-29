using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedParameter.Global
// ReSharper disable once UnusedMember.Global
namespace FuckWorkshop;
public static class Patcher
{
    private static Harmony _harmony;

    
    public static int Initialize(nint arg, int sizeBytes)
    {
        try
        {
            Console.WriteLine("========================================");
            Console.WriteLine("[FuckWorkshop] Initializing workshop patch...");
            Console.WriteLine("========================================");
            ApplyHarmonyPatches();
            Console.WriteLine("[FuckWorkshop] Patch initialized successfully");
            Console.WriteLine("========================================");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FuckWorkshop] ERROR: " + ex.Message);
            Console.WriteLine("[FuckWorkshop] Stack: " + ex.StackTrace);
            return -1;
        }
    }

    private static void ApplyHarmonyPatches()
    {
        try
        {
            _harmony = new Harmony("ink.terraria.fuckworkshop");
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "tModLoader");
            if (assembly != null)
            {
                ApplyPatchesInternal(assembly);
            }
            else
            {
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FuckWorkshop] Failed to apply Harmony patches: " + ex.Message);
            throw;
        }
    }

    private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
    {
        try
        {
            if (args.LoadedAssembly.GetName().Name != "tModLoader") return;
            ApplyPatchesInternal(args.LoadedAssembly);
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoaded;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FuckWorkshop] Error in OnAssemblyLoaded: " + ex.Message);
            Console.WriteLine("[FuckWorkshop] Stack: " + ex.StackTrace);
        }
    }

    private static void ApplyPatchesInternal(Assembly assembly)
    {
        var enumType = assembly.GetType("Terraria.Social.Steam.WorkshopHelper+WorkshopSearchReturnState");
        if (enumType == null)
        {
            Console.WriteLine("[FuckWorkshop] ERROR: Could not find WorkshopSearchReturnState enum");
            return;
        }

        if (!Enum.TryParse(enumType, "RetrievalFailed", out var successValue))
        {
            Console.WriteLine("[FuckWorkshop] ERROR: Could not parse Success value");
            return;
        }

        var targetMethod =
            assembly.GetType("Terraria.Social.Steam.WorkshopHelper+QueryHelper+AQueryInstance")!.GetMethod(
                "TryGetModDownloadItem", BindingFlags.Static | BindingFlags.NonPublic);

        if (targetMethod == null)
        {
            Console.WriteLine("[FuckWorkshop] ERROR: Could not find method returning WorkshopSearchReturnState");
            return;
        }

        WorkshopSearchReturnStatePatch.SetSuccessValue(successValue, assembly);

        _harmony.Patch(targetMethod,
            prefix: new HarmonyMethod(typeof(WorkshopSearchReturnStatePatch).GetMethod("Prefix")));
    }

    private static class WorkshopSearchReturnStatePatch
    {
        private static object _successValue;
        private static Assembly _targetAssembly;

        public static void SetSuccessValue(object successValue, Assembly assembly)
        {
            _successValue = successValue;
            _targetAssembly = assembly;
        }


        // ReSharper disable once InconsistentNaming
        public static bool Prefix(ref object __result)
        {
            try
            {
                Console.WriteLine("[FuckWorkshop] WorkshopSearchReturnState patch triggered!");

                if (_successValue == null || _targetAssembly == null) return true;

                __result = _successValue;
                Console.WriteLine("[FuckWorkshop] Returning Success state");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FuckWorkshop] Patch error: {ex.GetType().Name} - {ex.Message}");
                return true;
            }
        }
    }
}