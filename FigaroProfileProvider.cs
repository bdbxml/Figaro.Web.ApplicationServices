/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.Web.Profile;
using Figaro.Web.ApplicationServices.Data;

namespace Figaro.Web.ApplicationServices.Profile
{
    /// <summary>
    /// Manages storage of profile information for an ASP.NET application in a Figaro XML database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To use the <see cref="FigaroProfileProvider"/>, you must specify a path and file name in the <c>container</c> setting of the provider configuration. 
    /// </para>
    /// <example>
    /// The following example shows the <c>web.config</c> file for an ASP.NET application used to configure <see cref="FigaroProfileProvider"/>.
    /// <code lang="xml">
    ///&lt;profile automaticSaveEnabled="false" enabled="true" defaultProvider="FigaroProfileProvider"&gt;
    ///    &lt;providers&gt;
    ///        &lt;clear/&gt;
    ///        &lt;add name="FigaroProfileProvider" 
    ///         type="Figaro.Web.Profile.FigaroProfileProvider, Figaro.Web, Version=2.4.16.1, Culture=neutral, PublicKeyToken=ff51709ab7ef68cf" 
    ///         container="D:\data\db\Providers\Profile.dbxml" 
    ///         applicationName="/"/&gt;
    ///    &lt;/providers&gt;
    /// &lt;/profile&gt;
    /// </code>
    /// </example>
    /// <para>
    /// If the specified container does not exist, a node-storage container will automatically be created for you.
    /// </para>
    /// <para>
    /// <para>
    ///     Profiles are stored as XML messages and indexed according to user name. The schema definition for a profile entry is shown below:
    /// </para>
    /// <code lang="xml">
    ///&lt;?xml version="1.0" encoding="utf-8"?&gt;
    ///&lt;xs:schema id="FigaroUserProfile" targetNamespace="http://schemas.bdbxml.net/web/profile/2009/05/" xmlns:mstns="http://schemas.bdbxml.net/web/profile/2009/05/" xmlns="http://schemas.bdbxml.net/web/profile/2009/05/" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" attributeFormDefault="qualified" elementFormDefault="qualified"&gt;
    ///  &lt;xs:element name="FigaroUserProfile" msdata:IsDataSet="true" msdata:UseCurrentLocale="true"&gt;
    ///    &lt;xs:complexType&gt;
    ///      &lt;xs:choice minOccurs="0" maxOccurs="unbounded"&gt;
    ///        &lt;xs:element name="Property" nillable="true"&gt;
    ///          &lt;xs:complexType&gt;
    ///            &lt;xs:simpleContent msdata:ColumnName="Property_Text" msdata:Ordinal="4"&gt;
    ///              &lt;xs:extension base="xs:string"&gt;
    ///                &lt;xs:attribute name="name" form="unqualified" type="xs:string" /&gt;
    ///                &lt;xs:attribute name="serializeAs" form="unqualified" type="xs:string" /&gt;
    ///                &lt;xs:attribute name="propertyType" form="unqualified" type="xs:string" /&gt;
    ///                &lt;xs:attribute name="usingDefault" form="unqualified" type="xs:string" /&gt;
    ///              &lt;/xs:extension&gt;
    ///            &lt;/xs:simpleContent&gt;
    ///          &lt;/xs:complexType&gt;
    ///        &lt;/xs:element&gt;
    ///      &lt;/xs:choice&gt;
    ///    &lt;/xs:complexType&gt;
    ///  &lt;/xs:element&gt;
    ///&lt;/xs:schema&gt;
    /// </code>
    /// <para>
    ///     The following XML metadata is stored for each profile entry:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Metadata name</term>
    /// <description>Metadata URI</description>
    /// </listheader>
    /// <item>
    ///     <term>
    ///         name
    ///     </term>
    ///     <description>
    ///     http://www.sleepycat.com/2002/dbxml
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>
    ///     LastActivityDate
    ///     </term>
    ///     <description>
    ///     http://schemas.bdbxml.net/web/profile/2009/05/
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>
    ///     LastUpdatedTime
    ///     </term>
    ///     <description>
    ///     http://schemas.bdbxml.net/web/profile/2009/05/
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>
    ///     IsAuthenticated
    ///     </term>
    ///     <description>
    ///     http://schemas.bdbxml.net/web/profile/2009/05/
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>
    ///     Anonymous
    ///     </term>
    ///     <description>
    ///     http://schemas.bdbxml.net/web/profile/2009/05/
    ///     </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class FigaroProfileProvider : ProfileProvider
    {
        private string containerPath;
        /// <summary>
        /// Gets the path of the container used for storing profiles.
        /// </summary>
        public string ContainerPath
        {
            get
            {
                return containerPath ?? ContainerPathFromConfig();
            }
        }

        //private static NameValueCollection providerConfig;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.
        ///                 </param><param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.
        ///                 </param><exception cref="T:System.ArgumentNullException">The name of the provider is null.
        ///                 </exception><exception cref="T:System.ArgumentException">The name of the provider has a length of zero.
        ///                 </exception><exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        ///                 </exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            //providerConfig = config;

            if (string.IsNullOrEmpty(name))
                name = Name;
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", Description);
            }

            base.Initialize(name, config);

            if (null == config["container"])
                throw new ProviderException("container is a required setting for FigaroProfileProvider and must be a directory and file name for the Profile container.");

            containerPath = config["container"];
            if (string.IsNullOrEmpty(config["applicationName"]))
                ApplicationName = ProviderUtility.GetDefaultAppName();
        }
        /// <summary>
        /// Deletes all user-profile data for profiles from the profile container in which the last activity date occurred before the specified date.
        /// </summary>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.
        ///                 </param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  value of a user profile occurs on or before this date and time, the profile is considered inactive.
        ///                 </param>
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try
            {
                return FigaroProfileData.GetFigaroProfileData(ContainerPath).DeleteProfiles(authenticationOption,
                                                                                            userInactiveSinceDate);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "delete inactive profiles"), ex);
            }
        }

        /// <summary>
        /// Utility function to get the configuration information from the config file. 
        /// </summary>
        /// <returns></returns>
        private static string ContainerPathFromConfig()
        {
            //(ConfigurationElementCollection)
            var profile = (ProfileSection)WebConfigurationManager.GetSection("system.web/profile");
            foreach (ProviderSettings provider in profile.Providers)
            {
                if (provider.Type.Equals(typeof(FigaroProfileProvider).AssemblyQualifiedName))
                {
                    return provider.Parameters["container"];
                }
            }
            return null;
        }

        /// <summary>
        /// Deletes profile properties and information for profiles that match the supplied list of user names.
        /// </summary>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        /// <param name="usernames">A string array of user names for profiles to be deleted.
        ///                 </param>
        public override int DeleteProfiles(string[] usernames)
        {
            try
            {
                return FigaroProfileData.GetFigaroProfileData(ContainerPath).DeleteProfiles(usernames);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "delete profiles"), ex);
            }

        }

        /// <summary>
        /// Deletes profile properties and information from the provider container for the supplied list of profiles.
        /// </summary>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        /// <param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"/>  of information about profiles that are to be deleted.
        ///                 </param>
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            try
            {
                var names = new List<string>();

                foreach (ProfileInfo profile in profiles)
                {
                    names.Add(profile.UserName);
                }
                return FigaroProfileData.GetFigaroProfileData(ContainerPath).DeleteProfiles(names.ToArray());
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "delete profiles"), ex);
            }
        }

        /// <summary>
        /// Retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user profile information for inactive profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.
        ///                 </param><param name="usernameToMatch">The user name to search for.
        ///                 </param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/> value of a user profile occurs on or before this date and time, the profile is considered inactive.
        ///                 </param><param name="pageIndex">The index of the page of results to return.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">When this method returns, contains the total number of profiles.
        ///                 </param>
        public override ProfileInfoCollection
            FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption,
            string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                var profiles = FigaroProfileData.GetFigaroProfileData(ContainerPath).GetProfile(usernameToMatch);
                var pfs = new ProfileInfoCollection();

                foreach (ProfileInfo profile in profiles)
                {
                    if (profile.LastActivityDate > userInactiveSinceDate) continue;
                    pfs.Add(profile);

                }
                totalRecords = profiles.Count;
                if (null == profiles[usernameToMatch] || profiles[usernameToMatch].LastActivityDate > userInactiveSinceDate)
                {
                    totalRecords = 0;
                    return new ProfileInfoCollection();
                }
                return profiles;
            }
            catch (Exception ex)
            {
                totalRecords = 0;
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "find inactive profiles by user name"), ex);
            }
        }

        /// <summary>
        /// Retrieves profile information for profiles in which the user name matches the specified user names.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.
        ///                 </param><param name="usernameToMatch">The user name to search for.
        ///                 </param><param name="pageIndex">The index of the page of results to return.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">When this method returns, contains the total number of profiles.
        ///                 </param>
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                var profiles = FigaroProfileData.GetFigaroProfileData(ContainerPath).GetProfile(usernameToMatch);
                totalRecords = profiles.Count;
                return profiles;
            }
            catch (Exception ex)
            {
                totalRecords = 0;
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "find profiles by user name"), ex);
            }
        }

        /// <summary>
        /// Retrieves user-profile data from the data source for profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information about the inactive profiles.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.
        ///                 </param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.
        ///                 </param><param name="pageIndex">The index of the page of results to return.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">When this method returns, contains the total number of profiles.
        ///                 </param>
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                return
                    FigaroProfileData.GetFigaroProfileData(ContainerPath).FindPagedInactiveProfiles(
                        authenticationOption, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get all inactive profiles"), ex);
            }

        }

        /// <summary>
        /// Retrieves user profile data for all profiles in the profile container.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for all profiles in the data source.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.
        ///                 </param><param name="pageIndex">The index of the page of results to return.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">When this method returns, contains the total number of profiles.
        ///                 </param>
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                return FigaroProfileData.GetFigaroProfileData(ContainerPath).GetAllProfiles(authenticationOption,
                                                                                            pageIndex, pageSize,
                                                                                            out totalRecords);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get all profiles"), ex);
            }

        }

        /// <summary>
        /// Returns the number of profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <returns>
        /// The number of profiles in which the last activity date occurred on or before the specified date.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.
        ///                 </param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.
        ///                 </param>
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try
            {
                return
                    FigaroProfileData.GetFigaroProfileData(ContainerPath).GetNumberOfInactiveProfiles(
                        authenticationOption, userInactiveSinceDate);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get the number of inactive profiles"), ex);
            }

        }

        /// <summary>
        /// Gets or sets the name of the currently running application.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Returns the collection of settings property values for the specified application instance and settings property group.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> containing the values for the specified settings property group.
        /// </returns>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application use.
        ///                 </param><param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection"/> containing the settings property group whose values are to be retrieved.
        ///                 </param><filterpriority>2</filterpriority>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            try
            {

                var doc = null == context[ProfileResource.ContextUserName] ? null :
                                          FigaroProfileData.GetFigaroProfileData(ContainerPath).GetProfileDocument(
                                              context[ProfileResource.ContextUserName] as string);

                var props = FigaroProfileReader.GetProfileProperties(context, collection, doc);
                return props;
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get property values"), ex);
            }
        }

        /// <summary>
        /// Sets the values of the specified group of property settings.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application usage.
        ///                 </param><param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> representing the group of property settings to set.
        ///                 </param><filterpriority>2</filterpriority>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            //context keys:
            //  - PathUserName
            //  - IsAuthenticated
            try
            {

                if (null == context["UserName"]) return;

                FigaroProfileData.GetFigaroProfileData(ContainerPath).SetPropertyValues(context, collection);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "set property values"), ex);
            }

        }

        /// <summary>
        /// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        /// </summary>
        /// <returns>
        /// A brief, friendly description suitable for display in administrative tools or other UIs.
        /// </returns>
        public override string Description { get { return "Profile provider implementation using the Figaro .NET XML Database"; } }

        /// <summary>
        /// Gets the friendly name used to refer to the provider during configuration.
        /// </summary>
        /// <returns>
        /// The friendly name used to refer to the provider during configuration.
        /// </returns>
        public override string Name { get { return "FigaroProfileProvider"; } }

    }
}
