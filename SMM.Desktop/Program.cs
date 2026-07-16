using Photino.NET;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SMM.Desktop.Services.Setting;
using SMM.Desktop.Services.Game;
using SMM.Desktop.Bridge;
using System.Text.Json;
using SMM.Desktop.Services.Smapi;
using SMM.Desktop.Services.Mods;

using System.Diagnostics;
using SMM.Desktop.Services.Profiles;
using SMM.Desktop.Services.Language;
using static SMM.Desktop.Services.Setting.SettingsService;

namespace HelloPhotinoApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            void Log(string text)
            {
                File.AppendAllText(
                    Path.Combine(AppContext.BaseDirectory, "startup-log.txt"),
                    $"{DateTime.Now:HH:mm:ss} {text}{Environment.NewLine}"
                );
            }

            Log("1. start");

            // Window title declared here for visibility
            string windowTitle = "Stardewvally Mod Manager";

            var settingsService = new SettingsService();
            var settings = settingsService.Get();

            var gameService = new GameService(
                settingsService,
                new GameFinder(),
                new GameVersionReader());

            var lastGameRunning = false;

            var smapiService = new SmapiService(
                settingsService,
                new SmapiFinder(),
                new SmapiVersionReader());

            var modService = new ModService(
                gameService,
                new ModFinder(new ModReader()),
                new ModValidator(),
                new ModToggleService());

            var httpClient = new HttpClient();
            var modUpdateService = new ModUpdateService(
                httpClient,
                modService
            );

            var appUpdateService = new AppUpdateService(httpClient);

            var profileService = new ProfileService(
                settingsService,
                gameService,
                new ProfileFinder());

            var languageService = new LanguageService();

            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "icon.ico");
            var extractedWwwRoot = ExtractWwwRoot();
            var indexPath = Path.Combine(extractedWwwRoot, "index.html");

            Log($"2. icon exists: {File.Exists(iconPath)} / {iconPath}");
            Log($"3. index exists: {File.Exists(indexPath)} / {indexPath}");

            try
            {
                Log("4. before window");

                var window = new PhotinoWindow()
                    .SetTitle(windowTitle)
                    .SetUseOsDefaultSize(false)
                    .SetSize(new Size(1024, 800))
                    .SetResizable(true)
                    //.SetIconFile(iconPath)
                    .Center()
                    .Load("http://localhost:5173") // 개발

                    .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
                    {
                        var uri = new Uri(url);
                        var path = uri.AbsolutePath.TrimStart('/');

                        return GetEmbeddedResourceStream(path, out contentType);
                    })
                    .RegisterWebMessageReceivedHandler((object sender, string message) =>
                    {
                        var window = (PhotinoWindow)sender;

                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        if (message == "game.getInfo")
                        {
                            var gameInfo = gameService.GetGameInfo();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "game",
                                data = gameInfo
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message == "smapi.getInfo")
                        {
                            var smapiInfo = smapiService.GetSmapiInfo();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "smapi",
                                data = smapiInfo
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message == "mods.getList")
                        {
                            var mods = modService.GetMods();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "mods",
                                data = mods
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message.StartsWith("mods.toggle|"))
                        {
                            var folderPath = message["mods.toggle|".Length..];

                            var success = modService.ToggleMod(folderPath);

                            var mods = modService.GetMods();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "mods",
                                data = mods
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message.StartsWith("mods.openNexus|"))
                        {
                            var nexusUrl = message["mods.openNexus|".Length..];

                            if (!string.IsNullOrWhiteSpace(nexusUrl))
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = nexusUrl,
                                    UseShellExecute = true
                                });
                            }

                            return;
                        }

                        if (message == "profiles.getList")
                        {
                            var profiles = profileService.GetProfiles();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "profiles",
                                data = profiles
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message.StartsWith("profiles.switch|"))
                        {
                            var profileName = message["profiles.switch|".Length..];

                            var success = profileService.SwitchProfile(profileName);

                            var profiles = profileService.GetProfiles();
                            var mods = modService.GetMods();

                            var profileJson = JsonSerializer.Serialize(new
                            {
                                type = "profiles",
                                data = profiles
                            }, jsonOptions);

                            var modsJson = JsonSerializer.Serialize(new
                            {
                                type = "mods",
                                data = mods
                            }, jsonOptions);

                            window.SendWebMessage(profileJson);
                            window.SendWebMessage(modsJson);
                            return;
                        }

                        if (message.StartsWith("profiles.create|"))
                        {
                            var profileName = message["profiles.create|".Length..];

                            profileService.CreateProfile(profileName);

                            var profiles = profileService.GetProfiles();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "profiles",
                                data = profiles
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message.StartsWith("profiles.rename|"))
                        {
                            var payload = message["profiles.rename|".Length..];
                            var parts = payload.Split('|', 2);

                            if (parts.Length == 2)
                            {
                                profileService.RenameProfile(parts[0], parts[1]);
                            }

                            var profiles = profileService.GetProfiles();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "profiles",
                                data = profiles
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message.StartsWith("profiles.backup|"))
                        {
                            var profileName = message["profiles.backup|".Length..];

                            var success = profileService.BackupProfile(profileName);

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "profileAction",
                                data = new
                                {
                                    action = "backup",
                                    success
                                }
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message.StartsWith("profiles.export|"))
                        {
                            var profileName = message["profiles.export|".Length..];

                            using var dialog = new SaveFileDialog
                            {
                                Title = "Export Profile",
                                Filter = "Zip files (*.zip)|*.zip",
                                FileName = $"{profileName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
                            };

                            if (dialog.ShowDialog() != DialogResult.OK)
                                return;

                            var success = profileService.ExportProfile(profileName, dialog.FileName);

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "profileAction",
                                data = new
                                {
                                    action = "export",
                                    success
                                }
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message == "game.start")
                        {
                            var success = gameService.StartSmapi();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "gameStart",
                                data = new
                                {
                                    success
                                }
                            }, jsonOptions);

                            window.SendWebMessage(json);

                            return;
                        }

                        if (message == "mods.openFolder")
                        {
                            var modsPath = settingsService.Get().ModPath;

                            if (Directory.Exists(modsPath))
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = "explorer.exe",
                                    Arguments = $"\"{modsPath}\"",
                                    UseShellExecute = true
                                });
                            }

                            return;
                        }

                        if (message.StartsWith("mods.openFolder|"))
                        {
                            var folderPath = message["mods.openFolder|".Length..];

                            if (Directory.Exists(folderPath))
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = folderPath,
                                    UseShellExecute = true
                                });
                            }

                            return;
                        }

                        if (message == "app.openNexus")
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = "https://www.nexusmods.com/stardewvalley/mods/47462",
                                UseShellExecute = true
                            });

                            return;
                        }

                        if (message == "app.openKofi")
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = "https://ko-fi.com/ralotte",
                                UseShellExecute = true
                            });

                            return;
                        }

                        if (message == "app.getVersion")
                        {
                            var json = JsonSerializer.Serialize(new
                            {
                                type = "appVersion",
                                data = settingsService.GetVersion()
                            }, jsonOptions);

                            window.SendWebMessage(json);

                            return;
                        }

                        if (message == "language.get")
                        {
                            var settings = settingsService.Get();

                            var language = string.IsNullOrWhiteSpace(settings.Language)
                                ? "en"
                                : settings.Language;

                            var data = languageService.GetLanguage(language);

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "language",
                                data
                            }, jsonOptions);

                            window.SendWebMessage(json);
                            return;
                        }

                        if (message == "settings.get")
                        {
                            var settings = settingsService.Get();

                            var json = JsonSerializer.Serialize(new
                            {
                                type = "settings",
                                data = settings
                            }, jsonOptions);

                            window.SendWebMessage(json);

                            return;
                        }

                        if (message.StartsWith("settings.save|"))
                        {
                            var parts = message.Split('|');

                            if (parts.Length >= 3)
                            {
                                var language = parts[1];
                                var theme = parts[2];

                                settingsService.SaveSettings(language, theme);

                                var json = JsonSerializer.Serialize(new
                                {
                                    type = "settingsSaved"
                                }, jsonOptions);

                                window.SendWebMessage(json);

                                var lang = languageService.GetLanguage(language);

                                json = JsonSerializer.Serialize(new
                                {
                                    type = "language",
                                    data = lang
                                }, jsonOptions);

                                window.SendWebMessage(json);
                            }

                            return;
                        }

                        if (message == "mods.checkUpdates")
                        {
                            try
                            {
                                var updates = modUpdateService
                                    .CheckUpdatesAsync()
                                    .GetAwaiter()
                                    .GetResult();

                                var json = JsonSerializer.Serialize(new
                                {
                                    type = "modUpdates",
                                    data = updates
                                }, jsonOptions);

                                window.SendWebMessage(json);
                            }
                            catch (Exception ex)
                            {
                                var json = JsonSerializer.Serialize(new
                                {
                                    type = "modUpdates",
                                    data = Array.Empty<object>(),
                                    error = ex.Message
                                }, jsonOptions);

                                window.SendWebMessage(json);
                            }

                            return;
                        }

                        if (message == "app.checkUpdate")
                        {
                            var result = appUpdateService
                                .CheckAsync()
                                .GetAwaiter()
                                .GetResult();

                            window.SendWebMessage(JsonSerializer.Serialize(new
                            {
                                type = "appUpdate",
                                data = result
                            }, jsonOptions));

                            return;
                        }

                    });
                    //.Load(indexPath);

                var gameCheckTimer = new System.Timers.Timer(3000);
                gameCheckTimer.Elapsed += (_, _) =>
                {
                    var isRunning = gameService.IsGameRunning();

                    if (isRunning == lastGameRunning)
                        return;

                    lastGameRunning = isRunning;

                    var json = JsonSerializer.Serialize(new
                    {
                        type = "gameRunning",
                        data = new
                        {
                            running = isRunning
                        }
                    }, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    window.SendWebMessage(json);
                };

                gameCheckTimer.Start();

                window.WaitForClose();
            }
            catch (Exception ex)
            {
                Log("ERROR");
                Log(ex.ToString());
            }
        }

        private static string ExtractWwwRoot()
        {
            var assembly = typeof(Program).Assembly;

            var outputRoot = Path.Combine(
                Path.GetTempPath(),
                "SMM",
                "wwwroot");

            if (Directory.Exists(outputRoot))
                Directory.Delete(outputRoot, true);

            Directory.CreateDirectory(outputRoot);

            var resources = assembly
                .GetManifestResourceNames()
                .Where(x => x.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var resourceName in resources)
            {
                var relativePath = resourceName["wwwroot/".Length..];

                var outputPath = Path.Combine(
                    outputRoot,
                    relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                var directory = Path.GetDirectoryName(outputPath);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                using var resourceStream = assembly.GetManifestResourceStream(resourceName);

                if (resourceStream == null)
                    continue;

                using var fileStream = File.Create(outputPath);
                resourceStream.CopyTo(fileStream);
            }

            return outputRoot;
        }

        private static Stream GetEmbeddedResourceStream(string path, out string contentType)
        {
            var assembly = typeof(Program).Assembly;

            var normalizedPath = path
                .Replace("\\", "/")
                .TrimStart('/');

            if (string.IsNullOrWhiteSpace(normalizedPath))
                normalizedPath = "index.html";

            var resourcePath = $"wwwroot/{normalizedPath}";

            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name =>
                    name.Replace("\\", "/")
                        .Equals(resourcePath, StringComparison.OrdinalIgnoreCase));

            contentType = GetContentType(normalizedPath);

            if (resourceName == null)
            {
                contentType = "text/html";
                return new MemoryStream(Encoding.UTF8.GetBytes("Resource not found."));
            }

            return assembly.GetManifestResourceStream(resourceName)
                ?? new MemoryStream(Encoding.UTF8.GetBytes("Resource stream not found."));
        }

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".html" => "text/html",
                ".js" => "text/javascript",
                ".css" => "text/css",
                ".svg" => "image/svg+xml",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".ico" => "image/x-icon",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }
    }
}