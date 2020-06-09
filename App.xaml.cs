using System.Windows;

namespace MIRAGE_Launcher
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.GetType().ToString(), null, MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }
    }
}
