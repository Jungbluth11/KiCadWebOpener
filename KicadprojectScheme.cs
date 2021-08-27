using System;

namespace KiCadWebOpener
{
    /// <summary>
    ///  Represents the custom URL-scheme
    /// </summary>
    internal struct KicadprojectScheme
    {
        /// <summary>
        /// Type of the download URL (whether zip oder git).
        /// </summary>
        public string ProjectType { get; }

        /// <summary>
        /// URL form where the project will be downloaded.
        /// </summary>
        public string ProjectSource { get; }

        /// <summary>
        /// Name of the remote project .
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectType">Type of the download URL (whether zip oder git).</param>
        /// <param name="projectSource">URL form where the project will be downloaded.</param>
        /// <param name="projectName">Name of the remote project.</param>
        /// <exception cref="ArgumentNullException" />
        public KicadprojectScheme(string projectType, string projectSource, string projectName)
        {
            ProjectType = projectType ?? throw new ArgumentNullException(nameof(projectType));
            ProjectSource = projectSource ?? throw new ArgumentNullException(nameof(projectSource));
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        }
    }
}