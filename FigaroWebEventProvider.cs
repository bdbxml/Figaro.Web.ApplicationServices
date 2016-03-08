/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.Web.Management;
using Figaro.Web.ApplicationServices;
using Figaro.Web.ApplicationServices.Data;

namespace Figaro.Web.ApplicationServices
{
    /// <summary>
    /// Custom web event provider that exports events into a Figaro XML database.
    /// </summary>
    public sealed class FigaroWebEventProvider : WebEventProvider
    {
        private WebEventData data;

        /// <summary>
        /// Creates a new instance of the FigaroWebProvider class.
        /// </summary>
        public FigaroWebEventProvider()
        {
            Init();
        }

        private void Init()
        {
            System.Configuration.Configuration cfg = null;
            try
            {
                cfg = WebConfigurationManager.OpenWebConfiguration("~/web.config");
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, "FigaroWebEventProvider ctor");
                if (e != null) throw e;
            }

            var section = cfg != null ? cfg.GetSection("system.web/healthMonitoring") as HealthMonitoringSection :
                                         WebConfigurationManager.GetSection("system.web/healthMonitoring") as HealthMonitoringSection;

            if (section == null) throw new ProviderException("No membership section found in configuration file.");
            var settings = section.Providers["FigaroWebEventProvider"];
            if (settings == null) throw new ProviderException("No FigaroWebEventProvider configuration found in healthMonitoring section.");
            Initialize(settings.Name, settings.Parameters);
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the
        ///     name/value pairs representing the provider-specific
        ///     attributes specified in the configuration for this
        ///     provider.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///     An attempt is made to call 
        ///     <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/>
        ///     on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (data != null && data.Initialized) return;

            base.Initialize(name, config);
            if (null == config["container"])
                throw new ArgumentException("container is a required setting for FigaroWebEventProvider.");
            
            data = new WebEventData(config["container"],config["manager"],typeof(WebBaseEvent));
        }

        /// <summary>
        /// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        /// </summary>
        /// <returns>
        /// A brief, friendly description suitable for display in administrative tools or other UIs.
        /// </returns>
        public override string Description
        {
            get
            {
                return "Figaro XML Database Provider for ASP.NET web events and health monitoring.";
            }
        }

        /// <summary>
        /// Gets the friendly name used to refer to the provider during configuration.
        /// </summary>
        /// <returns>
        /// The friendly name used to refer to the provider during configuration.
        /// </returns>
        public override string Name
        {
            get
            {
                return "Figaro Web Event Provider";
            }
        }

        /// <summary>
        /// Flushes events into the XML database. 
        /// </summary>
        public override void Flush()
        {
            data.Flush();
        }

        /// <summary>
        /// Processes the event passed to the provider.
        /// </summary>
        /// <param name="raisedEvent">
        /// The <see cref="T:System.Web.Management.WebBaseEvent"/> 
        /// object to process.</param>
        public override void ProcessEvent(WebBaseEvent raisedEvent)
        {
            if (!data.Initialized) Init();                
            data.ProcessEvent(raisedEvent);
        }

        /// <summary>
        /// Performs tasks associated with shutting down the provider.
        /// </summary>
        public override void Shutdown()
        {
            if (!data.Initialized) return;
            data.Shutdown();
        }
    }
}
