using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KiCadWebOpener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal KiCadWebOpenerCore webOpener;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            webOpener = new KiCadWebOpenerCore();
            InitializeComponent();
        }

        /// <summary>
        /// Analyses the <see cref="HandlerUrl"/> and sets necessary variables to download and open the project (<seealso cref="KiCadWebOpenerCore.CreateFromHandlerUrl"/>). Sets the language.
        /// </summary>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.language != "en-US")
            {
                CultureInfo cultureInfo = new CultureInfo(Properties.Settings.Default.language);
                Properties.Resources.Culture = cultureInfo;
                LbLocaletringUrl.Content = Properties.Resources.LocaleStringUrl;
                LbLocaleStringDestinationPath.Content = Properties.Resources.LocaleStringDestinationPath;
                BtnLocaleStringDirectorySelector.Content = Properties.Resources.LocaleStringSelectDirectory;
                BtnLocaleStringOpen.Content = Properties.Resources.LocaleStringOpen;
            }
            if (!string.IsNullOrEmpty(webOpener.HandlerUrl))
            {
                try
                {
                    webOpener.CreateFromHandlerUrl();
                    TBoxUrl.Text = webOpener.Url;
                    TBoxDestinationPath.Text = Path.Combine(webOpener.DestinationPath, webOpener.ProjectName);
                }
                catch (Exception ex)
                {
                    webOpener.ShowError(ex.Message);
                }
            }
            TBoxUrl.TextChanged += TBoxUrl_TextChanged;
        }

        /// <summary>
        /// Updates <see cref="KiCadWebOpenerCore.Url"/> sets the destination path if URL ist manually inserted.
        /// </summary>
        private void TBoxUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            webOpener.Url = TBoxUrl.Text;
            webOpener.SetProjekt();
            TBoxDestinationPath.Text = Path.Combine(webOpener.DestinationPath, webOpener.ProjectName);
        }

        /// <summary>
        /// Updates <see cref="KiCadWebOpenerCore.DestinationPath"/>.
        /// </summary>
        private void TBoxDestinationPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            webOpener.DestinationPath = TBoxDestinationPath.Text;
        }

        /// <summary>
        /// Open a directory selector an updates the destination path.
        /// </summary>
        private void BtnLocaleStringDirectorySelector_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = webOpener.DirectorySelector();
                TBoxDestinationPath.Text = Path.Combine(path, webOpener.ProjectName);
            }
            catch (Exception ex)
            {
                webOpener.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Open the project with KiCad an closes the Application.
        /// </summary>
        private void BtnLocaleStringOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnLocaleStringOpen.Content = Properties.Resources.LocaleStringLoading;
                webOpener.OpenProject();
                Close();
            }
            catch (Exception ex)
            {
                BtnLocaleStringOpen.Content = Properties.Resources.LocaleStringOpen;
                webOpener.ShowError(ex.Message);
            }
        }
    }
}