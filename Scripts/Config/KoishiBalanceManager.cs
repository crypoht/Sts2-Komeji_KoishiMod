namespace KomeijiKoishi.Config;

public static class KoishiBalanceManager
{
    private static bool _initialized;
    private static bool _enabledAtStartup;

    public static bool IsEnabled => _initialized
        ? _enabledAtStartup
        : KomeijiKoishi.KoishiModConfig.EnableBalancePatch;

    public static void Initialize()
    {
        _enabledAtStartup = KomeijiKoishi.KoishiModConfig.EnableBalancePatch;
        _initialized = true;
    }

    public static void SetEnabledForRun(bool enabled)
    {
        _enabledAtStartup = enabled;
        _initialized = true;
    }

    public static decimal Value(decimal normal, decimal balanced)
    {
        return IsEnabled ? balanced : normal;
    }

    public static int Value(int normal, int balanced)
    {
        return IsEnabled ? balanced : normal;
    }

    public static bool EnabledValue(bool normal, bool balanced)
    {
        return IsEnabled ? balanced : normal;
    }
}
