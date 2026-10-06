using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using WPFHelper.Commands;

namespace KreativStend.ViewModels
{
    public class HistoryVM : WPFHelper.ViewModel
    {
        public HistoryVM()
        {
            Items = [];


            LoadData();
        }

        private async void LoadData()
        {
            //List<TestModel> dataItems = new List<TestModel>();
            //try
            //{
            //    dataItems = await context.TestCollection.ToListAsync();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

            //Items.Clear();

            //foreach (var item in dataItems)
            //{
            //    Items.Add(new HistoryItemVM
            //    {
            //        Id = item.Id,
            //        BrandNumber = item.BrandNumber,
            //        SerialNumberBridge = item.SerialNumberBridge,
            //        SerialNumberHydro = item.SerialNumberHydro,
            //        SerialNumberValve = item.SerialNumberValve,
            //        Operator = item.Operator,
            //        TestType = item.TestType,
            //        CreateDate = item.CreateDate,
            //    });
            //}
        }

        public ObservableCollection<HistoryItemVM> Items { get; set; }
        public HistoryItemVM SelectedItem { get => Get<HistoryItemVM>(); set => Set(value); }

        private ViewModelCommandHandler Open_Handler = new();
        private ViewModelCommandHandler Success_Handler = new();

        public ICommand Open_Command => Open_Handler.Get(On_Open_Command, () => SelectedItem != null);
        public ICommand Success_Command => Success_Handler.Get(On_Success_Command);

        private void On_Open_Command()
        {

        }

        private void On_Success_Command()
        {
            App.NavigateHide();
        }
    }

    public class HistoryItemVM : WPFHelper.ViewModel
    {
        public HistoryItemVM()
        {

        }

        public int Id { get; set; }
        public string TestType { get => Get<string>(); set => Set(value); }
        public string SerialNumberBridge { get => Get<string>(); set => Set(value); }
        public string SerialNumberHydro { get => Get<string>(); set => Set(value); }
        public string SerialNumberValve { get => Get<string>(); set => Set(value); }
        public string BrandNumber { get => Get<string>(); set => Set(value); }
        public string Operator { get => Get<string>(); set => Set(value); }
        public DateTime CreateDate { get => Get<DateTime>(); set => Set(value); }
    }
}
