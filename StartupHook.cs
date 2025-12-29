using System;
using System.IO;
using System.Reflection;
using FuckWorkshop;

// ReSharper disable once UnusedType.Global
// ReSharper disable once CheckNamespace
internal class StartupHook
{
    public static void Initialize()
    {
        Console.WriteLine("[StartupHook] DOTNET_STARTUP_HOOKS executing...");
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        var result = Patcher.Initialize(IntPtr.Zero, 0);
        Console.WriteLine($"[StartupHook] Patcher.Initialize returned: {result}");
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        try
        {
            var assemblyName = new AssemblyName(args.Name).Name ?? string.Empty;
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            
            var parentDirectory = Path.GetDirectoryName(currentDirectory) ?? string.Empty;
            var sharedAssemblyPath = Path.Combine(parentDirectory, $"{assemblyName}.dll");
            
            if (File.Exists(sharedAssemblyPath))
            {
                Console.WriteLine($"[StartupHook] Loading shared dependency: {assemblyName} from {sharedAssemblyPath}");
                return Assembly.LoadFrom(sharedAssemblyPath);
            }
            
            var localAssemblyPath = Path.Combine(currentDirectory, $"{assemblyName}.dll");
            
            if (File.Exists(localAssemblyPath))
            {
                Console.WriteLine($"[StartupHook] Loading local dependency: {assemblyName} from {localAssemblyPath}");
                return Assembly.LoadFrom(localAssemblyPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartupHook] Failed to resolve assembly {args.Name}: {ex.Message}");
        }
        
        return null;
    }
}