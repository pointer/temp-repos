using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Factory helping to create UniCache-instances (in particular used by COM clients)
    /// </summary>
    /// <remarks>
    /// This explicit interface definition is needed to keep full control over the COM interface used by early binding COM clients!
    /// </remarks>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.IUniCacheFactory" />
    [ComVisible(true)]
    [Guid("7CED0F12-EB26-4e71-B958-2427C518AFFB")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UniCacheLib.UniCacheFactory")]
    public class UniCacheFactory : IUniCacheFactory
    {

        #region SharePoint Server Name String Constants

        /// <summary>
        /// Key/Identifier for SharePoint server in production environment (<see cref="ServerInfos.ServerPROD"/>)
        /// </summary>
        [Obsolete("ServerMOSS is deprecated, use ServerPROD instead.")]
        public String ServerMOSS 
        {
            get { return ServerPROD; }
        }

        /// <summary>
        /// Key/Identifier for SharePoint server in production environment (<see cref="ServerInfos.ServerPROD"/>)
        /// </summary>
        public String ServerPROD
        {
            get { return ServerInfos.ServerPROD; }
        }


        /// <summary>
        /// Key/Identifier for SharePoint server in test environment (<see cref="ServerInfos.ServerQA"/>)
        /// </summary>
        [Obsolete("ServerMOSSQC is deprecated, use ServerQA instead.")]
        public String ServerMOSSQC
        {
            get { return ServerQA; }
        }

        /// <summary>
        /// Key/Identifier for SharePoint server in test environment (<see cref="ServerInfos.ServerQA"/>)
        /// </summary>
        public String ServerQA
        {
            get { return ServerInfos.ServerQA; }
        }


        /// <summary>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerInfos.ServerPROD" /> in production and <see cref="ServerInfos.ServerQA" /> in test environment
        /// </summary>
        [Obsolete("ServerMOSSDefault is deprecated, use ServerDefault instead.")]
        public String ServerMOSSDefault
        {
            get { return ServerDefault; }
        }

        /// <summary>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerInfos.ServerPROD" /> in production and <see cref="ServerInfos.ServerQA" /> in test environment
        /// </summary>
        public String ServerDefault
        {
            get { return ServerInfos.ServerDefault; }
        }

        /// <summary>
        /// Key/Identifier for SharePoint server in dev environment (<see cref="ServerInfos.ServerDEV"/>)
        /// </summary>
        public String ServerDEV
        {
            get { return ServerInfos.ServerDEV; }
        }
        #endregion 



        #region SharePointHelper Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in production environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in production environment.</returns>
        [Obsolete("CreateSharePointHelperMOSS is deprecated, use CreateSharePointHelperPROD instead.")]
        public SharePointHelper CreateSharePointHelperMOSS()
        {
            return UniCache.CreateSharePointHelperPROD();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in test environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in test environment.</returns>
        [Obsolete("CreateSharePointHelperMOSSQC is deprecated, use CreateSharePointHelperQA instead.")]
        public SharePointHelper CreateSharePointHelperMOSSQC()
        {
            return UniCache.CreateSharePointHelperQA();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in dev environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in dev environment.</returns>
        public SharePointHelper CreateSharePointHelperDEV()
        {
            return UniCache.CreateSharePointHelperDEV();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment.</returns>
        [Obsolete("CreateSharePointHelperMOSSDefault is deprecated, use CreateSharePointHelperDefault instead.")]
        public SharePointHelper CreateSharePointHelperMOSSDefault()
        {
            return UniCache.CreateSharePointHelperDefault();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in production environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in production environment.</returns>
        public SharePointHelper CreateSharePointHelperPROD()
        {
            return UniCache.CreateSharePointHelperPROD();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in test environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in test environment.</returns>
        public SharePointHelper CreateSharePointHelperQA()
        {
            return UniCache.CreateSharePointHelperQA();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment.</returns>
        public SharePointHelper CreateSharePointHelperDefault()
        {
            return UniCache.CreateSharePointHelperDefault();
        }
        #endregion



        #region UniCache Constructors


        /// <summary>
        /// Gets the initialisation-mode (<see cref="InitMode"/>) from a corresponding mode-name.
        /// </summary>
        /// <param name="name">Name of the initialisation-mode.</param>
        /// <returns><see cref="InitMode"/> corresponding to <c>name</c></returns>
        public InitMode GetInitMode(String name)
        {
            return UniCache.GetInitMode(name);
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
        public UniCache CreateUniCacheWithinTimespan(String applicationName, InitMode initMode, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            return UniCache.CreateUniCacheWithinTimespan(applicationName, initMode, maxTime, defaultTimeoutServerRequests);
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
        public UniCache CreateUniCacheWithinTimespanEx(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            return UniCache.CreateUniCacheWithinTimespan(applicationName, GetInitMode(initMode), serverName, maxTime, defaultTimeoutServerRequests);
        }


        /// <summary>
        /// Creates an <see cref="UniCache" /> in offline-mode for a given application and the default SharePoint server (<see cref="ServerDefault"/>).
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns immediately an initialized <see cref="UniCache" /> in offline-mode.
        /// </returns>
        public UniCache CreateUniCacheOffline(String applicationName)
        {
            return UniCache.CreateUniCacheOffline(applicationName);
        }


        /// <summary>
        /// Creates an <see cref="UniCache" /> synchronously for a given application with initMode <see cref="InitMode.AutoInit"/>.
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" />.
        /// </returns>
        public UniCache CreateUniCacheSync(String applicationName)
        {
            return UniCache.CreateUniCacheSync(applicationName);
        }


        /// <summary>
        /// Creates an initialized but <c>not</c> refreshed <see cref="UniCache" /> for a given application which can be refreshed asynchronously afterwards.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an initialized but <c>not</c> refreshed UniCache.
        /// </returns>
        public UniCache InitUniCacheForAsyncRefresh(String applicationName)
        {
            return UniCache.InitUniCacheForAsyncRefresh(applicationName);
        }


        /// <summary>
        /// Creates an <see cref="UniCache" /> in offline-mode and refreshes the application cache in background. 
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns immediately with an initialized <see cref="UniCache" /> in offline-mode.
        /// </returns>
        public UniCache CreateUniCacheOfflineRefreshBackground(String applicationName)
        {
            return UniCache.CreateUniCacheOfflineRefreshBackground(applicationName);
        }


        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application which will be updated the same day.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="doBackgroundRefresh">If set to <c>true</c> the refresh of UniCache in background will be perfomed even if already updated the same day.</param>
        /// <returns>
        /// Returns an UniCache (online or offline) which has been updated the same day (if no errors occured).
        /// </returns>
        /// <remarks>
        /// This method uses the offline unicache if cache has been updated the same day, otherwise the application cache will be refreshed.
        /// <para>Uses <see cref="InitMode.OnceADay"/> which will refresh the cache if it is not refreshed the same day.</para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// </remarks>
        public UniCache CreateUniCacheUpdatedDaily(String applicationName, Boolean doBackgroundRefresh)
        {
            return UniCache.CreateUniCacheUpdatedDaily(applicationName, doBackgroundRefresh);
        }

        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application which will be updated the same day.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an UniCache (online or offline) which has been updated the same day (if no errors occured).
        /// </returns>
        /// <remarks>
        /// This method uses the offline unicache if cache has been updated the same day, otherwise the application cache will be refreshed.
        /// <para>Uses <see cref="InitMode.OnceADay"/> which will refresh the cache if it is not refreshed the same day.</para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// </remarks>
        public UniCache CreateUniCacheUpdatedDaily(String applicationName)
        {
            return UniCache.CreateUniCacheUpdatedDaily(applicationName, false);
        }

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
        public UniCache CreateUniCache(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            return UniCache.CreateUniCache(applicationName, GetInitMode(initMode), maxTime, serverName, defaultTimeoutServerRequests);
        }


        #region Methods to start UniCache in background

        private static Object m_LockObject = new Object();
        
        /// <summary>
        /// Current instance of UniCache that is created by <see cref="StartCreateUpdatedDaily"/>, <see cref="StartCreateRefreshBackground"/> or <see cref="StartCreateUniCache"/>.
        /// Value is <c>null</c> when <see cref="UniCacheFactory"/> is created. 
        /// When a <c>StartCreate</c>-method is called the Value will be set to an <c>UniCache</c>-instance in offline-mode. When the creation in background is completed the value will be set to the newly created <see cref="UniCache"/>-instance. 
        /// </summary>
        private UniCache m_UniCache = null;

        /// <summary>
        /// <c>true</c> if background refresh is completed, <c>false</c> otherwise.
        /// </summary>
        private Boolean m_IsComplete = false;

        /// <summary>
        /// Is needed to prevent the <see cref="UniCacheFactory.UniCache"/> get-property to be changed after beeing called for the first time.
        /// <c>false</c> if background refresh is completed or if <see cref="UniCacheFactory.UniCache"/> get-property has been called, <c>true</c> otherwise.
        /// </summary>
        private Boolean m_AllowChangeUniCacheProperty = true;

        /// <summary>
        /// <c>true</c> if background refresh of a <c>StartCreate</c>-method is pending, <c>false</c> otherwise.
        /// </summary>
        private Boolean m_IsStartCreateRefreshPending = false;

        /// <summary>
        /// Current instance of UniCache that is created by <see cref="StartCreateUpdatedDaily"/>, <see cref="StartCreateRefreshBackground"/> or <see cref="StartCreateUniCache"/>.
        /// Value is <c>null</c> when <see cref="UniCacheFactory"/> is created. 
        /// When a <c>StartCreate</c>-method is called the value will be set to an <c>UniCache</c>-instance in offline-mode. When the creation in background is completed the value will be set to the newly created <see cref="UniCache"/>-instance. 
        /// </summary>
        /// <value>
        /// Current instance of UniCache (offline- or online-mode).
        /// </value>
        public UniCache UniCache
        {
            get
            {
                lock (m_LockObject)
                {
                    m_AllowChangeUniCacheProperty = false;
                    return m_UniCache;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a background refresh is completed. Might be used to poll for termination of the background refresh.
        /// </summary>
        /// <value>
        /// <c>true</c> if background refresh is completed, <c>false</c> otherwise.
        /// </value>
        public Boolean IsComplete
        {
            get
            {
                lock (m_LockObject)
                {
                    return m_IsComplete;
                }
            }
        }

        /// <summary>
        /// Sets the properties <see cref="UniCacheFactory.UniCache" /> and <see cref="UniCacheFactory.IsComplete" />
        /// </summary>
        /// <param name="uniCache">The <see cref="UniCache" />-instance to set.</param>
        /// <param name="isRefreshCompletEvent">Set to <c>true</c> if this method is called from the <see cref="IUniCacheEvents.OnRefreshComplete" />-event-handler.</param>
        internal void SetUniCache(UniCache uniCache, Boolean isRefreshCompletEvent)
        {
            lock (m_LockObject)
            {
                if (m_AllowChangeUniCacheProperty)
                {
                    m_UniCache = uniCache;
                    m_AllowChangeUniCacheProperty = !isRefreshCompletEvent;
                    m_IsComplete = isRefreshCompletEvent;
                }

                if (isRefreshCompletEvent)
                {
                    m_IsStartCreateRefreshPending = false;
                }
            }
        }

        /// <summary>
        /// Resets the properties <see cref="UniCacheFactory.UniCache" /> to <c>null</c> and <see cref="UniCacheFactory.IsComplete" /> to <c>false</c>.
        /// </summary>
        /// <exception cref="ApplicationException">Reset not allowed while refresh is pending</exception>
        private void ResetAndStart()
        {
            lock (m_LockObject)
            {
                if (m_IsStartCreateRefreshPending)
                {
                    throw new ApplicationException("Reset not allowed while refresh is pending");
                }
                else
                {
                    m_UniCache = null;
                    m_AllowChangeUniCacheProperty = true;
                    m_IsComplete = false;
                    m_IsStartCreateRefreshPending = true;
                }
            }
        }

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application when cache is not updated the same day.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="doBackgroundRefresh">if set to <c>true</c> a refresh is forced and will be done even if the cache is already updated the same day.</param>
        /// <remarks>
        /// <para>
        /// Creates an <see cref="UniCache" />-instance in offline-mode which is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// Then a second <see cref="UniCache" />-instance is created using init-mode <see cref="InitMode.OnceADay" /> or <see cref="InitMode.AutoInit" /> when <c>doBackgroundRefresh</c> is <c>true</c>.
        /// Creating the second <see cref="UniCache" />-instance will refresh the cache if <c>doBackgroundRefresh</c> is <c>false</c> or if the cahce is not refreshed the same day. 
        /// When the creation of this second instance is completed it is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// </para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest" />-property of <see cref="Config.GlobalConfig" /> is used.</para>
        /// </remarks>
        public void StartCreateUpdatedDaily(String applicationName, Boolean doBackgroundRefresh)
        {
            ResetAndStart();
            if (doBackgroundRefresh)
            {
                UniCacheLib.UniCache.StartCreateUpdatedDaily(applicationName, this);
            }
            else
            {
                UniCacheLib.UniCache.StartCreateRefreshBackground(applicationName, this);
            }
        }

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <remarks>
        /// <para>
        /// Creates an <see cref="UniCache" />-instance in offline-mode which is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// Then a second <see cref="UniCache" />-instance is created using init-mode <see cref="InitMode.AutoInit"/>. When the creation of this second instance is completed it is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// </para>
        /// <para>As server the default SharePoint is used which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment (<see cref="ServerInfos.ServerDefault"/>)</para>
        /// <para>As time for a single server request the <see cref="Config.DefaultTimeoutServerRequest"/>-property of <see cref="Config.GlobalConfig"/> is used.</para>
        /// </remarks>
        public void StartCreateRefreshBackground(String applicationName)
        {
            ResetAndStart();
            UniCacheLib.UniCache.StartCreateRefreshBackground(applicationName, this);
        }

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application, init-mode, maximum creation time, server and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache" /> is initialized and refreshed (<see cref="InitMode" /> InitMode)</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <remarks>
        /// Creates an <see cref="UniCache" />-instance in offline-mode which is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// Then a second <see cref="UniCache" />-instance is created using the given init-mode. When the creation of this second instance is completed it is set in the <see cref="UniCacheFactory" />-instance and could be got calling the <see cref="UniCacheFactory.UniCache" /> property.
        /// </remarks>
        public void StartCreateUniCache(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests)
        {
            ResetAndStart();
            UniCacheLib.UniCache.StartCreateUniCache(applicationName, GetInitMode(initMode), maxTime, serverName, defaultTimeoutServerRequests, this);
        }
                
        #endregion

        #endregion
    }

}
