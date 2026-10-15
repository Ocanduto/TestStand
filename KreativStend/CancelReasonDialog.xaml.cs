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
	/// Логика взаимодействия для CancelReasonDialog.xaml
	/// </summary>
	public partial class CancelReasonDialog : Window
	{
		public string Reason { get; private set; } = string.Empty;
		public CancelReasonDialog()
		{
			InitializeComponent();
			Loaded += (_, __) => ReasonTextBox.Focus();
		}
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			Reason = ReasonTextBox.Text?.Trim() ?? string.Empty;

			DialogResult = true;
		}
	}
}
