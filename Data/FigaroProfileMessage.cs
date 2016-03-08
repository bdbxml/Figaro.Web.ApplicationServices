/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
#define FILE
using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// Provides the structure of the XML message making up the user profile.
    /// </summary>
    internal sealed class FigaroProfileMessage
    {
        private readonly MemoryStream ms;
        private readonly XmlWriter writer;
        private bool saved;
        public FigaroProfileMessage(){}

        public FigaroProfileMessage(SettingsContext context)
        {
            ms = new MemoryStream();
            writer = XmlWriter.Create(ms);
            if (writer == null)
                throw new InvalidOperationException("Could not successfully create an XML writer object.");
            writer.WriteStartDocument();
            writer.WriteStartElement(ProfileResource.RootNode,ProfileResource.xmlns);
            
            //save it for now -- these values are going into the metadata.
            Context = context;
            //foreach (var key in context.Keys)
            //{
            //    writer.WriteAttributeString(key as string, context[key] as string);
            //}
        }

        public void WritePropertyValue(SettingsPropertyValue value)
        {
            writer.WriteStartElement("Property");
            writer.WriteAttributeString("name", value.Name);
            writer.WriteAttributeString("serializeAs", value.Property.SerializeAs.ToString());
            writer.WriteAttributeString("propertyType",value.Property.PropertyType.ToString());
            writer.WriteAttributeString("usingDefault",value.UsingDefaultValue.ToString());
            //writer.WriteAttributeString("dirty",value.IsDirty.ToString());
            if (value.SerializedValue != null)
            {
                if (value.Property.SerializeAs == SettingsSerializeAs.Binary)
                {
                    var bytes = (byte[]) value.SerializedValue;
                    writer.WriteBase64(bytes, 0, bytes.Length);
                }
                else if (value.Property.SerializeAs == SettingsSerializeAs.Xml)
                {
                    writer.WriteCData(value.SerializedValue as string);
                }
                else
                    writer.WriteValue(value.SerializedValue);                
            }

            writer.WriteEndElement(); 
        }

        public void SaveMessage()
        {
            if (!saved)
            {
                writer.Flush();
                writer.Close();
                ms.Seek(0, SeekOrigin.Begin);
#if SAVE_FILE_COPY
                //save a file copy
                var fw =
                    new FileStream(@"D:\dev\FigaroProductTest\Figaro.Web\Figaro.Web.Test\profileData\" + userName +
                                   ".xml",FileMode.Create);
                fw.Write(ms.ToArray(),0,ms.ToArray().Length);
                fw.Flush();
                fw.Close();
                fw.Dispose();
                ///////
#endif                
                var stm2 = new MemoryStream(ms.ToArray());
                Reader = XmlReader.Create(stm2);
                saved = true;
            }
        }

        public XmlReader Reader { get; private set; }
        public SettingsContext Context { get; private set; }

        ~FigaroProfileMessage()
        {
            if (null != Reader) Reader.Close();
            if (null != ms) ms.Dispose();
        }
    } 
}
