using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KreativStend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
			InitializeComponent();
		}

        private void On_OpenTestKeys(object sender, RoutedEventArgs e)
        {
            var wnd = new TestKeys();
            wnd.Owner = App.Current.MainWindow;
            wnd.ShowDialog();
        }

        private void On_OpenLogicConstructor(object sender, RoutedEventArgs e)
        {
            var wnd = new LogicConstructor();
            wnd.Owner = App.Current.MainWindow;
            wnd.ShowDialog();
        }

        private void On_OpenErrorWindow(object sender, RoutedEventArgs e)
        {
            var wnd = new ErrorWindow
            {
                Owner = App.Current.MainWindow,
                DataContext = App.ProcessKeys
            };

            wnd.ShowDialog();
        }


        private void On_VirtualKeyBoard(object sender, RoutedEventArgs e)
        {
            var wnd = new TestKeys();
            wnd.Owner = App.Current.MainWindow;
            wnd.Show();
        }
	}

    public class TabTipHelper
    {
        private const int WM_SYSCOMMAND = 274;
        private const uint SC_CLOSE = 61536;
        private const uint SC_RESTORE = 0xF120;
        private const uint SC_MAXIMIZE = 0xF030;
        private const uint SC_MINIMIZE = 0xF020;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegisterWindowMessage(string lpString);

        public static int ShowTaptip()
        {
            try
            {
                var file = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "microsoft shared", "ink", "TabTip.exe");
                if (!System.IO.File.Exists(file))
                    return -1;
                Process.Start(new ProcessStartInfo(file)
                {
                    UseShellExecute = true
                });
                return 1;
            }
            catch (Exception)
            {
                return 255;
            }
        }

        public static async Task ShowTaptipAsync()
        {
            await Task.Run(() => ShowTaptip());
        }

        public static void CloseTaptip()
        {
            var touchhWnd = new IntPtr(0);
            touchhWnd = FindWindow("IPTip_Main_Window", null);
            if (touchhWnd == IntPtr.Zero)
                return;
            PostMessage(touchhWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
        }
    }
}