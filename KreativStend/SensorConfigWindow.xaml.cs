
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace KreativStend
{
    public partial class SensorConfigWindow : Window
    {
        private readonly string _configPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "KreativStend",
                "sensor-limits.json");

        private readonly Dictionary<string, string> _defaults = new()
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

            ["FlowmeterMinPulses"] = "100",
            ["FlowmeterStartDelay"] = "2000",
            ["FlowmeterSampleInterval"] = "1000",
            ["FilterCheckDelay"] = "5000",
            ["Filter1MaxPressureDifference"] = "0",
            ["Filter2MaxPressureDifference"] = "0",
        };

        private readonly string[] _rangePrefixes =
        {
            "DD1_1", "DD1_2", "DD2_1", "DD2_2", "DD_Ger",
            "TM_Work", "TM_Clear", "TM_Work_Work", "TM_Clear_Work",
            "V1_X", "V1_Y", "V1_Z", "V1_M", "V1_T",
            "V2_X", "V2_Y", "V2_Z", "V2_M", "V2_T"
        };

        public SensorConfigWindow()
        {
            InitializeComponent();

            // Сначала показываем значения по умолчанию,
            // затем заменяем их сохранёнными, если файл существует.
            ApplyValues(_defaults);
            LoadSettings();
        }

        private void ApplyValues(Dictionary<string, string> values)
        {
            foreach (var item in values)
            {
                if (FindName(item.Key) is TextBox textBox)
                    textBox.Text = item.Value;
            }
        }

        private Dictionary<string, string> ReadValues()
        {
            var result = new Dictionary<string, string>();

            foreach (var key in _defaults.Keys)
            {
                if (FindName(key) is TextBox textBox)
                    result[key] = textBox.Text.Trim();
            }

            return result;
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    StatusText.Text = "Используются значения по умолчанию.";
                    return;
                }

                string json = File.ReadAllText(_configPath);

                var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (saved == null)
                    throw new InvalidDataException("Файл настроек пуст.");

                // Загружаем только известные параметры.
                foreach (var key in _defaults.Keys)
                {
                    if (saved.TryGetValue(key, out string? value))
                    {
                        if (FindName(key) is TextBox textBox)
                            textBox.Text = value;
                    }
                }

                if (saved.TryGetValue(nameof(V1_ZeroIsError), out string? v1))
                    V1_ZeroIsError.IsChecked = v1 == "True";

                if (saved.TryGetValue(nameof(V2_ZeroIsError), out string? v2))
                    V2_ZeroIsError.IsChecked = v2 == "True";

                StatusText.Text = "Загружены сохранённые настройки.";
            }
            catch (Exception ex)
            {
                ApplyValues(_defaults);

                MessageBox.Show(
                    $"Не удалось загрузить настройки.\n{ex.Message}\n\n" +
                    "Восстановлены значения по умолчанию.",
                    "Настройки датчиков",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private bool TryGetNumber(
            Dictionary<string, string> values,
            string key,
            out double number)
        {
            number = 0;

            if (!values.TryGetValue(key, out string? text))
                return false;

            // Принимаем и точку, и запятую в дробных числах.
            text = text.Replace(',', '.');

            return double.TryParse(
                       text,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out number)
                   && double.IsFinite(number);
        }

        private bool ValidateValues(Dictionary<string, string> values)
        {
            foreach (string key in values.Keys)
            {
                if (!TryGetNumber(values, key, out _))
                {
                    MessageBox.Show(
                        $"Проверьте поле «{key}».\n" +
                        "Необходимо ввести корректное конечное число.",
                        "Ошибка ввода",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }
            }

            foreach (string prefix in _rangePrefixes)
            {
                if (!TryGetNumber(values, prefix + "_Min", out double min) ||
                    !TryGetNumber(values, prefix + "_Max", out double max))
                {
                    MessageBox.Show(
                        $"Не удалось прочитать диапазон {prefix}.",
                        "Ошибка настроек",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                if (min >= max)
                {
                    MessageBox.Show(
                        $"Для {prefix} минимум должен быть меньше максимума.",
                        "Неверный диапазон",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }
            }

            string[] positiveIntegerKeys =
            {
                "FlowmeterMinPulses",
                "FlowmeterStartDelay",
                "FlowmeterSampleInterval",
                "FilterCheckDelay"
            };

            foreach (string key in positiveIntegerKeys)
            {
                if (!TryGetNumber(values, key, out double number) ||
                    number < 1 ||
                    number > int.MaxValue ||
                    number != Math.Truncate(number))
                {
                    MessageBox.Show(
                        $"Поле {key} должно содержать целое число больше нуля.",
                        "Неверный параметр",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var values = ReadValues();

            if (!ValidateValues(values))
                return;

            // Чекбоксы сохраняются в том же файле.
            values[nameof(V1_ZeroIsError)] =
                V1_ZeroIsError.IsChecked == true ? "True" : "False";

            values[nameof(V2_ZeroIsError)] =
                V2_ZeroIsError.IsChecked == true ? "True" : "False";

            try
            {
                string? directory = Path.GetDirectoryName(_configPath);

                if (directory == null)
                    throw new InvalidOperationException(
                        "Не удалось определить папку настроек.");

                Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(
                    values,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(_configPath, json);

                StatusText.Text = "Настройки сохранены.";
                MessageBox.Show(
                    "Настройки успешно сохранены.",
                    "Конфигуратор",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сохранить настройки:\n{ex.Message}",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вернуть все лимиты к исходным значениям?",
                "Значения по умолчанию",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            ApplyValues(_defaults);
            V1_ZeroIsError.IsChecked = true;
            V2_ZeroIsError.IsChecked = true;

            StatusText.Text =
                "Исходные значения загружены в поля. Нажмите «Сохранить», чтобы записать их.";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
