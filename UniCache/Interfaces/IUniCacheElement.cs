using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// COM interface of the <see cref="UniCacheElement" />-class
    /// </summary>
    /// <remarks>
    /// This explicit interface definition is needed to keep full control over the COM interface used by early binding COM clients!
    /// </remarks>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.UniCacheElement" />
    [ComVisible(true)]
    [Guid("431FAAB1-CE91-49a7-9C48-8FF4C6A73C6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IUniCacheElement
    {

        /// <summary>
        /// Gets an information about the file in the cache.
        /// </summary>
        /// <value>
        /// The information about the file in the cache.
        /// </value>
        [DispId(0x00)]
        String CacheFileInfo { get; }

        /// <summary>
        /// Gets the file version.
        /// </summary>
        /// <value>
        /// The file version.
        /// </value>
        [DispId(0x01)]
        String Version { get; }

        //[DispId(0x02)]
        //Boolean MarkedForRefresh { get; set; }

        /// <summary>
        /// Gets the logical filename.
        /// </summary>
        /// <value>
        /// The logical filename.
        /// </value>
        [DispId(0x03)]
        String LogicalFilename { get; }

        /// <summary>
        /// Gets the server relative filename.
        /// </summary>
        /// <value>
        /// The server relative filename.
        /// </value>
        [DispId(0x04)]
        String ServerRelativeFilename { get; }

        /// <summary>
        /// Gets the full URL of the cache file on the server.
        /// </summary>
        /// <value>
        /// The full URL of the cache file on the server.
        /// </value>
        [DispId(0x05)]
        String ServerFilename { get; }

        /// <summary>
        /// Returns the time of the last refresh/download (or <c>DateTime.MinValue</c> if not refreshed)
        /// </summary>
        /// <value>
        /// The last refresh time.
        /// </value>
        [DispId(0x06)]
        DateTime LastRefresh { get; }

        /// <summary>
        /// Determines whether the file corresponding to this UniCacheElement exists the in cache (which is independent of being up-to-date).
        /// </summary>
        /// <value>
        /// <c>true</c> if the file corresponding to this UniCacheElement exists the in cache; otherwise, <c>false</c>.
        /// </value>
        [DispId(0x07)]
        Boolean ExistsInCache { get; }
        
        //[DispId(0x08)]
        //ElementStatus Status { get; }

        /// <summary>
        /// Gets or sets the timeout for downloading the file from the server.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        [DispId(0x09)]
        Int32 Timeout { get; set; }

        /// <summary>
        /// Gets the (full) filename (with path) of the cached (uncopied) file.
        /// </summary>
        /// <param name="forceDownload">if set to <c>true</c> the file will be downloaded even if already cached.</param>
        /// <returns>
        /// Filename with path of the cached file
        /// </returns>
        [DispId(0x10)]
        String GetCacheFilename(Boolean forceDownload);

        /// <summary>
        /// Gets the (full) filename (with path) of a copy of the cached file.
        /// </summary>
        /// <param name="forceDownload">if set to <c>true</c> the file will be downloaded even if already cached.</param>
        /// <param name="destinationFilename">The destination filename of the cached file copy.</param>
        /// <returns>If successful the destination filename of the cached file copy will be returned; <c>null</c> otherwise</returns>
        [DispId(0x11)]
        String GetCacheFilenameCopy(Boolean forceDownload, String destinationFilename);

        /// <summary>
        /// Increments the usage statistic of this file, if statistic auto increment for this file is set to <c>true</c> (see content-xml) 
        /// or if it is forced by parameter
        /// </summary>
        /// <param name="forceIncStatistic">if set to <c>true</c> usage statistic of this file will be incremented even if auto increment is set to <c>false</c>.</param>
        [DispId(0x12)]
        void IncStatistic(Boolean forceIncStatistic);

    }


}
