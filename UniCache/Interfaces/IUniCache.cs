using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// COM interface for events of the <see cref="UniCache" />-class
    /// </summary>
    [ComVisible(true)]
    [Guid("1F348467-04D0-4d1c-A019-A0A2794D1E16")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IUniCacheEvents
    {
        /// <summary>
        /// Called when background refresh of UniCache is completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RefreshCompleteEventArgs"/> instance containing the event data.</param>
        void OnRefreshComplete(object sender, RefreshCompleteEventArgs e);

        /// <summary>
        /// Called when a cached file is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CacheElementLoadedEventArgs"/> instance containing the event data.</param>
        void OnCacheElementLoaded(object sender, CacheElementLoadedEventArgs e);
    }


    /// <summary>
    /// COM interface of the <see cref="UniCache" />-class
    /// </summary>
    /// <remarks>
    /// This explicit interface definition is needed to keep full control over the COM interface used by early binding COM clients!
    /// </remarks>
    [ComVisible(true)]
    [Guid("A8FB84F6-69F1-44d7-8373-B50EB4EE424D")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IUniCache 
    {

        /// <summary>
        /// Gets an enumerator to iterate through the collection of elements contained in the cache (<see cref="UniCacheElement"/>-instances).
        /// </summary>
        /// <returns>An enumerator to iterate through the collection of elements contained in the cache (<see cref="UniCacheElement"/>-instances)</returns>
        [DispId(-4)]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        [DispId(0x00)]
        String CacheName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is offline, e.g. uses cached data on client without having connected to the server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        [DispId(0x01)]
        Boolean IsOffline { get; }

        /// <summary>
        /// Gets the status of this UniCache-instance
        /// </summary>
        /// <value>
        /// The status of this UniCache-instance.
        /// </value>
        [DispId(0x02)]
        CacheStatus Status { get; }

        /// <summary>
        /// Gets the <see cref="SharePointHelper"/>-instance used by this <see cref="UniCache"/>-instance.
        /// </summary>
        /// <value>
        /// The <see cref="SharePointHelper"/>-instance used by this <see cref="UniCache"/>-instance.
        /// </value>
        [DispId(0x03)]
        SharePointHelper SharePointHelper { get; }

        /// <summary>
        /// Gets the URL of the applications SharePoint folder where the files to be cached are located. 
        /// </summary>
        /// <value>
        /// The URL of the applications SharePoint folder where the files to be cached are located.
        /// </value>
        [DispId(0x04)]
        String ApplicationsCacheUrl { get; }

        /// <summary>
        /// Gets the last update time of this <see cref="UniCacheElements"/>-instance.
        /// </summary>
        /// <value>
        /// The last update time which is the last-write-time of the cached content-xml.
        /// </value>
        [DispId(0x05)]
        DateTime LastUpdateTime { get;}

        /// <summary>
        /// Gets or sets a value requesting an asynchronous refresh to be aborted.
        /// </summary>
        /// <value>
        /// <c>true</c> if asynchronous refresh should be aborted; otherwise, <c>false</c>.
        /// </value>
        [DispId(0x06)]
        Boolean AbortRefreshing { get; set; }

        /// <summary>
        /// Gets or sets the default timeout for a single server request.
        /// </summary>
        /// <value>
        /// The default timeout.
        /// </value>
        [DispId(0x07)]
        Int32 DefaultTimeout { get; set; }

        /// <summary>
        /// Gets the version of UniCacheLib.dll.
        /// </summary>
        /// <value>
        /// The version of UniCacheLib.dll.
        /// </value>
        [DispId(0x08)]
        String Version { get; }


        /// <summary>
        /// Flushes the log.
        /// </summary>
        [DispId(0x10)]
        void FlushLog();

        /// <summary>
        /// Refreshes this already initialized <see cref="UniCache"/>-instance asynchronously (=in background)
        /// </summary>
        [DispId(0x11)]
        void AsyncRefresh();

        //[DispId(0x20)]
        //ElementStatus FileStatus(String logicalFilename);

        /// <summary>
        /// Determines whether the specified file exists the in cache
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <returns>
        /// Returns <c>true</c>, if file exists; otherwise <c>false</c>.
        /// </returns>
        [DispId(0x21)]
        Boolean ExistsInCache(String logicalFilename);

        /// <summary>
        /// Gets the filename of the cached file.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <param name="forceDownload">Set to <c>true</c> when the file should be downloaded even if it already exists in cache.</param>
        /// <returns>
        /// Filename of the cached file
        /// </returns>
        [DispId(0x22)]
        String GetCacheFilename(String logicalFilename, Boolean forceDownload);

        /// <summary>
        /// Gets the filename of the cached file or a copy. Copying the cached file can be used to avoid access conflicts.
        /// The copied file will not be deleted by <see cref="UniCache "/> when cleaning the cache. The caller is responsible for deleting the copied 
        /// file if it is not needed any more.
        /// </summary>
        /// <returns>
        /// Filename of the cached file or the copy (see parameter <c>createCopy</c>)
        /// </returns>
        [DispId(0x23)]
        String GetCacheFilenameCopy(String logicalFilename, Boolean forceDownload, String destinationFilename);

        /// <summary>
        /// Gets the <see cref="UniCacheElement"/>-instance for the specified cache-element.
        /// </summary>
        /// <param name="logicalFilename">Identifier of the file used by the application (see content-xml)</param>
        /// <returns>
        /// The <see cref="UniCacheElement"/>-instance for the specified cache-element.
        /// </returns>
        [DispId(0x24)]
        UniCacheElement GetCacheElement(String logicalFilename);
    }


    /// <summary>
    /// Status of a UniCache instance
    /// </summary>
    [ComVisible(true)]
    [Guid("F804478C-1374-444f-994A-7BDC9A5403A4")]
    public enum CacheStatus
    {
        /// <summary>
        /// Status of a UniCache instance while initializing
        /// </summary>
        Initializing,
        /// <summary>
        /// Status of a UniCache instance when initialized
        /// </summary>
        Initialized,
        /// <summary>
        /// Status of a UniCache instance while refreshing (after initialization)
        /// </summary>
        Refreshing,
        /// <summary>
        /// Status of a UniCache instance when refresh is completed and UniCache is ready to use
        /// </summary>
        Complete,
        /// <summary>
        /// Status of a UniCache instance when ready to use but in offline mode
        /// </summary>
        Offline
    }


    /// <summary>
    /// Mode how to initialize and refresh an <see cref="UniCache"/>-instance
    /// InitializeOnly: Does not load any files (for use with AsyncRefresh)
    /// Offline: Loads only the client content-xml and doesn't download any server file
    /// AutoInit: Loads content-xmls (from server and client) and content files.
    /// </summary>
    [ComVisible(true)]
    [Guid("BD3B287C-3B8C-4595-8994-988880D8CB88")]
    public enum InitMode
    {
        /// <summary>
        /// Initializes the <see cref="UniCache"/>-instance synchronously. Does not load any files. Is used to initialise an <see cref="UniCache"/>-instance 
        /// to be refreshed with <see cref="UniCache.AsyncRefresh()"/>.
        /// </summary>
        InitializeOnly = 0,
        
        /// <summary>
        /// Loads only the client content-xml and doesn't download any server file
        /// </summary>
        Offline = 1,
        
        ///// <summary>
        ///// obsolete
        ///// </summary>
        //NoAutoInit = 2, 
        
        /// <summary>
        /// AutoInit, loads content-xmls (from server and client) and replicates all files.
        /// </summary>
        AutoInit = 3,
        
        /// <summary>
        /// Returns with an offline UniCache if last replication has been this day. Otherwise it starts with AutoInit
        /// </summary>
        OnceADay = 4
    }


    /// <summary>
    /// COM interface of the <see cref="RefreshCompleteEventArgs" />-class
    /// </summary>
    [ComVisible(true)]
    [Guid("26341B3A-1A29-47a2-8E4D-6FFB7F06F6E7")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IRefreshCompleteEventArgs
    {
        /// <summary>
        /// Gets the <see cref="UniCache"/>-instance raising the event
        /// </summary>
        /// <value>
        /// The <see cref="UniCache"/>-instance raising the event
        /// </value>
        UniCache UniCache { get; }

        /// <summary>
        /// Result of the background refresh which will be <c>true</c> if UniCache is refreshed successfully. The result will be <c>false</c> if
        /// an error occurs or in all cases of falling into offline mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if refresh has been successful; otherwise <c>false</c>.
        /// </value>
        Boolean Result { get; }
    }


    /// <summary>
    /// COM interface of the <see cref="CacheElementLoadedEventArgs" />-class
    /// </summary>
    [ComVisible(true)]
    [Guid("38494BA0-061B-47ee-8B92-777B074A6567")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ICacheElementLoadedEventArgs
    {
        /// <summary>
        /// Gets the <see cref="UniCache"/>-instance raising the event
        /// </summary>
        /// <value>
        /// The <see cref="UniCache"/>-instance raising the event
        /// </value>
        UniCache UniCache { get; }

        /// <summary>
        /// Gets the loaded <see cref="UniCacheElement"/>
        /// </summary>
        /// <value>
        /// The loaded <see cref="UniCacheElement"/>
        /// </value>
        UniCacheElement LoadedCacheElement { get; }

        /// <summary>
        /// Gets a value indicating whether loading the file of <see cref="LoadedCacheElement" /> was successful, e.g. if the file is cached or missing.
        /// </summary>
        /// <value>
        /// <c>true</c> if the file of the <see cref="LoadedCacheElement" /> is cached; otherwise, <c>false</c>.
        /// </value>
        Boolean Result { get; }
    }


}
