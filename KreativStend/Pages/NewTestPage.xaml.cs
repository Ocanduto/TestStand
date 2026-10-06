using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KreativStend.Pages
{
    /// <summary>
    /// Логика взаимодействия для NewTestPage.xaml
    /// </summary>
    public partial class NewTestPage : Page
    {
        public NewTestPage()
        {
            InitializeComponent();
        }

        private void On_Keyboard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && Keyboard.FocusedElement is TextBox tb)
            {
                if(btn.CommandParameter is not string cmd)
                    cmd = string.Empty;

                var isBackspace = cmd == "delete";
                var isSpace = cmd == "space";

                var lastindex = tb.CaretIndex;
                if (isBackspace && tb.Text.Length > 0)
                {
                    var new_text = tb.Text.Remove(lastindex - 1);
                    tb.Text = new_text;
                    tb.CaretIndex = lastindex - 1;
                }
                else if (!isBackspace)
                {
                    var text = isSpace ? " " : btn.Content as string;
                    var new_text = tb.Text.Insert(lastindex, text);

                    tb.Text = new_text;
                    tb.CaretIndex = lastindex + 1;
                }
            }
        }
    }
}
