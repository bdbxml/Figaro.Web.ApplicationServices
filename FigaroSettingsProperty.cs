/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Configuration;

namespace Figaro.Web.ApplicationServices
{
    [Serializable]
    internal class FigaroSettingsProperty: SettingsProperty
    {
        public FigaroSettingsProperty() : base("FigaroSettingsProperty"){}
        public FigaroSettingsProperty(string propertyName) : base(propertyName){}
        public FigaroSettingsProperty(SettingsProperty propertyToCopy) : base(propertyToCopy){}
        

        public string UserName{ get; set;}
        public bool UserAuthenticated { get; set; }
        public string Version { get; set; }

    }
}
