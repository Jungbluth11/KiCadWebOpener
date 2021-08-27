using System.Windows;

namespace KiCadWebOpener
{
    /// <summary>
    /// Selects the Window to show - MainWindow if setup is done, DialogFirstRun if not.
    /// </summary>
    public static class StartUp
    {
        public static void Handle(StartupEventArgs args)
        {
            if (Properties.Settings.Default.kicadPath == string.Empty)
            {
                new DialogFirstRun(args).Show();
            }
            else
            {
                MainWindow window = new MainWindow();
                if (args.Args.Length == 1)
                {
                    window.webOpener.HandlerUrl = args.Args[0];
                }
                window.Show();
            }
        }
    }
}