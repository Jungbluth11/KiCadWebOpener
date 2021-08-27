using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Win32;

namespace KiCadWebOpener
{
    /// <summary>
    /// URL-handler for the custom scheme (kicad-project://)
    /// </summary>
    internal static class UrlHandler
    {
        private const string UrlScheme = "kicad-project";
        private const string FriendlyName = "KiCad Project";

        /// <summary>
        /// Registers the "kicad-project"-scheme on Windows.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">OS is not Windows.</exception>
        /// <exception cref="SecurityException">You don't have the permissions to register the scheme.</exception>
        /// <exception cref="UnauthorizedAccessException">You don't have the permissions to register the scheme.</exception>
        /// <exception cref="IOException">See individual message.</exception>
        public static void RegisterOnWindows(string applicationLocation)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("OS musst be Windows");
            }
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UrlScheme))
                {
                    key.SetValue("", "URL:" + FriendlyName);
                    key.SetValue("URL Protocol", "");
                    using (RegistryKey defaultIcon = key.CreateSubKey("DefaultIcon"))
                    {
                        defaultIcon.SetValue("", applicationLocation + ",1");
                    }
                    using (RegistryKey commandKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                    }
                }
            }
            catch (SecurityException ex)
            {
                throw new SecurityException(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Registers the "kicad-project"-scheme on Linux.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">OS is not Linux.</exception>
        /// <exception cref="UnauthorizedAccessException">You don't have the permissions to register the scheme.</exception>
        /// <exception cref="IOException">See individual message.</exception>
        /// <exception cref="FileNotFoundException" />
        public static void RegisterOnLinux(string applicationLocation)
        {
            if (!OperatingSystem.IsLinux())
            {
                throw new PlatformNotSupportedException("OS musst be Linux");
            }
            try
            {
                File.WriteAllText("/usr/share/applications/KiCadWebOpener.desktop", "[Desktop Entry]\nType=Application\nName=" + FriendlyName + "\nExec=" + applicationLocation + " %u\nStartupNotify=false\nMimeType=x-scheme-handler/" + UrlScheme + "; ");
                Process.Start("xdg-mime default kicadwebopner.desktop x-scheme-handler/" + UrlScheme);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Analyses the <see cref="Uri"/> and returns a struct of <see cref="KicadprojectScheme"/> to represent it.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to analyze.</param>
        /// <returns>A struct that's represents the scheme.</returns>
        /// <exception cref="FormatException">The given <see cref="Uri"/> is not a valid "kicad-project://" URI.</exception>
        public static KicadprojectScheme Handle(Uri uri)
        {
            if (Regex.IsMatch(uri.OriginalString, "kicad-project:\\/\\/(zip|git)\\?source=https?%3a%2f%2f.+\\.[a-z]{2,}%2f.+", RegexOptions.IgnoreCase))
            {
                string projectType = uri.Host;
                string projectSource = HttpUtility.UrlDecode(uri.Query.Substring(8));
                string projectName = GetProjektName(projectSource);
                return new KicadprojectScheme(projectType, projectSource, projectName);
            }
            throw new FormatException(uri + " " + Properties.Resources.LocaleStringErrorUrlHandlerInvalidUri);
        }

        /// <summary>
        /// Creates the project name form the download URL.
        /// </summary>
        /// <param name="url">The URL where to download the project from.</param>
        /// <returns>The name of the project.</returns>
        public static string GetProjektName(string url)
        {
            string projectName = url.Split('/').Last();
            if (projectName.EndsWith(".zip") || projectName.EndsWith(".git"))
            {
                projectName = projectName.Remove(projectName.Length - 4);
            }
            return projectName;
        }
    }
}