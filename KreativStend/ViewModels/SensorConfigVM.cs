using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KreativStend.ViewModels
{
    // Корневой объект конфигурации
    public class SensorConfig :  WPFHelper.ViewModel
    {
        public List<SensorGroup> Groups { get; set; } = new List<SensorGroup>();
        public List<SensorPreset> Presets { get; set; } = new List<SensorPreset>();
    }

    // Группа датчиков (список тегов)
    public class SensorGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> SensorTags { get; set; } = new List<string>();
    }

    // Пресет (набор групп с параметрами)
    public class SensorPreset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PresetGroupConfig> Groups { get; set; } = new List<PresetGroupConfig>();
    }

    // Конфигурация группы внутри пресета
    public class PresetGroupConfig
    {
        public string GroupId { get; set; }
        public double Period { get; set; }          // Период опроса в секундах
        public double Delay { get; set; }           // Задержка перед началом записи
        public AggregationType Aggregation { get; set; }
        public bool SaveHistory { get; set; }       // Сохранять все точки
    }

    // Типы агрегации
    public enum AggregationType
    {
        Average,
        Maximum,
        Minimum,
        Sum,
        LastValue,
		Delta,
        RMS
    }

    public class HistoryPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    // Результат работы группы за шаг
    public class SensorGroupResult
    {
        public string GroupName { get; set; }
        public string GroupId { get; set; }
        public int SampleCount { get; set; }
        public AggregationType AggregationType { get; set; }
        public Dictionary<string, double> AggregatedValues { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, List<HistoryPoint>> History { get; set; } // Только если SaveHistory = true
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

	public class ResultSaver
	{
		private readonly string _baseDirectory;

		public ResultSaver(string baseDirectory = null)
		{
			_baseDirectory = baseDirectory ?? "Results";
			if (!Directory.Exists(_baseDirectory))
				Directory.CreateDirectory(_baseDirectory);
		}

		// Сохранить результаты группы
		public async Task SaveGroupResultAsync(SensorGroupResult result, int stepIndex, string stepOperation)
		{
			try
			{
				// Создаем папку для шага
				var stepFolder = Path.Combine(_baseDirectory, $"Step_{stepIndex:000}_{SanitizeFileName(stepOperation)}");
				if (!Directory.Exists(stepFolder))
					Directory.CreateDirectory(stepFolder);

				// 1. Сохраняем агрегированные значения
				string resultFile = Path.Combine(stepFolder, $"{result.GroupName}_result.txt");
				await SaveAggregatedResultAsync(resultFile, result, stepIndex);

				// 2. Сохраняем историю, если есть
				if (result.History != null && result.History.Count > 0)
				{
					string historyFile = Path.Combine(stepFolder, $"{result.GroupName}_history_{DateTime.Now:HH-mm-ss}.json");
					await SaveHistoryAsync(historyFile, result);
				}

				// 3. Сохраняем краткий отчет (все группы в одном файле)
				await SaveSummaryReportAsync(result, stepIndex);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Ошибка сохранения результатов: {ex.Message}");
			}
		}

		private async Task SaveAggregatedResultAsync(string filePath, SensorGroupResult result, int stepIndex)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"=== Шаг {stepIndex} ===");
			sb.AppendLine($"Группа: {result.GroupName}");
			sb.AppendLine($"Тип агрегации: {result.AggregationType}");
			sb.AppendLine($"Количество выборок: {result.SampleCount}");
			sb.AppendLine($"Время начала: {result.StartTime:dd.MM.yyyy HH:mm:ss.fff}");
			sb.AppendLine($"Время окончания: {result.EndTime:dd.MM.yyyy HH:mm:ss.fff}");
			sb.AppendLine("--- Результаты ---");

			foreach (var kvp in result.AggregatedValues)
			{
				sb.AppendLine($"{kvp.Key} = {kvp.Value:F4}");
			}

			sb.AppendLine("--- Конец ---");
			sb.AppendLine();

			await File.AppendAllTextAsync(filePath, sb.ToString());
		}

		private async Task SaveHistoryAsync(string filePath, SensorGroupResult result)
		{
			var historyData = new
			{
				GroupName = result.GroupName,
				AggregationType = result.AggregationType.ToString(),
				SampleCount = result.SampleCount,
				TimeRange = new
				{
					Start = result.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
					End = result.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
				},
				Sensors = result.History.Select(kvp => new
				{
					SensorId = kvp.Key,
					Points = kvp.Value.Select(p => new
					{
						// Используем локальное время без смещения
						Time = p.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
						Value = p.Value
					}).ToList(),
					PointsCount = kvp.Value.Count
				}).ToList()
			};

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new JsonStringEnumConverter() }
			};

			string json = System.Text.Json.JsonSerializer.Serialize(historyData, options);
			await File.WriteAllTextAsync(filePath, json);
		}

		private async Task SaveSummaryReportAsync(SensorGroupResult result, int stepIndex)
		{
			string summaryFile = Path.Combine(_baseDirectory, $"Summary_Step_{stepIndex:000}.txt");

			var sb = new StringBuilder();
			sb.AppendLine($"--- Шаг {stepIndex}: {result.GroupName} ---");
			sb.AppendLine($"Время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
			sb.AppendLine($"Тип: {result.AggregationType}");

			foreach (var kvp in result.AggregatedValues)
			{
				sb.AppendLine($"{kvp.Key}: {kvp.Value:F4}");
			}

			sb.AppendLine($"Выборок: {result.SampleCount}");
			sb.AppendLine("---");

			await File.AppendAllTextAsync(summaryFile, sb.ToString());
		}

		private string SanitizeFileName(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return "Unknown";

			// Удаляем недопустимые символы
			char[] invalidChars = Path.GetInvalidFileNameChars();
			foreach (char c in invalidChars)
			{
				fileName = fileName.Replace(c, '_');
			}

			// Ограничиваем длину
			if (fileName.Length > 50)
				fileName = fileName.Substring(0, 50);

			return fileName;
		}
	}
	public class SensorAggregator
	{
		private readonly string _groupId;
		private readonly string _groupName;
		private readonly AggregationType _aggregationType;
		private readonly bool _saveHistory;

		// История теперь хранит HistoryPoint
		private readonly Dictionary<string, List<HistoryPoint>> _history = new Dictionary<string, List<HistoryPoint>>();

		private readonly Dictionary<string, double> _sumValues = new Dictionary<string, double>();
		private readonly Dictionary<string, double> _sumSquares = new Dictionary<string, double>();
		private readonly Dictionary<string, double> _minValues = new Dictionary<string, double>();
		private readonly Dictionary<string, double> _maxValues = new Dictionary<string, double>();
		private readonly Dictionary<string, double> _lastValues = new Dictionary<string, double>();
		private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

		private int _totalSamples = 0;
		private bool _isInitialized = false;

		public SensorAggregator(string groupId, string groupName, AggregationType aggregationType, bool saveHistory)
		{
			_groupId = groupId;
			_groupName = groupName;
			_aggregationType = aggregationType;
			_saveHistory = saveHistory;
		}

		// Добавить новые показания датчиков с временем
		public void AddData(Dictionary<string, double> readings, DateTime timestamp)
		{
			_totalSamples++;

			foreach (var kvp in readings)
			{
				string sensorId = kvp.Key;
				double value = kvp.Value;

				// Инициализация словарей для нового датчика
				if (!_counts.ContainsKey(sensorId))
				{
					InitializeSensor(sensorId);
				}

				// Обновляем счетчик
				_counts[sensorId]++;

				// Обновляем значения в зависимости от типа агрегации
				switch (_aggregationType)
				{
					case AggregationType.Average:
					case AggregationType.Sum:
						_sumValues[sensorId] += value;
						break;

					case AggregationType.Maximum:
						if (value > _maxValues[sensorId])
							_maxValues[sensorId] = value;
						break;

					case AggregationType.Minimum:
						if (value < _minValues[sensorId])
							_minValues[sensorId] = value;
						break;

					case AggregationType.RMS:
						_sumSquares[sensorId] += value * value;
						break;

					case AggregationType.LastValue:
						_lastValues[sensorId] = value;
						break;
					case AggregationType.Delta:
						if (value < _minValues[sensorId])
							_minValues[sensorId] = value;
						if (value > _maxValues[sensorId])
							_maxValues[sensorId] = value;
						break;
				}

				// Сохраняем историю с временем
				if (_saveHistory)
				{
					if (!_history.ContainsKey(sensorId))
						_history[sensorId] = new List<HistoryPoint>();

					_history[sensorId].Add(new HistoryPoint
					{
						Timestamp = timestamp,
						Value = value
					});
				}
			}

			_isInitialized = true;
		}

		// Перегрузка для совместимости со старым кодом (без времени)
		public void AddData(Dictionary<string, double> readings)
		{
			AddData(readings, DateTime.Now);
		}

		private void InitializeSensor(string sensorId)
		{
			if (!_counts.ContainsKey(sensorId))
				_counts[sensorId] = 0;

			if (!_sumValues.ContainsKey(sensorId))
				_sumValues[sensorId] = 0;

			if (!_sumSquares.ContainsKey(sensorId))
				_sumSquares[sensorId] = 0;

			if (!_minValues.ContainsKey(sensorId))
				_minValues[sensorId] = double.MaxValue;

			if (!_maxValues.ContainsKey(sensorId))
				_maxValues[sensorId] = double.MinValue;

			if (!_lastValues.ContainsKey(sensorId))
				_lastValues[sensorId] = 0;

			if (_saveHistory && !_history.ContainsKey(sensorId))
				_history[sensorId] = new List<HistoryPoint>();
		}

		// Получить результат агрегации
		public SensorGroupResult GetResult(DateTime startTime, DateTime endTime)
		{
			var result = new SensorGroupResult
			{
				GroupId = _groupId,
				GroupName = _groupName,
				AggregationType = _aggregationType,
				SampleCount = _totalSamples,
				StartTime = startTime,
				EndTime = endTime,
				AggregatedValues = new Dictionary<string, double>(),
				History = _saveHistory ? new Dictionary<string, List<HistoryPoint>>(_history) : null
			};

			// Вычисляем финальные значения для каждого датчика
			foreach (var sensorId in _counts.Keys)
			{
				double finalValue = 0;
				int count = _counts[sensorId];

				if (count == 0)
					continue;

				switch (_aggregationType)
				{
					case AggregationType.Average:
						finalValue = _sumValues[sensorId] / count;
						break;

					case AggregationType.Sum:
						finalValue = _sumValues[sensorId];
						break;

					case AggregationType.Maximum:
						finalValue = _maxValues[sensorId];
						break;

					case AggregationType.Minimum:
						finalValue = _minValues[sensorId];
						break;

					case AggregationType.RMS:
						finalValue = Math.Sqrt(_sumSquares[sensorId] / count);
						break;

					case AggregationType.LastValue:
						finalValue = _lastValues[sensorId];
						break;

					case AggregationType.Delta:
						finalValue = _maxValues[sensorId] - _minValues[sensorId];
						break;
				}

				result.AggregatedValues[sensorId] = finalValue;
			}

			return result;
		}

		public void Reset()
		{
			_totalSamples = 0;
			_isInitialized = false;
			_counts.Clear();
			_sumValues.Clear();
			_sumSquares.Clear();
			_minValues.Clear();
			_maxValues.Clear();
			_lastValues.Clear();
			if (_saveHistory)
				_history.Clear();
		}

		public bool HasData => _totalSamples > 0;
	}

	public class SensorConfigLoader
	{
		private static SensorConfig _config;
		private static readonly object _lock = new object();
		private static string _configPath = "SensorConfig.json";

		public static SensorConfig LoadConfig(string filePath = null)
		{
			if (!string.IsNullOrEmpty(filePath))
				_configPath = filePath;

			lock (_lock)
			{
				if (_config != null)
					return _config;

				try
				{
					if (!File.Exists(_configPath))
					{
						// Если файла нет — создаем пустую конфигурацию с примерами
						_config = CreateDefaultConfig();
						SaveConfig();
						return _config;
					}

					string json = File.ReadAllText(_configPath);
					//var options = new JsonSerializerOptions
					//{
					//    PropertyNameCaseInsensitive = true,
					//    WriteIndented = true
					//};

					_config = JsonConvert.DeserializeObject<SensorConfig>(json);
					return _config;
				}
				catch (Exception ex)
				{
					// Логируем ошибку и создаем дефолтную конфигурацию
					System.Diagnostics.Debug.WriteLine($"Ошибка загрузки конфигурации: {ex.Message}");
					_config = CreateDefaultConfig();
					return _config;
				}
			}
		}

		public static void SaveConfig()
		{
			lock (_lock)
			{
				if (_config == null) return;

				try
				{
					var options = new JsonSerializerOptions
					{
						WriteIndented = true
					};

					string json = System.Text.Json.JsonSerializer.Serialize(_config, options);
					File.WriteAllText(_configPath, json);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Ошибка сохранения конфигурации: {ex.Message}");
					throw;
				}
			}
		}

		private static SensorConfig CreateDefaultConfig()
		{
			return new SensorConfig
			{
				Groups = new List<SensorGroup>
				{
					new SensorGroup
					{
						Id = "grp_pressure",
						Name = "Фрикционы",
						Description = "Датчики давления фрикционов",
						SensorTags = new List<string> { "DD_A", "DD_B", "DD_C", "DD_D", "DD_E", "DD_F", "DD_G", "DD_H", "DD_J", "DD_P" }
					},
					new SensorGroup
					{
						Id = "grp_vibration",
						Name = "Вибрация",
						Description = "Датчики вибрации",
						SensorTags = new List<string> { "Vibro_1", "Vibro_2", "Vibro_3", "Vibro_4" }
					}
				},
				Presets = new List<SensorPreset>
				{
					new SensorPreset
					{
						Id = "prs_standard",
						Name = "Стандартный контроль",
						Description = "Стандартный режим записи",
						Groups = new List<PresetGroupConfig>
						{
							new PresetGroupConfig { GroupId = "grp_pressure", Period = 0.1, Delay = 0.5, Aggregation = AggregationType.Average, SaveHistory = false },
							new PresetGroupConfig { GroupId = "grp_vibration", Period = 0.05, Delay = 0.5, Aggregation = AggregationType.Maximum, SaveHistory = true }
						}
					}
				}
			};
		}

		// Получить пресет по ID
		public static SensorPreset GetPreset(string presetId)
		{
			var config = LoadConfig();
			return config.Presets.Find(p => p.Id == presetId);
		}

		// Получить группу по ID
		public static SensorGroup GetGroup(string groupId)
		{
			var config = LoadConfig();
			return config.Groups.Find(g => g.Id == groupId);
		}
	}

	public class StepExecutionManager
	{
		private SensorConfig _config;
		private Dictionary<string, SensorAggregator> _aggregators = new Dictionary<string, SensorAggregator>();
		private Dictionary<string, DateTime> _lastReadTimes = new Dictionary<string, DateTime>();
		private Dictionary<string, DateTime> _startTimes = new Dictionary<string, DateTime>();
		private Dictionary<string, bool> _delaySustained = new Dictionary<string, bool>();
		private Dictionary<string, PresetGroupConfig> _activeGroups = new Dictionary<string, PresetGroupConfig>();
		private List<string> _activeGroupIds = new List<string>();
		private bool _isRecording = false;

		public event Action<SensorGroupResult> OnGroupCompleted;
		public event Action<SensorGroupResult> OnDataUpdated; // Для реального времени

		// Подготовка шага к выполнению
		public bool PrepareStep(string presetId, SensorConfig sensorConfig)
		{
			try
			{
				// Загружаем конфигурацию
				_config = sensorConfig;    //SensorConfigLoader.LoadConfig("C:\\Users\\User\\Desktop\\KreativStend\\KreativStend\\KreativStend\\Properties\\SensorConfig.json");

				// Находим пресет
				var preset = SensorConfigLoader.GetPreset(presetId);
				if (preset == null)
				{
					System.Diagnostics.Debug.WriteLine($"Пресет '{presetId}' не найден");
					return false;
				}

				// Очищаем предыдущие данные
				_aggregators.Clear();
				_lastReadTimes.Clear();
				_startTimes.Clear();
				_delaySustained.Clear();
				_activeGroups.Clear();
				_activeGroupIds.Clear();

				// Инициализируем группы из пресета
				foreach (var groupConfig in preset.Groups)
				{
					var group = SensorConfigLoader.GetGroup(groupConfig.GroupId);
					if (group == null)
					{
						System.Diagnostics.Debug.WriteLine($"Группа '{groupConfig.GroupId}' не найдена");
						continue;
					}

					// Создаем агрегатор
					var aggregator = new SensorAggregator(
						group.Id,
						group.Name,
						groupConfig.Aggregation,
						groupConfig.SaveHistory
					);

					_aggregators[group.Id] = aggregator;
					_activeGroups[group.Id] = groupConfig;
					_activeGroupIds.Add(group.Id);

					System.Diagnostics.Debug.WriteLine($"Подготовлена группа: {group.Name} (период {groupConfig.Period}с)");
				}

				_isRecording = false;
				return _aggregators.Count > 0;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Ошибка подготовки шага: {ex.Message}");
				return false;
			}
		}

		// Запуск записи (вызывается в начале шага)
		public void StartRecording()
		{
			var now = DateTime.Now;

			foreach (var groupId in _activeGroupIds)
			{
				_startTimes[groupId] = now;
				_lastReadTimes[groupId] = now;
				_delaySustained[groupId] = false;
				_aggregators[groupId].Reset();
			}

			_isRecording = true;
			System.Diagnostics.Debug.WriteLine($"Запись начата в {now:T}");
		}

		// Обработка данных (вызывается в цикле шага)
		public async Task ProcessDataAsync(CancellationToken cancellationToken)
		{
			if (!_isRecording || _activeGroupIds.Count == 0)
				return;

			var now = DateTime.Now;

			// Сначала читаем ВСЕ датчики из ПЛК (один раз за цикл)
			var allReadings = await ReadAllSensorsFromPLCAsync();

			// Распределяем данные по группам
			foreach (var groupId in _activeGroupIds)
			{
				var groupConfig = _activeGroups[groupId];
				var group = SensorConfigLoader.GetGroup(groupId);
				var aggregator = _aggregators[groupId];

				if (group == null || aggregator == null)
					continue;

				// Проверяем задержку
				if (!_delaySustained[groupId])
				{
					var elapsedDelay = (now - _startTimes[groupId]).TotalSeconds;
					if (elapsedDelay >= groupConfig.Delay)
					{
						_delaySustained[groupId] = true;
						_lastReadTimes[groupId] = now;
						System.Diagnostics.Debug.WriteLine($"Задержка {groupConfig.Delay}с для группы {group.Name} прошла");
					}
					else
					{
						continue; // Задержка еще не прошла
					}
				}

				// Проверяем период опроса
				var elapsedSinceLastRead = (now - _lastReadTimes[groupId]).TotalSeconds;
				if (elapsedSinceLastRead >= groupConfig.Period)
				{
					// Собираем данные только для датчиков этой группы
					var groupReadings = new Dictionary<string, double>();
					foreach (var tag in group.SensorTags)
					{
						if (allReadings.ContainsKey(tag))
						{
							groupReadings[tag] = allReadings[tag];
						}
					}

					if (groupReadings.Any())
					{
						// Передаем в агрегатор
						aggregator.AddData(groupReadings, now);
						_lastReadTimes[groupId] = now;

						// Отправляем событие для обновления UI (опционально)
						if (OnDataUpdated != null)
						{
							var result = aggregator.GetResult(_startTimes[groupId], now);
							OnDataUpdated?.Invoke(result);
						}
					}
				}
			}

			// Небольшая пауза, чтобы не грузить процессор
			await Task.Delay(10, cancellationToken);
		}

		// Чтение всех датчиков из ПЛК (адаптируй под свой код)
		private async Task<Dictionary<string, double>> ReadAllSensorsFromPLCAsync()
		{
			var readings = new Dictionary<string, double>();

			try
			{
				// ==============================================
				// ЗДЕСЬ ТВОЙ КОД ЧТЕНИЯ ИЗ ПЛК
				// ==============================================
				// Например:
				// readings["DD_A"] = await ReadPLCValueAsync("DD_A");
				// readings["Vibro_1"] = await ReadPLCValueAsync("Vibro_1");
				// ...

				// Пока используем заглушку с случайными значениями
				//await Task.Delay(5); // Имитация задержки

				//// Заглушка: генерируем случайные значения для демонстрации
				//var random = new Random();
				//foreach (var groupId in _activeGroupIds)
				//{
				//    var group = SensorConfigLoader.GetGroup(groupId);
				//    if (group != null)
				//    {
				//        foreach (var tag in group.SensorTags)
				//        {
				//            if (!readings.ContainsKey(tag))
				//            {
				//                // Симуляция значений датчиков
				//                readings[tag] = 50 + (random.NextDouble() - 0.5) * 20;
				//            }
				//        }
				//    }
				//}

				foreach (var sensor in App.ProcessKeys.Sensors)
				{
					//readings[sensor.Key] = sensor.Value;
					Random rand = new Random();
					readings[sensor.Key] = 50 + (rand.NextDouble() - 0.5) * 20;
				}



				//readings["DD1_1"] = App.ProcessKeys.DD1_1;
				//readings["DD1_2"] = App.ProcessKeys.DD1_2;
				//readings["DD2_1"] = App.ProcessKeys.DD2_1;
				//readings["DD2_2"] = App.ProcessKeys.DD2_2;
				//readings["DD3"] = App.ProcessKeys.DD3;
				//readings["DD4"] = App.ProcessKeys.DD4;

				//readings["DD_A"] = App.ProcessKeys.DD_A;
				//readings["DD_B"] = App.ProcessKeys.DD_B;
				//readings["DD_C"] = App.ProcessKeys.DD_C;
				//readings["DD_D"] = App.ProcessKeys.DD_D;
				//readings["DD_E"] = App.ProcessKeys.DD_E;
				//readings["DD_F"] = App.ProcessKeys.DD_F;
				//readings["DD_G"] = App.ProcessKeys.DD_G;
				//readings["DD_H"] = App.ProcessKeys.DD_H;
				//readings["DD_J"] = App.ProcessKeys.DD_J;
				//readings["DD_P"] = App.ProcessKeys.DD_P;

				//readings["DD_Ger"] = App.ProcessKeys.DD_Ger;
				//readings["DD_SU"] = App.ProcessKeys.DD_SU;

				//readings["TM_Work"] = App.ProcessKeys.TM_Work;
				//readings["TM2"] = App.ProcessKeys.TM2;
				//readings["TM3"] = App.ProcessKeys.TM3;

				//readings["V_X_1"] = App.ProcessKeys.V_X_1;
				//readings["V_Y_1"] = App.ProcessKeys.V_Y_1;
				//readings["V_Z_1"] = App.ProcessKeys.V_Z_1;
				//readings["V_M_1"] = App.ProcessKeys.V_M_1;


				//readings["V_X_2"] = App.ProcessKeys.V_X_2;
				//readings["V_Y_2"] = App.ProcessKeys.V_Y_2;
				//readings["V_Z_2"] = App.ProcessKeys.V_Z_2;
				//readings["V_M_2"] = App.ProcessKeys.V_M_2;

				//readings["V_X_3"] = App.ProcessKeys.V_X_3;
				//readings["V_Y_3"] = App.ProcessKeys.V_Y_3;
				//readings["V_Z_3"] = App.ProcessKeys.V_Z_3;
				//readings["V_M_3"] = App.ProcessKeys.V_M_3;

				//readings["V_X_4"] = App.ProcessKeys.V_X_4;
				//readings["V_Y_4"] = App.ProcessKeys.V_Y_4;
				//readings["V_Z_4"] = App.ProcessKeys.V_Z_4;
				//readings["V_M_4"] = App.ProcessKeys.V_M_4;

				//readings["V_X_5"] = App.ProcessKeys.V_X_5;
				//readings["V_Y_5"] = App.ProcessKeys.V_Y_5;
				//readings["V_Z_5"] = App.ProcessKeys.V_Z_5;
				//readings["V_M_5"] = App.ProcessKeys.V_M_5;

				//readings["V_X_6"] = App.ProcessKeys.V_X_6;
				//readings["V_Y_6"] = App.ProcessKeys.V_Y_6;
				//readings["V_Z_6"] = App.ProcessKeys.V_Z_6;
				//readings["V_M_6"] = App.ProcessKeys.V_M_6;

				//readings["V_X_7"] = App.ProcessKeys.V_X_7;
				//readings["V_Y_7"] = App.ProcessKeys.V_Y_7;
				//readings["V_Z_7"] = App.ProcessKeys.V_Z_7;
				//readings["V_M_7"] = App.ProcessKeys.V_M_7;

				//readings["V_X_8"] = App.ProcessKeys.V_X_8;
				//readings["V_Y_8"] = App.ProcessKeys.V_Y_8;
				//readings["V_Z_8"] = App.ProcessKeys.V_Z_8;
				//readings["V_M_8"] = App.ProcessKeys.V_M_8;

				//readings["Speed"] = App.ProcessKeys.Speed;
				//readings["Torque"] = App.ProcessKeys.Torque;


				return readings;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Ошибка чтения ПЛК: {ex.Message}");
				return readings;
			}
		}

		// Финализация шага (вызывается по окончании шага)
		public List<SensorGroupResult> FinalizeRecording()
		{
			var results = new List<SensorGroupResult>();
			var endTime = DateTime.Now;

			if (!_isRecording)
				return results;

			foreach (var groupId in _activeGroupIds)
			{
				var aggregator = _aggregators[groupId];
				if (aggregator != null && aggregator.HasData)
				{
					var result = aggregator.GetResult(_startTimes[groupId], endTime);
					results.Add(result);

					// Вызываем событие завершения группы
					OnGroupCompleted?.Invoke(result);

					System.Diagnostics.Debug.WriteLine($"Группа {result.GroupName}: записано {result.SampleCount} выборок");
				}
			}

			_isRecording = false;
			return results;
		}

		// Получить список активных групп (для отладки)
		public List<string> GetActiveGroupIds() => _activeGroupIds;

		// Проверка, идет ли запись
		public bool IsRecording => _isRecording;
	}
}
