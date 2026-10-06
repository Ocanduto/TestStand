using System.Windows;
using System.Windows.Controls;

namespace KreativStend.Pages
{
    public partial class ProcessPage : Page
    {
        public ProcessPage()
        {
            InitializeComponent();
        }
    }

    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Header { get; set; }
        public DataTemplate Informer { get; set; }
        public DataTemplate Message { get; set; }
        public DataTemplate Warning { get; set; }
        public DataTemplate Editor { get; set; }
        public DataTemplate TwoColumn { get; set; }
        public DataTemplate Selector { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if(element != null && item != null && item is ViewModels.MessageVM vm)
            {
                element.DataContext = vm;
                switch (vm.Type)
                {
                    case ViewModels.MessageTypeEnum.Header:
                        return Header;
                    case ViewModels.MessageTypeEnum.Informer:
                        return Informer;
                    case ViewModels.MessageTypeEnum.Message:
                        return Message;
                    case ViewModels.MessageTypeEnum.Warning:
                        return Warning;
                    case ViewModels.MessageTypeEnum.Editor:
                        return Editor;
                    case ViewModels.MessageTypeEnum.TwoColumn:
                        return TwoColumn;
                    case ViewModels.MessageTypeEnum.Selector:
                        return Selector;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
