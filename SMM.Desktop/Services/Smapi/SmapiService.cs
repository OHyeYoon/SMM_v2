using SMM.Desktop.Models;
using SMM.Desktop.Services.Setting;

namespace SMM.Desktop.Services.Smapi;

public sealed class SmapiService
{
    private readonly SettingsService _settingsService;
    private readonly SmapiFinder _smapiFinder;
    private readonly SmapiVersionReader _versionReader;

    public SmapiService(
        SettingsService settingsService,
        SmapiFinder smapiFinder,
        SmapiVersionReader versionReader)
    {
        _settingsService = settingsService;
        _smapiFinder = smapiFinder;
        _versionReader = versionReader;
    }

    public SmapiInfo GetSmapiInfo()
    {
        var settings = _settingsService.Get();

        var smapi = _smapiFinder.Find(settings.GamePath);

        if (!smapi.IsInstalled)
            return smapi;

        var version = _versionReader.ReadVersion(smapi.ExecutablePath);

        return new SmapiInfo
        {
            IsInstalled = true,
            SmapiPath = smapi.SmapiPath,
            ExecutablePath = smapi.ExecutablePath,
            Version = version,
            ErrorMessage = string.Empty
        };
    }
}