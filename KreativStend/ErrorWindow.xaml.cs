using Data;
using KreativStend.ViewModels;
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

namespace KreativStend
{
	/// <summary>
	/// Логика взаимодействия для ErrorWindow.xaml
	/// </summary>
	public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            
            InitializeComponent();
        }

        private void ActiveErrorsView_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is ErrorItem error)
            {
                e.Accepted = error.IsActive;
            }
            else
            { 
                e.Accepted = false;
            }
        }
        private void ResetErrors_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TestKeysVM vm)
                return;
            foreach (var error in vm.ErrorManager.Errors)
            {
                error.IsActive = false;
            }
        }
    }
}
