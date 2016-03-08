using System;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using System.Web;
#if ORACLE_NS
using Figaro.BerkeleyDB;
using Figaro;
#else
using Figaro;
#endif

namespace Figaro.Web.Configuration
{
#if CDS || TDS

    /// <summary>
    /// Creates <see cref="FigaroEnv"/> instances using configuration.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class EnvFactory
    {
        /// <summary>
        /// Create a new <see cref="FigaroEnv"/> instance using the first <see cref="FigaroEnvElement"/> object found in configuration.
        /// </summary>
        /// <returns>A new <see cref="FigaroEnv"/> instance, or <c>null</c> if no configuration instances exist.</returns>
        public static FigaroEnv Create()
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
            return section == null ? null : Create(section.Environments[0]);
        }
        /// <summary>
        /// Create a new <see cref="FigaroEnv"/> instance using the specified <see cref="FigaroEnvElement"/> object.
        /// </summary>
        /// <param name="element">The <see cref="FigaroEnvElement"/> to use.</param>
        /// <returns>A new <see cref="FigaroEnv"/> instance.</returns>
        public static FigaroEnv Create(FigaroEnvElement element)
        {
            var env = new FigaroEnv();
            bool panic = false;

            #region Tracing
            if (element.Tracing != null)
            {                
                if (!string.IsNullOrEmpty(element.Tracing.MessageFile))
                {
                    var msgPath = Path.GetDirectoryName(PathUtility.ResolvePath(element.Tracing.MessageFile));
                    var msgFile = Path.GetFileName(element.Tracing.MessageFile);
                    if (!Directory.Exists(msgPath))
                    {
                        Directory.CreateDirectory(msgPath);
                    }
                    env.SetMessageFile(Path.Combine(msgPath,msgFile));
                }
                if (!string.IsNullOrEmpty(element.Tracing.Category))
                    LogConfiguration.SetCategory(
                        (LogConfigurationCategory)
                        Enum.Parse(typeof(LogConfigurationCategory), element.Tracing.Category.Replace(" ",",")), true);
                if (!string.IsNullOrEmpty(element.Tracing.Level))
                    LogConfiguration.SetLogLevel((LogConfigurationLevel)Enum.Parse(typeof(LogConfigurationLevel), element.Tracing.Level.Replace(" ",",")), true);

                if (!string.IsNullOrEmpty(element.Tracing.ErrorPrefix)) env.ErrorPrefix = element.Tracing.ErrorPrefix;
                if (!string.IsNullOrEmpty(element.Tracing.ErrorFile))
                {
                    var errPath = Path.GetDirectoryName(PathUtility.ResolvePath(element.Tracing.ErrorFile));
                    var errFile = Path.GetFileName(element.Tracing.ErrorFile);

                    if (!Directory.Exists(errPath))
                    {
                        Directory.CreateDirectory(errPath);
                    }
                    env.SetErrorFile(Path.Combine(errPath,errFile));
                }

            }
            #endregion

            #region DataDirectories
            if (element.DataDirectories != null)
            {
                if (element.DataDirectories.Count > 0)
                {
                    foreach (DataDirectoryElement directoryElement in element.DataDirectories)
                    {
                        if (directoryElement.Create && !Directory.Exists(PathUtility.ResolvePath(directoryElement.Path)))
                            Directory.CreateDirectory(PathUtility.ResolvePath(directoryElement.Path));
                        env.AddDataDirectory(PathUtility.ResolvePath(directoryElement.Path));
                    }
                }
            }
            #endregion

            #region Encryption
            if (element.Encryption != null)
            {
                env.SetEncryption(element.Encryption.Password, element.Encryption.Enabled);
            }
            #endregion

            #region EnvConfig
            if (element.EnvConfig != null)
            {
                if (element.EnvConfig.Count > 0)
                {
                    foreach (EnvConfigElement config in element.EnvConfig)
                    {
                        if (config.Setting.ToLower().Contains("panicenvironment"))
                        {
                            panic = true;
                            continue;
                        }
                        env.SetEnvironmentOption((EnvConfig) Enum.Parse(typeof (EnvConfig), config.Setting.Replace(" ",","), true),
                                                 config.Enabled);
                    }
                }
            }

            #endregion

            #region Cache & CacheMax
            if (element.Cache != null) env.SetCacheSize(new EnvCacheSize(element.Cache.GBytes, element.Cache.Bytes), element.Cache.Regions);
            if (element.CacheMax != null) env.SetCacheMax(new EnvCacheSize(element.CacheMax.GBytes, element.CacheMax.Bytes));
            #endregion
            
            #region Locking
            if (element.Locking != null)
            {
                if (!string.IsNullOrEmpty(element.Locking.AutoDetect)) env.DeadlockDetectPolicy = (DeadlockDetectType)Enum.Parse(typeof(DeadlockDetectType), element.Locking.AutoDetect.Replace(" ",","));
                if (element.Locking.LockPartitions > 0) env.SetLockPartitions(element.Locking.LockPartitions);
                if (element.Locking.MaxLockers > 0) env.SetMaxLockers(element.Locking.MaxLockers);
                if (element.Locking.MaxLockObjects > 0) env.SetMaxLockedObjects(element.Locking.MaxLockObjects);
                if (element.Locking.MaxLocks > 0) env.SetMaxLocks(element.Locking.MaxLocks);
                if (element.Locking.Timeout > 0) env.SetTimeout(element.Locking.Timeout, EnvironmentTimeoutType.Lock);
            }
            #endregion

            #region Log
#if TDS
                if (element.Log != null)
                {
                    if (element.Log.Create && !Directory.Exists(element.Log.Directory))
                        Directory.CreateDirectory(PathUtility.ResolvePath(element.Log.Directory));
                    if (element.Log.BufferSize >0) env.SetLogBufferSize(element.Log.BufferSize);
                    if (!string.IsNullOrEmpty(element.Log.Directory)) env.SetLogDirectory(PathUtility.ResolvePath(element.Log.Directory));

                    if (!string.IsNullOrEmpty(element.Log.LogOptions))
                    {
                        var logOptions =
                            (EnvLogOptions) Enum.Parse(typeof (EnvLogOptions), element.Log.LogOptions.Replace(" ",","), true);
                        env.SetLogOptions(logOptions, true);
                    }

                    if (element.Log.MaxFileSize > 0) env.MaxLogSize = element.Log.MaxFileSize;
                    if (element.Log.MaxRegionSize >0) env.SetMaxLogRegion(element.Log.MaxRegionSize);
                }
#endif
                #endregion            

            #region MemoryFileMap
            if (element.MemoryFileMap != null)
            {
                if (element.MemoryFileMap.MaxFile >0) env.SetMaxFileDescriptors(element.MemoryFileMap.MaxFile);
                if (element.MemoryFileMap.MaxFileMapSize > 0) env.MaxFileMapSize = element.MemoryFileMap.MaxFileMapSize;
                if (element.MemoryFileMap.MaxWrite >0 && element.MemoryFileMap.WriteSleep >0) env.SetMaxSequentialWriteOperations(element.MemoryFileMap.MaxWrite,element.MemoryFileMap.WriteSleep);
            }
            #endregion

            #region Mutex
            if (element.Mutex != null)
            {
                if (element.Mutex.Align >0) env.SetMutexAlign(element.Mutex.Align);
                if (element.Mutex.Increment >0) env.SetMutexIncrement(element.Mutex.Increment);
                if (element.Mutex.TasSpinCount >0) env.SetTestAndSetSpinCount(element.Mutex.TasSpinCount);
            }
            #endregion

            #region TempDirectory
            if (element.TempDirectory != null)
            {
                if (!string.IsNullOrEmpty(element.TempDirectory.Path))
                {
                    if (!Directory.Exists(PathUtility.ResolvePath(element.TempDirectory.Path)) && element.TempDirectory.Create)
                        Directory.CreateDirectory(PathUtility.ResolvePath(element.TempDirectory.Path));

                    env.SetTempDirectory(PathUtility.ResolvePath(element.TempDirectory.Path));
                }
            }
            #endregion

            #region ThreadCount
            if (element.ThreadCount > 0)
                env.SetThreadCount(element.ThreadCount);
            #endregion

            #region Transactions
                #if TDS
                    if (element.Transactions != null)
                    {
                        if (element.Transactions.MaxTransactions > 0)
                            env.SetMaxTransactions(element.Transactions.MaxTransactions);

                        if (element.Transactions.Timeout > 0)
                            env.SetTimeout(element.Transactions.Timeout, EnvironmentTimeoutType.Transaction);
                    }
                #endif
            #endregion

            #region Open

            if (element.Open != null)
            {
                EnvOpenOptions openOptions = EnvOpenOptions.None;
                if (!string.IsNullOrEmpty(element.Open.Options))
                    openOptions = (EnvOpenOptions) Enum.Parse(typeof (EnvOpenOptions), element.Open.Options.Replace(" ",","), true);

                if (openOptions == EnvOpenOptions.None && string.IsNullOrEmpty(element.Open.Home))
                    openOptions = EnvOpenOptions.UseEnvironmentRoot | EnvOpenOptions.Create;

                if (element.Open.Create && !Directory.Exists(PathUtility.ResolvePath(element.Open.Home)))
                    Directory.CreateDirectory(PathUtility.ResolvePath(element.Open.Home));
                
                env.Open(PathUtility.ResolvePath(element.Open.Home),openOptions);
            }
            #endregion

            #region post-open
#if TDS
            if (panic)
            {
                env.SetEnvironmentOption(EnvConfig.PanicEnvironment, true);
            }
#endif
            #endregion

            return env;
        }

        /// <summary>
        /// Create a new <see cref="FigaroEnv"/> instance using the specified <see cref="FigaroEnvElement"/> object.
        /// </summary>
        /// <param name="envName">The <see cref="FigaroEnvElement"/> instance to use.</param>
        /// <returns>A new <see cref="FigaroEnv"/> instance, or <c>null</c> if the named instance does not exist.</returns>
        public static FigaroEnv Create(string envName)
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
            foreach (FigaroEnvElement element in section.Environments)
            {
                if (element.Name.Equals(envName))
                    return Create(element);
            }
            
            return null;
        }
    }

#endif
}
