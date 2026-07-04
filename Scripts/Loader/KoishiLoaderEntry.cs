using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace KomeijiKoishi.Loader;

[ModInitializer("Init")]
public sealed class KoishiLoaderEntry
{
    private const string ModId = "Komeiji_Koishi";
    private const string StableAssemblyName = "Komeiji_Koishi.Stable.dll";
    private const string BetaAssemblyName = "Komeiji_Koishi.Beta.dll";
    private const string ImplementationEntryType = "KomeijiKoishi.Scripts.Entry";
    private const string ImplementationEntryMethod = "Init";

    public static void Init()
    {
        bool isBeta = DetectBetaApi();
        string selectedAssemblyName = isBeta ? BetaAssemblyName : StableAssemblyName;

        try
        {
            string modDirectory = GetModDirectory();
            string selectedAssemblyPath = Path.Combine(modDirectory, selectedAssemblyName);

            Log.Info(
                "[KoishiLoader] " +
                $"gameVersion={GetGameVersionForLog()} betaApi={isBeta} " +
                $"loading={selectedAssemblyName}");

            if (!File.Exists(selectedAssemblyPath))
            {
                throw new FileNotFoundException(
                    $"Koishi implementation DLL not found. Expected '{selectedAssemblyPath}'. " +
                    "Build/copy both Komeiji_Koishi.Stable.dll and Komeiji_Koishi.Beta.dll before publishing.",
                    selectedAssemblyPath);
            }

            Assembly implementationAssembly = LoadImplementationAssembly(selectedAssemblyPath);
            AssociateImplementationAssembly(implementationAssembly);

            Type entryType = implementationAssembly.GetType(ImplementationEntryType, throwOnError: true)!;
            MethodInfo initMethod = entryType.GetMethod(ImplementationEntryMethod, BindingFlags.Public | BindingFlags.Static)
                ?? throw new MissingMethodException(ImplementationEntryType, ImplementationEntryMethod);

            initMethod.Invoke(null, null);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            Log.Error($"[KoishiLoader] Failed to initialize selected implementation: {ex.InnerException}");
            throw ex.InnerException;
        }
        catch (Exception ex)
        {
            Log.Error($"[KoishiLoader] Failed to load selected implementation: {ex}");
            throw;
        }
    }

    private static bool DetectBetaApi()
    {
        Assembly sts2Assembly = typeof(ModInitializerAttribute).Assembly;
        Type? cardModelType =
            sts2Assembly.GetType("MegaCrit.Sts2.Core.Entities.Cards.CardModel")
            ?? sts2Assembly.GetType("MegaCrit.Sts2.Core.Models.CardModel");

        return cardModelType?.GetMethod(
            "GetResultPileTypeAndPositionForCardPlay",
            BindingFlags.Instance | BindingFlags.NonPublic) != null;
    }

    private static Assembly LoadImplementationAssembly(string assemblyPath)
    {
        AssemblyLoadContext? context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        return context?.LoadFromAssemblyPath(assemblyPath) ?? Assembly.LoadFrom(assemblyPath);
    }

    private static void AssociateImplementationAssembly(Assembly implementationAssembly)
    {
        MethodInfo? associateAssemblyWithMod = typeof(ModManager).GetMethod(
            "AssociateAssemblyWithMod",
            BindingFlags.Public | BindingFlags.Static);

        if (associateAssemblyWithMod != null)
        {
            associateAssemblyWithMod.Invoke(null, new object[] { ModId, implementationAssembly });
            return;
        }

        ModManager.OnModDetected += mod => AssociateModObject(mod, implementationAssembly);

        foreach (object mod in EnumerateMods())
        {
            if (AssociateModObject(mod, implementationAssembly))
            {
                return;
            }
        }

        Log.Warn($"[KoishiLoader] Deferred implementation assembly association for mod {ModId} until OnModDetected.");
    }

    private static bool AssociateModObject(object mod, Assembly implementationAssembly)
    {
        object? manifest = GetMemberValue(mod, "manifest") ?? GetMemberValue(mod, "Manifest");
        string? id =
            GetMemberValue(manifest, "id") as string
            ?? GetMemberValue(manifest, "Id") as string;
        if (id != ModId)
        {
            return false;
        }

        if ((GetMemberValue(mod, "assemblies") ?? GetMemberValue(mod, "Assemblies")) is IList assemblies)
        {
            if (!assemblies.Contains(implementationAssembly))
            {
                assemblies.Add(implementationAssembly);
            }

            Log.Info($"[KoishiLoader] Associated assembly {implementationAssembly} with mod {ModId}");
            return true;
        }

        FieldInfo? assemblyField = mod.GetType().GetField(
            "assembly",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (assemblyField != null)
        {
            assemblyField.SetValue(mod, implementationAssembly);
            Log.Info($"[KoishiLoader] Associated single assembly {implementationAssembly} with mod {ModId}");
            return true;
        }

        Log.Warn($"[KoishiLoader] Found mod {ModId}, but no assembly field/list was available.");
        return false;
    }

    private static IEnumerable EnumerateMods()
    {
        MethodInfo? getLoadedMods = typeof(ModManager).GetMethod("GetLoadedMods", BindingFlags.Public | BindingFlags.Static);
        if (getLoadedMods?.Invoke(null, null) is IEnumerable loadedMods)
        {
            return loadedMods;
        }

        return ModManager.Mods;
    }

    private static object? GetMemberValue(object? instance, string name)
    {
        if (instance == null)
        {
            return null;
        }

        Type type = instance.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return type.GetField(name, flags)?.GetValue(instance)
            ?? type.GetProperty(name, flags)?.GetValue(instance);
    }

    private static string GetModDirectory()
    {
        string? location = Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrWhiteSpace(location))
        {
            string? directory = Path.GetDirectoryName(location);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                return directory;
            }
        }

        return AppContext.BaseDirectory;
    }

    private static string GetGameVersionForLog()
    {
        try
        {
            Assembly sts2Assembly = typeof(ModInitializerAttribute).Assembly;
            Type? nGameType = sts2Assembly.GetType("MegaCrit.Sts2.Core.Nodes.NGame");
            MethodInfo? getGameVersion = nGameType?.GetMethod("GetGameVersion", BindingFlags.Public | BindingFlags.Static);
            return getGameVersion?.Invoke(null, null)?.ToString() ?? "UNKNOWN";
        }
        catch
        {
            return "UNKNOWN";
        }
    }
}
