using System;
using System.IO;
using System.Text.Json;

namespace HospitalQuality.Services
{
    public class AppSettings
    {
        public bool IsDarkTheme { get; set; }
        public bool IsSettingsSectionExpanded { get; set; }
    }

    public class SettingsService
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;

        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public SettingsService()
        {
            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HospitalQuality"
            );

            _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");
        }

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(_settingsFilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new AppSettings();
                }

                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(_settingsDirectory);

                var json = JsonSerializer.Serialize(settings, _serializerOptions);

                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
            }
        }
    }
}