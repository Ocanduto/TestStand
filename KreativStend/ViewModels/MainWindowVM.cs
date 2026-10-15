using Newtonsoft.Json;

using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KreativStend.ViewModels
{
    public class MainWindowVM : WPFHelper.ViewModel
    {
		public static MainWindowVM Current { get; private set; }

		public MainWindowVM()
        {
			Current = this;

			ScriptItems = [];

            _timer = new Timer(On_Tick_Callback);
            _timer.Change(0, 250);

            UserName = " ";

			TriangleColor = new SolidColorBrush(Color.FromRgb(0x33,0x99,0xff));

            //On_LoadScripts(null);
        }



        private Timer _timer;

        public string Title { get => Get<string>(); set => Set(value); }
        public string UserName { get => Get<string>(); set => Set(value); }

        public ObservableCollection<ScriptObjectVM> ScriptItems { get; set; }
        public ScriptObjectVM SelectedScript { get => Get<ScriptObjectVM>(); set => Set(value); }

        private WPFHelper.Commands.ViewModelCommandHandler<string> _StartScript = new();
        public ICommand StartScript => _StartScript.Get(On_StartScript);

        private WPFHelper.Commands.ViewModelCommandHandler<string> _StartNewCycle = new();
        public ICommand StartNewCycle => _StartNewCycle.Get(On_StartNewCycle);

        private WPFHelper.Commands.ViewModelCommandHandler<string> _OpenHistory = new();
        public ICommand OpenHistory => _OpenHistory.Get(On_OpenHistory);

        private WPFHelper.Commands.ViewModelCommandHandler<string> _LoadScripts = new();
        public ICommand LoadScripts => _LoadScripts.Get(On_LoadScripts);

		public Brush TriangleColor { get => Get<Brush>(); set => Set(value); }

		private void On_StartNewCycle(string arg)
        {
			var data = File.ReadAllText(SelectedScript.ScriptName);
			var items = JsonConvert.DeserializeObject<List<LogicItemVM>>(data);


			App.Navigate<Pages.NewTestPage>(q => q.DataContext = new ProcessStepsVM
            {
                TestType = arg,
				LogicItems = new ObservableCollection<LogicItemVM>(items)
			});
        }

        private void On_OpenHistory(string arg)
        {
            App.Navigate<Pages.HistoryPage>(q => q.DataContext = new HistoryVM());
        }

        public void On_LoadScripts(string arg)
        {
            ScriptItems.Clear();

            var scriptFiles = Directory.GetFiles(Properties.Settings.Default.ScriptsFolder, "*.s");
            foreach (var scriptFile in scriptFiles)
            {
                var fileInfo = new FileInfo(scriptFile);
                ScriptItems.Add(new ScriptObjectVM
                {
                    ScriptName = fileInfo.FullName,
                    Title = fileInfo.Name.Replace(fileInfo.Extension, ""),
                });
            }

            SelectedScript = ScriptItems.FirstOrDefault(q => q.Title == Properties.Settings.Default.DefaultScript);
        }

        private void On_StartScript(string arg)
        {
            var data = File.ReadAllText(SelectedScript.ScriptName);
            var items = JsonConvert.DeserializeObject<List<LogicItemVM>>(data);

            var wnd = new RunningScript
            {
                Owner = App.Current.MainWindow,
                DataContext = new RunningScriptVM
                {
                    LogicItems = new ObservableCollection<LogicItemVM>(items)
                }
            };
            wnd.ShowDialog();
        }

        private void On_Tick_Callback(object? state)
        {
            Title = $"Дата: {DateTime.Now:dd.MM.yyyy} | Время: {DateTime.Now:HH:mm:ss}";
        }
	}
}
