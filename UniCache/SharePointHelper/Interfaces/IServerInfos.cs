using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Web;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// COM interface of the <see cref="ServerInfos"/>-class
    /// </summary>
    /// <remarks>This explicit interface definition is needed to keep full control over the COM interface used by early binding COM clients!</remarks>
    [ComVisible(true)]
    [GuidAttribute("1D035A04-D0AB-4ff5-9214-EB5D330262D7")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IServerInfos : IEnumerable
    {

        /// <summary>
        /// COM-visible indexer-property to get the <see cref="ServerInfo" /> with the specified servername (=id/key).
        /// </summary>
        [DispId(0x00)]
        ServerInfo this[String servername] { get; }

        /// <summary>
        /// COM-visible property to get key/identifier for SharePoint server in production environment
        /// </summary>
        [Obsolete("ServerNameMOSS is deprecated, use ServerNamePROD instead.")] 
        [DispId(0x01)]
        String ServerNameMOSS { get; }

        /// <summary>
        /// COM-visible property to get key/identifier for SharePoint server in test environment
        /// </summary>
        [Obsolete("ServerNameMOSSQC is deprecated, use ServerNameQA instead.")]
        [DispId(0x02)]
        String ServerNameMOSSQC { get; }

        /// <summary>
        /// COM-visible property to get key/identifier for the default SharePoint server which is <see cref="ServerInfos.ServerPROD" /> in production and <see cref="ServerInfos.ServerQA" /> in test environment
        /// </summary>
        [Obsolete("ServerNameMOSSDefault is deprecated, use ServerNameDefault instead.")]
        [DispId(0x04)]
        String ServerNameMOSSDefault { get; }

        /// <summary>
        /// COM-visible property to get key/identifier for SharePoint server in test environment
        /// </summary>
        [DispId(0x03)]
        String ServerNameDEV { get; }


        /// <summary>
        /// COM-visible property to get key/identifier for SharePoint server in production environment
        /// </summary>
        [DispId(0x10)]
        String ServerNamePROD { get; }

        /// <summary>
        /// COM-visible property to get key/identifier for SharePoint server in test environment
        /// </summary>
        [DispId(0x11)]
        String ServerNameQA { get; }

        /// <summary>
        /// COM-visible property to get key/identifier for the default SharePoint server which is <see cref="ServerInfos.ServerNamePROD" /> in production and <see cref="ServerInfos.ServerNameQA" /> in test environment
        /// </summary>
        [DispId(0x12)]
        String ServerNameDefault { get; }
    }

}
