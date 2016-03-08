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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Profile;
using System.Xml;
using System.Xml.Serialization;

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// Helper class for reading in profile records and profile object conversion.
    /// </summary>
    static class FigaroProfileReader
    {
        public static ProfileInfoCollection GetAllProfiles(SettingsContext context, SettingsPropertyCollection collection, XmlResults results)
        {
            var profiles = new ProfileInfoCollection();
            try
            {
                while (results.HasNext())
                {
                    var info = new Dictionary<string, string>();
                    var doc = results.NextDocument();
                    using(var meta = doc.GetMetadataIterator())
                    {
                        info.Add(meta.Name, meta.Value.AsString);
                    }
                   
                    var profile = new ProfileInfo(info["name"], bool.Parse(info[ProfileResource.Anonymous]), DateTime.Parse(info[ProfileResource.LastActivityDate]), DateTime.Parse(info[ProfileResource.LastUpdatedTime]),doc.ToString().Length);                    
                    profiles.Add(profile);
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, ProfileResource.Source);
                if (null != e) throw e;
            }
            finally
            {
                results.Dispose();
            }
            return profiles;
        }

        public static ProfileInfoCollection GetAllProfiles(XmlResults results)
        {
            var w = new Stopwatch();
            w.Start();
            var profiles = new ProfileInfoCollection();
            try
            {
                while (results.HasNext())
                {
                    profiles.Add(GetProfile(results.NextDocument()));
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, ProfileResource.Source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                w.Stop();
                Trace("[FigaroProfileReader] GetAllProfiles completed in {0} seconds.", w.Elapsed.TotalSeconds);
            }
            return profiles;
        }

        public static SettingsPropertyValueCollection GetFigaroProfileProperties(SettingsContext context, SettingsPropertyCollection properties, XmlDocument doc)
        {
            var svc = new SettingsPropertyValueCollection();
            if (properties.Count >= 1)
            {
                foreach (SettingsProperty property in properties)
                {
                    if (property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                    {
                        if (property.PropertyType.IsPrimitive || (property.PropertyType == typeof(string)))
                        {
                            property.SerializeAs = SettingsSerializeAs.String;
                        }
                        else
                        {
                            property.SerializeAs = SettingsSerializeAs.Xml;
                        }
                    }
                    svc.Add(new SettingsPropertyValue(property));
                }
                if (null != doc)
                {
                    var reader = doc.GetContentAsXmlReader();
                    try
                    {
                        reader.Read();
                        //read (and read over) the root node
                        if (reader.EOF) return svc;
                        reader.ReadStartElement(ProfileResource.RootNode);
                        while (reader.IsStartElement("Property"))
                        {
                            var attrs = new Dictionary<string, string>();

                            for (int i = 0; i < reader.AttributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                attrs.Add(reader.Name, reader.ReadContentAsString());
                            }
                            var ps = new SettingsProperty(attrs["propertyName"])
                                         {
                                             PropertyType = Type.GetType(attrs["propertyType"]),
                                             SerializeAs =
                                                 (SettingsSerializeAs)
                                                 Enum.Parse(typeof (SettingsSerializeAs), attrs["serializeAs"])
                                         };

                            var val = new SettingsPropertyValue(ps);
                            reader.MoveToContent();
                            val.SerializedValue = reader.ReadContentAsObject();
                            svc.Add(val);
                            reader.ReadElementString("Property");
                        }
                        return svc;
                    }
                    catch (Exception ex)
                    {
                        var e = ExceptionHandler.HandleException(ex, ProfileResource.Source);
                        if (null != e) throw e;
                    }
                    finally
                    {
                        doc.Dispose();
                        reader.Close();
                    }
                }
            }
            return svc;            
        }

        public static ProfileInfoCollection ResultsToProfiles(XmlResults results)
        {
            var profiles = new ProfileInfoCollection();
            while (results.HasNext())
            {
                profiles.Add(GetProfile(results.NextDocument()));
            }
            return profiles;
        }

        public static ProfileInfo GetProfile(XmlDocument doc)
        {
            var w = new Stopwatch();
            w.Start();
            try
            {
                var info = new Dictionary<string, string>();
                using (var meta = doc.GetMetadataIterator())
                {
                    info.Add(meta.Name, meta.Value.AsString);
                }

                return new ProfileInfo(info["name"], bool.Parse(info[ProfileResource.Anonymous]), DateTime.Parse(info[ProfileResource.LastActivityDate]), DateTime.Parse(info[ProfileResource.LastUpdatedTime]), doc.ToString().Length);                    
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, ProfileResource.Source);
                if (null != e) throw e;
            }
            finally
            {
                w.Stop();
                Trace("[FigaroProfileReader] GetProfile completed in {0} seconds.", w.Elapsed.TotalSeconds);
            }
            return null;
        }
        
        public static SettingsPropertyValueCollection GetProfileProperties(SettingsContext context, SettingsPropertyCollection collection, XmlDocument doc)
        {
            var settingsProps = new SettingsPropertyValueCollection();
            foreach (SettingsProperty property in collection)
            {
                var value = new SettingsPropertyValue(property);
                settingsProps.Add(value);
            }

            if (doc != null)
            {
                var sr = new StringReader(doc.ToString());
                var reader = new XmlTextReader(sr);

                try
                {
                    reader.Read();
                    //read (and read over) the root node
                    if (reader.EOF) return settingsProps;
                    reader.ReadStartElement();
                    while (reader.IsStartElement("Property"))
                    {
                        var attrs = new Dictionary<string, string>();

                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            reader.MoveToAttribute(i);
                            attrs.Add(reader.Name, reader.ReadContentAsString());
                        }
                        reader.MoveToContent();
                        
                        var propName = attrs["name"];
                        if (settingsProps[propName] == null) continue;

                        switch(settingsProps[propName].Property.SerializeAs)
                        {                                    
                            case SettingsSerializeAs.Xml:
                                {                                    
                                    var stringReader = new StringReader(reader.ReadContentAsString());
// ReSharper disable once AssignNullToNotNullAttribute
                                    settingsProps[propName].PropertyValue = new XmlSerializer(Type.GetType(attrs["propertyType"])).Deserialize(stringReader);
                                    break;                                    
                                }
                            case SettingsSerializeAs.Binary:
                                {
                                    var buf = Convert.FromBase64String(reader.ReadElementContentAsString());
                                    var ms = new MemoryStream(buf);
                                    //settingsProps[propName].Property.PropertyType = Type.GetType(attrs["propertyType"],true);
                                    settingsProps[propName].PropertyValue = new BinaryFormatter().Deserialize(ms);
                                    ms.Dispose();                                    
                                    break;
                                }
                            default:
                                {
                                    settingsProps[propName].PropertyValue = reader.ReadElementContentAsString();
                                    break;
                                }                                
                        }
                    }
                    return settingsProps;
                }
                catch (Exception ex)
                {
                    var e = ExceptionHandler.HandleException(ex, ProfileResource.Source);
                    if (null != e) throw e;
                }
                finally
                {
                    doc.Dispose();
                    reader.Close();
                }
            }
            return settingsProps;
        }

        static void Trace(string message, params object[] args)
        {
            TraceHelper.Write("FigaroProfileProvider", "[FigaroProfileReader] " + message, args);
        }
    }
}
