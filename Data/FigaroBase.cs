using Figaro.Web.Annotations;
// ReSharper disable AssignNullToNotNullAttribute
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
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Figaro.Configuration;

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// Base class for Figaro data access management.
    /// </summary>
    class FigaroBase
    {
        protected FigaroBase(){}

        protected XmlSerializer serializer;
        private Stopwatch watch;
        /// <summary>
        /// Exception Handler source variable.
        /// </summary>
        protected static string        source;
        /// <summary>
        /// The path and name of the container to access.
        /// </summary>
        protected string containerPath;
        /// <summary>
        /// The directory containing the container.
        /// </summary>
        protected string containerDirectory;
        /// <summary>
        /// The name of the container.
        /// </summary>
        [UsedImplicitly] protected string containerName;
        /// <summary>
        /// Designated manager object.
        /// </summary>
        protected XmlManager    mgr;
        /// <summary>
        /// The container to manage.
        /// </summary>
        protected Container     container;

        protected bool initialized;

        [UsedImplicitly] protected QueryContext context;

        protected readonly Dictionary<string, QueryUnit> catalog;

        /// <summary>
        /// Initialize the Figaro subsystem and open (or create) the container. 
        /// </summary>
        /// <param name="source">The name of the object created; used by the <see cref="ExceptionHandler"/>.</param>
        /// <param name="containerPath">The path and name of the container to access.</param>
        /// <param name="managerName">The name of the <see cref="XmlManager"/> from configuration.</param>
        /// <param name="ns">The namespace used by the XML data object in the serializer.</param>
        /// <param name="serializeAs">The <see cref="Type"/> of object being serialized to XML.</param>
        /// <param name="containerAlias">The alias to assign to the container for XQuery access.</param>
        public FigaroBase(string source, string containerPath, string managerName, Type serializeAs, string ns, string containerAlias)
        {
            try
            {
                Init(source, containerPath, managerName, serializeAs, ns, containerAlias);
                catalog = new Dictionary<string, QueryUnit>();
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
        }

        /// <summary>
        /// Initialize the Figaro subsystem and open (or create) the container. 
        /// </summary>
        /// <param name="source">The name of the object created; used by the <see cref="ExceptionHandler"/>.</param>
        /// <param name="containerPath">The path and name of the container to access.</param>
        /// <param name="ns">The namespace used by the XML data object in the serializer.</param>
        /// <param name="serializeAs">The <see cref="Type"/> of object being serialized to XML.</param>
        /// <param name="containerAlias">The alias to assign to the container for XQuery access.</param>
        public FigaroBase(string source, string containerPath, Type serializeAs, string ns, string containerAlias)
        {
            try
            {
                Init(source, containerPath, serializeAs, ns, containerAlias);
                catalog = new Dictionary<string, QueryUnit>();
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
        }

        /// <summary>
        /// Initialize the data access layer.
        /// </summary>
        /// <param name="sourceObj">the name of the accessing object, used for diagnostic purposes.</param>
        /// <param name="managerName">The name of the <see cref="XmlManager"/> from configuration.</param>
        /// <param name="sourceContainerPath">The container path.</param>
        /// <param name="serializeAs">The <see cref="Type"/> to set the <see cref="XmlSerializer"/> to.</param>
        /// <param name="ns">the namespace used in the serialized object.</param>
        /// <param name="containerAlias">the default alias name for the container, used in the XQuery queries.</param>
        public void Init(string sourceObj, string sourceContainerPath, string managerName, Type serializeAs, string ns, string containerAlias)
        {
            try
            {
                var cs = new ContainerConfig
                {
                    AllowCreate = true,
                    CompressionEnabled = false,
                    ContainerType = XmlContainerType.NodeContainer,
                    IndexNodes = ConfigurationState.On,
                    PageSize = 8192,
#if TDS
                    Transactional = false,
#endif
                    Statistics = ConfigurationState.Off
                };

                watch = new Stopwatch();
                StartTimer();
                //we may be calling by mistake
                if (container != null || mgr != null) return;
                if (null != serializeAs)
                    serializer = new XmlSerializer(serializeAs, ns);

                source = sourceObj;
                containerPath = ResolveContainerPath(sourceContainerPath);
                containerName = Path.GetFileName(sourceContainerPath);
                containerDirectory = Path.GetDirectoryName(containerPath);

                try
                {
                    if (string.IsNullOrEmpty(containerDirectory)) return;
                    if (!Directory.Exists(containerDirectory))
                        throw new DirectoryNotFoundException(string.Format("Path does not exist: {0}", containerPath));
                    mgr = string.IsNullOrEmpty(managerName)
                        ? new XmlManager(ManagerInitOptions.AllowAutoOpen)
                        : ManagerFactory.Create(managerName);
                    container = mgr.OpenContainer(containerPath, cs, XmlContainerType.NodeContainer);
                    context = mgr.CreateQueryContext(EvaluationType.Eager);
                    container.AddAlias(containerAlias);
                    initialized = true;
                }
                catch (Exception ex)
                {
                    var e = ExceptionHandler.HandleException(ex, sourceObj);
                    if (null != e) throw e; else throw;
                }
                finally
                {
                    StopTimer(sourceObj);
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, sourceObj);
                if (null != e) throw e;
            }
        }

        /// <summary>
        /// Initialize the data access layer.
        /// </summary>
        /// <param name="sourceObj">the name of the accessing object, used for diagnostic purposes.</param>
        /// <param name="sourceContainerPath">The container path.</param>
        /// <param name="serializeAs">The <see cref="Type"/> to set the <see cref="XmlSerializer"/> to.</param>
        /// <param name="ns">the namespace used in the serialized object.</param>
        /// <param name="containerAlias">the default alias name for the container, used in the XQuery queries.</param>
        public void Init(string sourceObj, string sourceContainerPath, Type serializeAs, string ns, string containerAlias)
        {
            try
            {
                                                            
                var cs = new ContainerConfig
                             {
                                 AllowCreate = true,
                                 CompressionEnabled = false,
                                 ContainerType = XmlContainerType.NodeContainer,
                                 IndexNodes = ConfigurationState.On,
                                 PageSize = 8192,
#if TDS
                                 Transactional = false,
#endif
                                 Statistics = ConfigurationState.Off
                             };

                watch = new Stopwatch();
                StartTimer();
                //we may be calling by mistake
                if (container != null || mgr != null) return;
                if (null != serializeAs)
                    serializer = new XmlSerializer(serializeAs, ns);

                source = sourceObj;
                containerPath = ResolveContainerPath(sourceContainerPath);
                containerName = Path.GetFileName(sourceContainerPath);
                containerDirectory = Path.GetDirectoryName(containerPath);

                try
                {
                    if (!Directory.Exists(containerDirectory))
                        throw new DirectoryNotFoundException(string.Format("Path does not exist: {0}", containerPath));

                    mgr = new XmlManager(ManagerInitOptions.AllowAutoOpen);
                    container = mgr.OpenContainer(containerPath, cs, XmlContainerType.NodeContainer);                    
                    context = mgr.CreateQueryContext(EvaluationType.Eager);
                    container.AddAlias(containerAlias);
                    initialized = true;
                }
                catch (Exception ex)
                {
                    var e = ExceptionHandler.HandleException(ex, sourceObj);
                    if (null != e) throw e; else throw;
                }
                finally
                {
                    StopTimer(sourceObj);
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, sourceObj);
                if (null != e) throw e;
            }
        }

        protected ulong RecordCount()
        {
#if TDS
            ulong num;
            if (container.Transactional)
            {
                var trans = mgr.CreateTransaction();
                num = container.GetNumDocuments(trans);
                trans.Commit();
            }
            else
            {
                num = container.GetNumDocuments();
            }
#else
            var num = container.GetNumDocuments();
#endif
            return num;
        }

        /// <summary>
        /// Make sure we're getting an absolute container path.
        /// </summary>
        /// <param name="path">The container path to check.</param>
        /// <returns>An absolute path.</returns>
        protected string ResolveContainerPath(string path)
        {
            //if the path contains a colon (':') then it must be absolute, 
            //otherwise treat it as relative
            if (path.Contains(@":\")) return Path.GetFullPath(path);
 
            var p = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,path));
            return p.Replace(@"~\", string.Empty);
        }

        /// <summary>
        /// Start the timer so we can see how fast our operations are performing.
        /// </summary>
        protected void StartTimer()
        {
            if (null == watch) watch = new Stopwatch();
            watch.Start();
        }
        /// <summary>
        /// Stop the timer and trace the output.
        /// </summary>
        /// <param name="timedAction">The name of the timed action.</param>
        protected void StopTimer(string timedAction)
        {
            watch.Stop();
            Trace("{0}.{1} completed in {2} seconds ({3} ms).",source,timedAction,watch.Elapsed.TotalSeconds,watch.Elapsed.TotalMilliseconds);
            watch.Reset();
        }
        /// <summary>
        /// Write our trace output to the <see cref="TraceHelper"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="args">The arguments used in the <paramref name="message"/> argument, if any.</param>
        protected static void Trace(string message, params object[] args)
        {
            TraceHelper.Write(source,message, args);
        }

        /// <summary>
        /// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
        /// </summary>
        ~FigaroBase()
        {
            if (null != container)
            {
                if (container.ContainerState != ContainerState.Closed)
                {
                    container.Sync();
                }
                container.Dispose();
            }

            if (null == mgr) return;
            mgr.Dispose();
            mgr = null;
        }
    }    
}
