namespace KreativStend.ViewModels
{
    public class LogItemVM : WPFHelper.ViewModel
    {
        public DateTime Time { get => Get<DateTime>(); set => Set(value); }
        public string Operation { get => Get<string>(); set => Set(value); }
        public string Status { get => Get<string>(); set => Set(value); }
    }
}
