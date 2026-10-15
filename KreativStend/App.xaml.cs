using KreativStend.ViewModels;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace KreativStend
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _timer = new Timer(On_Connection_Callback, null, 0, 500);
        }

        private Timer _timer;

        private void On_Connection_Callback(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            ProcessKeys.IsConnected = true;
            //if (!ProcessKeys.IsConnected)
            //{
            //    ProcessKeys.OpenConnection.Execute(null);
            //}

            _timer.Change(0, 5000);
        }

        protected override void OnActivated(EventArgs e)
        {

        }


        private static TestKeysVM _ProcessKeys;
        public static TestKeysVM ProcessKeys => _ProcessKeys ??= new TestKeysVM();

        public static void Navigate<T>(Action<T> init = null)
        {
            var wmd = App.Current.MainWindow as MainWindow;
            if (wmd != null)
            {
                var instance = Activator.CreateInstance<T>();
                init?.Invoke(instance);
                ClearHistory(wmd.frame);
                wmd.frame.Navigate(instance);
            }
        }

        public static void NavigateHide()
        {
            var wmd = App.Current.MainWindow as MainWindow;
            if (wmd != null)
            {
                ClearHistory(wmd.frame);
                wmd.frame.Navigate(null);
            }
        }

        private static void ClearHistory(Frame frame)
        {
            if (!frame.CanGoBack && !frame.CanGoForward)
            {
                return;
            }

            var entry = frame.RemoveBackEntry();
            while (entry != null)
            {
                entry = frame.RemoveBackEntry();
            }

            frame.Navigate(new PageFunction<string>() { RemoveFromJournal = true });
        }
    }

}
