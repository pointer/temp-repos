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
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.UniCacheFactory" />
    [ComVisible(true)]
    [Guid("738CC210-5F7B-482c-9765-4F260810C189")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IUniCacheFactory
    {
        /// <summary>
        /// Key/Identifier for SharePoint server in production environment (<see cref="ServerInfos.ServerPROD"/>)
        /// </summary>
        [DispId(0x01)]
        [Obsolete("ServerMOSS is deprecated, use ServerPROD instead.")]
        String ServerMOSS { get; }

        /// <summary>
        /// Key/Identifier for SharePoint server in production environment (<see cref="ServerInfos.ServerPROD"/>)
        /// </summary>
        [DispId(0x51)]
        String ServerPROD { get; }

        /// <summary>
        /// Key/Identifier for SharePoint server in test environment (<see cref="ServerInfos.ServerQA"/>)
        /// </summary>
        [DispId(0x02)]
        [Obsolete("ServerMOSSQC is deprecated, use ServerQA instead.")]
        String ServerMOSSQC { get; }

        /// <summary>
        /// Key/Identifier for SharePoint server in test environment (<see cref="ServerInfos.ServerQA"/>)
        /// </summary>
        [DispId(0x52)]
        String ServerQA { get; }

        
        /// <summary>
        /// Key/Identifier for SharePoint server in dev environment (<see cref="ServerInfos.ServerDEV"/>)
        /// </summary>
        [DispId(0x03)]
        String ServerDEV { get; }

        /// <summary>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerInfos.ServerPROD" /> in production and <see cref="ServerInfos.ServerQA" /> in test environment
        /// </summary>
        [DispId(0x04)]
        [Obsolete("ServerMOSSDefault is deprecated, use ServerDefault instead.")]
        String ServerMOSSDefault { get; }

        /// <summary>
        /// Key/Identifier for the default SharePoint server which is <see cref="ServerInfos.ServerPROD" /> in production and <see cref="ServerInfos.ServerQA" /> in test environment
        /// </summary>
        [DispId(0x54)]
        String ServerDefault { get; }

        
        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in production environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in production environment.</returns>
        [DispId(0x10)]
        [Obsolete("CreateSharePointHelperMOSS is deprecated, use CreateSharePointHelperPROD instead.")]
        SharePointHelper CreateSharePointHelperMOSS();

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in test environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in test environment.</returns>
        [DispId(0x11)]
        [Obsolete("CreateSharePointHelperMOSSQC is deprecated, use CreateSharePointHelperQA instead.")]
        SharePointHelper CreateSharePointHelperMOSSQC();

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in dev environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in dev environment.</returns>
        [DispId(0x12)]
        SharePointHelper CreateSharePointHelperDEV();

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in 
        /// production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in 
        /// production and <see cref="ServerQA" /> in test environment.</returns>
        [DispId(0x13)]
        [Obsolete("CreateSharePointHelperMOSSDefault is deprecated, use CreateSharePointHelperDefault instead.")]
        SharePointHelper CreateSharePointHelperMOSSDefault();



        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in production environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in production environment.</returns>
        [DispId(0x14)]
        SharePointHelper CreateSharePointHelperPROD();

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the SharePoint server in test environment.
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper"/> class for the SharePoint server in test environment.</returns>
        [DispId(0x15)]
        SharePointHelper CreateSharePointHelperQA();

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in 
        /// production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <returns>A new instance of the <see cref="SharePointHelper" /> class for the default SharePoint server which is <see cref="ServerPROD" /> in 
        /// production and <see cref="ServerQA" /> in test environment.</returns>
        [DispId(0x16)]
        SharePointHelper CreateSharePointHelperDefault();



        /// <summary>
        /// Gets the initialisation-mode (<see cref="InitMode"/>) from a corresponding mode-name.
        /// </summary>
        /// <param name="name">Name of the initialisation-mode.</param>
        /// <returns><see cref="InitMode"/> corresponding to <c>name</c></returns>
        [DispId(0x27)]
        InitMode GetInitMode(String name);

        /// <summary>
        /// Creates an <see cref="UniCache" /> using default SharePoint for a given application, init-mode, maximum creation time and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache"/> is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime
        /// an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" /> in online- or offline-mode.
        /// </returns>
        [DispId(0x26)]
        UniCache CreateUniCacheWithinTimespan(String applicationName, InitMode initMode, Int32 maxTime, Int32 defaultTimeoutServerRequests);

        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application, init-mode, server, maximum creation time and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache" /> is initialized and refreshed (<see cref="InitMode" /> InitMode)</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime 
        /// an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" /> in online- or offline-mode.
        /// </returns>
        [DispId(0x28)]
        UniCache CreateUniCacheWithinTimespanEx(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests);

        /// <summary>
        /// Creates an <see cref="UniCache" /> in offline-mode for a given application and the default SharePoint server (<see cref="ServerDefault"/>).
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns immediately an initialized <see cref="UniCache" /> in offline-mode.
        /// </returns>
        [DispId(0x23)]
        UniCache CreateUniCacheOffline(String applicationName);


        /// <summary>
        /// Creates an <see cref="UniCache" /> synchronously for a given application with init-mode <see cref="InitMode.AutoInit"/>.
        /// Server will be SP2013apps in production and SP2013apps.qa in test environment.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an initialized and refreshed <see cref="UniCache" />.
        /// </returns>
        [DispId(0x30)]
        UniCache CreateUniCacheSync(String applicationName);

        /// <summary>
        /// Creates an initialized but <c>not</c> refreshed <see cref="UniCache" /> for a given application which can be refreshed asynchronously afterwards.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns an initialized but <c>not</c> refreshed UniCache.
        /// </returns>
        [DispId(0x31)]
        UniCache InitUniCacheForAsyncRefresh(String applicationName);

        /// <summary>
        /// Creates an <see cref="UniCache" /> in offline-mode and refreshes the application cache in background. 
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>
        /// Returns immediately with an initialized <see cref="UniCache" /> in offline-mode.
        /// </returns>
        [DispId(0x32)]
        UniCache CreateUniCacheOfflineRefreshBackground(String applicationName);

        /// <summary>
        /// Creates an <see cref="UniCache" /> for a given application which will be updated the same day.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="doBackgroundRefresh">Performs refresh of UniCache in background if already updated today</param>
        /// <returns>
        /// Returns an UniCache (online or offline) which has been updated the same day (if no errors occurred).
        /// </returns>
        [DispId(0x33)]
        UniCache CreateUniCacheUpdatedDaily(String applicationName, Boolean doBackgroundRefresh);

        /// <summary>
        /// Creates an <see cref="UniCache"/> for a given application, init-mode, maximum creation time, server and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache"/> is initialized and refreshed (<see cref="InitMode"/> InitMode)</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime
        /// an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        /// <returns></returns>
        [DispId(0x34)]
        UniCache CreateUniCache(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests);


        /// <summary>
        /// Current instance of UniCache that is created by <see cref="StartCreateUpdatedDaily"/>, <see cref="StartCreateRefreshBackground"/> or
        /// <see cref="StartCreateUniCache"/>.
        /// Value is <c>null</c> when <see cref="UniCacheFactory"/> is created. 
        /// When a <c>StartCreate</c>-method is called the value will be set to an <c>UniCache</c>-instance in offline-mode. When the creation in
        /// background is completed the value will be set to the newly created <see cref="UniCache"/>-instance. 
        /// </summary>
        [DispId(0x40)]
        UniCache UniCache { get; }

        /// <summary>
        /// Gets a value indicating the whether background refresh is completed. Might be used to poll for termination of the background refresh.
        /// </summary>
        [DispId(0x41)]
        Boolean IsComplete { get; }

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        [DispId(0x44)]
        void StartCreateRefreshBackground(String applicationName);

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application when cache is not updated the same day.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="doBackgroundRefresh">if set to <c>true</c> a refresh is forced and will be done even if the cache is already updated the same day.</param>
        [DispId(0x45)]
        void StartCreateUpdatedDaily(String applicationName, Boolean doBackgroundRefresh);

        /// <summary>
        /// Starts an asynchronous refresh in background for a given application, init-mode, maximum creation time, server and timeout for a single server request.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="initMode">Determines how the <see cref="UniCache" /> is initialized and refreshed (<see cref="InitMode" /> InitMode)</param>
        /// <param name="serverName">Name of server UniCache should get files from</param>
        /// <param name="maxTime">The maximum time for initialising and refreshing. If initialising and refreshing exceeds the maxTime 
        /// an <see cref="UniCache" />-instance in offline-mode is returned.</param>
        /// <param name="defaultTimeoutServerRequests">The timeout for a single server request.</param>
        [DispId(0x46)]
        void StartCreateUniCache(String applicationName, String initMode, String serverName, Int32 maxTime, Int32 defaultTimeoutServerRequests);

    }

}
