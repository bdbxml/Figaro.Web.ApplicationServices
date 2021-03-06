﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Figaro.Web.ApplicationServices {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ProviderResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ProviderResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Figaro.Web.ApplicationServices.ProviderResource", typeof(ProviderResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to let $prop := for $x in doc(&quot;dbxml:/profile/{0}&quot;)/FigaroUserProfile/Property where $x/@name = $name return $x
        ///return (if (exists($prop)) then 
        ///	replace value of node $prop with {1}
        ///else
        ///	insert nodes {1} as last into doc(&quot;dbxml:/profile/{0}&quot;)/FigaroUserProfile).
        /// </summary>
        internal static string PropertyUpsert {
            get {
                return ResourceManager.GetString("PropertyUpsert", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to let $app := for $x in doc(&apos;dbxml:/role/{0}&apos;)/FigaroRole/Apps/App where xs:string($x) = $name return $x
        ///return (if (not(exists($app))) then 
        ///	insert nodes &lt;App&gt;$name&lt;/App&gt; as last into doc(&apos;dbxml:/role/{0}&apos;)/FigaroRole/Apps
        ///	else replace value of node $app with $name
        ///	).
        /// </summary>
        internal static string QueryRoleAppNameUpsert {
            get {
                return ResourceManager.GetString("QueryRoleAppNameUpsert", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; ?&gt;
        ///&lt;FigaroRole xmlns =&quot;http://schemas.bdbxml.net/web/role/2009/05/&quot;&gt;
        ///&lt;Apps&gt;&lt;/Apps&gt;
        ///	&lt;Users&gt;
        ///	&lt;/Users&gt;
        ///&lt;/FigaroRole&gt;.
        /// </summary>
        internal static string RoleRecord {
            get {
                return ResourceManager.GetString("RoleRecord", resourceCulture);
            }
        }
    }
}
