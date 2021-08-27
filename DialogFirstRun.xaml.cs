using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KiCadWebOpener
{
    /// <summary>
    /// Interaktionslogik für DialogFirstRun.xaml
    /// </summary>
    public partial class DialogFirstRun : Window
    {
        private string kicadPath;
        private string defaultSavePath;
        private readonly StartupEventArgs args;
        private readonly KiCadWebOpenerCore webOpener;

        /// <summary>
        /// Constructor, presets the language.
        /// </summary>
        public DialogFirstRun(StartupEventArgs args)
        {
            InitializeComponent();
            LbLocaleStringKicadPath.Content = Properties.Resources.LocaleStringKicadPath;
            LbLocaleStringDefaultSavePath.Content = Properties.Resources.LocaleStringDefaultSavePath;
            LbLocaleStringSelectLanguage.Content = Properties.Resources.LocaleStringSelectLanguage;
            DropdownLanguage.Text = CultureInfo.CurrentCulture.Name;
            BtnLocaleStringFileSelectorKicadPath.Content = Properties.Resources.LocaleStringSelectFile;
            BtnLocaleStringDirectorySelectorDefaultSavePath.Content = Properties.Resources.LocaleStringSelectDirectory;
            BtnLocaleStringSave.Content = Properties.Resources.LocaleStringSave;
            this.args = args;
            webOpener = new KiCadWebOpenerCore();
        }

        /// <summary>
        /// Checks whether KiCad installation path and default save path are set or not.
        /// </summary>
        /// <returns>Returns true if both paths are set, else false.</returns>
        private bool PathsAreSet()
        {
            if (kicadPath != null && defaultSavePath != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the path of given the Textbox are valid or not and colorize them and enables the save button if KiCad installation path and default save path are set.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <param name="textBox">Textbox thats displays the path.</param>
        private void ValdiatePath(ref string path, ref TextBox textBox)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                textBox.Background = Brushes.White;
                if (PathsAreSet())
                {
                    BtnLocaleStringSave.IsEnabled = true;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Background = Brushes.White;
                }
                else
                {
                    textBox.Background = Brushes.Red;
                }
                path = null;
            }
        }

        /// <summary>
        ///  Saves the settings when paths are set and shows <see cref="MainWindow"/>.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PathsAreSet())
            {
                Properties.Settings.Default.language = DropdownLanguage.Text;
                Properties.Settings.Default.kicadPath = kicadPath;
                Properties.Settings.Default.defaultSavePath = defaultSavePath;
                Properties.Settings.Default.Save();
                StartUp.Handle(args);
            }
        }

        /// <summary>
        /// Sets the KiCad path.
        /// </summary>
        private void TBoxKicadPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            kicadPath = TBoxKicadPath.Text;
            ValdiatePath(ref kicadPath, ref TBoxKicadPath);
        }

        /// <summary>
        /// Sets the default save path.
        /// </summary>
        private void TBoxDefaultSavePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            defaultSavePath = TBoxDefaultSavePath.Text;
            ValdiatePath(ref defaultSavePath, ref TBoxDefaultSavePath);
        }

        /// <summary>
        /// Open file selector and updates the KiCad path.
        /// </summary>
        private void BtnLocaleStringFileSelectorKicadPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = webOpener.FileSelector();
                TBoxKicadPath.Text = path;
            }
            catch (Exception ex)
            {
                webOpener.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Open a directory selector and updates default save path.
        /// </summary>
        private void BtnLocaleStringDirectorySelectorDefaultSavePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = webOpener.DirectorySelector();
                TBoxDefaultSavePath.Text = path;
            }
            catch (Exception ex)
            {
                webOpener.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Sets the custom URL-scheme and closes the window
        /// </summary>
        private void BtnLocaleStringSave_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (OperatingSystem.IsWindows())
            {
                UrlHandler.RegisterOnWindows(path);
            }
            if (OperatingSystem.IsLinux())
            {
                UrlHandler.RegisterOnLinux(path);
            }
            Close();
        }
    }
}