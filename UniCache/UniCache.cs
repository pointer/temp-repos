using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Xml;
using System.Net;
using Microsoft.Win32;
using System.ComponentModel;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Delegate defining the signature of an OnRefreshComplete event handler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RefreshCompleteEventArgs" /> instance containing the event data.</param>
    [ComVisible(false)]
    public delegate void RefreshCompleteHandler(object sender, RefreshCompleteEventArgs e);

    /// <summary>
    /// Delegate defining the signature of an OnCacheElementLoaded event handler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="CacheElementLoadedEventArgs" /> instance containing the event data.</param>
    [ComVisible(false)]
    public delegate void CacheElementLoadedHandler(object sender, CacheElementLoadedEventArgs e);

    // UniCache class is a partial class devided into two files:
    // + UniCache.cs: Contains caching logic
    // + UniCacheCreate.cs: Contains methods to create UniCache-instances (support of UniCacheFactory)


    /// <summary>
    /// Caches SharePoint server files and makes them available on the client
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.IUniCache" />
    /// <seealso cref="System.Collections.IEnumerable" />
    [ComVisible(true)]
    [Guid("A0D1F20A-2AC5-4b67-ABCD-4C4986AD644F")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IUniCacheEvents))]
    [ProgId("UniCacheLib.UniCache")]
    public partial class UniCache : IUniCache, IUniCacheTestData, IEnumerable
    {

        /// <summary>
        /// Gets the version of UniCacheLib.dll.
        /// </summary>
        /// <value>
        /// The version of UniCacheLib.dll.
        /// </value>
        public String Version
        {
            get { return "V1.5.0.2"; }
        }


        /// <summary>
        /// Key for SharePoint server in production
        /// </summary>
        [Obsolete("ServerMOSS is deprecated, use ServerPROD instead.")]
        public static String ServerMOSS = ServerInfos.ServerPROD;

        /// <summary>
        /// Key for SharePoint server in QA (test-environment)
        /// </summary>
        [Obsolete("ServerMOSSQC is deprecated, use ServerQA instead.")]
        public static String ServerMOSSQC = ServerInfos.ServerQA;


        /// <summary>
        /// Key for SP server in production
        /// </summary>
        public static String ServerPROD = ServerInfos.ServerPROD;

        /// <summary>
        /// Key for SP server in QA (test-environment)
        /// </summary>
        public static String ServerQA = ServerInfos.ServerQA;


        /// <summary>
        /// Key for SP server in dev-environment
        /// </summary>
        public static String ServerDEV = ServerInfos.ServerDEV;


        /// <summary>
        /// Is raised when background-refresh of UniCache is completed (<see cref="AsyncRefresh()"/>).
        /// </summary>
        public event RefreshCompleteHandler OnRefreshComplete;

        /// <summary>
        /// Is raised when a (missing) file has been loaded from the server.
        /// </summary>
        public event CacheElementLoadedHandler OnCacheElementLoaded;

        /// <summary>
        /// The initialisation log (used in UniCacheTestConfig)
        /// </summary>
        private StringBuilder m_InitLog;
        
        /// <summary>
        /// Gets the initialisation log.
        /// </summary>
        /// <value>
        /// The initialisation log.
        /// </value>
        public String InitLog
        {
            get { return m_InitLog != null ? m_InitLog.ToString() : String.Empty; }

        }

        /// <summary>
        /// Logger used to log creation and usage of this UniCache-instance
        /// </summary>
        private Logger m_UniCacheLogger;
        
        /// <summary>
        /// Gets the logger for this UniCache-instance.
        /// </summary>
        /// <value>
        /// The logger for this UniCache-instance.
        /// </value>
        internal Logger UniCacheLogger
        {
            get { return m_UniCacheLogger; }
        }

        /// <summary>
        /// Gets the logs filename (used in test tool).
        /// </summary>
        /// <value>
        /// The log filename.
        /// </value>
        public String UniCacheLogFilename
        {
            get { return m_UniCacheLogger.LogFilename; }
        }

        /// <summary>
        /// Gets or sets the default timeout for a single server request.
        /// </summary>
        /// <value>
        /// The default timeout.
        /// </value>
        public Int32 DefaultTimeout
        {
            get 
            { 
                return m_SharePointHelper == null ? -1 : m_SharePointHelper.DefaultTimeout; 
            }
            set 
            {
                if (m_SharePointHelper != null) m_SharePointHelper.DefaultTimeout = value; 
            }
        }

        /// <summary>
        /// Indicates whether this instance is offline, e.g. uses cached data on client without having connected to the server.
        /// </summary>
        Boolean m_IsOffline;

        /// <summary>
        /// Gets a value indicating whether this instance is offline, e.g. uses cached data on client without having connected to the server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsOffline
        {
            get { return m_IsOffline; }
        }

        /// <summary>
        /// The lock object to assert thread safety setting the status
        /// </summary>
        private Object m_StatusLockObject = new Object();
        
        /// <summary>
        /// The status of this UniCache-instance
        /// </summary>
        private volatile CacheStatus m_Status;
        
        /// <summary>
        /// Gets the status of this UniCache-instance
        /// </summary>
        /// <value>
        /// The status of this UniCache-instance.
        /// </value>
        public CacheStatus Status
        {
            get
            {
                CacheStatus cs;
                lock (m_StatusLockObject)   // thread safety -> getting/setting Status
                {
                    cs = m_Status;
                }
                return cs;
            }
            private set
            {
                lock (m_StatusLockObject)   // thread safety -> getting/setting Status
                {
                    m_Status = value;
                }
            }
        }

        /// <summary>
        /// The lock object to assert thread safety setting the property <see cref="AbortRefreshing"/>
        /// </summary>
        private Object m_AbortRefreshingLockObject = new Object();
        
        /// <summary>
        /// Will be set to true if an asynchronous refresh is to be aborted.
        /// </summary>
        private volatile Boolean m_AbortRefreshing;
        
        /// <summary>
        /// Gets or sets a value requesting an asynchronous refresh to be aborted.
        /// </summary>
        /// <value>
        /// <c>true</c> if asynchronous refresh should be aborted; otherwise, <c>false</c>.
        /// </value>
        public Boolean AbortRefreshing
        {
            get
            {
                Boolean abort;
                lock (m_AbortRefreshingLockObject)  // thread safety -> getting/setting AbortRefreshing
                {
                    abort = m_AbortRefreshing;
                }
                return abort;
            }
            set
            {
                if (Status == CacheStatus.Refreshing || value == false)
                {
                    lock (m_AbortRefreshingLockObject)  // thread safety -> getting/setting AbortRefreshing
                    {
                        m_AbortRefreshing = value;
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="SharePointHelper"/>-instance to be used to download/upload files from/to the SharePoint-server 
        /// </summary>
        private SharePointHelper m_SharePointHelper;
        
        /// <summary>
        /// Gets the <see cref="SharePointHelper"/>-instance used by this <see cref="UniCache"/>-instance.
        /// </summary>
        /// <value>
        /// The <see cref="SharePointHelper"/>-instance used by this <see cref="UniCache"/>-instance.
        /// </value>
        public SharePointHelper SharePointHelper
        {
            get { return m_SharePointHelper; }
        }

        /// <summary>
        /// The URL of a SharePoint document library where the UniCache-files are located (within application related subfolders). A typical value is https://sp2013apps.intranet.unicredit.eu/sites/servo/UniCache.
        /// </summary>
        private String m_UniCacheUrl;

        /// <summary>
        /// The name of the application this <see cref="UniCache"/>-instance is working for. 
        /// </summary>
        private String m_UniCacheApplicationName;
        
        /// <summary>
        /// Gets the name of the application this <see cref="UniCache"/>-instance is working for. Typical values are "AllKos", "UniLogoPrint", "UniLogoSelect", "UniOffice", etc.
        /// </summary>
        /// <value>
        /// The name of name of the application this <see cref="UniCache"/>-instance is working for.
        /// </value>
        /// <remarks>
        /// The application name is part of the path where the cached files are located.
        /// </remarks>
        internal String UniCacheApplicationName
        {
            get { return m_UniCacheApplicationName; }
        }

        /// <summary>
        /// The URL of the applications SharePoint folder where the files to be cached are located.
        /// </summary>
        private String m_ApplicationsCacheUrl;

        /// <summary>
        /// Gets the URL of the applications SharePoint folder where the files to be cached are located. A typical value is https://sp2013apps.intranet.unicredit.eu/sites/servo/UniCache/UniLogoPrint.
        /// </summary>
        /// <value>
        /// The URL of the applications SharePoint folder where the files to be cached are located.
        /// </value>
        public String ApplicationsCacheUrl
        {
            get { return m_ApplicationsCacheUrl; }
        }

        /// <summary>
        /// Gets the URL of the applications content-xml. A typical value is https://sp2013apps.intranet.unicredit.eu/sites/servo/UniCache/UniLogoPrint/content.xml.
        /// </summary>
        /// <value>
        /// The  URL of the applications content-xml.
        /// </value>
        internal String ApplicationsCacheContentUrl
        {
            get { return m_ApplicationsCacheUrl + Config.ContentXmlName; }
        }

        /// <summary>
        /// The root folder path on the client where the UniCache application folders are located. A typical value is C:\Users\Public\IPGM\UniCache.
        /// </summary>
        private String m_UniCacheRootFolder;

        /// <summary>
        /// The folder path on the client where the cached application files are located.
        /// </summary>
        private String m_ApplicationsCacheTempFolder;
        
        /// <summary>
        /// Gets the folder path where the cached application files are located. A typical value is <c>C:\Users\Public\IPGM\UniCache\UniLogoPrint</c>.
        /// </summary>
        /// <value>
        /// The folder path where the cached application files are located.
        /// </value>
        public String ApplicationsCacheTempFolder
        {
            get { return m_ApplicationsCacheTempFolder; }
        }

        /// <summary>
        /// The <see cref="UniCachedContentXml"/>-instance representing the applications content-xml on the client
        /// </summary>
        private UniCachedContentXml m_UniCachedContentXml;
        /// <summary>
        /// Gets the <see cref="UniCachedContentXml"/>-instance representing the applications content-xml on the client.
        /// </summary>
        /// <value>
        /// The <see cref="UniCachedContentXml"/>-instance representing the applications content-xml on the client.
        /// </value>
        internal UniCachedContentXml UniCachedContentXml
        {
            get { return m_UniCachedContentXml; }
        }


        /// <summary>
        /// The <see cref="UniCacheElements"/>-instance providing the collection of <see cref="UniCacheElement"/>-instances.
        /// </summary>
        private UniCacheElements m_UniCacheElements;
        
        /// <summary>
        /// Gets the <see cref="UniCacheElements"/>-instance.
        /// </summary>
        /// <value>
        /// The <see cref="UniCacheElements"/>-instance.
        /// </value>
        internal UniCacheElements UniCacheElements
        {
            get { return m_UniCacheElements; }
        }

        /// <summary>
        /// Gets the last update time of this <see cref="UniCacheElements"/>-instance.
        /// </summary>
        /// <value>
        /// The last update time which is the last-write-time of the cached content-xml.
        /// </value>
        public DateTime LastUpdateTime
        {
            get
            {
                return m_UniCachedContentXml.LastUpdateTime;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UniCache" /> class.
        /// </summary>
        ~UniCache()
        {
            FlushLog();
        }

        /// <summary>
        /// Flushes the log.
        /// </summary>
        public void FlushLog()
        {
            try
            {
                if (m_UniCacheLogger != null) m_UniCacheLogger.Flush();
            }
            catch { };
        }

        #region Constructors

        /// <summary>
        /// Creates synchronously an <see cref="UniCache"/> for a given application using default SharePoint as server (with init-mode <see cref="InitMode.AutoInit"/>).
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        public UniCache(String applicationname)
            : this(applicationname, InitMode.AutoInit, -1, ServerInfos.GetServerInfo(ServerInfos.ServerDefault))
        {
        }

        /// <summary>
        /// Creates synchronously an <see cref="UniCache"/> for a given application and server (with init-mode <see cref="InitMode.AutoInit"/>).
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="servername">Name of server UniCache should get files from</param>
        public UniCache(String applicationname, String servername)
            : this(applicationname, InitMode.AutoInit, -1, ServerInfos.GetServerInfo(servername))
        {
        }

        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application and init-mode using default SharePoint as server.
        /// </summary>
        /// <remarks>Default SharePoint is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment</remarks>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        public UniCache(String applicationname, InitMode initMode)
            : this(applicationname, initMode, -1, ServerInfos.GetServerInfo(ServerInfos.ServerDefault))
        {
        }

        /// <summary>
        /// Creates synchronously an <see cref="UniCache"/> for a given application and timeout using default SharePoint as server.
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously. If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public UniCache(String applicationname, Int32 asyncTimeoutMilliseconds)
            : this(applicationname, InitMode.AutoInit, asyncTimeoutMilliseconds, ServerInfos.GetServerInfo(ServerInfos.ServerDefault))
        {
        }

        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode and timeout using default SharePoint as server.
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously. If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public UniCache(String applicationname, InitMode initMode, Int32 asyncTimeoutMilliseconds)
            : this(applicationname, initMode, asyncTimeoutMilliseconds, ServerInfos.GetServerInfo(ServerInfos.ServerDefault))
        {
        }

        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode, timeout and server.
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously. If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <param name="servername">Name of server UniCache should get files from</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public UniCache(String applicationname, InitMode initMode, Int32 asyncTimeoutMilliseconds, String servername)
            : this(applicationname, initMode, asyncTimeoutMilliseconds, ServerInfos.GetServerInfo(servername))
        {
        }


        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode and server.
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="servername">Name of server UniCache should get files from</param>
        public UniCache(String applicationname, InitMode initMode, String servername)
            : this(applicationname, initMode, -1, ServerInfos.GetServerInfo(servername))
        {
        }

        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode, timeout and server (<see cref="ServerInfo"/>).
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously. If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <param name="serverinfo"><see cref="ServerInfo"/> describing from which server UniCache should get files from</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public UniCache(String applicationname, InitMode initMode,
                        Int32 asyncTimeoutMilliseconds, ServerInfo serverinfo)
        {
            InitUniCache(applicationname, initMode, asyncTimeoutMilliseconds, serverinfo);
        }

        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode, timeout and server (<see cref="ServerInfo"/>).
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously. If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <param name="serverinfo"><see cref="ServerInfo"/> describing from which server UniCache should get files from</param>
        /// <param name="defaultTimeoutServerRequest">The timeout for a single server request.</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public UniCache(String applicationname, InitMode initMode, Int32 asyncTimeoutMilliseconds, 
                        ServerInfo serverinfo, Int32 defaultTimeoutServerRequest)
        {
            InitUniCache(applicationname, initMode, asyncTimeoutMilliseconds, serverinfo, defaultTimeoutServerRequest);
        }

        #endregion Constructors


        #region Initializing of UniCache

        /// <summary>
        /// Initializes this <see cref="UniCache"/>-instance.
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously.
        /// If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <param name="serverinfo"><see cref="ServerInfo"/> describing from which server UniCache should get files from</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        private void InitUniCache(String applicationname, InitMode initMode, Int32 asyncTimeoutMilliseconds,
                                  ServerInfo serverinfo)
        {
            InitUniCache(applicationname, initMode, asyncTimeoutMilliseconds, serverinfo, Config.GlobalConfig.DefaultTimeoutServerRequest);
        }

        /// <summary>
        /// Initializes this <see cref="UniCache"/>-instance.
        /// </summary>
        /// <param name="applicationname">Name of the application (identifies the cache to be used)</param>
        /// <param name="initMode">Determines how the cache is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="asyncTimeoutMilliseconds">Milliseconds to wait for loading and refreshing files asynchronously.
        /// If value is less than zero or is Int32.MaxValue the loading files is done synchronously.</param>
        /// <param name="serverinfo"><see cref="ServerInfo"/> describing from which server UniCache should get files from</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <remarks>
        /// Parameter <c>asyncTimeoutMilliseconds</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        private void InitUniCache(String applicationname, InitMode initMode, Int32 asyncTimeoutMilliseconds,
                                  ServerInfo serverinfo, Int32 defaultTimeoutServerRequests)
        {
            try
            {
                m_InitLog = new StringBuilder();

                Config globalConfig = Config.GlobalConfig;

                applicationname = applicationname.Replace(@"/", "").Replace(@"\", "");
                m_UniCacheApplicationName = applicationname;
                m_UniCacheLogger = new Logger(applicationname);
                UniCacheLogger.WriteDelimiter();
                UniCacheLogger.WriteDelimiter();
                UniCacheLogger.WriteLineAppendTime("Creating UniCache ({0}) for application '{1}' ", Version, applicationname);
                globalConfig.WriteLog(m_UniCacheLogger);

                if (globalConfig.IsTestApplication(applicationname))
                {
                    if (globalConfig.ForceOfflineMode && initMode != InitMode.Offline)
                    {
                        LogText("Did not force initMode to '{0}' (from '{1}') for application '{2}'!", InitMode.Offline, initMode, applicationname);
                    }
                    LogText(String.Format("Apps initMode '{0}' will be used.", initMode));
                }
                else
                {
                    if (globalConfig.ForceOfflineIsDefault)
                    {
                        LogText("The default replication behavior for environment '{0}' is active.", Config.UniCreditEnvironment);
                        LogText(globalConfig.IsTestEnvironment ? "Preset UniCache content is protected!" : "UniCache content will be updated on request!");
                    }
                    else
                    {
                        LogText("The default replication behavior for environment '{0}' is deactivated!", Config.UniCreditEnvironment);
                        LogText(globalConfig.IsTestEnvironment ? "!?! Preset UniCache content might be changed!" : "!?! UniCache content will not be updated!");
                    }

                    if (globalConfig.ForceOfflineMode && initMode != InitMode.Offline)
                    {
                        LogText("Forcing initMode to '{0}' (from '{1}')", InitMode.Offline, initMode);
                        initMode = InitMode.Offline;
                    }
                    else
                    {
                        LogText(String.Format("Apps initMode '{0}' will be used", initMode));
                    }

                }

                if (String.IsNullOrEmpty(globalConfig.ServerURLToUse))
                {
                    LogText("Will use apps server parameter ({0})!", serverinfo);
                    m_SharePointHelper = new SharePointHelper(serverinfo);
                }
                else
                {
                    LogText("!?! Config overrides apps server parameter '{0}' by setting URL '{1}'!", serverinfo.ServerLongname, globalConfig.ServerURLToUse);

                    m_SharePointHelper = new SharePointHelper(new ServerInfo("URL", "URL of server to use", globalConfig.ServerURLToUse, null));
                }
                m_SharePointHelper.UseDefaultCredentials = true;
                m_SharePointHelper.UniCacheLogger = UniCacheLogger;
                m_SharePointHelper.DefaultTimeout = defaultTimeoutServerRequests;

                UniCacheLogger.WriteLine("InitMode: '{0}' ", initMode.ToString());
                UniCacheLogger.WriteLine("TimeOut: {0} (ms)", asyncTimeoutMilliseconds);

                UniCacheLogger.WriteLine("Server: {0} ({1})", m_SharePointHelper.ServerInfo.ServoRootUrl, m_SharePointHelper.ServerInfo.ServerLongname);
                UniCacheLogger.WriteLine("Default timeout for server requests: {0}", defaultTimeoutServerRequests);

                Status = CacheStatus.Initializing;

                m_UniCacheUrl = m_SharePointHelper.ServerInfo.ServoRootUrl + Config.CachesWebRelativeUrl;
                UniCacheLogger.WriteLine("UniCacheUrl: {0}", m_UniCacheUrl);
                m_ApplicationsCacheUrl = m_UniCacheUrl + applicationname + "/";
                UniCacheLogger.WriteLine("ApplicationsCacheUrl: {0}", m_ApplicationsCacheUrl);

                SetFolderAssertExist(out m_UniCacheRootFolder, globalConfig.UniCacheRootFolder, "UniCacheClientRoot: ");

                SetFolderAssertExist(out m_ApplicationsCacheTempFolder, m_UniCacheRootFolder + applicationname, "ApplicationsRootFolder: ");

                UniCacheLogger.WriteDelimiter();
                
                //Now the settings are loaded (e.g. where is the clients cache, server url, ...)

                if (initMode == InitMode.InitializeOnly)
                {
                    Status = CacheStatus.Initialized;
                    // UniCache is prepared to be refreshed asynchronous (-> RefreshAsync())
                }
                else
                {
                    Status = CacheStatus.Refreshing;
                    if (asyncTimeoutMilliseconds < 0 || asyncTimeoutMilliseconds == Int32.MaxValue)
                    {
                        UniCacheLogger.WriteLine("Create UniCacheElements synchronously.");
                        UniCacheLogger.WriteDelimiter();
                        RefreshUniCacheElementsSync(initMode, false);
                    }
                    else
                    {
                        UniCacheLogger.WriteLine("Create UniCacheElements asynchronously.");
                        UniCacheLogger.WriteDelimiter();
                        AsyncRefresh(initMode, asyncTimeoutMilliseconds); // Calls InitializeUniCacheElementsSync(...) asynchronously
                    }
                };
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in UniCache-constructor", ex);
                throw;
            }
            UniCacheLogger.WriteDelimiter();
            UniCacheLogger.WriteDelimiter();
            UniCacheLogger.WriteNewline();
            UniCacheLogger.Flush();
        }

        private void LogText(String text, params Object[] args)
        {
            String logText;
            logText = String.Format(text, args);
            m_InitLog.AppendLine(logText);
            UniCacheLogger.WriteLine(logText);
        }

        /// <summary>
        /// Sets a folder-path variable and asserts that the folder exists (creates the folder if it doesn't exist).
        /// </summary>
        /// <param name="folderVar">Reference to the folder variable which is to be set.</param>
        /// <param name="path">The path.</param>
        /// <param name="logText">The log text.</param>
        private void SetFolderAssertExist(out String folderVar, String path, String logText)
        {
            if (!path.EndsWith(@"\"))
            {
                path = path + @"\";
            }
            folderVar = path;
            if (!Directory.Exists(folderVar)) Directory.CreateDirectory(folderVar);
            UniCacheLogger.WriteLine(logText + folderVar);
        }
        
        #endregion


        #region Asynchronous refresh of UniCache

        /// <summary>
        /// Refreshes this already initialized <see cref="UniCache"/>-instance asynchronously (=in background)
        /// </summary>
        /// <exception cref="ApplicationException">Refreshing process is already running!</exception>
        public void AsyncRefresh()
        {
            if (Status == CacheStatus.Refreshing) throw new ApplicationException("Refreshing process is already running!");
            UniCacheLogger.WriteDelimiter();
            UniCacheLogger.WriteLine("Start AsyncRefresh");
            try
            {
                UniCacheLogger.WriteLine("AsyncRefresh with InitMode=AutoInit.");
                Status = CacheStatus.Refreshing;
                AsyncRefresh(Config.GlobalConfig.ForceOfflineMode ? InitMode.Offline : InitMode.AutoInit, 0);
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in public 'AsyncRefresh'", ex);
            }
            UniCacheLogger.WriteLine("End AsyncRefresh");
            UniCacheLogger.WriteDelimiter();
            UniCacheLogger.Flush();
        }


        /// <summary>
        /// Refreshes this already initialized <see cref="UniCache"/>-instance asynchronously (=in background) 
        /// by invoking an asynchronous callback (<see cref="AsyncCallback" />)
        /// </summary>
        /// <param name="initMode">The initialize-mode (see <see cref="InitMode"/>).</param>
        /// <param name="timoutInMilliseconds">The timeout in milliseconds this method will return if refresh-time exceeds the timeout.</param>
        internal void AsyncRefresh(InitMode initMode, Int32 timoutInMilliseconds)
        {
            try
            {
                UniCacheLogger.WriteLine("AsyncRefresh with InitMode='{0}' and a timout of {1} ms.", initMode.ToString(), timoutInMilliseconds);
                UniCacheLogger.WriteDelimiter(); 
                AsyncRefreshOfUniCacheElements asyncRefreshDelegate = new AsyncRefreshOfUniCacheElements(this.RefreshUniCacheElementsSyncCallback);
                IAsyncResult iAsynchResult = asyncRefreshDelegate.BeginInvoke(initMode, new AsyncCallback(AsyncInitializeCallback), asyncRefreshDelegate);
                if (timoutInMilliseconds >= 0)
                {
                    iAsynchResult.AsyncWaitHandle.WaitOne(timoutInMilliseconds);
                }
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'AsyncRefresh'", ex);
            }
        }

        /// <summary>
        /// Will be called when asynchronous call of <see cref="RefreshUniCacheElementsSyncCallback" /> terminates (see <see cref="AsyncRefresh(InitMode,Int32)"/>).
        /// </summary>
        /// <param name="iAsynchResult">The <see cref="IAsyncResult"/>.</param>
        private void AsyncInitializeCallback(IAsyncResult iAsynchResult)
        {
            try
            {
                UniCacheLogger.WriteDelimiter();
                UniCacheLogger.WriteLine("Start AsyncInitializeCallback");
                AsyncRefreshOfUniCacheElements asyncInitDelegate = (AsyncRefreshOfUniCacheElements)iAsynchResult.AsyncState;
                Boolean result = asyncInitDelegate.EndInvoke(iAsynchResult);   // result of RefreshUniCacheElementsSync
                RefreshCompleteHandler handler = OnRefreshComplete;            // Avoid race conditions (Best Practices 17.7)
                if (handler != null)
                {
                    try
                    {
                        UniCacheLogger.WriteLine("Calling 'OnRefreshComplete' event handler");
                        handler(this, new RefreshCompleteEventArgs(result, this));
                    }
                    catch (Exception ex)
                    {
                        UniCacheLogger.WriteException("Exception in 'OnRefreshComplete' event handler", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'AsyncInitializeCallback'", ex);
            }
            UniCacheLogger.WriteLine("End AsyncInitializeCallback");
            UniCacheLogger.WriteDelimiter();
            UniCacheLogger.Flush();
        }

        /// <summary>
        /// Delegate for asynchronous refresh (see <see cref="AsyncRefresh(InitMode,Int32)"/>)
        /// </summary>
        /// <param name="initMode">The initialize-mode (see <see cref="InitMode"/>).</param>
        /// <returns></returns>
        internal delegate Boolean AsyncRefreshOfUniCacheElements(InitMode initMode);

        /// <summary>
        /// Raises the on-cache-element-loaded event.
        /// </summary>
        /// <param name="loadedCacheElement">The loaded <see cref="UniCacheElement"/>-instance.</param>
        /// <param name="isCached">Is set to <c>true</c> if the file corresponding to the <see cref="UniCacheElement"/>-instance is cached.</param>
        internal void RaiseOnCacheElementLoadedEvent(UniCacheElement loadedCacheElement, Boolean isCached)
        {
            CacheElementLoadedHandler handler = OnCacheElementLoaded; // Avoid race conditions (Best Practices 17.7)
            if (handler != null)
            {
                try
                {
                    UniCacheLogger.WriteLine("Calling 'OnCacheElementLoaded' event handler");
                    handler(this, new CacheElementLoadedEventArgs(loadedCacheElement, isCached, this));
                }
                catch (Exception ex)
                {
                    UniCacheLogger.WriteException("Exception in 'OnCacheElementLoaded' event handler", ex);
                }
            }
        }

        #endregion


        /// <summary>
        /// Initializes and refreshes the collection of UniCacheElements (= files) synchronously and is invoked as callback (see <see cref="AsyncRefresh(InitMode,Int32)"/>).
        /// </summary>
        /// <param name="initMode"><see cref="InitMode"/> to be used. Must not be <see cref="InitMode.InitializeOnly"/>!</param>
        /// <returns></returns>
        private Boolean RefreshUniCacheElementsSyncCallback(InitMode initMode)
        {
            return RefreshUniCacheElementsSync(initMode, true);
        }

        /// <summary>
        /// Initializes and refreshes the collection of <see cref="UniCacheElement"/>-instances (= files) synchronously (if init-mode != <see cref="InitMode.InitializeOnly"/>).
        /// Forces offline-mode if there is no exclusive access to the clients content.xml or the server cannot be accessed.
        /// </summary>
        /// <param name="initMode"><see cref="InitMode"/> to be used. Must not be <see cref="InitMode.InitializeOnly"/>!</param>
        /// <param name="isAsync">Set to <c>true</c>, if <see cref="RaiseOnCacheElementLoadedEvent"/> should be raised and the refresh should be abortable using <see cref="AbortRefreshing"/>.
        /// Should be <c>true</c>, if <see cref="RefreshUniCacheElementsSync"/> is called from <see cref="AsyncRefresh(InitMode,Int32)"/></param>
        /// <returns>
        /// Returns <c>true</c> if UniCache is refreshed successfully (-&gt; will return <c>false</c> in all cases of offline mode)
        /// </returns>
        /// <exception cref="ApplicationException">Do not call InitializeUniCacheElementsSync with init-mode 'InitializeOnly'!</exception>
        private Boolean RefreshUniCacheElementsSync(InitMode initMode, Boolean isAsync)
        {
            m_InitLog.AppendLine();
            if (initMode == InitMode.InitializeOnly) throw new ApplicationException("Do not call InitializeUniCacheElementsSync with InitMode 'InitializeOnly'!");
            Boolean useOfflineMode = initMode == InitMode.Offline;
            Boolean onlineRefreshSucceded = false;
            Boolean exclusiveAccess = false;
            UniCacheLogger.EnterSection("Initialize UniCache elements");
            try
            {
                m_InitLog.AppendFormat("UniCache init mode: {0}", initMode);
                m_InitLog.AppendLine();

                UniCachedContentXml uniCachedOfflineContentXml;
                XmlDocument offlineContentXmlDoc;
                LoadOfflineContentXml(out uniCachedOfflineContentXml, out offlineContentXmlDoc);
                
                UniCacheElements tmpOfflineUniCache = new UniCacheElements(this, offlineContentXmlDoc, "offline content-xml"); // Migrates cached files to new filenames if needed
                if (useOfflineMode && tmpOfflineUniCache.UniCacheSeemsCorrupt)
                {
                    useOfflineMode = false;
                    m_InitLog.AppendFormat("Offline UniCache seems to be corrupt! useOfflineMode is set from true to false.", initMode);
                    m_InitLog.AppendLine();
                }

                Dictionary<String, UniCacheElement> elementsToTouch = new Dictionary<String, UniCacheElement>();
                if (!useOfflineMode)
                {
                    TryRefreshUniCache(initMode, isAsync, ref useOfflineMode, ref onlineRefreshSucceded, ref exclusiveAccess);
                    AddUniCacheElements(elementsToTouch, m_UniCacheElements);
                }

                UniCacheElements clientsOfflineUniCacheElements = new UniCacheElements(this, offlineContentXmlDoc, "offline content-xml"); // Migrates cached files to new filenames if needed
                if (useOfflineMode)
                {
                    m_UniCacheElements = clientsOfflineUniCacheElements;
                    m_UniCachedContentXml = uniCachedOfflineContentXml;
                }


                AddUniCacheElements(elementsToTouch, clientsOfflineUniCacheElements);
                TouchCachedFiles(elementsToTouch.Values);
                m_InitLog.AppendLine("Touched cached files of content xmls (online+offline to prevent files from beeing deleted while cleaning UniCache)!");

                UniCacheLogger.WriteLineAppendTime("Using {0} content xml.", useOfflineMode ? "offline" : "online");


                TryCleanCache(onlineRefreshSucceded, exclusiveAccess, clientsOfflineUniCacheElements);

            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'InitializeUniCacheElements'", ex);
                onlineRefreshSucceded = false;
            }
            finally
            {
                m_IsOffline = useOfflineMode;
                CloseUniCachedContentXml(initMode, useOfflineMode, exclusiveAccess);

                UniCacheLogger.ExitSection();
            }
            Status = IsOffline ? CacheStatus.Offline : CacheStatus.Complete;
            return onlineRefreshSucceded;
        }

        private void AddUniCacheElements(Dictionary<String, UniCacheElement> elementsToTouch, UniCacheElements uniCacheElements)
        {
            if (uniCacheElements != null)
            {
                foreach (UniCacheElement ue in uniCacheElements)
                {
                    if (!elementsToTouch.ContainsKey(ue.LogicalFilename))
                    {
                        elementsToTouch.Add(ue.LogicalFilename, ue);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to the refresh the cache.
        /// </summary>
        /// <param name="initMode"><see cref="InitMode"/> to be used.</param>
        /// <param name="isAsync">Set to <c>true</c>, if <see cref="RaiseOnCacheElementLoadedEvent"/> should be raised and the refresh should be abortable using <see cref="AbortRefreshing"/>.</param>
        /// <param name="useOfflineMode">Reference will be set to <c>true</c> if cache falls to offline-mode which will happen 
        /// if there is no exclusive access to the clients content-xml or the server cannot be accessed.</param>
        /// <param name="refreshSucceded">Reference will be set to <c>true</c> if refresh succeeded; otherwise <c>false</c>.</param>
        /// <param name="exclusiveAccess">Reference will be set to <c>true</c> if the clients content-xml can be accessed exclusively; otherwise <c>false</c>.</param>
        private void TryRefreshUniCache(InitMode initMode, Boolean isAsync, ref Boolean useOfflineMode, ref Boolean refreshSucceded, ref Boolean exclusiveAccess)
        {
            UniCachedContentXml uniCachedOnlineContentXml = new UniCachedContentXml(this, false /* get online content xml*/);
            if (uniCachedOnlineContentXml.Exists)
            {
                m_InitLog.AppendFormat("Got online content xml (LastWriteTime={0})", uniCachedOnlineContentXml.LastUpdateTime);
            }
            else
            {
                m_InitLog.AppendFormat("Online content xml doesn't exists!");
            }
            m_InitLog.AppendLine();

            Boolean onlineContentXmlIsUpdatedToDay = uniCachedOnlineContentXml.IsUpdatedToday;

            if (initMode == InitMode.OnceADay && onlineContentXmlIsUpdatedToDay)
            {
                LogLineToLoggers("Content.xml is updated today -> offline-mode is forced!");
                useOfflineMode = true;
            }
            else
            {
                if (initMode == InitMode.OnceADay)
                {
                    LogLineToLoggers("Content.xml isn't updated today -> Try to update!");
                }
                
                LogLineToLoggers(String.Format("Loading servers content-xml '{0}' ...", ApplicationsCacheContentUrl));
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                XmlDocument serversContentXml = this.SharePointHelper.GetXmlTimeout(ApplicationsCacheContentUrl, DefaultTimeout);

                stopwatch.Stop();

                LogLineToLoggers(String.Format("Loaded content xml from server ({0}ms)!", stopwatch.ElapsedMilliseconds));
                m_InitLog.AppendLine();

                Boolean forcedOfflineDueToServer = (serversContentXml == null || !UniCacheElements.HasUniCacheElements(serversContentXml));
                if (forcedOfflineDueToServer)
                {
                    LogLineToLoggers("Could not get servers content.xml -> offline-mode is forced!");
                    useOfflineMode = true;
                }
                else
                {
                    exclusiveAccess = uniCachedOnlineContentXml.OpenExclusive(Config.GlobalConfig.TryToLockTimeout);
                    if (exclusiveAccess)
                    {
                        UniCacheElements clientsUniCacheElements = new UniCacheElements(this, uniCachedOnlineContentXml.ContentXml, "online content-xml"); // Migrates cached files to new filenames if needed

                        UniCacheElements serversUniCacheElements = new UniCacheElements(this, serversContentXml, "servers content-xml");
                        m_UniCacheElements = serversUniCacheElements;
                        m_UniCachedContentXml = uniCachedOnlineContentXml;

                        m_InitLog.AppendLine("Refreshing UniCache ...");
                        stopwatch = new Stopwatch();
                        stopwatch.Start();

                        refreshSucceded = Refresh(isAsync);

                        stopwatch.Stop();
                        m_InitLog.AppendFormat("Refreshed UniCache ({0}ms)!", stopwatch.ElapsedMilliseconds);
                        m_InitLog.AppendLine();
                    }
                    else
                    {
                        // use offline content xml
                        LogLineToLoggers("Could not get exclusive access to clients content.xml -> offline-mode is forced!");
                        useOfflineMode = true;
                    }
                }
            }
        }

        /// <summary>
        /// Logs a text line to the loggers.
        /// </summary>
        /// <param name="logText">The text to log.</param>
        private void LogLineToLoggers(String logText)
        {
            UniCacheLogger.WriteLine(logText);
            m_InitLog.AppendLine(logText);
        }

        /// <summary>
        /// Closes the clients content-xml.
        /// </summary>
        /// <param name="initMode"><see cref="InitMode"/> to be used.</param>
        /// <param name="useOfflineMode"><c>true</c> if cache is in offline-mode; otherwise <c>false</c>.</param>
        /// <param name="exclusiveAccess"><c>true</c> if the clients content-xml is accessed exclusively; otherwise <c>false</c>.</param>
        private void CloseUniCachedContentXml(InitMode initMode, Boolean useOfflineMode, Boolean exclusiveAccess)
        {
            if (exclusiveAccess && m_UniCachedContentXml != null)
            {
                if (useOfflineMode)
                {
                    m_UniCachedContentXml.CloseAndDiscardChanges();
                }
                else
                {
                    Boolean writeUnchangedXml = (initMode == InitMode.OnceADay);
                    m_UniCachedContentXml.CloseAndWrite(writeUnchangedXml);
                }
            }
        }

        /// <summary>
        /// Loads the clients offline-content-xml.
        /// </summary>
        /// <param name="uniCachedOfflineContentXml">Reference to be set to a <see cref="UniCachedContentXml"/>-instance representing the clients offline-content-xml.</param>
        /// <param name="offlineContentXmlDoc">Reference to be set to a <see cref="XmlDocument"/>-instance of the clients offline-content-xml.</param>
        private void LoadOfflineContentXml(out UniCachedContentXml uniCachedOfflineContentXml, out XmlDocument offlineContentXmlDoc)
        {
            uniCachedOfflineContentXml = new UniCachedContentXml(this, true /* get offline content xml*/);
            offlineContentXmlDoc = uniCachedOfflineContentXml.ContentXml;

            Boolean offlineContentXmlHasUniCacheElements = false;
            if (offlineContentXmlDoc != null)
            {
                try
                {
                    offlineContentXmlHasUniCacheElements = UniCacheElements.HasUniCacheElements(offlineContentXmlDoc);
                }
                catch (Exception ex)
                {
                    UniCacheLogger.WriteException("Exception calling UniCacheElements.HasUniCacheElements()", ex);
                }
            }

            if (offlineContentXmlHasUniCacheElements)
            {
                m_InitLog.AppendFormat("Got offline content xml (LastWriteTime={0})", uniCachedOfflineContentXml.LastUpdateTime);
            }
            else
            {
                m_InitLog.AppendFormat("Offline content xml is empty!");
            }
            m_InitLog.AppendLine();
        }

        
        /// <summary>
        /// Set date (last accessed) of cached files to prevent files from being deleted while cleaning <see cref="UniCache" /> (<see cref="CleanCache" />)
        /// </summary>
        /// <param name="uniCacheElements">The collection of <see cref="UniCacheElement" />-instances.</param>
        private void TouchCachedFiles(IEnumerable<UniCacheElement> uniCacheElements)
        {
            UniCacheLogger.EnterSection("Start BackgroundWorker for touching cached files");
            try
            {
                List<UniCacheElement> elements = new List<UniCacheElement>(uniCacheElements);
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += ((object sender, DoWorkEventArgs e) => 
                {
                    Logger threadsLogger = new Logger();
                    threadsLogger.EnterSection("Touch cached files");
                    try
                    {
                        List<UniCacheElement> elemList = (List<UniCacheElement>)e.Argument;
                        DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        foreach (UniCacheElement uce in elemList)
                        {
                            uce.TouchCachedFile(threadsLogger, today);
                        }
                    }
                    catch (Exception ex)
                    {
                        threadsLogger.WriteException("Error touching UniCache's files!", ex);
                    }
                    threadsLogger.ExitSection();
                    if (threadsLogger.HasLoggedError)
                    {
                        threadsLogger.Flush(this.UniCacheApplicationName + "_BGTouchFiles");
                    }
                });
                bw.WorkerReportsProgress = false;
                bw.WorkerSupportsCancellation = false;
                bw.RunWorkerAsync(elements);
                UniCacheLogger.WriteLine("BackgroundWorker touching cached files has been started ...");
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Failed to start BackgroundWorker for touching cached files!", ex);
            }
            UniCacheLogger.ExitSection();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refreshes this <see cref="UniCache" />-instance (i.e. loads content files if necessary)
        /// </summary>
        /// <param name="isAsync">Set to <c>true</c>, if <see cref="RaiseOnCacheElementLoadedEvent"/> should be raised and the refresh should be abortable using <see cref="AbortRefreshing"/>.</param> 
        /// <returns>
        /// Returns <c>true</c>, if the refresh is completed successfully; otherwise <c>false</c>.
        /// </returns>
        private Boolean Refresh(Boolean isAsync)
        {
            Boolean refreshResult = true;
            UniCacheLogger.EnterSection("Refreshing UniCache");
            try
            {
                UniCacheLogger.WriteLineAppendTime("Refreshing online content");
                UniCacheLogger.WriteNewline();

                Status = CacheStatus.Refreshing;
                AbortRefreshing = false;
                Boolean abort = false;
                foreach (UniCacheElement uce in m_UniCacheElements)
                {
                    try
                    {
                        if (abort)
                        {
                            UniCacheLogger.WriteLine("Abort refreshing!", uce);
                            break;
                        }
                        UniCacheLogger.WriteLine("Load file if not uptodate for UniCacheElement '{0}'", uce);
                        refreshResult = !abort && uce.LoadIfFileIsMissing(false, isAsync) && refreshResult;
                        if (isAsync && !abort) abort = AbortRefreshing;
                    }
                    catch (Exception ex)
                    {
                        UniCacheLogger.WriteException("Exception refreshing UniCacheElement '{0}'", ex, uce);
                        refreshResult = false;
                    }
                }
                AbortRefreshing = false;
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'Refresh'", ex);
                refreshResult = false;
            }
            UniCacheLogger.ExitSection();
            return refreshResult;
        }


        /// <summary>
        /// Cleans the cache by deleting cached files which are not needed any more. The cache will only be cleaned when there
        /// has been a successful refresh with exclusive access providing a instantiated collection of cached (online/offline) files
        /// </summary>
        /// <param name="onlineRefreshSucceded">Set to <c>true</c> when there has been a successful online refresh.</param>
        /// <param name="exclusiveAccess">Set to <c>true</c> when there is exclusive access.</param>
        /// <param name="clientsOfflineUniCacheElements">The offline cache elements on the client, i.e. files referred to by offline-content-xml</param>
        private void TryCleanCache(Boolean onlineRefreshSucceded, Boolean exclusiveAccess, UniCacheElements clientsOfflineUniCacheElements)
        {
            if (exclusiveAccess && onlineRefreshSucceded && m_UniCacheElements != null && clientsOfflineUniCacheElements != null)
            {
                CleanCache(m_UniCacheElements, clientsOfflineUniCacheElements);
                m_InitLog.AppendLine("Cleaned UniCache");
            }
            else
            {
                if (!exclusiveAccess)
                {
                    UniCacheLogger.WriteLineAppendTime("Did not clean UniCache (no exclusive access)!");
                }
                else if (!onlineRefreshSucceded)
                {
                    UniCacheLogger.WriteLineAppendTime("Did not clean UniCache (online refresh failed or not performed)!");
                }
                else if (m_UniCacheElements == null)
                {
                    UniCacheLogger.WriteLineAppendTime("Did not clean UniCache (UniCacheElements == null)!");
                }
                else if (clientsOfflineUniCacheElements == null)
                {
                    UniCacheLogger.WriteLineAppendTime("Did not clean UniCache (clientsOfflineUniCacheElements == null)!");
                }
            }
        }

        /// <summary>
        /// Cleans the cache, i.e. deletes cached files not needed any more
        /// </summary>
        /// <param name="onlineCacheElements">The online <see cref="UniCacheElements"/>, i.e. files referred to by online-content-xml.</param>
        /// <param name="offlineCacheElements">The offline <see cref="UniCacheElements"/>, e.g. files referred to by offline-content-xml</param>
        private void CleanCache(UniCacheElements onlineCacheElements, UniCacheElements offlineCacheElements)
        {
            UniCacheLogger.EnterSection("Cleaning UniCache");
            try
            {
                if (LastUpdateTime != DateTime.MinValue 
                    && TimeSpan.FromTicks(DateTime.Now.Ticks - LastUpdateTime.Ticks).TotalHours > 12)
                {
                    // Try to delete cached files 
                    String[] files = Directory.GetFiles(m_ApplicationsCacheTempFolder);
                    foreach (String fileFullname in files)
                    {
                        FileInfo fileInfo = null;
                        try
                        {
                            fileInfo = new FileInfo(fileFullname);
                            Boolean isContentXml = String.Compare(fileInfo.Name, Config.ContentXmlName, true) == 0 ||       // do not delete online content xml
                                                   String.Compare(fileInfo.Name, Config.OfflineContentXmlName, true) == 0;  // do not delete offline content xml
                            if (!isContentXml)
                            {
                                if (DateTime.Now.Subtract(fileInfo.LastAccessTime).TotalDays > 90)                         // do not delete if touched within the last 90 days
                                {
                                    UniCacheElement uce;
                                    Uri fullname = new Uri(fileFullname);
                                    Boolean isNotUsedInContentXmls = !onlineCacheElements.TryGetValue(fullname, out uce) &&
                                                                     !offlineCacheElements.TryGetValue(fullname, out uce);
                                    if (isNotUsedInContentXmls)
                                    {
                                        try
                                        {
                                            File.Delete(fileInfo.FullName);
                                        }
                                        catch (Exception ex)
                                        {
                                            UniCacheLogger.WriteException("Exception deleting file '{0}'!", ex, fileInfo.FullName);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UniCacheLogger.WriteException("Exception cleaning file '{0}'!", ex, fileInfo.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception cleaning files!", ex);
            }
            UniCacheLogger.ExitSection();
        }


        /// <summary>
        /// Determines whether the specified file exists the in cache
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <returns>
        /// Returns <c>true</c>, if file exists; otherwise <c>false</c>.
        /// </returns>
        public Boolean ExistsInCache(String logicalFilename)
        {
            Boolean result = false;
            try
            {
                UniCacheElement uce;
                if (m_UniCacheElements.TryGetValue(logicalFilename, out uce)) result = uce.ExistsInCache;
                UniCacheLogger.WriteLine("ExistsInCache '{0}': {1}", logicalFilename, result);
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'ExistsInCache' for file '{0}'", ex, logicalFilename);
            }
            return result;
        }


        /// <summary>
        /// Gets the filename of the cached file.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <returns>
        /// Filename of the cached file 
        /// </returns>
        [ComVisible(false)]
        public String GetCacheFilename(String logicalFilename)
        {
            return GetCacheFilename(logicalFilename, false, false);
        }

        /// <summary>
        /// Gets the filename of the cached file.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <param name="forceDownload">Set to <c>true</c> when the file should be downloaded even if it already exists in cache.</param>
        /// <returns>
        /// Filename of the cached file
        /// </returns>
        public String GetCacheFilename(String logicalFilename, Boolean forceDownload)
        {
            return GetCacheFilename(logicalFilename, forceDownload, false);
        }

        /// <summary>
        /// Gets the filename of the cached file or a copy. Copying the cached file can be used to avoid access conflicts.
        /// The copied file will not be deleted by <see cref="UniCache "/> when cleaning the cache. The caller is responsible for deleting the copied file if it is not needed any more.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <param name="forceDownload">Set to <c>true</c> when the file should be downloaded even if it already exists in cache.</param>
        /// <param name="createCopy">Set to <c>true</c> when the cached file should be copied to a temporary folder; otherwise <c>false</c>.</param>
        /// <returns>
        /// Filename of the cached file or the copy (see parameter <c>createCopy</c>)
        /// </returns>
        [ComVisible(false)]
        public String GetCacheFilename(String logicalFilename, Boolean forceDownload, Boolean createCopy)
        {
            return GetCacheFilenameCopy(logicalFilename, forceDownload, createCopy ? String.Empty : null);
        }

        /// <summary>
        /// Gets the filename of the cached file or a copy. Copying the cached file can be used to avoid access conflicts.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <param name="forceDownload">Set to <c>true</c> when the file should be downloaded even if it already exists in cache.</param>
        /// <param name="destinationFilename">Path and name where the cached file should be copied to. 
        /// If <c>destinationFilename</c> is <c>null</c> the cached file will not be copied.
        /// If <c>destinationFilename</c> is an empty string a destination-filename will be created and the cached file will be copied. 
        /// </param>
        /// <returns>
        /// Filename of the cached file or the copy (see parameter <c>destinationFilename</c>)
        /// </returns>
        /// <exception cref="ApplicationException">Operation not allowed while refreshing UniCache!</exception>
        public String GetCacheFilenameCopy(String logicalFilename, Boolean forceDownload, String destinationFilename)
        {
            if (Status == CacheStatus.Refreshing) throw new ApplicationException("Operation not allowed while refreshing UniCache!");
            String result = null;
            UniCacheLogger.EnterSection("GetCacheFilename for file '{0}'", logicalFilename);
            try
            {
                UniCacheLogger.WriteLine("GetCacheFilename with forceDownload={0} and destinationFilename='{1}'", 
                                         forceDownload, destinationFilename);
                UniCacheElement uce;
                if (m_UniCacheElements.TryGetValue(logicalFilename, out uce))
                {
                    Boolean isOk = uce.LoadIfFileIsMissing(forceDownload, false);
                    if (isOk)
                    {
                        result = m_ApplicationsCacheTempFolder + uce.CachedFilename;
                        if (destinationFilename != null)
                        {
                            String sourceFilename = result;
                            if (destinationFilename.Length == 0)
                            {
                                destinationFilename = sourceFilename.Substring(sourceFilename.LastIndexOf('\\') + 1);
                                Int32 posPoint = destinationFilename.LastIndexOf('.');
                                String ticks = DateTime.Now.Ticks.ToString();
                                if (posPoint > -1)
                                {
                                    destinationFilename = destinationFilename.Substring(0, posPoint) + ticks
                                                          + destinationFilename.Substring(posPoint);
                                }
                                else
                                {
                                    destinationFilename += ticks;
                                }
                                destinationFilename = m_UniCacheRootFolder + destinationFilename;
                            }
                            if (!String.IsNullOrEmpty(destinationFilename))
                            {
                                UniCacheLogger.WriteLine("Copy file from '{0}' to '{1}'", sourceFilename, destinationFilename);
                                File.Copy(sourceFilename, destinationFilename, true);
                                result = destinationFilename;
                            }
                        }
                        uce.IncStatistic();
                    }
                }
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'GetCacheFilename' for file '{0}'", ex, logicalFilename);
            }
            UniCacheLogger.ExitSection();
            UniCacheLogger.Flush();
            return result;

        }

        /// <summary>
        /// Gets an enumerator to iterate through the collection of elements contained in the cache (<see cref="UniCacheElement"/>-instances).
        /// </summary>
        /// <returns>An enumerator to iterate through the collection of elements contained in the cache (<see cref="UniCacheElement"/>-instances)</returns>
        /// <exception cref="ApplicationException">Enumerator not available while refreshing UniCache!</exception>
        public IEnumerator GetEnumerator()  
        {
            if (Status == CacheStatus.Refreshing) throw new ApplicationException("Enumerator not available while refreshing UniCache!");
            List<UniCacheElement> result = null;
            if (m_UniCacheElements != null) result = new List<UniCacheElement>(m_UniCacheElements);
            return result.GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="UniCacheElement"/>-instance for the specified cache-element.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <returns>
        /// The <see cref="UniCacheElement"/>-instance for the specified cache-element.
        /// </returns>
        /// <exception cref="ApplicationException">Operation not allowed while refreshing UniCache!</exception>
        public UniCacheElement GetCacheElement(String logicalFilename)
        {
            if (Status == CacheStatus.Refreshing) throw new ApplicationException("Operation not allowed while refreshing UniCache!");
            UniCacheElement uce = null;
            m_UniCacheElements.TryGetValue(logicalFilename, out uce);
            return uce;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("UniCache '{0}' ({1})", m_UniCacheApplicationName, m_SharePointHelper.ServerInfo.Servername);
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        public String CacheName
        {
            get { return ToString(); }
        }


        /// <summary>
        /// Gets the applications (offline- or online-) content-xml fullname on the client.
        /// </summary>
        /// <value>
        /// the applications (offline- or online-) content-xml fullname on the client.
        /// </value>
        String IUniCacheTestData.ApplicationsCachedContentXmlName
        {
            get { return m_UniCachedContentXml == null ? "NA" : m_UniCachedContentXml.ApplicationsCachedContentXmlName; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is updated today.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is updated today; otherwise, <c>false</c>.
        /// </value>
        Boolean IUniCacheTestData.IsUpdatedToday
        {
            get { return m_UniCachedContentXml == null ? false : m_UniCachedContentXml.IsUpdatedToday; }
        }


        String IUniCacheTestData.WebExceptions
        {
            get { return m_SharePointHelper.WebExceptions; }
        }
    }

    /// <summary>
    /// Event arguments for the OnRefreshComplete-event (<see cref="UGIS.de.OfficeComponents.UniCacheLib.IUniCacheEvents.OnRefreshComplete" />
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.IRefreshCompleteEventArgs" />
    [ComVisible(true)]
    [Guid("2741887C-2447-4ec7-8FAA-F2E92ACE0C7D")]
    [ClassInterface(ClassInterfaceType.None)]
    public class RefreshCompleteEventArgs : IRefreshCompleteEventArgs
    {

        /// <summary>
        /// The <see cref="UniCache"/>-instance raising the event
        /// </summary>
        private UniCache m_UniCache;
        
        /// <summary>
        /// Gets the <see cref="UniCache"/>-instance raising the event
        /// </summary>
        /// <value>
        /// The <see cref="UniCache"/>-instance raising the event
        /// </value>
        public UniCache UniCache
        {
            get { return m_UniCache; }
        }

        /// <summary>
        /// Result of the background refresh which will be <c>true</c> if UniCache is refreshed successfully. 
        /// The result will be <c>false</c> if an error occurs or in all cases of falling into offline mode.
        /// </summary>
        private Boolean m_Result;

        /// <summary>
        /// Result of the background refresh which will be <c>true</c> if UniCache is refreshed successfully. 
        /// The result will be <c>false</c> if an error occurs or in all cases of falling into offline mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if refresh has been successful; otherwise <c>false</c>.
        /// </value>
        public Boolean Result
        {
            get { return m_Result; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshCompleteEventArgs"/> class.
        /// </summary>
        /// <param name="result"><c>true</c> if refresh has been successful; otherwise <c>false</c>.</param>
        /// <param name="uniCache">The <see cref="UniCache"/>-instance raising the event.</param>
        internal RefreshCompleteEventArgs(Boolean result, UniCache uniCache)
        {
            m_Result = result;
            m_UniCache = uniCache;
        }
    }

    /// <summary>
    /// Event arguments for the OnCacheElementLoaded-event (<see cref="UGIS.de.OfficeComponents.UniCacheLib.IUniCacheEvents.OnCacheElementLoaded" />
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.ICacheElementLoadedEventArgs" />
    [ComVisible(true)]
    [Guid("3D04B6D9-8772-42ca-BDC6-589D68D5B40B")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CacheElementLoadedEventArgs : ICacheElementLoadedEventArgs
    {

        /// <summary>
        /// Gets the <see cref="UniCache"/>-instance raising the event
        /// </summary>
        private UniCache m_UniCache;

        /// <summary>
        /// Gets the <see cref="UniCache"/>-instance raising the event
        /// </summary>
        /// <value>
        /// The <see cref="UniCache"/>-instance raising the event
        /// </value>
        public UniCache UniCache
        {
            get { return m_UniCache; }
        }

        /// <summary>
        /// Gets the loaded <see cref="UniCacheElement"/>
        /// </summary>
        private UniCacheElement m_LoadedCacheElement;
        /// <summary>
        /// Gets the loaded <see cref="UniCacheElement"/>
        /// </summary>
        /// <value>
        /// The loaded <see cref="UniCacheElement"/>
        /// </value>
        public UniCacheElement LoadedCacheElement
        {
            get { return m_LoadedCacheElement; }
        }

        /// <summary>
        /// <c>true</c> if the file of the loaded <see cref="UniCacheElement"/>-instance is cached; otherwise, <c>false</c>.
        /// </summary>
        private Boolean m_Result;
        /// <summary>
        /// Gets a value indicating whether loading the file of the loaded <see cref="UniCacheElement"/>-instance (<see cref="LoadedCacheElement"/>) was successful, 
        /// e.g. if the file is cached or missing.
        /// </summary>
        /// <value>
        /// <c>true</c> if the file of the loaded <see cref="UniCacheElement"/>-instance is cached; otherwise, <c>false</c>.
        /// </value>
        public Boolean Result
        {
            get { return m_Result; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheElementLoadedEventArgs"/> class.
        /// </summary>
        /// <param name="loadedCacheElement">The loaded <see cref="UniCacheElement"/>-instance.</param>
        /// <param name="isCached"><c>true</c> if the file of loaded <see cref="UniCacheElement"/>-instance (see <paramref name="loadedCacheElement"/>) is cached; 
        /// otherwise, <c>false</c>.</param>
        /// <param name="uniCache">The <see cref="UniCache"/>-instance raising the event.</param>
        internal CacheElementLoadedEventArgs(UniCacheElement loadedCacheElement, Boolean isCached, UniCache uniCache)
        {
            m_LoadedCacheElement = loadedCacheElement;
            m_Result = isCached;
            m_UniCache = uniCache;
        }
    }


}
