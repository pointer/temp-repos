using System;
using System.Collections.Generic;
using System.Text;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    // UniCache class is a partial class divided into two files:
    // + UniCache.cs: Contains caching logic
    // + UniCacheCreate.cs: Contains methods to create UniCache-instances (support of UniCacheFactory)


    public partial class UniCache 
    {

        /// <summary>
        /// Gets the initialisation-mode (<see cref="InitMode"/>) from a corresponding mode-name.
        /// </summary>
        /// <param name="name">Name of the initialisation-mode.</param>
        /// <returns><see cref="InitMode"/> corresponding to <c>name</c></returns>
        internal static InitMode GetInitMode(String name)
        {
            return (InitMode)Enum.Parse(typeof(InitMode), name, true);
        }

        /// <summary>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <value>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </value>
        [Obsolete("ServerMOSSDefault is deprecated, use ServerDefault instead.")]
        public static String ServerMOSSDefault
        {
            get { return ServerInfos.ServerDefault; }
        }

        /// <summary>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <value>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </value>
        public static String ServerDefault
        {
            get { return ServerInfos.ServerDefault; }
        }



        #region Static SharePointHelper Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in production environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in production environment.</returns>
        [Obsolete("CreateSharePointHelperMOSS is deprecated, use CreateSharePointHelperPROD instead.")]
        public static SharePointHelper CreateSharePointHelperMOSS()
        {
            return CreateSharePointHelperPROD();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in test environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in test environment.</returns>
        [Obsolete("CreateSharePointHelperMOSSQC is deprecated, use CreateSharePointHelperQA instead.")]
        public static SharePointHelper CreateSharePointHelperMOSSQC()
        {
            return CreateSharePointHelperQA();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in dev environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in dev environment.</returns>
        public static SharePointHelper CreateSharePointHelperDEV()
        {
            return new SharePointHelper(ServerInfos.ServerDEV);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment.</returns>
        [Obsolete("CreateSharePointHelperMOSSDefault is deprecated, use CreateSharePointHelperDefault instead.")]
        public static SharePointHelper CreateSharePointHelperMOSSDefault()
        {
            return CreateSharePointHelperDefault();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in production environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in production environment.</returns>
        public static SharePointHelper CreateSharePointHelperPROD()
        {
            return new SharePointHelper(ServerInfos.ServerPROD);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in test environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in test environment.</returns>
        public static SharePointHelper CreateSharePointHelperQA()
        {
            return new SharePointHelper(ServerInfos.ServerQA);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment.</returns>
        public static SharePointHelper CreateSharePointHelperDefault()
        {
            return new SharePointHelper(ServerInfos.ServerDefault);
        }
        #endregion



        #region Static UniCache Constructors


        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode, maximum creation time, server and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache"/> is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" /> in online- or offline-mode.
        /// </returns>
        /// <remarks>
        /// Parameter <c>maxTime</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public static UniCache CreateUniCache(String applicationName, InitMode initMode, Int32 maxTime,
                                              String serverName, Int32 defaultTimeoutServerRequests)
        {
            return new UniCache(applicationName, initMode, maxTime,
                                ServerInfos.GetServerInfo(serverName), defaultTimeoutServerRequests);
        }

        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application, init-mode, server, maximum creation time and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache" /> is initialized and refreshed (<see cref="InitMode" /> InitMode)</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" /> in online- or offline-mode.
        /// </returns>
        /// <remarks>
        /// Parameter <c>maxTime</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.
        /// </remarks>
        public static UniCache CreateUniCache(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            return UniCache.CreateUniCache(applicationName, UniCache.GetInitMode(initMode), maxTime, serverName, defaultTimeoutServerRequests);
        }




        /// <summary>
        /// Creates an <see cref="UniCache" /> using default SharePoint for a given application, init-mode, maximum creation time and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache"/> is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" /> in online- or offline-mode.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Allowed Range for maxTime (in milliseconds): 0 &lt;= maxTime &lt;= 600000!
        /// or
        /// Values for init-mode must not be InitializeOnly and Offline!
        /// or
        /// Allowed values for defaultTimeoutServerRequests: defaultTimeoutServerRequests &gt; 0!
        /// </exception>
        /// <remarks>
        /// <para>If initialising and refreshing exceeds the maximum creation time an <see cref="UniCache" />-instance in offline-mode is returned.</para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// <para>Parameter <c>maxTime</c> will be ignored if <see cref="InitMode.InitializeOnly"/> is used.</para>
        /// </remarks>
        public static UniCache CreateUniCacheWithinTimespan(String applicationName, InitMode initMode, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            return CreateUniCacheWithinTimespan(applicationName, initMode, ServerInfos.ServerDefault, maxTime, defaultTimeoutServerRequests);
        }

        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application, init-mode, server, maximum creation time and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache" /> is initialized and refreshed (<see cref="InitMode" /> InitMode)</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" /> in online- or offline-mode.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Allowed Range for maxTime (in milliseconds): 0 &lt;= maxTime &lt;= 600000!
        /// or
        /// Values for init-mode must not be InitializeOnly and Offline!
        /// or
        /// Allowed values for defaultTimeoutServerRequests: defaultTimeoutServerRequests &gt; 0!
        /// </exception>
        /// <remarks>
        /// If initialising and refreshing exceeds the maximum creation time an <see cref="UniCache" />-instance in offline-mode is returned.
        /// </remarks>
        public static UniCache CreateUniCacheWithinTimespan(String applicationName, InitMode initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            UniCache result;
            if (maxTime < 0 || maxTime > 600000) throw new ArgumentOutOfRangeException("Allowed Range for maxTime (in milliseconds): 0 <= maxTime <= 600000!");
            if (initMode == InitMode.InitializeOnly || initMode == InitMode.Offline) throw new ArgumentOutOfRangeException("initMode must not be InitializeOnly and Offline!");
            if (defaultTimeoutServerRequests <= 0) throw new ArgumentOutOfRangeException("Allowed values for defaultTimeoutServerRequests: defaultTimeoutServerRequests > 0!");

            UniCache offlineUniCache = UniCache.CreateUniCacheOffline(applicationName);
            if (Config.GlobalConfig.IsTestApplication(applicationName) || !Config.GlobalConfig.ForceOfflineMode)
            {
                UniCache refreshedUniCache = UniCache.CreateUniCache(applicationName, initMode, maxTime, serverName, defaultTimeoutServerRequests);
                result = refreshedUniCache.Status == CacheStatus.Complete ? refreshedUniCache : offlineUniCache;
            }
            else
            {
                result = offlineUniCache;
            }
            return result;
        }



        /// <summary>
        /// Creates an <see cref="UniCache" /> in offline-mode for a given application and the default SharePoint server (<see cref="ServerDefault"/>).
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns immediately an initialized <see cref="UniCache" /> in offline-mode.
        /// </returns>
        /// <remarks>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// </remarks>
        public static UniCache CreateUniCacheOffline(String applicationName)
        {
            return new UniCache(applicationName, InitMode.Offline, ServerDefault);
        }



        /// <summary>
        /// Creates an <see cref="UniCache" /> synchronously for a given application with initMode <see cref="InitMode.AutoInit"/>.
        /// As server will be used SP2013apps in production and SP2013apps.qa in test environment.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" />.
        /// </returns>
        /// <remarks>
        /// <para>Uses <see cref="InitMode.AutoInit"/> which will refresh the cache.</para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// </remarks>
        public static UniCache CreateUniCacheSync(String applicationName)
        {
            return UniCache.CreateUniCache(applicationName, InitMode.AutoInit, -1, ServerDefault, Config.GlobalConfig.DefaultTimeoutServerRequest);
        }


        /// <summary>
        /// Creates an initialized but <c>not</c> refreshed <see cref="UniCache" /> for a given application which can be refreshed asynchronously afterwards.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an initialized but <c>not</c> refreshed UniCache.
        /// </returns>
        /// <remarks>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// <para>To refresh the <see cref="UniCache" /> call <see cref="AsyncRefresh()"/>.</para>
        /// </remarks>
        public static UniCache InitUniCacheForAsyncRefresh(String applicationName)
        {
            return UniCache.CreateUniCache(applicationName, InitMode.InitializeOnly, -1, ServerDefault, Config.GlobalConfig.DefaultTimeoutServerRequest);
        }

        /// <summary>
        /// Creates an <see cref="UniCache" /> in offline-mode and refreshes the application cache in background. 
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns immediately with an initialized <see cref="UniCache" /> in offline-mode.
        /// </returns>
        /// <remarks>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// </remarks>
        public static UniCache CreateUniCacheOfflineRefreshBackground(String applicationName)
        {
            UniCache offlineUniCache = CreateUniCacheOffline(applicationName);

            if (Config.GlobalConfig.IsTestApplication(applicationName) || !Config.GlobalConfig.ForceOfflineMode)
            {
                UniCache uniCache = InitUniCacheForAsyncRefresh(applicationName);
                uniCache.AsyncRefresh();
            }
            else
            {
                // Avoid connecting the server for a non-test application when ForceOfflineMode is true 
            }
            return offlineUniCache;
        }



        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application which will be updated the same day.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="doBackgroundRefresh">If set to <c>true</c> the refresh of UniCache in background will be performed even if already updated the same day.</param>
        /// <returns>
        /// Returns an UniCache (online or offline) which has been updated the same day (if no errors occurred).
        /// </returns>
        /// <remarks>
        /// This method uses the offline UniCache if cache has been updated the same day, otherwise the application cache will be refreshed.
        /// <para>Uses <see cref="InitMode.OnceADay"/> which will refresh the cache if it is not refreshed the same day.</para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// </remarks>
        public static UniCache CreateUniCacheUpdatedDaily(String applicationName, Boolean doBackgroundRefresh)
        {
            UniCache unicache = UniCache.CreateUniCache(applicationName, InitMode.OnceADay, -1, ServerDefault, Config.GlobalConfig.DefaultTimeoutServerRequest);
            if (unicache.IsOffline && doBackgroundRefresh)
            {
                if (Config.GlobalConfig.IsTestApplication(applicationName) || !Config.GlobalConfig.ForceOfflineMode)
                {
                    UniCache uniCacheUpdatedInBackground = InitUniCacheForAsyncRefresh(applicationName);
                    uniCacheUpdatedInBackground.AsyncRefresh();
                }
                else
                {
                    // Avoid connecting the server for a non-test application when ForceOfflineMode is true 
                }
            }
            return unicache;
        }


        /// <summary>
        /// Starts an asynchronous refresh in background for a given application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="uniCacheFactory">The <see cref="UniCacheFactory"/>-instance which will get the refreshed <see cref="UniCache"/>-instance set when <see cref="OnRefreshComplete"/>-event is raised.</param>
        /// <remarks>
        /// <para>
        /// Creates an <see cref="UniCache" />-instance in offline-mode which is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// Then a second <see cref="UniCache" />-instance is created using init-mode <see cref="InitMode.AutoInit"/>. When the creation of this second instance is completed it is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// </para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// </remarks>
        internal static void StartCreateRefreshBackground(String applicationName, UniCacheFactory uniCacheFactory)
        {
            StartCreateUniCache(applicationName, InitMode.AutoInit, -1, ServerDefault, Config.GlobalConfig.DefaultTimeoutServerRequest, uniCacheFactory);
        }

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="uniCacheFactory">The <see cref="UniCacheFactory"/>-instance which will get the refreshed <see cref="UniCache"/>-instance set when <see cref="OnRefreshComplete"/>-event is raised.</param>
        /// <remarks>
        /// <para>
        /// Creates an <see cref="UniCache" />-instance in offline-mode which is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// Then a second <see cref="UniCache" />-instance is created using init-mode <see cref="InitMode.OnceADay" /> or <see cref="InitMode.AutoInit" /> when <c>doBackgroundRefresh</c> is <c>true</c>.
        /// Creating the second <see cref="UniCache" />-instance will refresh the cache if <c>doBackgroundRefresh</c> is <c>false</c> or if the cache is not refreshed the same day. 
        /// When the creation of this second instance is completed it is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// </para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest" />-property of <see cref="Config.GlobalConfig" /> is used.</para>
        /// </remarks>
        internal static void StartCreateUpdatedDaily(String applicationName, UniCacheFactory uniCacheFactory)
        {
            StartCreateUniCache(applicationName, InitMode.OnceADay, -1, ServerDefault, Config.GlobalConfig.DefaultTimeoutServerRequest, uniCacheFactory);
        }


        /// <summary>
        /// Starts an asynchronous refresh in background for a given application, init-mode, maximum creation time, server and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache" /> is initialized and refreshed (<see cref="InitMode" /> InitMode)</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <param name="uniCacheFactory">The <see cref="UniCacheFactory"/>-instance which will get the refreshed <see cref="UniCache"/>-instance set when <see cref="OnRefreshComplete"/>-event is raised.</param>
        /// <remarks>
        /// Creates an <see cref="UniCache" />-instance in offline-mode which is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// Then a second <see cref="UniCache" />-instance is created using the given init-mode. When the creation of this second instance is completed it is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// </remarks>
        internal static void StartCreateUniCache(String applicationName, InitMode initMode, Int32 maxTime,
                                                 String serverName, Int32 defaultTimeoutServerRequests, UniCacheFactory uniCacheFactory)
        {
            uniCacheFactory.SetUniCache(new UniCache(applicationName, InitMode.Offline, serverName), false);

            if (Config.GlobalConfig.IsTestApplication(applicationName) || !Config.GlobalConfig.ForceOfflineMode)
            {
                UniCache uniCache = UniCache.CreateUniCache(applicationName, InitMode.InitializeOnly, maxTime, serverName, defaultTimeoutServerRequests);
                uniCache.OnRefreshComplete += new RefreshCompleteHandler((sender, eventArgs) => { uniCacheFactory.SetUniCache(eventArgs.UniCache, true); });
                uniCache.AsyncRefresh(initMode, maxTime);
            }
            else
            {
                // Avoid connecting the server for a non-test application when ForceOfflineMode is true 
            }
        }



        #endregion


    }

}
