using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace KiCadWebOpener
{
    public class KiCadWebOpenerCore
    {
        private string gitError;

        /// <summary>
        /// An URL of the "kicad-project" scheme.
        /// </summary>
        public string HandlerUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL where to download from.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Path where to save the downloaded files.
        /// </summary>
        public string DestinationPath { get; set; } = Properties.Settings.Default.defaultSavePath;

        /// <summary>
        /// Type of the project (zip or git).
        /// </summary>
        public string ProjecType { get; set; }

        /// <summary>
        /// Name of project.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Analyses the <see cref="HandlerUrl"/> and sets necessary variables to download and open the project.
        /// </summary>
        /// <exception cref="Exception"><see cref="HandlerUrl"/> is not a valid "kicad-project://" URL.</exception>
        public void CreateFromHandlerUrl()
        {
            if (Uri.TryCreate(HandlerUrl, UriKind.Absolute, out Uri uri) && string.Equals(uri.Scheme, "kicad-project", StringComparison.OrdinalIgnoreCase))
            {
                KicadprojectScheme kicadprojectScheme = UrlHandler.Handle(uri);
                ProjectName = kicadprojectScheme.ProjectName;
                ProjecType = kicadprojectScheme.ProjectType;
                Url = kicadprojectScheme.ProjectSource;
            }
            else
            {
                HandlerUrl = string.Empty;
                throw new Exception(Url + " " + Properties.Resources.LocaleStringErrorHandlerUrl);
            }
        }

        /// <summary>
        /// Sets necessary variables to download an open the project.
        /// </summary>
        public void SetProjekt()
        {
            ProjectName = UrlHandler.GetProjektName(Url);
            ProjecType = "git";
            if (Url.EndsWith(".zip"))
            {
                ProjecType = "zip";
            }
        }

        /// <summary>
        /// Open a dialog for selecting a folder an returns the path.
        /// </summary>
        /// <returns>The selected path.</returns>
        /// <exception cref="PlatformNotSupportedException" />
        public string DirectorySelector()
        {
            if (OperatingSystem.IsWindows())
            {
                VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
                folderBrowserDialog.ShowDialog();
                return folderBrowserDialog.SelectedPath;
            }
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Open a dialog for selecting a file an returns the path.
        /// </summary>
        /// <returns>The path to the selected file.</returns>
        /// <exception cref="PlatformNotSupportedException" />
        public string FileSelector()
        {
            if (OperatingSystem.IsWindows())
            {
                VistaOpenFileDialog fileDialog = new VistaOpenFileDialog();
                fileDialog.ShowDialog();
                return fileDialog.FileName;
            }
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Downloads the project from <see cref="Url"/> and opened it in KiCad.
        /// </summary>
        /// <exception cref="Exception">See individual message.</exception>
        /// <exception cref="IOException" />
        /// <exception cref="UnauthorizedAccessException" />
        /// <exception cref="PathTooLongException" />
        public void OpenProject()
        {
            string projectFile = string.Empty;
            try
            {
                Directory.CreateDirectory(DestinationPath);
            }
            catch (PathTooLongException ex)
            {
                throw new PathTooLongException(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            if (ProjecType == "zip")
            {
                DownloadZip();
            }
            else
            {
                DownloadGit();
            }
#if DEBUG
            string[] files = Directory.GetFiles(DestinationPath);
#endif
            foreach (string file in Directory.GetFiles(DestinationPath))
            {
                if (file.EndsWith(".kicad_pro", StringComparison.OrdinalIgnoreCase))
                {
                    projectFile = file;
                    break;
                }
            }
            if (string.IsNullOrEmpty(projectFile))
            {
                throw new IOException(Url + " " + Properties.Resources.LocaleStringErrorInvalidProject);
            }
            try
            {
                Process kicad = new Process();
                kicad.StartInfo.FileName = Properties.Settings.Default.kicadPath;
                kicad.StartInfo.Arguments = projectFile;
#if DEBUG
                kicad.StartInfo.UseShellExecute = true;
#endif
                kicad.Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Downloads a zip file from <see cref="Url"/> and extract the files it contains. Deletes the zip file afterwards.
        /// </summary>
        /// <exception cref="WebException" >Could not download the project.</exception>

        private void DownloadZip()
        {
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile(Url, DestinationPath + ".zip");
            }
            catch (WebException ex)
            {
                throw new WebException(ex.Message);
            }
            finally
            {
                webClient.Dispose();
            }
            ZipFile.ExtractToDirectory(DestinationPath + ".zip", DestinationPath);
            File.Delete(DestinationPath + ".zip");
        }

        /// <summary>
        /// Downloads a git repository from <see cref="Url"/>.
        /// </summary>
        /// <exception cref="WebException" >Could not download the project.</exception>
        /// <exception cref="Exception">See individual message.</exception>
        /// <exception cref="FileNotFoundException"/>
        private void DownloadGit()
        {
            try
            {
                Process git = new Process();
                git.StartInfo.FileName = "git";
                git.StartInfo.Arguments = "clone " + Url + " " + DestinationPath;
#if DEBUG
                git.StartInfo.UseShellExecute = true;
#else
                git.StartInfo.CreateNoWindow = true;
#endif
                git.OutputDataReceived += Git_OutputDataReceived;
                git.Start();
                git.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            if (gitError != null)
            {
                if (gitError.Contains("could not resolve host", StringComparison.OrdinalIgnoreCase))
                {
                    throw new WebException(gitError);
                }
                else
                {
                    throw new FileNotFoundException(gitError);
                }
            }
        }

        /// <summary>
        /// Eventhandler that checks for errors occurring on git.
        /// </summary>
        private void Git_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data.StartsWith("fatal", StringComparison.OrdinalIgnoreCase))
            {
                gitError = "Git: " + e.Data;
            }
        }

        /// <summary>
        /// Shows an error dialog.
        /// </summary>
        /// <param name="msg">Message to show.</param>
        public void ShowError(string msg)
        {
            MessageBox.Show(msg, "KiCad Web Opener - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}