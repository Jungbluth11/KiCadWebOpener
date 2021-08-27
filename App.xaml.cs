using System.Windows;

namespace KiCadWebOpener
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs args)
        {
            StartUp.Handle(args);
        }
    }
}