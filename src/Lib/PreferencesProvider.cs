using Newtonsoft.Json;
using System;
using System.IO;

namespace TetrifactClient
{
    public class PreferencesProvider : IPreferencesProvider
    {
        private static Preferences _instance;

        Preferences IPreferencesProvider.GetInstance()
        {
            if (_instance == null)
            {
                string settingsPath = Path.Combine(PathHelper.GetInternalDirectory(), "settings.json");
                if (File.Exists(settingsPath))
                {
                    string rawSettings = null;
                    try
                    {
                        rawSettings = File.ReadAllText(settingsPath);
                    }
                    catch (Exception ex)
                    {
                        Alert alert = new Alert();
                        alert.SetContent("Fatal error", $"Could not load settings.json from disk : {ex}");
                        alert.ShowDialog(MainWindow.Instance);
                        alert.OnAccept += () => { Environment.FailFast("Shutting down"); };
                    }

                    try
                    {
                        _instance = JsonConvert.DeserializeObject<Preferences>(rawSettings);
                    }
                    catch (Exception ex)
                    {
                        Alert alert = new Alert();
                        alert.SetContent("Fatal error", $"Could not parse settings.json, file likely corrupt : {ex}");
                        alert.ShowDialog(MainWindow.Instance);
                        alert.OnAccept += () => { Environment.FailFast("Shutting down"); };
                    }
                }
                else 
                { 
                    _instance = new Preferences();
                    _instance.Save();
                }
            }

            return _instance;
        }
    }
}
