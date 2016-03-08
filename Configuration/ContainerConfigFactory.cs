using System;
using System.Configuration;
using Figaro;

namespace Figaro.Web.Configuration
{
    /// <summary>
    /// Factory object for creating new <see cref="ContainerConfig"/> instances from configuration.
    /// </summary>
    public class ContainerConfigFactory
    {
        /// <summary>
        /// Creates a new <see cref="ContainerConfig"/> instance using the <see cref="DefaultContainerElement"/> set at the section root level.
        /// </summary>
        /// <returns>A new <see cref="ContainerConfig"/> instance, or <c>null</c> if it doesn't exist.</returns>
        public static ContainerConfig Create()
        {
            FigaroSection section;
            try
            {
                section = (FigaroSection)ConfigurationManager.GetSection("Figaro");
            }
            catch (Exception)
            {
                return null;
            }
            if (section == null) return null;
            return section.DefaultContainerSettings == null ? null : Create(section.DefaultContainerSettings);
        }

        /// <summary>
        /// Creates a new <see cref="ContainerConfig"/> instance from configuration.
        /// </summary>
        /// <param name="element">The configuration element.</param>
        /// <returns>A new <see cref="ContainerConfig"/> instance.</returns>
        public static ContainerConfig Create(DefaultContainerElement element)
        {
            var cfg = new ContainerConfig
                          {
                              ConfigurationName = "Default Container",
                              AllowCreate = element.AllowCreate,
                              AllowValidation = element.AllowValidation,
                              Checksum = element.Checksum,
                              CompressionEnabled = element.Compression,
                              ExclusiveCreate = element.ExclusiveCreate,
                              NoMMap = element.NoMMap,
                              ReadOnly = element.ReadOnly,
#if CDS || TDS
                              MultiVersion = element.Multiversion,
                              Encrypted = element.Encrypted,
#elif TDS
                              ReadUncommitted = element.ReadUncommitted,
                              Transactional = element.Transactional,
                              TransactionNotDurable = element.TransactionNotDurable
#endif
                              Threaded = element.Threaded,
                          };
#if TDS
#endif

            if (!string.IsNullOrEmpty(element.ContainerType))
                cfg.ContainerType =
                    (XmlContainerType) Enum.Parse(typeof (XmlContainerType), element.ContainerType, true);
            
            if (!string.IsNullOrEmpty(element.IndexNodes))
                cfg.IndexNodes = (ConfigurationState)Enum.Parse(typeof(ConfigurationState),element.IndexNodes,true);
            
            if (element.PageSize > 0)
                cfg.PageSize = element.PageSize;

            if (element.SequenceIncrement >0)
                cfg.SequenceIncrement = element.SequenceIncrement;

            if (!string.IsNullOrEmpty(element.Statistics))
                cfg.Statistics = (ConfigurationState)Enum.Parse(typeof (ConfigurationState), element.Statistics, true);
            
            return cfg;
        }

        /// <summary>
        /// Creates a new <see cref="ContainerConfig"/> instance from the first container in the named <see cref="ManagerElement"/>.
        /// </summary>
        /// <param name="managerName">The named <see cref="ManagerElement"/> instance.</param>
        /// <returns>A new <see cref="ContainerConfig"/> instance, or <c>null</c> if no instance exists.</returns>
        public static ContainerConfig Create(string managerName)
        {
            if (string.IsNullOrEmpty(managerName)) return null;
            FigaroSection section;
            try
            {
                section = (FigaroSection)ConfigurationManager.GetSection("Figaro");
            }
            catch (Exception)
            {
                return null;
            }
            if (section == null) return null;

            if (section.Managers == null || section.Managers.Count == 0) return null;

            ManagerElement managerElement = null;
            foreach (ManagerElement manager in section.Managers)
            {
                if (!manager.Name.Equals(managerName)) continue;
                managerElement = manager;
                break;
            }
            if (managerElement == null) return null;
            if (managerElement.Containers == null || managerElement.Containers.Count == 0) return null;
            return Create(managerElement.Containers[0]);
        }

        /// <summary>
        /// Creates a new <see cref="ContainerConfig"/> instance from the <see cref="ManagerElement"/> default container settings.
        /// </summary>
        /// <param name="managerName">The named <see cref="ManagerElement"/> instance.</param>
        /// <returns>A new <see cref="ContainerConfig"/> instance, or <c>null</c> if no instance exists.</returns>
        public static ContainerConfig CreateDefault(string managerName)
        {
            if (string.IsNullOrEmpty(managerName)) return null;
            FigaroSection section;
            try
            {
                section = (FigaroSection)ConfigurationManager.GetSection("Figaro");
            }
            catch (Exception)
            {
                return null;
            }
            if (section == null) return null;

            if (section.Managers == null || section.Managers.Count == 0) return null;

            ManagerElement managerElement = null;
            foreach (ManagerElement manager in section.Managers)
            {
                if (!manager.Name.Equals(managerName)) continue;
                managerElement = manager;
                break;
            }
            if (managerElement == null) return null;
            return managerElement.DefaultContainerSettings == null
                ? null
                : Create(managerElement.DefaultContainerSettings);
        }

        /// <summary>
        /// Creates a new <see cref="ContainerConfig"/> instance from configuration.
        /// </summary>
        /// <param name="managerName">The named <see cref="ManagerElement"/> instance.</param>
        /// <param name="containerName">The named Container element.</param>
        /// <returns>A new <see cref="ContainerConfig"/> instance, or <c>null</c> if it doesn't exist.</returns>
        public static ContainerConfig Create(string managerName , string containerName)
        {
            if (string.IsNullOrEmpty(managerName)) return null;
            FigaroSection section;
            try
            {
                section = (FigaroSection)ConfigurationManager.GetSection("Figaro");
            }
            catch (Exception)
            {
                return null;
            }
            if (section == null) return null;

            if (section.Managers == null || section.Managers.Count == 0 ) return null;

            ManagerElement managerElement = null;
            foreach (ManagerElement manager in section.Managers)
            {
                if (!manager.Name.Equals(managerName)) continue;
                managerElement = manager;
                break;
            }
            if (managerElement == null) return null;
            if (managerElement.Containers == null || managerElement.Containers.Count == 0) return null;
            ContainerElement containerElement = null;
            foreach (ContainerElement element in managerElement.Containers)
            {
                if (!element.Name.Equals(containerName)) continue;
                containerElement = element;
                break;
            }
            return containerElement == null ? null : Create(containerElement);
        }

        /// <summary>
        /// Creates a new <see cref="ContainerConfig"/> instance from configuration.
        /// </summary>
        /// <param name="element">The configuration element.</param>
        /// <returns>A new <see cref="ContainerConfig"/> instance.</returns>
        public static ContainerConfig Create(ContainerElement element)
        {
            var cfg = new ContainerConfig
            {                
                ConfigurationName = element.Name,
                Path = element.Path,
                AllowCreate = element.AllowCreate,
                AllowValidation = element.AllowValidation,
                Checksum = element.Checksum,
                CompressionEnabled = element.Compression,
                ExclusiveCreate = element.ExclusiveCreate,
                NoMMap = element.NoMMap,
                ReadOnly = element.ReadOnly,
                Threaded = element.Threaded,
#if CDS || TDS
                Encrypted = element.Encrypted,
                MultiVersion = element.Multiversion,
#elif TDS
                ReadUncommitted = element.ReadUncommitted,
                Transactional = element.Transactional,
                TransactionNotDurable = element.TransactionNotDurable
#endif
            };
#if TDS
#endif

            if (!string.IsNullOrEmpty(element.ContainerType))
                cfg.ContainerType =
                    (XmlContainerType)Enum.Parse(typeof(XmlContainerType), element.ContainerType, true);

            if (!string.IsNullOrEmpty(element.IndexNodes))
                cfg.IndexNodes = (ConfigurationState)Enum.Parse(typeof(ConfigurationState), element.IndexNodes, true);

            if (element.PageSize > 0)
                cfg.PageSize = element.PageSize;

            if (element.SequenceIncrement > 0)
                cfg.SequenceIncrement = element.SequenceIncrement;

            if (!string.IsNullOrEmpty(element.Statistics))
                cfg.Statistics = (ConfigurationState)Enum.Parse(typeof(ConfigurationState), element.Statistics, true);

            return cfg;
        }
    }
}
