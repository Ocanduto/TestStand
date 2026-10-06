using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KreativStend.Components
{
    [ContentProperty(nameof(Children))]
    public partial class HeaderedGroup : UserControl, INotifyPropertyChanged
    {
        private string _Title = "HEADERED GROUP";
        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                OnPropertyChanged(this, nameof(Title));
            }
        }

        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(HeaderedGroup),
            new PropertyMetadata());

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }

        public HeaderedGroup()
        {
            InitializeComponent();
            DataContext = this;
            Children = PART_Host.Children;
        }

        public void OnPropertyChanged(object sender, string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
