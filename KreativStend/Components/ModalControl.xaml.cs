using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace KreativStend.Components
{
    [ContentProperty(nameof(Children))]
    public partial class ModalControl : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(ModalControl),
            new PropertyMetadata());

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }

        public static readonly DependencyProperty BottomProperty = DependencyProperty.Register(
            nameof(Bottom),
            typeof(UIElementCollection),
            typeof(ModalControl),
            new PropertyMetadata());

        public UIElementCollection Bottom
        {
            get { return (UIElementCollection)GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(ModalControl),
            new PropertyMetadata());

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty ContentVerticalAlignmentProperty = DependencyProperty.Register(
            nameof(ContentVerticalAlignment),
            typeof(VerticalAlignment),
            typeof(ModalControl),
            new PropertyMetadata());

        public VerticalAlignment ContentVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(ContentVerticalAlignmentProperty); }
            set { SetValue(ContentVerticalAlignmentProperty, value); }
        }

        public ModalControl()
        {
            InitializeComponent();
            DataContext = this;
            Children = PART_Host.Children;
            Bottom = PART_Bottom.Children;
        }
    }
}
