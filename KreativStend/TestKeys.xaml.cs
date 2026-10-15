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
using System.Windows.Shapes;

using System.Text.RegularExpressions;

namespace KreativStend
{
    /// <summary>
    /// Логика взаимодействия для TestKeys.xaml
    /// </summary>
    public partial class TestKeys : Window
    {
        public TestKeys()
        {
            InitializeComponent();
        }

        private static readonly Regex IntegerInputRegex =
         new(@"^-?\d*$");

        private void MotorSpeedInput_PreviewTextInput(
            object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            string newText = textBox.Text.Remove(
                textBox.SelectionStart,
                textBox.SelectionLength);

            newText = newText.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !IntegerInputRegex.IsMatch(newText);
        }

        private void MotorSpeedInput_Pasting(
            object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string));

            if (!IntegerInputRegex.IsMatch(pastedText))
                e.CancelCommand();
        }

        private void Window_Closing(
                object? sender,
                System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is KreativStend.ViewModels.TestKeysVM vm)
            {
                vm.StartMotor = false;
                vm.StopMotor = true;
            }
        }

        public ICommand ApplyMotorSpeedCommand { get; }
    }
}
