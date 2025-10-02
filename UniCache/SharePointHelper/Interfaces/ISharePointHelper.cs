using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// COM interface of the <see cref="SharePointHelper"/>-class
    /// </summary>
    /// <remarks>This explicit interface definition is needed to keep full control over the COM interface used by early binding COM clients!</remarks>
    [ComVisible(true)]
    [GuidAttribute("10E1ACF7-4D05-4e9a-B723-2E31179058B6")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ISharePointHelper
    {
        /// <summary>
        /// <see cref="UGIS.de.OfficeComponents.UniCacheLib.ServerInfo"/>-instance providing server-related data defining which server will be used for download/upload of files.
        /// (see <see cref="SharePointHelper.ServerInfo"/>)
        /// </summary>
        [DispId(0x00)]
        ServerInfo ServerInfo { get; }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in production environment (see <see cref="SharePointHelper.ServerNamePROD"/>)
        /// </summary>
        [Obsolete("ServerNameMOSS is deprecated, use ServerNamePROD instead.")]
        [DispId(0x01)]
        String ServerNameMOSS { get; }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in production environment (see <see cref="SharePointHelper.ServerNamePROD"/>)
        /// </summary>
        [DispId(0x41)]
        String ServerNamePROD { get; }


        /// <summary>
        /// The server name used as key/identifier for SharePoint server in test environment (see <see cref="SharePointHelper.ServerNameQA"/>)
        /// </summary>
        [Obsolete("ServerNameMOSSQC is deprecated, use ServerNameQA instead.")]
        [DispId(0x02)]
        String ServerNameMOSSQC { get; }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in test environment (see <see cref="SharePointHelper.ServerNameQA"/>)
        /// </summary>
        [DispId(0x42)]
        String ServerNameQA { get; }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in dev environment (see <see cref="SharePointHelper.ServerNameDEV"/>)
        /// </summary>
        [DispId(0x03)]
        String ServerNameDEV { get; }

        /// <summary>
        /// The default timeout for server requests when downloading files (see <see cref="SharePointHelper.DefaultTimeout"/>).
        /// </summary>
        [DispId(0x04)]
        Int32 DefaultTimeout { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether default credentials have to be used when downloading/uploading files (see <see cref="SharePointHelper.UseDefaultCredentials"/>).
        /// </summary>
        [DispId(0x10)]
        Boolean UseDefaultCredentials { get; set; }

        /// <summary>
        /// Sets <see cref="ServerInfo"/> (see <see cref="SharePointHelper.SetServerInfo"/>). 
        /// </summary>
        [DispId(0x11)]
        void SetServerInfo(String servername);

        /// <summary>
        /// Increments the UniCache statistic (see <see cref="SharePointHelper.IncStatistic"/>)
        /// </summary>
        [DispId(0x12)]
        Boolean IncStatistic(String statisticPath);


        /// <summary>
        /// Increments the UniCache statistic for an application (see <see cref="SharePointHelper.IncUniCacheStatistic"/>).
        /// </summary>
        [DispId(0x13)]
        Boolean IncUniCacheStatistic(String application, String attribName);

        /// <summary>
        /// Increments the UniCache statistic (see <see cref="SharePointHelper.IncStatistic"/>)
        /// </summary>
        [DispId(0x14)]
        void IncStatisticAsync(String statisticPath);
        
        /// <summary>
        /// Gets a file stream (see <see cref="SharePointHelper.GetFileStream(String)"/>).
        /// </summary>
        [DispId(0x20)]
        Stream GetFileStream(String fileUrl);

        /// <summary>
        /// Gets a file stream (see <see cref="SharePointHelper.GetFileStreamTimeout"/>).
        /// </summary>
        [DispId(0x21)]
        Stream GetFileStreamTimeout(String fileUrl, Int32 timeout);

        /// <summary>
        /// Gets a xml-document (see <see cref="SharePointHelper.GetXml"/>).
        /// </summary>
        [DispId(0x22)]
        XmlDocument GetXml(String fileUrl);

        /// <summary>
        /// Gets a xml-document (see <see cref="SharePointHelper.GetXmlTimeout"/>).
        /// </summary>
        [DispId(0x23)]
        XmlDocument GetXmlTimeout(String fileUrl, Int32 timeout);
        
        /// <summary>
        /// Gets a file as ASCII-encoded string (see <see cref="SharePointHelper.GetFileASCII"/>).
        /// </summary>
        [DispId(0x24)]
        String GetFileASCII(String fileUrl, Int32 timeout);

        /// <summary>
        /// Gets a file as UTF8-encoded string (see <see cref="SharePointHelper.GetFileUTF8"/>).
        /// </summary>
        [DispId(0x25)]
        String GetFileUTF8(String fileUrl, Int32 timeout);

        /// <summary>
        /// Gets a file as Unicode-encoded string (see <see cref="SharePointHelper.GetFileUnicode"/>).
        /// </summary>
        [DispId(0x26)]
        String GetFileUnicode(String fileUrl, Int32 timeout);

        /// <summary>
        /// Uploads a file using WebDAV or FrontPage-RPC (see <see cref="SharePointHelper.UploadFile"/>).
        /// </summary>
        [DispId(0x30)]
        Boolean UploadFile(String webUrl, String documentName, Byte[] content);

        /// <summary>
        /// Uploads a file using WebDAV (see <see cref="SharePointHelper.UploadFileWebDAV"/>).
        /// </summary>
        [DispId(0x31)]
        Boolean UploadFileWebDAV(String fileUrl, Byte[] content);

        /// <summary>
        /// Uploads a file using FrontPage-RPC (see <see cref="SharePointHelper.UploadFileFPRPC(String, String, Byte[])"/>).
        /// </summary>
        [DispId(0x32)]
        Boolean UploadFileFPRPC(String webUrl, String documentName, Byte[] bytes);


    }

}
