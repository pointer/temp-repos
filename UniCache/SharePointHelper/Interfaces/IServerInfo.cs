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
    /// COM interface of the <see cref="ServerInfo"/>-class
    /// </summary>
    /// <remarks>This explicit interface definition is needed to keep full control over the COM interface used by early binding COM clients!</remarks>
    [ComVisible(true)]
    [GuidAttribute("19B973EE-65E9-4b47-B94F-4710E3C2BD4E")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IServerInfo
    {

        /// <summary>
        /// COM-visible property to get the servername (used as key/id)
        /// </summary>
        [DispId(0x00)]
        String Servername { get; }

        /// <summary>
        /// COM-visible property to get the servers SerVo-site-root URL
        /// </summary>
        [DispId(0x01)]
        String ServoRootUrl { get; }

    }

}
