namespace KreativStend.ViewModels
{
    public class ScriptObjectVM : WPFHelper.ViewModel
    {
        public ScriptObjectVM()
        {
        }

        public string Title
        {
            get => Get<string>();
            set => Set(value);
        }

        public string ScriptName
        {
            get => Get<string>();
            set => Set(value);
        }
    }
}
