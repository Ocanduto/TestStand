using Microsoft.Win32;

using Newtonsoft.Json;

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KreativStend.ViewModels
{
    public class LogicConstructorVM : WPFHelper.ViewModel
    {
        public LogicConstructorVM()
        {
            LogicItems = [];
            Functions = [];

            Functions.Add("Delay");
            Functions.Add("Message");
            Functions.Add("Editor");
			Functions.Add("End");
            Functions.Add("Counter");
            Functions.Add("Wait Signal");
            Functions.Add("Delta");
            Functions.Add("DD_Ger");
            Functions.Add("Pump Out");
            Functions.Add("Go Back");
            Functions.Add("CheckGer");
        }

        public ObservableCollection<LogicItemVM> LogicItems { get; set; }
        public ObservableCollection<string> Functions { get; set; }

        public LogicItemVM SelectedItem { get => Get<LogicItemVM>(); set => Set(value); }

        private WPFHelper.Commands.ViewModelCommandHandler _CreateItem = new();
        private WPFHelper.Commands.ViewModelCommandHandler _RemoveItem = new();
        private WPFHelper.Commands.ViewModelCommandHandler _UpItem = new();
        private WPFHelper.Commands.ViewModelCommandHandler _DownItem = new();
        private WPFHelper.Commands.ViewModelCommandHandler _CloneItem = new();
        private WPFHelper.Commands.ViewModelCommandHandler _SaveFile = new();
        private WPFHelper.Commands.ViewModelCommandHandler _OpenFile = new();

        public ICommand CreateItem => _CreateItem.Get(On_CreateItem);
        public ICommand RemoveItem => _RemoveItem.Get(On_RemoveItem, () => SelectedItem != null);
        public ICommand UpItem => _UpItem.Get(On_UpItem, () => SelectedItem != null);
        public ICommand DownItem => _DownItem.Get(On_DownItem, () => SelectedItem != null);
        public ICommand CloneItem => _CloneItem.Get(On_CloneItem, () => SelectedItem != null);

        public ICommand SaveFile => _SaveFile.Get(On_SaveFile, ()=> LogicItems.Count>0);
        public ICommand OpenFile => _OpenFile.Get(On_OpenFile);

        private void On_CreateItem()
        {
            LogicItems.Add(new LogicItemVM());
        }

        private void On_RemoveItem()
        {
            LogicItems.Remove(SelectedItem);
            SelectedItem = null;
        }

        private void On_UpItem()
        {
            var index = LogicItems.IndexOf(SelectedItem);
            if (index > 0)
            {
                LogicItems.Move(index, index - 1);
            }
            else
            {
                LogicItems.Move(index, LogicItems.Count - 1);
            }
        }

        private void On_DownItem()
        {
            var index = LogicItems.IndexOf(SelectedItem);
            if (index < LogicItems.Count - 1)
            {
                LogicItems.Move(index, index + 1);
            }
            else
            {
                LogicItems.Move(index, 0);
            }
        }

        private void On_CloneItem()
        {
            var index = LogicItems.IndexOf(SelectedItem);
            var tmpText = JsonConvert.SerializeObject(LogicItems[index]);
            var newItem = JsonConvert.DeserializeObject<LogicItemVM>(tmpText);
            LogicItems.Insert(index, newItem);
        }

        private void On_SaveFile()
        {
            var state = MessageBox.Show("Вы уверены, что хотите сохранить файл?", "ВНИМАНИЕ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (state != MessageBoxResult.Yes) return;

            var dlg = new SaveFileDialog
            {
                Filter = "Скрипты|*.s",
                InitialDirectory = Properties.Settings.Default.ScriptsFolder,
            };
            var dlgResult = dlg.ShowDialog();
            if (!dlgResult.HasValue || !dlgResult.Value) return;

            var data = JsonConvert.SerializeObject(LogicItems.ToList());
            File.WriteAllText(dlg.FileName, data);

            MessageBox.Show("Файл успешно сохранен.","ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void On_OpenFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Скрипты|*.s",
                InitialDirectory = Properties.Settings.Default.ScriptsFolder,
            };
            var dlgResult = dlg.ShowDialog();
            if (!dlgResult.HasValue || !dlgResult.Value) return;

            var data = File.ReadAllText(dlg.FileName);

            var items = JsonConvert.DeserializeObject<List<LogicItemVM>>(data);
            LogicItems.Clear();
            foreach(var item in items)
            {
                LogicItems.Add(item);
            }

            MessageBox.Show("Файл успешно открыт.", "ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
