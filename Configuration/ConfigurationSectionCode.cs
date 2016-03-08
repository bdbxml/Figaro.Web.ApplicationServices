using System;
using System.Configuration;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;

#if ORACLE_NS
using Figaro;
#else
using Figaro;
#endif
#region FigaroSection
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// Represents a Figaro configuration section from within a configuration file.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class FigaroSection : ConfigurationSection
	{
        #region Environments Property
#if CDS || TDS
        /// <summary>
        /// The XML name of the <see cref="Environments"/> property.
        /// </summary>
        internal const string EnvironmentsPropertyName = "Environments";

        /// <summary>
        /// Gets or sets gets or sets the <see cref="EnvElementCollection"/>.
        /// </summary>
        [ConfigurationProperty(EnvironmentsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public EnvElementCollection Environments
        {
            get
            {
                return (EnvElementCollection)base[EnvironmentsPropertyName];
            }
            set
            {
                base[EnvironmentsPropertyName] = value;
            }
        }
#endif
        #endregion

        #region DefaultContainerSettings Property

        /// <summary>
        /// The XML name of the <see cref="DefaultContainerSettings"/> property.
        /// </summary>
        internal const string DefaultContainerSettingsPropertyName = "DefaultContainerSettings";

        /// <summary>
        /// Gets or sets the DefaultContainerSettings.
        /// </summary>
        [ConfigurationProperty(DefaultContainerSettingsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public DefaultContainerElement DefaultContainerSettings
        {
            get
            {
                return (DefaultContainerElement)base[DefaultContainerSettingsPropertyName];
            }
            set
            {
                base[DefaultContainerSettingsPropertyName] = value;
            }
        }

        #endregion

        #region Managers Property

        /// <summary>
        /// The XML name of the <see cref="Managers"/> property.
        /// </summary>
        internal const string ManagersPropertyName = "Managers";

        /// <summary>
        /// Gets or sets gets or sets the <see cref="ManagerElementCollection"/>.
        /// </summary>
        [ConfigurationProperty(ManagersPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public ManagerElementCollection Managers
        {
            get
            {
                return (ManagerElementCollection)base[ManagersPropertyName];
            }
            set
            {
                base[ManagersPropertyName] = value;
            }
        }

        #endregion

        #region Singleton Instance

        /// <summary>
        /// The XML name of the <see cref="FigaroSection"/> Configuration Section.
        /// </summary>
        internal const string FigaroSectionSectionName = "Figaro";

        /// <summary>
        /// Gets the <see cref="FigaroSection"/> instance.
        /// </summary>
        public static FigaroSection Instance
        {
            get
            {
                return ConfigurationManager.GetSection(FigaroSectionSectionName) as FigaroSection;
            }
        }

        #endregion

        #region Xmlns Property

        /// <summary>
        /// The XML name of the <see cref="Xmlns"/> property.
        /// </summary>
        internal const string XmlnsPropertyName = "xmlns";

        /// <summary>
        /// Gets the XML namespace of this Configuration Section.
        /// </summary>
        /// <remarks>
        /// This property makes sure that if the configuration file contains the XML namespace,
        /// the parser doesn't throw an exception because it encounters the unknown "xmlns" attribute.
        /// </remarks>
        [ConfigurationProperty(XmlnsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Xmlns
        {
            get
            {
                return (string)base[XmlnsPropertyName];
            }
        }

        #endregion
    }
}
#endregion

#region Env Element Collection
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The collection of environment objects for Figaro configuration.
	/// </summary>
	[ConfigurationCollection(typeof(FigaroEnvElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = FigaroEnvElementPropertyName)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class EnvElementCollection : ConfigurationElementCollection
	{
		#region Constants
		
		/// <summary>
		/// The XML name of the individual <see cref="FigaroEnvElement"/> instances in this collection.
		/// </summary>
		internal const string FigaroEnvElementPropertyName = "FigaroEnv";

		#endregion

		#region Overrides

		/// <summary>
		/// Gets the type of the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <param name="elementName">The name of the element to verify.</param>
		/// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. <para>The default is <see langword="false"/>.</para>
		/// </returns>
		protected override bool IsElementName(string elementName)
		{
			return (elementName == FigaroEnvElementPropertyName);
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((FigaroEnvElement)element).Name;
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new FigaroEnvElement();
		}

		#endregion
		
		#region Indexer

		/// <summary>
		/// Gets the <see cref="FigaroEnvElement"/> at the specified index.
		/// </summary>
		/// <param name="index">The index of the <see cref="FigaroEnvElement"/> to retrieve</param>
		public FigaroEnvElement this[int index]
		{
			get
			{
				return (FigaroEnvElement)BaseGet(index);
			}
		}

        /// <summary>
        /// Gets the named <see cref="FigaroEnvElement"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="FigaroEnvElement"/> to retrieve.</param>
        /// <returns></returns>
	    public new FigaroEnvElement this[string name]
	    {
	        get
	        {
	            return (FigaroEnvElement) BaseGet(name);
	        }
	    }


		#endregion
		
		#region Add

		/// <summary>
		/// Adds the specified <see cref="FigaroEnvElement"/>.
		/// </summary>
		/// <param name="FigaroEnv">The <see cref="FigaroEnvElement"/> to add.</param>
		public void Add(FigaroEnvElement FigaroEnv)
		{
			BaseAdd(FigaroEnv);
		}

		#endregion
		
		#region Remove

		/// <summary>
		/// Removes the specified <see cref="FigaroEnvElement"/>.
		/// </summary>
		/// <param name="FigaroEnv">The <see cref="FigaroEnvElement"/> to remove.</param>
		public void Remove(FigaroEnvElement FigaroEnv)
		{
			BaseRemove(FigaroEnv);
		}

		#endregion
	}
#endif
}
#endregion

#region Manager Element Collection
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// Gets the collection of manager objects for Figaro configuration.
	/// </summary>
	[ConfigurationCollection(typeof(ManagerElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = ManagerElementPropertyName)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ManagerElementCollection : ConfigurationElementCollection
	{
		#region Constants
		
		/// <summary>
		/// The XML name of the individual <see cref="ManagerElement"/> instances in this collection.
		/// </summary>
		internal const string ManagerElementPropertyName = "XmlManager";

		#endregion

		#region Overrides

		/// <summary>
		/// Gets the type of the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <param name="elementName">The name of the element to verify.</param>
		/// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. <para>The default is <see langword="false"/>.</para>
		/// </returns>
		protected override bool IsElementName(string elementName)
		{
			return (elementName == ManagerElementPropertyName);
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ManagerElement)element).Name;
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new ManagerElement();
		}

		#endregion
		
		#region Indexer

		/// <summary>
		/// Gets the <see cref="ManagerElement"/> at the specified index.
		/// </summary>
		/// <param name="index">The index of the <see cref="ManagerElement"/> to retrieve</param>
		public ManagerElement this[int index]
		{
			get
			{
				return (ManagerElement)BaseGet(index);
			}
		}

        /// <summary>
        /// Gets the named <see cref="ManagerElement"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="ManagerElement"/> to retrieve.</param>
        /// <returns></returns>
        public new ManagerElement this[string name]
        {
            get
            {
                return (ManagerElement)BaseGet(name);
            }
        }

		#endregion
		
		#region Add

		/// <summary>
		/// Adds the specified <see cref="ManagerElement"/>.
		/// </summary>
		/// <param name="XmlManager">The <see cref="ManagerElement"/> to add.</param>
		public void Add(ManagerElement XmlManager)
		{
			BaseAdd(XmlManager);
		}

		#endregion
		
		#region Remove

		/// <summary>
		/// Removes the specified <see cref="ManagerElement"/>.
		/// </summary>
		/// <param name="XmlManager">The <see cref="ManagerElement"/> to remove.</param>
		public void Remove(ManagerElement XmlManager)
		{
			BaseRemove(XmlManager);
		}

		#endregion
	}
}
#endregion

#region Container Element Collection
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// A collection of <see cref="ContainerElement"/> instances.
	/// </summary>
	[ConfigurationCollection(typeof(ContainerElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = ContainerElementPropertyName)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ContainerElementCollection : ConfigurationElementCollection
	{
		#region Constants
		
		/// <summary>
		/// The XML name of the individual <see cref="ContainerElement"/> instances in this collection.
		/// </summary>
		internal const string ContainerElementPropertyName = "Container";

		#endregion

		#region Overrides

		/// <summary>
		/// Gets the type of the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <param name="elementName">The name of the element to verify.</param>
		/// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. <para>The default is <see langword="false"/>.</para>
		/// </returns>
		protected override bool IsElementName(string elementName)
		{
			return (elementName == ContainerElementPropertyName);
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ContainerElement)element).Name;
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new ContainerElement();
		}

		#endregion
		
		#region Indexer

		/// <summary>
		/// Gets the <see cref="ContainerElement"/> at the specified index.
		/// </summary>
		/// <param name="index">The index of the <see cref="ContainerElement"/> to retrieve</param>
		public ContainerElement this[int index]
		{
			get
			{
				return (ContainerElement)BaseGet(index);
			}
		}

        /// <summary>
        /// Gets the named <see cref="ContainerElement"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="ContainerElement"/> to retrieve.</param>
        /// <returns></returns>
        public new ContainerElement this[string name]
        {
            get
            {
                return (ContainerElement)BaseGet(name);
            }
        }
		#endregion
		
		#region Add

		/// <summary>
		/// Adds the specified <see cref="ContainerElement"/>.
		/// </summary>
		/// <param name="Container">The <see cref="ContainerElement"/> to add.</param>
		public void Add(ContainerElement Container)
		{
			BaseAdd(Container);
		}

		#endregion
		
		#region Remove

		/// <summary>
		/// Removes the specified <see cref="ContainerElement"/>.
		/// </summary>
		/// <param name="Container">The <see cref="ContainerElement"/> to remove.</param>
		public void Remove(ContainerElement Container)
		{
			BaseRemove(Container);
		}

		#endregion
	}
}
#endregion

#region FigaroEnv Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// Configure a <see cref="Figaro.FigaroEnv"/> instance.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class FigaroEnvElement : ConfigurationElement
	{
		#region Name Property
		
		/// <summary>
		/// The XML name of the <see cref="Name"/> property.
		/// </summary>
		internal const string NamePropertyName = "name";
		
		/// <summary>
		/// Gets or sets the name of the element instance.
		/// </summary>
		[ConfigurationProperty(NamePropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
		public string Name
		{
			get
			{
				return (string)base[NamePropertyName];
			}
			set
			{
				base[NamePropertyName] = value;
			}
		}
		
		#endregion

		#region ThreadCount Property
		
		/// <summary>
		/// The XML name of the <see cref="ThreadCount"/> property.
		/// </summary>
		internal const string ThreadCountPropertyName = "threadCount";
		
		/// <summary>
		/// Gets or sets the estimated thread count for the <see cref="Figaro.FigaroEnv"/> instance.
		/// </summary>
		[ConfigurationProperty(ThreadCountPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public byte ThreadCount
		{
			get
			{
				return (byte)base[ThreadCountPropertyName];
			}
			set
			{
				base[ThreadCountPropertyName] = value;
			}
		}
		
		#endregion

		#region DataDirectories Property
		
		/// <summary>
		/// The XML name of the <see cref="DataDirectories"/> property.
		/// </summary>
		internal const string DataDirectoriesPropertyName = "DataDirectories";
		
		/// <summary>
		/// Gets or sets gets or sets the collection of data directories used by the <see cref="Figaro.FigaroEnv"/> instance.
		/// </summary>
		[ConfigurationProperty(DataDirectoriesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public DataDirectoryElementCollection DataDirectories
		{
			get
			{
				return (DataDirectoryElementCollection)base[DataDirectoriesPropertyName];
			}
			set
			{
				base[DataDirectoriesPropertyName] = value;
			}
		}
		
		#endregion

		#region Tracing Property
		
		/// <summary>
		/// The XML name of the <see cref="Tracing"/> property.
		/// </summary>
		internal const string TracingPropertyName = "Tracing";
		
		/// <summary>
		/// Gets or sets gets or sets tracing configuration for the Figaro XML Database environment.
		/// </summary>
		[ConfigurationProperty(TracingPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public TraceConfigurationElement Tracing
		{
			get
			{
				return (TraceConfigurationElement)base[TracingPropertyName];
			}
			set
			{
				base[TracingPropertyName] = value;
			}
		}
		
		#endregion

		#region Encryption Property
		
		/// <summary>
		/// The XML name of the <see cref="Encryption"/> property.
		/// </summary>
		internal const string EncryptionPropertyName = "Encryption";
		
		/// <summary>
		/// Gets or sets gets or sets the encryption settings for the configured environment instance.
		/// </summary>
		[ConfigurationProperty(EncryptionPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public EncryptionElement Encryption
		{
			get
			{
				return (EncryptionElement)base[EncryptionPropertyName];
			}
			set
			{
				base[EncryptionPropertyName] = value;
			}
		}
		
		#endregion

		#region Log Property
		
		/// <summary>
		/// The XML name of the <see cref="Log"/> property.
		/// </summary>
		internal const string LogPropertyName = "Log";
		
		/// <summary>
		/// Gets or sets transaction log settings for the <see cref="FigaroEnv"/> instance.
		/// </summary>
		[ConfigurationProperty(LogPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public LogElement Log
		{
			get
			{
				return (LogElement)base[LogPropertyName];
			}
			set
			{
				base[LogPropertyName] = value;
			}
		}
		
		#endregion

		#region MemoryFileMap Property
		
		/// <summary>
		/// The XML name of the <see cref="MemoryFileMap"/> property.
		/// </summary>
		internal const string MemoryFileMapPropertyName = "MemoryFileMap";
		
		/// <summary>
		/// Gets or sets gets or sets the memory file map settings for the <see cref="Figaro.FigaroEnv"/> instance.
		/// </summary>
		[ConfigurationProperty(MemoryFileMapPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public MemoryFileMapElement MemoryFileMap
		{
			get
			{
				return (MemoryFileMapElement)base[MemoryFileMapPropertyName];
			}
			set
			{
				base[MemoryFileMapPropertyName] = value;
			}
		}
		
		#endregion

		#region Cache Property
		
		/// <summary>
		/// The XML name of the <see cref="Cache"/> property.
		/// </summary>
		internal const string CachePropertyName = "Cache";
		
		/// <summary>
		/// Gets or sets gets or sets the Cache configuration settings for the configured environment instance.
		/// </summary>
		[ConfigurationProperty(CachePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public CacheElement Cache
		{
			get
			{
				return (CacheElement)base[CachePropertyName];
			}
			set
			{
				base[CachePropertyName] = value;
			}
		}
		
		#endregion

		#region CacheMax Property
		
		/// <summary>
		/// The XML name of the <see cref="CacheMax"/> property.
		/// </summary>
		internal const string CacheMaxPropertyName = "CacheMax";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum cache configuration for the environment instance.
		/// </summary>
		[ConfigurationProperty(CacheMaxPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public CacheMaxElement CacheMax
		{
			get
			{
				return (CacheMaxElement)base[CacheMaxPropertyName];
			}
			set
			{
				base[CacheMaxPropertyName] = value;
			}
		}
		
		#endregion

		#region Open Property
		
		/// <summary>
		/// The XML name of the <see cref="Open"/> property.
		/// </summary>
		internal const string OpenPropertyName = "Open";
		
		/// <summary>
		/// Gets or sets the Open.
		/// </summary>
		[ConfigurationProperty(OpenPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public OpenElement Open
		{
			get
			{
				return (OpenElement)base[OpenPropertyName];
			}
			set
			{
				base[OpenPropertyName] = value;
			}
		}
		
		#endregion

		#region TempDirectory Property
		
		/// <summary>
		/// The XML name of the <see cref="TempDirectory"/> property.
		/// </summary>
		internal const string TempDirectoryPropertyName = "TempDirectory";
		
		/// <summary>
		/// Gets or sets gets or sets the temporary directory settings for the configured environment instance.
		/// </summary>
		[ConfigurationProperty(TempDirectoryPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public TempDirectoryElement TempDirectory
		{
			get
			{
				return (TempDirectoryElement)base[TempDirectoryPropertyName];
			}
			set
			{
				base[TempDirectoryPropertyName] = value;
			}
		}
		
		#endregion

		#region Transactions Property
#if TDS		
		/// <summary>
		/// The XML name of the <see cref="Transactions"/> property.
		/// </summary>
		internal const string TransactionsPropertyName = "Transactions";
		
		/// <summary>
		/// Gets or sets gets or sets the transaction subsystem settings of the environment instance. 
		/// </summary>
		[ConfigurationProperty(TransactionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public TransactionsElement Transactions
		{
			get
			{
				return (TransactionsElement)base[TransactionsPropertyName];
			}
			set
			{
				base[TransactionsPropertyName] = value;
			}
		}
#endif	
		#endregion

		#region Locking Property
#if CDS || TDS		
		/// <summary>
		/// The XML name of the <see cref="Locking"/> property.
		/// </summary>
		internal const string LockingPropertyName = "Locking";
		
		/// <summary>
		/// Gets or sets gets or sets the locking subsystem configuration settings.
		/// </summary>
		[ConfigurationProperty(LockingPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public LockingElement Locking
		{
			get
			{
				return (LockingElement)base[LockingPropertyName];
			}
			set
			{
				base[LockingPropertyName] = value;
			}
		}
		#endif
		#endregion

		#region Mutex Property
		
		/// <summary>
		/// The XML name of the <see cref="Mutex"/> property.
		/// </summary>
		internal const string MutexPropertyName = "Mutex";
		
		/// <summary>
		/// Gets or sets gets or sets advanced environment mutex configuration settings.
		/// </summary>
		[ConfigurationProperty(MutexPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public MutexElement Mutex
		{
			get
			{
				return (MutexElement)base[MutexPropertyName];
			}
			set
			{
				base[MutexPropertyName] = value;
			}
		}
		
		#endregion

		#region EnvConfig Property
		
		/// <summary>
		/// The XML name of the <see cref="EnvConfig"/> property.
		/// </summary>
		internal const string EnvConfigPropertyName = "EnvConfig";
		
		/// <summary>
		/// Gets or sets gets or sets the collection of environment configuration settings to enable or disable.
		/// </summary>
		[ConfigurationProperty(EnvConfigPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public EnvConfigElementCollection EnvConfig
		{
			get
			{
				return (EnvConfigElementCollection)base[EnvConfigPropertyName];
			}
			set
			{
				base[EnvConfigPropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region Data Directory Element Collection
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// Gets or sets the data directories used by the Figaro library objects.
	/// </summary>
	[ConfigurationCollection(typeof(DataDirectoryElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class DataDirectoryElementCollection : ConfigurationElementCollection
	{

		#region Constants
		
		/// <summary>
		/// The XML name of the individual <see cref="DataDirectoryElement"/> instances in this collection.
		/// </summary>
		internal const string DataDirectoryElementPropertyName = "Directory";

		#endregion

		#region Overrides

		/// <summary>
		/// Gets the type of the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <param name="elementName">The name of the element to verify.</param>
		/// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. <para>The default is <see langword="false"/>.</para>
		/// </returns>
		protected override bool IsElementName(string elementName)
		{
			return (elementName == DataDirectoryElementPropertyName);
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((DataDirectoryElement)element).Path;
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new DataDirectoryElement();
		}

		#endregion
		
		#region Indexer

		/// <summary>
		/// Gets the <see cref="DataDirectoryElement"/> at the specified index.
		/// </summary>
		/// <param name="index">The index of the <see cref="DataDirectoryElement"/> to retrieve</param>
		public DataDirectoryElement this[int index]
		{
			get
			{
				return (DataDirectoryElement)BaseGet(index);
			}
		}

		#endregion
		
		#region Add

		/// <summary>
		/// Adds the specified <see cref="DataDirectoryElement"/>.
		/// </summary>
		/// <param name="Directory">The <see cref="DataDirectoryElement"/> to add.</param>
		public void Add(DataDirectoryElement Directory)
		{
			BaseAdd(Directory);
		}

		#endregion
		
		#region Remove

		/// <summary>
		/// Removes the specified <see cref="DataDirectoryElement"/>.
		/// </summary>
		/// <param name="Directory">The <see cref="DataDirectoryElement"/> to remove.</param>
		public void Remove(DataDirectoryElement Directory)
		{
			BaseRemove(Directory);
		}

		#endregion
	}
}
#endregion

#region Data Directory Element
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// The configuration element containing data directory paths to configure for the configured Figaro object instances.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class DataDirectoryElement : ConfigurationElement
	{
	    #region Create Property

	    /// <summary>
	    /// The XML name of the <see cref="Create"/> property.
	    /// </summary>
	    internal const string CreatePropertyName = "create";

	    /// <summary>
	    /// Gets or sets whether to create the directories if they don't exist.
	    /// </summary>
	    [ConfigurationProperty(CreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
	    public bool Create
	    {
	        get { return (bool) base[CreatePropertyName]; }
	        set { base[CreatePropertyName] = value; }
	    }

	    #endregion
	
		#region Path Property
		
		/// <summary>
		/// The XML name of the <see cref="Path"/> property.
		/// </summary>
		internal const string PathPropertyName = "path";
		
		/// <summary>
		/// Gets or sets gets or sets the data directory to add.
		/// </summary>
		[ConfigurationProperty(PathPropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
		public string Path
		{
			get
			{

			    return HostingEnvironment.MapPath((string) base[PathPropertyName]);
                //var p = (string)base[PathPropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);
                //return (string)base[PathPropertyName];
			}
			set
			{
				base[PathPropertyName] = value;
			}
		}
		
		#endregion

	}
}
#endregion

#region Manager Element
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// Configuration properties for an <see cref="Figaro.XmlManager"/> instance.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ManagerElement : ConfigurationElement
	{
		#region Name Property
		
		/// <summary>
		/// The XML name of the <see cref="Name"/> property.
		/// </summary>
		internal const string NamePropertyName = "name";
		
		/// <summary>
		/// Gets or sets gets the name of the <see cref="Figaro.XmlManager"/> configuration instance.
		/// </summary>
		[ConfigurationProperty(NamePropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
		public string Name
		{
			get
			{
				return (string)base[NamePropertyName];
			}
			set
			{
				base[NamePropertyName] = value;
			}
		}
		
		#endregion

        #region Home Property
#if DS

        /// <summary>
        /// The XML name of the <see cref="Home"/> property.
        /// </summary>
        internal const string HomePropertyName = "home";

        /// <summary>
        /// Gets or sets a home directory. This should only be used if <see cref="FigaroEnv"/> is unavailable.
        /// </summary>
        [ConfigurationProperty(HomePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Home
        {
            get { return (string)base[HomePropertyName]; }
            set { base[HomePropertyName] = value; }
        }
#endif

        #endregion

        #region Env Property
#if CDS || TDS
        /// <summary>
		/// The XML name of the <see cref="Env"/> property.
		/// </summary>
		internal const string EnvPropertyName = "env";
		
		/// <summary>
		/// Gets or sets gets or sets the name of a configured environment instance to use in the manager instance.
		/// </summary>
		[ConfigurationProperty(EnvPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Env
		{
			get
			{
				return (string)base[EnvPropertyName];
			}
			set
			{
				base[EnvPropertyName] = value;
			}
		}
#endif
		#endregion

		#region Options Property
		
		/// <summary>
		/// The XML name of the <see cref="Options"/> property.
		/// </summary>
		internal const string OptionsPropertyName = "options";
		
		/// <summary>
		/// Gets or sets gets or sets manager initialization options.
		/// </summary>
		[ConfigurationProperty(OptionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Options
		{
			get
			{
				return (string)base[OptionsPropertyName];
			}
			set
			{
				base[OptionsPropertyName] = value;
			}
		}
		
		#endregion

		#region DefaultContainerType Property
		
		/// <summary>
		/// The XML name of the <see cref="DefaultContainerType"/> property.
		/// </summary>
		internal const string DefaultContainerTypePropertyName = "defaultContainerType";
		
		/// <summary>
		/// Gets or sets gets or sets the default container type (Node or Wholedoc) when creating new containers
		/// </summary>
		[ConfigurationProperty(DefaultContainerTypePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string DefaultContainerType
		{
			get
			{
				return (string)base[DefaultContainerTypePropertyName];
			}
			set
			{
				base[DefaultContainerTypePropertyName] = value;
			}
		}
		
		#endregion

		#region DefaultPageSize Property
		
		/// <summary>
		/// The XML name of the <see cref="DefaultPageSize"/> property.
		/// </summary>
		internal const string DefaultPageSizePropertyName = "defaultPageSize";
		
		/// <summary>
		/// Gets or sets gets or sets the default page size, in bytes, for containers created by this manager instance.
		/// </summary>
		[ConfigurationProperty(DefaultPageSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int DefaultPageSize
		{
			get
			{
				return (int)base[DefaultPageSizePropertyName];
			}
			set
			{
				base[DefaultPageSizePropertyName] = value;
			}
		}
		
		#endregion

		#region DefaultSequenceIncrement Property
		
		/// <summary>
		/// The XML name of the <see cref="DefaultSequenceIncrement"/> property.
		/// </summary>
		internal const string DefaultSequenceIncrementPropertyName = "defaultSequenceIncrement";
		
		/// <summary>
		/// Gets or sets gets or sets the default integer increment used when pre-allocating IDs at document creation.
		/// </summary>
		[ConfigurationProperty(DefaultSequenceIncrementPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int DefaultSequenceIncrement
		{
			get
			{
				return (int)base[DefaultSequenceIncrementPropertyName];
			}
			set
			{
				base[DefaultSequenceIncrementPropertyName] = value;
			}
		}
		
		#endregion

		#region DataDirectories Property
#if DS
		/// <summary>
		/// The XML name of the <see cref="DataDirectories"/> property.
		/// </summary>
		internal const string DataDirectoriesPropertyName = "DataDirectories";
		
		/// <summary>
		/// Gets or sets gets or sets the database directories accessed by the <see cref="Figaro.XmlManager"/> instance.
		/// </summary>
		[ConfigurationProperty(DataDirectoriesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public DataDirectoryElementCollection DataDirectories
		{
			get
			{
				return (DataDirectoryElementCollection)base[DataDirectoriesPropertyName];
			}
			set
			{
				base[DataDirectoriesPropertyName] = value;
			}
		}
#endif
		#endregion

		#region DefaultContainerSettings Property
		
		/// <summary>
		/// The XML name of the <see cref="DefaultContainerSettings"/> property.
		/// </summary>
		internal const string DefaultContainerSettingsPropertyName = "DefaultContainerSettings";
		
		/// <summary>
		/// Gets or sets the DefaultContainerSettings.
		/// </summary>
		[ConfigurationProperty(DefaultContainerSettingsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public DefaultContainerElement DefaultContainerSettings
		{
			get
			{
				return (DefaultContainerElement)base[DefaultContainerSettingsPropertyName];
			}
			set
			{
				base[DefaultContainerSettingsPropertyName] = value;
			}
		}
		
		#endregion

        #region Containers Property

        /// <summary>
        /// The XML name of the <see cref="Containers"/> property.
        /// </summary>
        internal const string ContainersPropertyName = "Containers";

        /// <summary>
        /// Gets or sets gets or sets the <see cref="ContainerElementCollection"/>.
        /// </summary>
        [ConfigurationProperty(ContainersPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public ContainerElementCollection Containers
        {
            get
            {
                return (ContainerElementCollection)base[ContainersPropertyName];
            }
            set
            {
                base[ContainersPropertyName] = value;
            }
        }

        #endregion

        #region Queries Property

        /// <summary>
        /// The XML name of the <see cref="Queries"/> property.
        /// </summary>
        internal const string QueriesPropertyName = "Queries";

        /// <summary>
        /// Gets or sets gets or sets the collection of query configuration items available for preparing and executing queries.
        /// </summary>
        [ConfigurationProperty(QueriesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public QueryCollectionElement Queries
        {
            get
            {
                return (QueryCollectionElement)base[QueriesPropertyName];
            }
            set
            {
                base[QueriesPropertyName] = value;
            }
        }

        #endregion
    }
}
#endregion

#region Container Element
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// The configuration properties for opening and/or creating a <see cref="Figaro.Container"/> object.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ContainerElement : ConfigurationElement
	{
		#region Name Property
		
		/// <summary>
		/// The XML name of the <see cref="Name"/> property.
		/// </summary>
		internal const string NamePropertyName = "name";
		
		/// <summary>
		/// Gets or sets gets the name of the <see cref="Figaro.Container"/> configuration instance.
		/// </summary>
		[ConfigurationProperty(NamePropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
		public string Name
		{
			get
			{
				return (string)base[NamePropertyName];
			}
			set
			{
				base[NamePropertyName] = value;
			}
		}
		
		#endregion

		#region Alias Property
		
		/// <summary>
		/// The XML name of the <see cref="Alias"/> property.
		/// </summary>
		internal const string AliasPropertyName = "alias";
		
		/// <summary>
		/// Gets or sets gets or sets the container alias used in XQuery expressions.
		/// </summary>
		[ConfigurationProperty(AliasPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Alias
		{
			get
			{
				return (string)base[AliasPropertyName];
			}
			set
			{
				base[AliasPropertyName] = value;
			}
		}
		
		#endregion

		#region Compression Property
		
		/// <summary>
		/// The XML name of the <see cref="Compression"/> property.
		/// </summary>
		internal const string CompressionPropertyName = "compression";
		
		/// <summary>
		/// Gets or sets gets or sets whether compression is enabled for the container.
		/// </summary>
		[ConfigurationProperty(CompressionPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Compression
		{
			get
			{
				return (bool)base[CompressionPropertyName];
			}
			set
			{
				base[CompressionPropertyName] = value;
			}
		}
		
		#endregion

		#region SequenceIncrement Property
		
		/// <summary>
		/// The XML name of the <see cref="SequenceIncrement"/> property.
		/// </summary>
		internal const string SequenceIncrementPropertyName = "sequenceIncrement";
		
		/// <summary>
		/// Gets or sets gets or sets the integer increment used when pre-allocating IDs at document creation.
		/// </summary>
		[ConfigurationProperty(SequenceIncrementPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint SequenceIncrement
		{
			get
			{
				return (uint)base[SequenceIncrementPropertyName];
			}
			set
			{
				base[SequenceIncrementPropertyName] = value;
			}
		}
		
		#endregion

		#region PageSize Property
		
		/// <summary>
		/// The XML name of the <see cref="PageSize"/> property.
		/// </summary>
		internal const string PageSizePropertyName = "pageSize";
		
		/// <summary>
		/// Gets or sets gets or sets the container page size, in bytes.
		/// </summary>
		[ConfigurationProperty(PageSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint PageSize
		{
			get
			{
				return (uint)base[PageSizePropertyName];
			}
			set
			{
				base[PageSizePropertyName] = value;
			}
		}
		
		#endregion

		#region Checksum Property
		
		/// <summary>
		/// The XML name of the <see cref="Checksum"/> property.
		/// </summary>
		internal const string ChecksumPropertyName = "checksum";
		
		/// <summary>
		/// Gets or sets gets or sets whether to do checksum verification of pages read into the cache from the backing file store. 
		/// </summary>
		[ConfigurationProperty(ChecksumPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Checksum
		{
			get
			{
				return (bool)base[ChecksumPropertyName];
			}
			set
			{
				base[ChecksumPropertyName] = value;
			}
		}
		
		#endregion

		#region ReadUncommitted Property
#if TDS
		/// <summary>
		/// The XML name of the <see cref="ReadUncommitted"/> property.
		/// </summary>
		internal const string ReadUncommittedPropertyName = "readUncommitted";
		
		/// <summary>
		/// Gets or sets gets or sets the ability to do dirty container reads.
		/// </summary>
		[ConfigurationProperty(ReadUncommittedPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool ReadUncommitted
		{
			get
			{
				return (bool)base[ReadUncommittedPropertyName];
			}
			set
			{
				base[ReadUncommittedPropertyName] = value;
			}
		}
#endif	
		#endregion

		#region Encrypted Property
#if CDS || TDS	
		/// <summary>
		/// The XML name of the <see cref="Encrypted"/> property.
		/// </summary>
		internal const string EncryptedPropertyName = "encrypted";
		
		/// <summary>
		/// Gets or sets gets or sets whether to enable container encryption.
		/// </summary>
		[ConfigurationProperty(EncryptedPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Encrypted
		{
			get
			{
				return (bool)base[EncryptedPropertyName];
			}
			set
			{
				base[EncryptedPropertyName] = value;
			}
		}
#endif
		#endregion

		#region AllowValidation Property
		
		/// <summary>
		/// The XML name of the <see cref="AllowValidation"/> property.
		/// </summary>
		internal const string AllowValidationPropertyName = "allowValidation";
		
		/// <summary>
		/// Gets or sets gets or sets whether to validate XML if it refers to a DTD or schema.
		/// </summary>
		[ConfigurationProperty(AllowValidationPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool AllowValidation
		{
			get
			{
				return (bool)base[AllowValidationPropertyName];
			}
			set
			{
				base[AllowValidationPropertyName] = value;
			}
		}
		
		#endregion

		#region Statistics Property
		
		/// <summary>
		/// The XML name of the <see cref="Statistics"/> property.
		/// </summary>
		internal const string StatisticsPropertyName = "statistics";
		
		/// <summary>
		/// Gets or sets gets or sets whether to enable container statistics.
		/// </summary>
		[ConfigurationProperty(StatisticsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Statistics
		{
			get
			{
				return (string)base[StatisticsPropertyName];
			}
			set
			{
				base[StatisticsPropertyName] = value;
			}
		}
		
		#endregion

		#region IndexNodes Property
		
		/// <summary>
		/// The XML name of the <see cref="IndexNodes"/> property.
		/// </summary>
		internal const string IndexNodesPropertyName = "indexNodes";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not to enable document or node indexing.
		/// </summary>
		[ConfigurationProperty(IndexNodesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string IndexNodes
		{
			get
			{
				return (string)base[IndexNodesPropertyName];
			}
			set
			{
				base[IndexNodesPropertyName] = value;
			}
		}
		
		#endregion

		#region Transactional Property
#if TDS	
		/// <summary>
		/// The XML name of the <see cref="Transactional"/> property.
		/// </summary>
		internal const string TransactionalPropertyName = "transactional";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not the container is configured for transactions.
		/// </summary>
		[ConfigurationProperty(TransactionalPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Transactional
		{
			get
			{
				return (bool)base[TransactionalPropertyName];
			}
			set
			{
				base[TransactionalPropertyName] = value;
			}
		}
#endif	
		#endregion

		#region AllowCreate Property
		
		/// <summary>
		/// The XML name of the <see cref="AllowCreate"/> property.
		/// </summary>
		internal const string AllowCreatePropertyName = "allowCreate";
		
		/// <summary>
		/// Gets or sets gets or sets whether a container can be created if it does not already exist.
		/// </summary>
		[ConfigurationProperty(AllowCreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool AllowCreate
		{
			get
			{
				return (bool)base[AllowCreatePropertyName];
			}
			set
			{
				base[AllowCreatePropertyName] = value;
			}
		}
		
		#endregion

		#region Multiversion Property
#if CDS || TDS
		/// <summary>
		/// The XML name of the <see cref="Multiversion"/> property.
		/// </summary>
		internal const string MultiversionPropertyName = "multiVersion";
		
		/// <summary>
		/// Gets or sets gets or sets whether the database will be opened with support for Multiversion concurrency control (MVCC). This will cause updates to  the container to follow a copy-on-write protocol which is required to support snapshot isolation. 
		/// </summary>
		[ConfigurationProperty(MultiversionPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Multiversion
		{
			get
			{
				return (bool)base[MultiversionPropertyName];
			}
			set
			{
				base[MultiversionPropertyName] = value;
			}
		}
#endif
		#endregion

		#region ExclusiveCreate Property
		
		/// <summary>
		/// The XML name of the <see cref="ExclusiveCreate"/> property.
		/// </summary>
		internal const string ExclusiveCreatePropertyName = "exclusiveCreate";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not to throw exceptions if the container already exists when trying to create another instance.
		/// </summary>
		[ConfigurationProperty(ExclusiveCreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool ExclusiveCreate
		{
			get
			{
				return (bool)base[ExclusiveCreatePropertyName];
			}
			set
			{
				base[ExclusiveCreatePropertyName] = value;
			}
		}
		
		#endregion

		#region NoMMap Property
		
		/// <summary>
		/// The XML name of the <see cref="NoMMap"/> property.
		/// </summary>
		internal const string NoMMapPropertyName = "noMMap";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not the container will be mapped to process memory.
		/// </summary>
		[ConfigurationProperty(NoMMapPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool NoMMap
		{
			get
			{
				return (bool)base[NoMMapPropertyName];
			}
			set
			{
				base[NoMMapPropertyName] = value;
			}
		}
		
		#endregion

		#region ReadOnly Property
		
		/// <summary>
		/// The XML name of the <see cref="ReadOnly"/> property.
		/// </summary>
		internal const string ReadOnlyPropertyName = "readOnly";
		
		/// <summary>
		/// Gets or sets gets or sets whether the container is configured for read-only access.
		/// </summary>
		[ConfigurationProperty(ReadOnlyPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool ReadOnly
		{
			get
			{
				return (bool)base[ReadOnlyPropertyName];
			}
			set
			{
				base[ReadOnlyPropertyName] = value;
			}
		}
		
		#endregion

		#region Threaded Property
		
		/// <summary>
		/// The XML name of the <see cref="Threaded"/> property.
		/// </summary>
		internal const string ThreadedPropertyName = "threaded";
		
		/// <summary>
		/// Gets or sets gets or sets whether the container handle is free-threaded. 
		/// </summary>
		[ConfigurationProperty(ThreadedPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Threaded
		{
			get
			{
				return (bool)base[ThreadedPropertyName];
			}
			set
			{
				base[ThreadedPropertyName] = value;
			}
		}
		
		#endregion

		#region TransactionNotDurable Property
#if TDS
		/// <summary>
		/// The XML name of the <see cref="TransactionNotDurable"/> property.
		/// </summary>
		internal const string TransactionNotDurablePropertyName = "transactionNotDurable";
		
		/// <summary>
		/// Gets or sets gets or sets transaction durability.
		/// </summary>
		[ConfigurationProperty(TransactionNotDurablePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool TransactionNotDurable
		{
			get
			{
				return (bool)base[TransactionNotDurablePropertyName];
			}
			set
			{
				base[TransactionNotDurablePropertyName] = value;
			}
		}
#endif	
		#endregion

		#region ContainerType Property
		
		/// <summary>
		/// The XML name of the <see cref="ContainerType"/> property.
		/// </summary>
		internal const string ContainerTypePropertyName = "containerType";
		
		/// <summary>
		/// Gets or sets gets or sets the container type.
		/// </summary>
		[ConfigurationProperty(ContainerTypePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string ContainerType
		{
			get
			{
				return (string)base[ContainerTypePropertyName];
			}
			set
			{
				base[ContainerTypePropertyName] = value;
			}
		}
		
		#endregion

		#region Path Property
		/// <summary>
		/// The XML name of the <see cref="Path"/> property.
		/// </summary>
		internal const string PathPropertyName = "path";
		
		/// <summary>
		/// Gets or sets gets or sets the path and name of the configured container.
		/// </summary>
		[ConfigurationProperty(PathPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Path
		{
			get
			{
                return HostingEnvironment.MapPath((string)base[PathPropertyName]);
                //var p = (string)base[PathPropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);
                //return (string)base[PathPropertyName];
			}
			set
			{
				base[PathPropertyName] = value;
			}
		}
		
		#endregion
	}
}
#endregion

#region Trace Configuration Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
    /// <summary>
	/// Gets or sets the tracing configuration for the Figaro XML Database environment.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class TraceConfigurationElement : ConfigurationElement
	{
        #region ErrorPrefix Property

        /// <summary>
        /// The XML name of the <see cref="ErrorPrefix"/> property.
        /// </summary>
        internal const string ErrorPrefixPropertyName = "errorPrefix";

        /// <summary>
        /// Gets or sets the error prefix to use in the error logging.
        /// </summary>
        [ConfigurationProperty(ErrorPrefixPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string ErrorPrefix
        {
            get { return (string)base[ErrorPrefixPropertyName]; }
            set { base[ErrorPrefixPropertyName] = value; }
        }

        #endregion

		#region Category Property
		
		/// <summary>
		/// The XML name of the <see cref="Category"/> property.
		/// </summary>
		internal const string CategoryPropertyName = "category";
		
		/// <summary>
		/// Gets or sets gets or sets the message category to trace.
		/// </summary>
		[ConfigurationProperty(CategoryPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Category
		{
			get
			{
				return (string)base[CategoryPropertyName];
			}
			set
			{
				base[CategoryPropertyName] = value;
			}
		}
		
		#endregion

		#region Level Property
		
		/// <summary>
		/// The XML name of the <see cref="Level"/> property.
		/// </summary>
		internal const string LevelPropertyName = "level";
		
		/// <summary>
		/// Gets or sets gets or sets the message severity to trace.
		/// </summary>
		[ConfigurationProperty(LevelPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Level
		{
			get
			{
				return (string)base[LevelPropertyName];
			}
			set
			{
				base[LevelPropertyName] = value;
			}
		}
		
		#endregion

		#region MessageFile Property
		
		/// <summary>
		/// The XML name of the <see cref="MessageFile"/> property.
		/// </summary>
		internal const string MessageFilePropertyName = "messageFile";
		
		/// <summary>
		/// Gets or sets gets or sets the path and file name of the message trace file, if configured.
		/// </summary>
		[ConfigurationProperty(MessageFilePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string MessageFile
		{
			get
			{
                return HostingEnvironment.MapPath((string)base[MessageFilePropertyName]);
                //var p = (string)base[MessageFilePropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);
                //return (string)base[MessageFilePropertyName];
			}
			set
			{
				base[MessageFilePropertyName] = value;
			}
		}
		
		#endregion

		#region ErrorFile Property
		
		/// <summary>
		/// The XML name of the <see cref="ErrorFile"/> property.
		/// </summary>
		internal const string ErrorFilePropertyName = "errorFile";
		
		/// <summary>
		/// Gets or sets gets or sets the path and file name of the error trace file, if configured.
		/// </summary>
		[ConfigurationProperty(ErrorFilePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string ErrorFile
		{
			get
			{
                return HostingEnvironment.MapPath((string)base[ErrorFilePropertyName]);
                //var p = (string)base[ErrorFilePropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);
                //return (string)base[ErrorFilePropertyName];
			}
			set
			{
				base[ErrorFilePropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region Encryption Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// Encryption settings for containers within the configured environment.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class EncryptionElement : ConfigurationElement
	{
		#region Enabled Property
		
		/// <summary>
		/// The XML name of the <see cref="Enabled"/> property.
		/// </summary>
		internal const string EnabledPropertyName = "enabled";
		
		/// <summary>
		/// Gets or sets enables or disables encryption.
		/// </summary>
		[ConfigurationProperty(EnabledPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = false)]
		public bool Enabled
		{
			get
			{
				return (bool)base[EnabledPropertyName];
			}
			set
			{
				base[EnabledPropertyName] = value;
			}
		}
		
		#endregion

		#region Password Property
		
		/// <summary>
		/// The XML name of the <see cref="Password"/> property.
		/// </summary>
		internal const string PasswordPropertyName = "password";
		
		/// <summary>
		/// Gets or sets the password used to access the containers within the configured environment.
		/// </summary>
		[ConfigurationProperty(PasswordPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Password
		{
			get
			{
				return (string)base[PasswordPropertyName];
			}
			set
			{
				base[PasswordPropertyName] = value;
			}
		}
		
		#endregion
	}
#endif
}
#endregion

#region Log Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// If specified, configures the logging subsystem of the configured <see cref="Figaro.FigaroEnv"/> instance.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class LogElement : ConfigurationElement
	{
        #region Create Property

        /// <summary>
        /// The XML name of the <see cref="Create"/> property.
        /// </summary>
        internal const string CreatePropertyName = "create";

        /// <summary>
        /// Gets or sets whether to create the directory if it doesn't exist.
        /// </summary>
        [ConfigurationProperty(CreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public bool Create
        {
            get { return (bool)base[CreatePropertyName]; }
            set { base[CreatePropertyName] = value; }
        }

        #endregion

		#region BufferSize Property
		
		/// <summary>
		/// The XML name of the <see cref="BufferSize"/> property.
		/// </summary>
		internal const string BufferSizePropertyName = "bufferSize";
		
		/// <summary>
		/// Gets or sets the maximum log buffer size for a configured <seealso cref="FigaroEnv"/> instance.
		/// </summary>
		[ConfigurationProperty(BufferSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint BufferSize
		{
			get
			{
				return (uint)base[BufferSizePropertyName];
			}
			set
			{
				base[BufferSizePropertyName] = value;
			}
		}
		
		#endregion

		#region Directory Property
		
		/// <summary>
		/// The XML name of the <see cref="Directory"/> property.
		/// </summary>
		internal const string DirectoryPropertyName = "directory";
		
		/// <summary>
		/// Gets or sets the directory containing the environment's log files.
		/// </summary>
		[ConfigurationProperty(DirectoryPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Directory
		{
			get
			{
                return HostingEnvironment.MapPath((string)base[DirectoryPropertyName]);
                //var p = (string)base[DirectoryPropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);

                //return (string)base[DirectoryPropertyName];
			}
			set
			{
				base[DirectoryPropertyName] = value;
			}
		}
		
		#endregion

		#region MaxFileSize Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxFileSize"/> property.
		/// </summary>
		internal const string MaxFileSizePropertyName = "maxFileSize";
		
		/// <summary>
		/// Gets or sets the maximum log file size, in bytes.
		/// </summary>
		[ConfigurationProperty(MaxFileSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxFileSize
		{
			get
			{
				return (uint)base[MaxFileSizePropertyName];
			}
			set
			{
				base[MaxFileSizePropertyName] = value;
			}
		}
		
		#endregion

		#region MaxRegionSize Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxRegionSize"/> property.
		/// </summary>
		internal const string MaxRegionSizePropertyName = "maxRegionSize";
		
		/// <summary>
		/// Gets or sets the maximum log region size, in bytes.
		/// </summary>
		[ConfigurationProperty(MaxRegionSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxRegionSize
		{
			get
			{
				return (uint)base[MaxRegionSizePropertyName];
			}
			set
			{
				base[MaxRegionSizePropertyName] = value;
			}
		}
		
		#endregion

		#region LogOptions Property
		
		/// <summary>
		/// The XML name of the <see cref="LogOptions"/> property.
		/// </summary>
		internal const string LogOptionsPropertyName = "logOptions";
		
		/// <summary>
		/// Gets or sets configuration options for the logging subsystem.
		/// </summary>
		[ConfigurationProperty(LogOptionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string LogOptions
		{
			get
			{
				return (string)base[LogOptionsPropertyName];
			}
			set
			{
				base[LogOptionsPropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region MemoryFileMap Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The MemoryFileMapElement Configuration Element.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class MemoryFileMapElement : ConfigurationElement
	{
		#region MaxFileMapSize Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxFileMapSize"/> property.
		/// </summary>
		internal const string MaxFileMapSizePropertyName = "maxFileMapSize";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum size, in bytes, for the memory file configuration.
		/// </summary>
		[ConfigurationProperty(MaxFileMapSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxFileMapSize
		{
			get
			{
				return (uint)base[MaxFileMapSizePropertyName];
			}
			set
			{
				base[MaxFileMapSizePropertyName] = value;
			}
		}
		
		#endregion

		#region MaxFile Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxFile"/> property.
		/// </summary>
		internal const string MaxFilePropertyName = "maxFileDescriptors";
		
		/// <summary>
		/// Gets or sets the maximum number of file descriptors that can be concurrently opened by the library when flushing dirty pages from the cache.
		/// </summary>
		[ConfigurationProperty(MaxFilePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxFile
		{
			get
			{
				return (uint)base[MaxFilePropertyName];
			}
			set
			{
				base[MaxFilePropertyName] = value;
			}
		}
		
		#endregion

		#region MaxWrite Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxWrite"/> property.
		/// </summary>
		internal const string MaxWritePropertyName = "maxWriteOperations";
		
		/// <summary>
		/// Gets or sets the maximum number of concurrent write operations opened by the library when flushing dirty pages from the cache.
		/// </summary>
		[ConfigurationProperty(MaxWritePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxWrite
		{
			get
			{
				return (uint)base[MaxWritePropertyName];
			}
			set
			{
				base[MaxWritePropertyName] = value;
			}
		}
		
		#endregion

		#region WriteSleep Property
		
		/// <summary>
		/// The XML name of the <see cref="WriteSleep"/> property.
		/// </summary>
		internal const string WriteSleepPropertyName = "writeSleep";
		
		/// <summary>
		/// Gets or sets the number of microseconds a thread should pause before scheduling further write operations. 
		/// </summary>
		[ConfigurationProperty(WriteSleepPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint WriteSleep
		{
			get
			{
				return (uint)base[WriteSleepPropertyName];
			}
			set
			{
				base[WriteSleepPropertyName] = value;
			}
		}
		
		#endregion
	}
#endif
}
#endregion

#region Cache Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The CacheElement Configuration Element.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class CacheElement : ConfigurationElement
	{
		#region Bytes Property
		
		/// <summary>
		/// The XML name of the <see cref="Bytes"/> property.
		/// </summary>
		internal const string BytesPropertyName = "bytes";
		
		/// <summary>
		/// Gets or sets gets or sets the number of bytes in the cache.
		/// </summary>
		[ConfigurationProperty(BytesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int Bytes
		{
			get
			{
				return (int)base[BytesPropertyName];
			}
			set
			{
				base[BytesPropertyName] = value;
			}
		}
		
		#endregion

		#region GBytes Property
		
		/// <summary>
		/// The XML name of the <see cref="GBytes"/> property.
		/// </summary>
		internal const string GBytesPropertyName = "gigaBytes";
		
		/// <summary>
		/// Gets or sets gets or sets the number of gigabytes in the cache.
		/// </summary>
		[ConfigurationProperty(GBytesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int GBytes
		{
			get
			{
				return (int)base[GBytesPropertyName];
			}
			set
			{
				base[GBytesPropertyName] = value;
			}
		}
		
		#endregion

		#region Regions Property
		
		/// <summary>
		/// The XML name of the <see cref="Regions"/> property.
		/// </summary>
		internal const string RegionsPropertyName = "regions";
		
		/// <summary>
		/// Gets or sets gets or sets the number of regions in a cache.
		/// </summary>
		[ConfigurationProperty(RegionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int Regions
		{
			get
			{
				return (int)base[RegionsPropertyName];
			}
			set
			{
				base[RegionsPropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region CacheMax Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The CacheMaxElement Configuration Element.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class CacheMaxElement : ConfigurationElement
	{
		#region Bytes Property
		
		/// <summary>
		/// The XML name of the <see cref="Bytes"/> property.
		/// </summary>
		internal const string BytesPropertyName = "bytes";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum number of bytes in the environment cache.
		/// </summary>
		[ConfigurationProperty(BytesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int Bytes
		{
			get
			{
				return (int)base[BytesPropertyName];
			}
			set
			{
				base[BytesPropertyName] = value;
			}
		}
		
		#endregion

		#region GBytes Property
		
		/// <summary>
		/// The XML name of the <see cref="GBytes"/> property.
		/// </summary>
		internal const string GBytesPropertyName = "gigaBytes";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum number of gigabytes in the environment cache.
		/// </summary>
		[ConfigurationProperty(GBytesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int GBytes
		{
			get
			{
				return (int)base[GBytesPropertyName];
			}
			set
			{
				base[GBytesPropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region Open Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The configuration settings used for opening an environment instance.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class OpenElement : ConfigurationElement
	{
        #region Create Property

        /// <summary>
        /// The XML name of the <see cref="Create"/> property.
        /// </summary>
        internal const string CreatePropertyName = "create";

        /// <summary>
        /// Gets or sets whether to create the directory if it doesn't exist.
        /// </summary>
        [ConfigurationProperty(CreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public bool Create
        {
            get { return (bool)base[CreatePropertyName]; }
            set { base[CreatePropertyName] = value; }
        }

        #endregion
        
        #region options Property
		
		/// <summary>
		/// The XML name of the <see cref="Options"/> property.
		/// </summary>
		internal const string optionsPropertyName = "options";
		
		/// <summary>
		/// Gets or sets gets or sets the options specified at environment open.
		/// </summary>
		[ConfigurationProperty(optionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Options
		{
			get
			{
				return (string)base[optionsPropertyName];
			}
			set
			{
				base[optionsPropertyName] = value;
			}
		}
		
		#endregion

		#region home Property
		
		/// <summary>
		/// The XML name of the <see cref="Home"/> property.
		/// </summary>
		internal const string homePropertyName = "home";
		
		/// <summary>
		/// Gets or sets gets or sets the environment's home directory.
		/// </summary>
		[ConfigurationProperty(homePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Home
		{
			get
			{
                return HostingEnvironment.MapPath((string)base[homePropertyName]);
                //var p = (string)base[homePropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);
                //return (string)base[homePropertyName];

			}
			set
			{
				base[homePropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region Temp Directory Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// Configuration settings for the environment temporary directory.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class TempDirectoryElement : ConfigurationElement
	{
        #region Create Property

        /// <summary>
        /// The XML name of the <see cref="Create"/> property.
        /// </summary>
        internal const string CreatePropertyName = "create";

        /// <summary>
        /// Gets or sets whether to create the directory if it doesn't exist.
        /// </summary>
        [ConfigurationProperty(CreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public bool Create
        {
            get { return (bool)base[CreatePropertyName]; }
            set { base[CreatePropertyName] = value; }
        }

        #endregion
        
        #region path Property
		
		/// <summary>
		/// The XML name of the <see cref="Path"/> property.
		/// </summary>
		internal const string pathPropertyName = "path";
		
		/// <summary>
		/// Gets or sets gets or sets the path to the temporary directory used by the environment instance.
		/// </summary>
		[ConfigurationProperty(pathPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Path
		{
			get
			{
                return HostingEnvironment.MapPath((string)base[pathPropertyName]);
                //var p = (string)base[pathPropertyName];
                //if (p.Contains("~"))
                //    return HostingEnvironment.MapPath(p);

                //return (string)base[pathPropertyName];
			}
			set
			{
				base[pathPropertyName] = value;
			}
		}
		
		#endregion
	}
#endif
}
#endregion

#region Transactions Element
#if TDS
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// The settings for the transactional subsystem of the environment instance.
	/// </summary>
	public class TransactionsElement : ConfigurationElement
	{
#region MaxTransactions Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxTransactions"/> property.
		/// </summary>
		internal const string MaxTransactionsPropertyName = "maxTransactions";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum number of simultaneous transactions running in an environment instance.
		/// </summary>
		[ConfigurationProperty(MaxTransactionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public ushort MaxTransactions
		{
			get
			{
				return (ushort)base[MaxTransactionsPropertyName];
			}
			set
			{
				base[MaxTransactionsPropertyName] = value;
			}
		}
		
        #endregion

#region Timeout Property
		
		/// <summary>
		/// The XML name of the <see cref="Timeout"/> property.
		/// </summary>
		internal const string timeoutPropertyName = "timeout";
		
		/// <summary>
		/// Gets or sets gets or sets the configured transaction timeout, in microseconds.
		/// </summary>
		[ConfigurationProperty(timeoutPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int Timeout
		{
			get
			{
				return (int)base[timeoutPropertyName];
			}
			set
			{
				base[timeoutPropertyName] = value;
			}
		}
		
        #endregion

	}
}
#endif
#endregion

#region LockingElement
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// Configures the locking subsystem of the environment instance.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class LockingElement : ConfigurationElement
	{
		#region AutoDetect Property
		
		/// <summary>
		/// The XML name of the <see cref="AutoDetect"/> property.
		/// </summary>
		internal const string AutoDetectPropertyName = "autoDetect";
		
		/// <summary>
		/// Gets or sets gets or sets the deadlock detection policy for automatic deadlock detection.
		/// </summary>
		[ConfigurationProperty(AutoDetectPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string AutoDetect
		{
			get
			{
				return (string)base[AutoDetectPropertyName];
			}
			set
			{
				base[AutoDetectPropertyName] = value;
			}
		}

		#endregion

		#region Timeout Property
		
		/// <summary>
		/// The XML name of the <see cref="Timeout"/> property.
		/// </summary>
		internal const string TimeoutPropertyName = "timeout";
		
		/// <summary>
		/// Gets or sets gets or sets the deadlock timeout value, in microseconds.
		/// </summary>
		[ConfigurationProperty(TimeoutPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public int Timeout
		{
			get
			{
				return (int)base[TimeoutPropertyName];
			}
			set
			{
				base[TimeoutPropertyName] = value;
			}
		}
		
		#endregion

		#region MaxLocks Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxLocks"/> property.
		/// </summary>
		internal const string MaxLocksPropertyName = "maxLocks";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum number of locks supported by the environment instance.
		/// </summary>
		[ConfigurationProperty(MaxLocksPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxLocks
		{
			get
			{
				return (uint)base[MaxLocksPropertyName];
			}
			set
			{
				base[MaxLocksPropertyName] = value;
			}
		}
		
		#endregion

		#region MaxLockers Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxLockers"/> property.
		/// </summary>
		internal const string MaxLockersPropertyName = "maxLockers";
		
		/// <summary>
		/// Gets or sets  Gets or sets the maximum number of locking entities supported by the environment instance.
		/// </summary>
		[ConfigurationProperty(MaxLockersPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxLockers
		{
			get
			{
				return (uint)base[MaxLockersPropertyName];
			}
			set
			{
				base[MaxLockersPropertyName] = value;
			}
		}
		
		#endregion

		#region MaxLockObjects Property
		
		/// <summary>
		/// The XML name of the <see cref="MaxLockObjects"/> property.
		/// </summary>
		internal const string MaxLockObjectsPropertyName = "maxLockObjects";
		
		/// <summary>
		/// Gets or sets gets or sets the maximum lock objects for the environment instance.
		/// </summary>
		[ConfigurationProperty(MaxLockObjectsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint MaxLockObjects
		{
			get
			{
				return (uint)base[MaxLockObjectsPropertyName];
			}
			set
			{
				base[MaxLockObjectsPropertyName] = value;
			}
		}
		#endregion

		#region LockPartitions Property

		/// <summary>
		/// The XML name of the <see cref="LockPartitions"/> property.
		/// </summary>
		internal const string LockPartitionsPropertyName = "partitions";
		
		/// <summary>
		/// Gets or sets gets or sets the number of lock partitions for the environment instance.
		/// </summary>
		[ConfigurationProperty(LockPartitionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint LockPartitions
		{
			get
			{
				return (uint)base[LockPartitionsPropertyName];
			}
			set
			{
				base[LockPartitionsPropertyName] = value;
			}
        }
        #endregion
    }
#endif
}
#endregion

#region Mutex Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The environment mutex configuration settings.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class MutexElement : ConfigurationElement
	{
		#region TasSpinCount Property
		
		/// <summary>
		/// The XML name of the <see cref="TasSpinCount"/> property.
		/// </summary>
		internal const string TasSpinCountPropertyName = "tasSpinCount";
		
		/// <summary>
		/// Gets or sets gets or sets the mutex test-and-set spin count.
		/// </summary>
		[ConfigurationProperty(TasSpinCountPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint TasSpinCount
		{
			get
			{
				return (uint)base[TasSpinCountPropertyName];
			}
			set
			{
				base[TasSpinCountPropertyName] = value;
			}
		}
		
		#endregion

		#region Align Property
		
		/// <summary>
		/// The XML name of the <see cref="Align"/> property.
		/// </summary>
		internal const string AlignPropertyName = "align";
		
		/// <summary>
		/// Gets or sets gets or sets mutex alignment.
		/// </summary>
		[ConfigurationProperty(AlignPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint Align
		{
			get
			{
				return (uint)base[AlignPropertyName];
			}
			set
			{
				base[AlignPropertyName] = value;
			}
		}
		
		#endregion

		#region Increment Property
		
		/// <summary>
		/// The XML name of the <see cref="Increment"/> property.
		/// </summary>
		internal const string IncrementPropertyName = "increment";
		
		/// <summary>
		/// Gets or sets gets or sets the mutex increment.
		/// </summary>
		[ConfigurationProperty(IncrementPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint Increment
		{
			get
			{
				return (uint)base[IncrementPropertyName];
			}
			set
			{
				base[IncrementPropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region EnvConfig Collection Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The collection of environment configuration settings to enable or disable against the environment instance.
	/// </summary>
	[ConfigurationCollection(typeof(EnvConfigElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class EnvConfigElementCollection : ConfigurationElementCollection
	{
		#region Constants
		
		/// <summary>
		/// The XML name of the individual <see cref="EnvConfigElement"/> instances in this collection.
		/// </summary>
		internal const string EnvConfigElementPropertyName = "ConfigItem";

		#endregion

		#region Overrides

		/// <summary>
		/// Gets the type of the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
		/// </summary>
		/// <param name="elementName">The name of the element to verify.</param>
		/// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. <para>The default is <see langword="false"/>.</para>
		/// </returns>
		protected override bool IsElementName(string elementName)
		{
			return (elementName == EnvConfigElementPropertyName);
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((EnvConfigElement)element).Setting;
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new EnvConfigElement();
		}

		#endregion
		
		#region Indexer

		/// <summary>
		/// Gets the <see cref="EnvConfigElement"/> at the specified index.
		/// </summary>
		/// <param name="index">The index of the <see cref="EnvConfigElement"/> to retrieve</param>
		public EnvConfigElement this[int index]
		{
			get
			{
				return (EnvConfigElement)BaseGet(index);
			}
		}

		#endregion
		
		#region Add

		/// <summary>
		/// Adds the specified <see cref="EnvConfigElement"/>.
		/// </summary>
		/// <param name="ConfigItem">The <see cref="EnvConfigElement"/> to add.</param>
		public void Add(EnvConfigElement ConfigItem)
		{
			BaseAdd(ConfigItem);
		}

		#endregion
		
		#region Remove

		/// <summary>
		/// Removes the specified <see cref="EnvConfigElement"/>.
		/// </summary>
		/// <param name="ConfigItem">The <see cref="EnvConfigElement"/> to remove.</param>
		public void Remove(EnvConfigElement ConfigItem)
		{
			BaseRemove(ConfigItem);
		}

		#endregion
	}
#endif
}
#endregion

#region EnvConfig Element
namespace Figaro.Web.Configuration
{
#if CDS || TDS
	/// <summary>
	/// The environment configuration setting instance.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class EnvConfigElement : ConfigurationElement
	{
		#region Setting Property
		
		/// <summary>
		/// The XML name of the <see cref="Setting"/> property.
		/// </summary>
		internal const string SettingPropertyName = "setting";
		
		/// <summary>
		/// Gets or sets gets or sets the environment setting to enable or disable.
		/// </summary>
		[ConfigurationProperty(SettingPropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
		public string Setting
		{
			get
			{
				return (string)base[SettingPropertyName];
			}
			set
			{
				base[SettingPropertyName] = value;
			}
		}
		
		#endregion

		#region Enabled Property
		
		/// <summary>
		/// The XML name of the <see cref="Enabled"/> property.
		/// </summary>
		internal const string EnabledPropertyName = "enabled";
		
		/// <summary>
		/// Gets or sets the Enabled.
		/// </summary>
		[ConfigurationProperty(EnabledPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Enabled
		{
			get
			{
				return (bool)base[EnabledPropertyName];
			}
			set
			{
				base[EnabledPropertyName] = value;
			}
		}
		
		#endregion

	}
#endif
}
#endregion

#region Default Container Element
namespace Figaro.Web.Configuration
{
	/// <summary>
	/// Default configuration settings to be used when creating new containers.
	/// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class DefaultContainerElement : ConfigurationElement
	{
		#region Compression Property
		
		/// <summary>
		/// The XML name of the <see cref="Compression"/> property.
		/// </summary>
		internal const string CompressionPropertyName = "compression";
		
		/// <summary>
		/// Gets or sets gets or sets container compression.
		/// </summary>
		[ConfigurationProperty(CompressionPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Compression
		{
			get
			{
				return (bool)base[CompressionPropertyName];
			}
			set
			{
				base[CompressionPropertyName] = value;
			}
		}
		
		#endregion

		#region SequenceIncrement Property
		
		/// <summary>
		/// The XML name of the <see cref="SequenceIncrement"/> property.
		/// </summary>
		internal const string SequenceIncrementPropertyName = "sequenceIncrement";
		
		/// <summary>
		/// Gets or sets gets or sets the integer increment used when pre-allocating IDs at document creation.
		/// </summary>
		[ConfigurationProperty(SequenceIncrementPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint SequenceIncrement
		{
			get
			{
				return (uint)base[SequenceIncrementPropertyName];
			}
			set
			{
				base[SequenceIncrementPropertyName] = value;
			}
		}
		
		#endregion

		#region PageSize Property
		
		/// <summary>
		/// The XML name of the <see cref="PageSize"/> property.
		/// </summary>
		internal const string PageSizePropertyName = "pageSize";
		
		/// <summary>
		/// Gets or sets gets or sets the container page size.
		/// </summary>
		[ConfigurationProperty(PageSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public uint PageSize
		{
			get
			{
				return (uint)base[PageSizePropertyName];
			}
			set
			{
				base[PageSizePropertyName] = value;
			}
		}
		
		#endregion

		#region Checksum Property
		
		/// <summary>
		/// The XML name of the <see cref="Checksum"/> property.
		/// </summary>
		internal const string ChecksumPropertyName = "checksum";
		
		/// <summary>
        /// Gets or sets whether to do checksum verification of pages read into the cache from the backing file store.
		/// </summary>
		[ConfigurationProperty(ChecksumPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false )]
		public bool Checksum
		{
			get
			{
				return (bool)base[ChecksumPropertyName];
			}
			set
			{
				base[ChecksumPropertyName] = value;
			}
		}
		
		#endregion

		#region ReadUncommitted Property
		
		/// <summary>
		/// The XML name of the <see cref="ReadUncommitted"/> property.
		/// </summary>
		internal const string ReadUncommittedPropertyName = "readUncommitted";
		
		/// <summary>
		/// Gets or sets gets or sets the ability to do dirty container reads.
		/// </summary>
		[ConfigurationProperty(ReadUncommittedPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool ReadUncommitted
		{
			get
			{
				return (bool)base[ReadUncommittedPropertyName];
			}
			set
			{
				base[ReadUncommittedPropertyName] = value;
			}
		}
		
		#endregion

		#region Encrypted Property
		
		/// <summary>
		/// The XML name of the <see cref="Encrypted"/> property.
		/// </summary>
		internal const string EncryptedPropertyName = "encrypted";
		
		/// <summary>
		/// Gets or sets gets or sets whether the container is encrypted.
		/// </summary>
		[ConfigurationProperty(EncryptedPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Encrypted
		{
			get
			{
				return (bool)base[EncryptedPropertyName];
			}
			set
			{
				base[EncryptedPropertyName] = value;
			}
		}
		
		#endregion

		#region AllowValidation Property
		
		/// <summary>
		/// The XML name of the <see cref="AllowValidation"/> property.
		/// </summary>
		internal const string AllowValidationPropertyName = "allowValidation";
		
		/// <summary>
		/// Gets or sets gets or sets whether to validate XML if it refers to a DTD or schema.
		/// </summary>
		[ConfigurationProperty(AllowValidationPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool AllowValidation
		{
			get
			{
				return (bool)base[AllowValidationPropertyName];
			}
			set
			{
				base[AllowValidationPropertyName] = value;
			}
		}
		
		#endregion

		#region Statistics Property
		
		/// <summary>
		/// The XML name of the <see cref="Statistics"/> property.
		/// </summary>
		internal const string StatisticsPropertyName = "statistics";
		
		/// <summary>
		/// Gets or sets gets or sets whether to enable container statistics.
		/// </summary>
		[ConfigurationProperty(StatisticsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string Statistics
		{
			get
			{
				return (string)base[StatisticsPropertyName];
			}
			set
			{
				base[StatisticsPropertyName] = value;
			}
		}
		
		#endregion

		#region IndexNodes Property
		
		/// <summary>
		/// The XML name of the <see cref="IndexNodes"/> property.
		/// </summary>
		internal const string IndexNodesPropertyName = "indexNodes";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not to enable document or node indexing.
		/// </summary>
		[ConfigurationProperty(IndexNodesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string IndexNodes
		{
			get
			{
                return (string)base[IndexNodesPropertyName];
			}
			set
			{
				base[IndexNodesPropertyName] = value;
			}
		}
		
		#endregion

		#region Transactional Property
		
		/// <summary>
		/// The XML name of the <see cref="Transactional"/> property.
		/// </summary>
		internal const string TransactionalPropertyName = "transactional";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not the container is configured for transactions.
		/// </summary>
		[ConfigurationProperty(TransactionalPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Transactional
		{
			get
			{
				return (bool)base[TransactionalPropertyName];
			}
			set
			{
				base[TransactionalPropertyName] = value;
			}
		}
		
		#endregion

		#region AllowCreate Property
		
		/// <summary>
		/// The XML name of the <see cref="AllowCreate"/> property.
		/// </summary>
		internal const string AllowCreatePropertyName = "allowCreate";
		
		/// <summary>
		/// Gets or sets gets or sets whether a container can be created if it does not already exist.
		/// </summary>
		[ConfigurationProperty(AllowCreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool AllowCreate
		{
			get
			{
				return (bool)base[AllowCreatePropertyName];
			}
			set
			{
				base[AllowCreatePropertyName] = value;
			}
		}
		
		#endregion

		#region Multiversion Property
		
		/// <summary>
		/// The XML name of the <see cref="Multiversion"/> property.
		/// </summary>
		internal const string MultiversionPropertyName = "multiVersion";
		
		/// <summary>
		/// Gets or sets gets or sets whether the database will be opened with support for Multiversion concurrency control (MVCC). This will cause updates to  the container to follow a copy-on-write protocol which is required to support snapshot isolation. 
		/// </summary>
		[ConfigurationProperty(MultiversionPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Multiversion
		{
			get
			{
				return (bool)base[MultiversionPropertyName];
			}
			set
			{
				base[MultiversionPropertyName] = value;
			}
		}
		
		#endregion

		#region ExclusiveCreate Property
		
		/// <summary>
		/// The XML name of the <see cref="ExclusiveCreate"/> property.
		/// </summary>
		internal const string ExclusiveCreatePropertyName = "exclusiveCreate";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not to throw exceptions if the container already exists when trying to create another instance.
		/// </summary>
		[ConfigurationProperty(ExclusiveCreatePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool ExclusiveCreate
		{
			get
			{
				return (bool)base[ExclusiveCreatePropertyName];
			}
			set
			{
				base[ExclusiveCreatePropertyName] = value;
			}
		}
		
		#endregion

		#region NoMMap Property
		
		/// <summary>
		/// The XML name of the <see cref="NoMMap"/> property.
		/// </summary>
		internal const string NoMMapPropertyName = "noMMap";
		
		/// <summary>
		/// Gets or sets gets or sets whether or not the container will be mapped to process memory.
		/// </summary>
		[ConfigurationProperty(NoMMapPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool NoMMap
		{
			get
			{
				return (bool)base[NoMMapPropertyName];
			}
			set
			{
				base[NoMMapPropertyName] = value;
			}
		}
		
		#endregion

		#region Threaded Property
		
		/// <summary>
		/// The XML name of the <see cref="Threaded"/> property.
		/// </summary>
		internal const string ThreadedPropertyName = "threaded";
		
		/// <summary>
		/// Gets or sets gets or sets whether the container handle is free-threaded. 
		/// </summary>
		[ConfigurationProperty(ThreadedPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool Threaded
		{
			get
			{
				return (bool)base[ThreadedPropertyName];
			}
			set
			{
				base[ThreadedPropertyName] = value;
			}
		}
		
		#endregion

		#region ReadOnly Property
		
		/// <summary>
		/// The XML name of the <see cref="ReadOnly"/> property.
		/// </summary>
		internal const string ReadOnlyPropertyName = "readOnly";
		
		/// <summary>
		/// Gets or sets gets or sets whether the container is configured for read-only access.
		/// </summary>
		[ConfigurationProperty(ReadOnlyPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool ReadOnly
		{
			get
			{
				return (bool)base[ReadOnlyPropertyName];
			}
			set
			{
				base[ReadOnlyPropertyName] = value;
			}
		}
		
		#endregion

		#region TransactionNotDurable Property
		
		/// <summary>
		/// The XML name of the <see cref="TransactionNotDurable"/> property.
		/// </summary>
		internal const string TransactionNotDurablePropertyName = "transactionNotDurable";
		
		/// <summary>
		/// Gets or sets gets or sets transaction durability.
		/// </summary>
		[ConfigurationProperty(TransactionNotDurablePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public bool TransactionNotDurable
		{
			get
			{
				return (bool)base[TransactionNotDurablePropertyName];
			}
			set
			{
				base[TransactionNotDurablePropertyName] = value;
			}
		}
		
		#endregion

		#region ContainerType Property
		
		/// <summary>
		/// The XML name of the <see cref="ContainerType"/> property.
		/// </summary>
		internal const string ContainerTypePropertyName = "containerType";
		
		/// <summary>
		/// Gets or sets gets or sets the container type.
		/// </summary>
		[ConfigurationProperty(ContainerTypePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
		public string ContainerType
		{
			get
			{
				return (string)base[ContainerTypePropertyName];
			}
			set
			{
				base[ContainerTypePropertyName] = value;
			}
		}
		
		#endregion

	}
}
#endregion

#region QueryCollectionElement
namespace Figaro.Web.Configuration
{
    /// <summary>
    /// The collection of query configuration items available for preparing and executing queries.
    /// </summary>
    [ConfigurationCollection(typeof(QueryElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = QueryElementPropertyName)]
    public class QueryCollectionElement : ConfigurationElementCollection
    {
        #region Constants

        /// <summary>
        /// The XML name of the individual <see cref="QueryElement"/> instances in this collection.
        /// </summary>
        internal const string QueryElementPropertyName = "queryElement";

        #endregion

        #region Overrides

        /// <summary>
        /// Gets the type of the <see cref="ConfigurationElementCollection"/>.
        /// </summary>
        /// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
        /// </summary>
        /// <param name="elementName">The name of the element to verify.</param>
        /// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </returns>
        protected override bool IsElementName(string elementName)
        {
            return (elementName == QueryElementPropertyName);
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="Object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((QueryElement)element).Name;
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new QueryElement();
        }

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the <see cref="QueryElement"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the <see cref="QueryElement"/> to retrieve</param>
        public QueryElement this[int index]
        {
            get
            {
                return (QueryElement)BaseGet(index);
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds the specified <see cref="QueryElement"/>.
        /// </summary>
        /// <param name="queryElement">The <see cref="QueryElement"/> to add.</param>
        public void Add(QueryElement queryElement)
        {
            BaseAdd(queryElement);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified <see cref="QueryElement"/>.
        /// </summary>
        /// <param name="queryElement">The <see cref="QueryElement"/> to remove.</param>
        public void Remove(QueryElement queryElement)
        {
            BaseRemove(queryElement);
        }

        #endregion
    }
}
#endregion

#region QueryElement
namespace Figaro.Web.Configuration
{
    /// <summary>
    /// Information used for preparing and performing XQuery expressions against the containers.
    /// </summary>
    public class QueryElement : ConfigurationElement
    {
        #region Name Property

        /// <summary>
        /// The XML name of the <see cref="Name"/> property.
        /// </summary>
        internal const string NamePropertyName = "name";

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [ConfigurationProperty(NamePropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
        public string Name
        {
            get
            {
                return (string)base[NamePropertyName];
            }
            set
            {
                base[NamePropertyName] = value;
            }
        }

        #endregion

        #region QueryPath Property

        /// <summary>
        /// The XML name of the <see cref="QueryPath"/> property.
        /// </summary>
        internal const string QueryPathPropertyName = "queryPath";

        /// <summary>
        /// Gets or sets gets or sets the path to the XQuery module to execute.
        /// </summary>
        [ConfigurationProperty(QueryPathPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string QueryPath
        {
            get
            {
                return (string)base[QueryPathPropertyName];
            }
            set
            {
                base[QueryPathPropertyName] = value;
            }
        }

        #endregion

        #region Prepare Property

        /// <summary>
        /// The XML name of the <see cref="Prepare"/> property.
        /// </summary>
        internal const string PreparePropertyName = "prepare";

        /// <summary>
        /// Gets or sets gets or sets whether to prepare a query before executing.
        /// </summary>
        [ConfigurationProperty(PreparePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public bool Prepare
        {
            get
            {
                return (bool)base[PreparePropertyName];
            }
            set
            {
                base[PreparePropertyName] = value;
            }
        }

        #endregion

        #region Namespaces Property

        /// <summary>
        /// The XML name of the <see cref="Namespaces"/> property.
        /// </summary>
        internal const string NamespacesPropertyName = "Namespaces";

        /// <summary>
        /// Gets or sets the namespaces used by the query expression.
        /// </summary>
        [ConfigurationProperty(NamespacesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public NamespaceElementCollection Namespaces
        {
            get
            {
                return (NamespaceElementCollection)base[NamespacesPropertyName];
            }
            set
            {
                base[NamespacesPropertyName] = value;
            }
        }

        #endregion

        #region Variables Property

        /// <summary>
        /// The XML name of the <see cref="Variables"/> property.
        /// </summary>
        internal const string VariablesPropertyName = "Variables";

        /// <summary>
        /// Gets or sets the variables used by the query expression.
        /// </summary>
        [ConfigurationProperty(VariablesPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public VariableElementCollection Variables
        {
            get
            {
                return (VariableElementCollection)base[VariablesPropertyName];
            }
            set
            {
                base[VariablesPropertyName] = value;
            }
        }

        #endregion

    }
}
#endregion

#region NamespaceElementCollection
namespace Figaro.Web.Configuration
{
    /// <summary>
    /// The collection of elements used to configure query expressions.
    /// </summary>
    [ConfigurationCollection(typeof(NamespaceElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class NamespaceElementCollection : ConfigurationElementCollection
    {
        #region Constants

        /// <summary>
        /// The XML name of the individual <see cref="NamespaceElement"/> instances in this collection.
        /// </summary>
        internal const string NamespaceElementPropertyName = "Namespace";

        #endregion

        #region Overrides

        /// <summary>
        /// Gets the type of the <see cref="ConfigurationElementCollection"/>.
        /// </summary>
        /// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
        /// </summary>
        /// <param name="elementName">The name of the element to verify.</param>
        /// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </returns>
        protected override bool IsElementName(string elementName)
        {
            return (elementName == NamespaceElementPropertyName);
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NamespaceElement)element).Prefix;
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new NamespaceElement();
        }

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the <see cref="NamespaceElement"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the <see cref="NamespaceElement"/> to retrieve</param>
        public NamespaceElement this[int index]
        {
            get
            {
                return (NamespaceElement)BaseGet(index);
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds the specified <see cref="NamespaceElement"/>.
        /// </summary>
        /// <param name="Namespace">The <see cref="NamespaceElement"/> to add.</param>
        public void Add(NamespaceElement Namespace)
        {
            BaseAdd(Namespace);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified <see cref="NamespaceElement"/>.
        /// </summary>
        /// <param name="Namespace">The <see cref="NamespaceElement"/> to remove.</param>
        public void Remove(NamespaceElement Namespace)
        {
            BaseRemove(Namespace);
        }

        #endregion
    }
}
#endregion

#region NamespaceElement
namespace Figaro.Web.Configuration
{
    /// <summary>
    /// A namespace object used in an XQuery expression.
    /// </summary>
    public class NamespaceElement : ConfigurationElement
    {
        #region Prefix Property

        /// <summary>
        /// The XML name of the <see cref="Prefix"/> property.
        /// </summary>
        internal const string PrefixPropertyName = "prefix";

        /// <summary>
        /// Gets or sets gets or sets the namespace prefix
        /// </summary>
        [ConfigurationProperty(PrefixPropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
        public string Prefix
        {
            get
            {
                return (string)base[PrefixPropertyName];
            }
            set
            {
                base[PrefixPropertyName] = value;
            }
        }

        #endregion

        #region Value Property

        /// <summary>
        /// The XML name of the <see cref="Value"/> property.
        /// </summary>
        internal const string ValuePropertyName = "value";

        /// <summary>
        /// Gets or sets gets or sets the namespace value.
        /// </summary>
        [ConfigurationProperty(ValuePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Value
        {
            get
            {
                return (string)base[ValuePropertyName];
            }
            set
            {
                base[ValuePropertyName] = value;
            }
        }

        #endregion

    }
}
#endregion

#region VariableElementCollection
namespace Figaro.Web.Configuration
{
    /// <summary>
    /// The collection of variables used by the query expression.
    /// </summary>
    [ConfigurationCollection(typeof(VariableElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class VariableElementCollection : ConfigurationElementCollection
    {
        #region Constants

        /// <summary>
        /// The XML name of the individual <see cref="VariableElement"/> instances in this collection.
        /// </summary>
        internal const string VariableElementPropertyName = "Variable";

        #endregion

        #region Overrides

        /// <summary>
        /// Gets the type of the <see cref="ConfigurationElementCollection"/>.
        /// </summary>
        /// <returns>The <see cref="ConfigurationElementCollectionType"/> of this collection.</returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="ConfigurationElement"/> exists in the <see cref="ConfigurationElementCollection"/>.
        /// </summary>
        /// <param name="elementName">The name of the element to verify.</param>
        /// <returns>
        /// <see langword="true"/> if the element exists in the collection; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </returns>
        protected override bool IsElementName(string elementName)
        {
            return (elementName == VariableElementPropertyName);
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((VariableElement)element).Name;
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new VariableElement();
        }

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the <see cref="VariableElement"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the <see cref="VariableElement"/> to retrieve</param>
        public VariableElement this[int index]
        {
            get
            {
                return (VariableElement)BaseGet(index);
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds the specified <see cref="VariableElement"/>.
        /// </summary>
        /// <param name="Variable">The <see cref="VariableElement"/> to add.</param>
        public void Add(VariableElement Variable)
        {
            BaseAdd(Variable);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified <see cref="VariableElement"/>.
        /// </summary>
        /// <param name="Variable">The <see cref="VariableElement"/> to remove.</param>
        public void Remove(VariableElement Variable)
        {
            BaseRemove(Variable);
        }

        #endregion
    }
}
#endregion

#region VariableElement
namespace Figaro.Web.Configuration
{
    /// <summary>
    /// Variables used by the XQuery expression.
    /// </summary>
    public class VariableElement : ConfigurationElement
    {
        #region Name Property

        /// <summary>
        /// The XML name of the <see cref="Name"/> property.
        /// </summary>
        internal const string NamePropertyName = "name";

        /// <summary>
        /// Gets or sets gets or sets the variable name.
        /// </summary>
        [ConfigurationProperty(NamePropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
        public string Name
        {
            get
            {
                return (string)base[NamePropertyName];
            }
            set
            {
                base[NamePropertyName] = value;
            }
        }

        #endregion

        #region Value Property

        /// <summary>
        /// The XML name of the <see cref="Value"/> property.
        /// </summary>
        internal const string ValuePropertyName = "value";

        /// <summary>
        /// Gets or sets gets or sets the variable value.
        /// </summary>
        [ConfigurationProperty(ValuePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Value
        {
            get
            {
                return (string)base[ValuePropertyName];
            }
            set
            {
                base[ValuePropertyName] = value;
            }
        }

        #endregion

    }
}
#endregion