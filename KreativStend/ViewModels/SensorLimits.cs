using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace KreativStend
{
    public static class SensorLimits
    {
        private static readonly object Sync = new();

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KreativStend",
            "sensor-limits.json");

        private static readonly Dictionary<string, string> Defaults = new()
        {
            ["DD1_1_Min"] = "-100",
            ["DD1_1_Max"] = "2400",
            ["DD1_2_Min"] = "-100",
            ["DD1_2_Max"] = "2400",
            ["DD2_1_Min"] = "-100",
            ["DD2_1_Max"] = "2400",
            ["DD2_2_Min"] = "-100",
            ["DD2_2_Max"] = "2400",

            ["DD_Ger_Min"] = "-10",
            ["DD_Ger_Max"] = "6000",

            ["TM_Work_Min"] = "-60",
            ["TM_Work_Max"] = "180",
            ["TM_Clear_Min"] = "-60",
            ["TM_Clear_Max"] = "180",

            ["TM_Work_Work_Min"] = "70",
            ["TM_Work_Work_Max"] = "80",
            ["TM_Clear_Work_Min"] = "70",
            ["TM_Clear_Work_Max"] = "80",

            ["Filter1MaxPressureDifference"] = "0",
            ["Filter2MaxPressureDifference"] = "0",

            ["FilterCheckDelay"] = "5000",
            ["FlowmeterMinPulses"] = "100",
            ["FlowmeterStartDelay"] = "2000",
            ["FlowmeterSampleInterval"] = "1000",

            ["V1_X_Min"] = "-999",
            ["V1_X_Max"] = "999",
            ["V1_Y_Min"] = "-999",
            ["V1_Y_Max"] = "999",
            ["V1_Z_Min"] = "-999",
            ["V1_Z_Max"] = "999",
            ["V1_M_Min"] = "0",
            ["V1_M_Max"] = "999",
            ["V1_T_Min"] = "-50",
            ["V1_T_Max"] = "200",

            ["V2_X_Min"] = "-999",
            ["V2_X_Max"] = "999",
            ["V2_Y_Min"] = "-999",
            ["V2_Y_Max"] = "999",
            ["V2_Z_Min"] = "-999",
            ["V2_Z_Max"] = "999",
            ["V2_M_Min"] = "0",
            ["V2_M_Max"] = "999",
            ["V2_T_Min"] = "-50",
            ["V2_T_Max"] = "200",

            ["V1_ZeroIsError"] = "true",
            ["V2_ZeroIsError"] = "true"
        };

        private static Dictionary<string, string> _values =
            new(Defaults);

        static SensorLimits()
        {
            Reload();
        }

        public static void Reload()
        {
            lock (Sync)
            {
                var loaded = new Dictionary<string, string>(Defaults);

                try
                {
                    if (File.Exists(ConfigPath))
                    {
                        string json = File.ReadAllText(ConfigPath);

                        var saved =
                            JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (saved != null)
                        {
                            foreach (var item in saved)
                            {
                                if (Defaults.ContainsKey(item.Key))
                                    loaded[item.Key] = item.Value;
                            }
                        }
                    }
                }
                catch
                {
                    // Если файл повреждён, используем значения по умолчанию.
                }

                _values = loaded;
            }
        }

        public static string GetString(string key)
        {
            lock (Sync)
            {
                return _values.TryGetValue(key, out var value)
                    ? value
                    : Defaults[key];
            }
        }

        public static double GetDouble(string key)
        {
            string value = GetString(key);

            if (double.TryParse(value, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double result)
                && double.IsFinite(result))
            {
                return result;
            }

            if (double.TryParse(value, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out result)
                && double.IsFinite(result))
            {
                return result;
            }

            return double.Parse(
                Defaults[key], CultureInfo.InvariantCulture);
        }

        public static int GetInt(string key)
        {
            double value = GetDouble(key);

            if (value < 0 || value > int.MaxValue)
                return int.Parse(Defaults[key], CultureInfo.InvariantCulture);

            return (int)value;
        }

        public static bool GetBool(string key)
        {
            return bool.TryParse(GetString(key), out bool result)
                ? result
                : bool.Parse(Defaults[key]);
        }

        public static void Save(Dictionary<string, string> values)
        {
            lock (Sync)
            {
                var merged = new Dictionary<string, string>(Defaults);

                foreach (var item in values)
                {
                    if (Defaults.ContainsKey(item.Key))
                        merged[item.Key] = item.Value;
                }

                string? directory = Path.GetDirectoryName(ConfigPath);

                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(
                    ConfigPath,
                    JsonSerializer.Serialize(
                        merged,
                        new JsonSerializerOptions { WriteIndented = true }));

                _values = merged;
            }
        }
    }
}