
using System.Configuration;
using System.Security.Permissions;
using System.Web;
using System.Web.Configuration;

namespace Figaro.Web.Configuration
{
    /// <summary>
    /// Manages the creation of <see cref="FigaroSection"/> objects from configuration files.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class SectionFactory
	{
        /// <summary>
        /// Creates a new <see cref="FigaroSection"/> instance from the
        /// specified configuration file.
        /// </summary>
        /// <remarks>
        /// This method will attempt to read a configuration file and extract
        /// the <see cref="FigaroSection"/> with  the section name of 'Figaro'
        /// from  within the configuration file. If the configuration section
        /// does not exist or is not specified, the  method will return a 
        /// <c>null</c> value indicating the section was not found.
        /// </remarks>
        /// <param name="configurationFilePath">The path and file name of the
        /// configuration file.</param>
        /// <returns>A <see cref="FigaroSection"/> instance from configuration,
        /// or <c>null</c> if the section does not exist.</returns>
        public static FigaroSection Create(string configurationFilePath)
        {
            return Create(configurationFilePath, "Figaro");
        }

        /// <summary>
        /// Creates a new <see cref="FigaroSection"/> instance from the
        /// specified configuration file.
        /// </summary>
        /// <remarks>
        /// This method will attempt to read a configuration file and extract
        /// the <see cref="FigaroSection"/> with  the specified section name 
        /// from  within the configuration file. If the configuration section
        /// does not exist or is not specified, the  method will return a 
        /// <c>null</c> value indicating the section was not found.
        /// </remarks>
        /// <param name="configurationFilePath">The path and file name of the
        /// configuration file.</param>
        /// <param name="sectionName">The name of the <see cref="FigaroSection"/> instance.</param>
        /// <returns>A <see cref="FigaroSection"/> instance from configuration,
        /// or <c>null</c> if the section does not exist.</returns>
        public static FigaroSection Create(string configurationFilePath, string sectionName)
        {
            var map = new ExeConfigurationFileMap { ExeConfigFilename = configurationFilePath };
            var cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);            
            var section = cfg.GetSection(sectionName);
            return section == null ? null : section as FigaroSection;
        }

        /// <summary>
        /// Create a new <see cref="FigaroSection"/> instance from the existing
        /// configuration environment.
        /// </summary>
        /// <remarks>
        /// If the <see cref="FigaroSection"/> is configured, then a
        /// </remarks>
        /// <returns>The <see cref="FigaroSection"/> instance from
        /// configuration, or <c>null</c> if the section does not exist.
        /// </returns>
        public static FigaroSection Create()
        {
            var cfg = WebConfigurationManager.GetSection("Figaro");
            return cfg == null ? null : cfg as FigaroSection;
        }

	}
}
