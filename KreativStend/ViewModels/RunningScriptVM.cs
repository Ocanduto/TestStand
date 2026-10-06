using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace KreativStend.ViewModels
{
    public class RunningScriptVM : WPFHelper.ViewModel
    {
        public RunningScriptVM()
        {
            LogicItems = [];
            Logs = [];

            _timer = new Timer(On_Tick, null, 0, 500);
        }

        private Timer _timer;

        public ObservableCollection<LogItemVM> Logs { get; set; }
        public ObservableCollection<LogicItemVM> LogicItems { get; set; }
        public bool IsRunning { get => Get<bool>(); set => Set(value); }

        private WPFHelper.Commands.ViewModelCommandHandler _StartCycle = new();
        private WPFHelper.Commands.ViewModelCommandHandler _StopCycle = new();

        public ICommand StartCycle => _StartCycle.Get(On_StartCycle, () => !IsRunning);
        public ICommand StopCycle => _StopCycle.Get(On_StopCycle, () => IsRunning);

        private LogItemVM WriteLog(string operation)
        {
            var item = new LogItemVM
            {
                Time = DateTime.Now,
                Operation = operation,
            };

            App.Current.Dispatcher.Invoke(() =>
            {
                Logs.Insert(0, item);
            });

            return item;
        }

        private async void On_Tick(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            if (!IsRunning)
            {
                _timer.Change(0, 500);
                return;
            }

            foreach (var operation in LogicItems)
            {
                var current = WriteLog(operation.Operation);

                if (!IsRunning)
                {
                    current.Status = LogStatuses.STOPPED;
                    continue;
                }

                try
                {
                    current.Status = LogStatuses.RUNNING;

                    await RunFunction(operation.Operation, operation.Function, operation.FunctionArgs, operation.Variable);
                    await SetTags(operation);

                    current.Status = LogStatuses.SUCCESS;
                }
                catch (Exception e)
                {
                    current.Status = LogStatuses.ERROR;
                }

            }

            On_StopCycle();

            _timer.Change(0, 500);
        }

        private async Task SetTags(LogicItemBaseVM operation)
        {
            await Task.Delay(500);
        }

        private async Task RunFunction(string operation, string function, string functionArgs, string Variable)
        {
            if (function == null) return;

            if (function == "Delay")
            {
                if (int.TryParse(functionArgs, out int arg))
                    await Task.Delay(arg);
            }

            if (function == "Message")
            {
                MessageBox.Show(functionArgs, operation, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if(function == "Editor")
            {
                var wnd = new EditorWindow
                {
                    Owner = App.Current.MainWindow,
                    Title = operation
                };
                var task = Task.Run(wnd.ShowDialog);
                await task.ConfigureAwait(false);
            }
            if(function == "Wait Signal")
            {
                MessageBox.Show(functionArgs, operation, MessageBoxButton.OK, MessageBoxImage.Information);
                //switch (Variable)
                //{
                //    case "L1":
                //        int i = 0;
                //        break;
                //    case "L2":
                //        int i = 0;
                //        break;
                //    case "R1":
                //        int i = 0;
                //        break;
                //    case "R2":
                //        int i = 0;
                //        break;
                //    default:
                //        break;
                //}
            }
        }

        private void ShowDialog(Action action)
        {
        }

        private void On_StartCycle()
        {
            IsRunning = true;
            WriteLog("Начало цикла").Status = LogStatuses.SUCCESS;
        }

        private void On_StopCycle()
        {
            IsRunning = false;
            WriteLog("Конец цикла").Status = LogStatuses.SUCCESS;

        }
    }

    public class LogStatuses
    {
        public static string SUCCESS => "Выполнено";
        public static string RUNNING => "В процессе";
        public static string STOPPED => "Прервано";
        public static string ERROR => "Ошибка";
    }
}
