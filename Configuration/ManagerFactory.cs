using System;
using System.Configuration;
using System.Security.Permissions;
using System.Web;
#if ORACLE_NS
    #if CDS || TDS
        using Figaro.BerkeleyDB;
    #endif
    using Figaro;
#else
using Figaro;
#endif

namespace Figaro.Web.Configuration
{
    /// <summary>
    /// Creates <see cref="XmlManager"/> instances from configuration.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ManagerFactory
    {
        /// <summary>
        /// Create a new <see cref="XmlManager"/> instance using the first available <see cref="ManagerElement"/> instance in configuration.
        /// </summary>
        /// <returns>A new <see cref="XmlManager"/> instance, or <c>null</c> if no instances exist.</returns>
        public static XmlManager Create()
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
            if (section.Managers == null || section.Managers.Count == 0) return null;
            return Create(section.Managers[0]);
        }

        /// <summary>
        /// Create a new <see cref="XmlManager"/> instance using the named <see cref="ManagerElement"/> instance.
        /// </summary>
        /// <param name="managerName">The named <see cref="ManagerElement"/> to use.</param>
        /// <returns>A new <see cref="XmlManager"/> instance, or <c>null</c> if the instance doesn't exist.</returns>
        public static XmlManager Create(string managerName)
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
            if (section.Managers == null || section.Managers.Count == 0) return null;
            foreach (ManagerElement managerElement in section.Managers)
            {
                if (managerElement.Name.Equals(managerName))
                    return Create(managerElement);
            }
            return null;
        }

        /// <summary>
        /// Create a new <see cref="XmlManager"/> instance using the specified <see cref="ManagerElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="ManagerElement"/> to use.</param>
        /// <returns>A new <see cref="XmlManager"/> instance.</returns>
        public static XmlManager Create(ManagerElement element)
        {
#if CDS || TDS
            var env = string.IsNullOrEmpty(element.Env) ? null : EnvFactory.Create(element.Env);
            if (env == null && !string.IsNullOrEmpty(element.Env))
                throw new ConfigurationErrorsException(string.Format("The specified Env configuration instance '{0}' does not exist.",element.Env));
#endif
            var opts = ManagerInitOptions.None;
            
            if (!string.IsNullOrEmpty(element.Options))
                opts = (ManagerInitOptions) Enum.Parse(typeof (ManagerInitOptions), element.Options, true);
#if CDS || TDS
            var mgr = env != null ? new XmlManager(env, opts) {ConfigurationName = element.Name} : 
                                                                                               new XmlManager(opts);
#endif
#if DS
            var mgr = string.IsNullOrEmpty(element.Home) ? new XmlManager(opts){ConfigurationName = element.Name} :
                new XmlManager(PathUtility.ResolvePath(element.Home), opts) { ConfigurationName = element.Name };
#endif           
            var ctype = XmlContainerType.NodeContainer;
            if (!string.IsNullOrEmpty(element.DefaultContainerType))
                ctype = (XmlContainerType) Enum.Parse(typeof (XmlContainerType), element.DefaultContainerType, true);

            mgr.DefaultContainerType = ctype;

            if (element.DefaultPageSize >0)
                mgr.DefaultPageSize = element.DefaultPageSize;
            
            if (element.DefaultSequenceIncrement >0)
                mgr.DefaultSequenceIncrement = element.DefaultSequenceIncrement;

            if (element.DefaultContainerSettings != null)
                mgr.DefaultContainerSettings = ContainerConfigFactory.Create(element.DefaultContainerSettings);

            return mgr;
        }

#if CDS || TDS
        /// <summary>
        /// Create a new <see cref="XmlManager"/> instance using the specified <see cref="ManagerElement"/>.
        /// </summary>
        /// <param name="managerElement">The <see cref="ManagerElement"/> to use.</param>
        /// <param name="envElement">The <see cref="FigaroEnv"/> configuration instance to assign to the manager. </param>
        /// <returns>A new <see cref="XmlManager"/> instance.</returns>
        public static XmlManager Create(ManagerElement managerElement, FigaroEnvElement envElement)
        {
            var env = envElement == null ? null : EnvFactory.Create(envElement);
            if (env == null && envElement != null)
                throw new ConfigurationErrorsException(string.Format("The specified Env configuration instance '{0}' does not exist.", envElement.Name));
            var opts = ManagerInitOptions.None;

            if (!string.IsNullOrEmpty(managerElement.Options))
                opts = (ManagerInitOptions)Enum.Parse(typeof(ManagerInitOptions), managerElement.Options, true);
            var mgr = env != null ? 
                new XmlManager(env, opts) { ConfigurationName = managerElement.Name } :
                new XmlManager(opts) { ConfigurationName = managerElement.Name };
            var ctype = XmlContainerType.NodeContainer;
            if (!string.IsNullOrEmpty(managerElement.DefaultContainerType))
                ctype = (XmlContainerType)Enum.Parse(typeof(XmlContainerType), managerElement.DefaultContainerType, true);

            mgr.DefaultContainerType = ctype;

            if (managerElement.DefaultPageSize > 0)
                mgr.DefaultPageSize = managerElement.DefaultPageSize;

            if (managerElement.DefaultSequenceIncrement > 0)
                mgr.DefaultSequenceIncrement = managerElement.DefaultSequenceIncrement;

            if (managerElement.DefaultContainerSettings != null)
                mgr.DefaultContainerSettings = ContainerConfigFactory.Create(managerElement.DefaultContainerSettings);

            return mgr;
        }
#endif

    }
}
