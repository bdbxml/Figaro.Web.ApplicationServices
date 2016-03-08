/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.IO;
using System.Web.Management;
using System.Xml.Serialization;

namespace Figaro.Web.ApplicationServices.Data
{
    class WebEventData: FigaroBase
    {
        public WebEventData(string containerPath, string managerName, Type serializeAs) : 
            base("FigaroWebEventProvider",containerPath,managerName,serializeAs,WebEventResource.xmlns,WebEventResource.alias)
        {            
        }

        public void Flush()
        {
            container.Sync();
        }

        public void Shutdown()
        {
            container.Dispose();
        }

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public void ProcessEvent(WebBaseEvent theEvent)
        {
            var ser = new XmlSerializer(theEvent.GetType(), WebEventResource.xmlns);
            StartTimer();
            var ms = new MemoryStream();
            ser.Serialize(ms,theEvent);
            ms.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(ms);
            var str = sr.ReadToEnd();

            var doc = mgr.CreateDocument();
            doc.SetContent(str);
            doc.Name = theEvent.EventID.ToString("D");
            doc.SetMetadata("FigaroWebEvent","SerializedTypeName",new XmlValue(theEvent.GetType().ToString()));
            container.PutDocument(doc,mgr.CreateUpdateContext());
            StopTimer("ProcessEvent");
        }
    }
}
