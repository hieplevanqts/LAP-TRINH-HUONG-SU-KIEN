using System.IO;
using System.Text.Json;

namespace HotelManager.WinForms;

public static class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HotelManager",
        "payment_settings.json");

    public static PaymentSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new PaymentSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<PaymentSettings>(json) ?? new PaymentSettings();
        }
        catch
        {
            return new PaymentSettings();
        }
    }

    public static void Save(PaymentSettings settings)
    {
        var directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
