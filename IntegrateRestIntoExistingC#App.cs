
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
// Define Service Contract (Interface)
namespace YourNamespace
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/GetSoftwareVersion", ResponseFormat = WebMessageFormat.Json)]
        string GetSoftwareVersion();

        // Add more methods with RESTful attributes
        [OperationContract]
        [WebInvoke(
            Method = "POST", 
            UriTemplate = "/UpdateItem", 
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json
        )]
        bool UpdateItem(ItemData item);
    }
}

//Implement the Service

public class Service : IService
{
    public string GetSoftwareVersion()
    {
        // Call existing logic from your app
        return ExistingApp.GetVersion(); 
    }

    public bool UpdateItem(ItemData item)
    {
        // Map to existing business logic
        return ExistingApp.UpdateItem(item.Id, item.Value);
    }
}

// Host the Service
// Option A: Self-host in Console/Windows Service
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

class Program
{
    static void Main()
    {
        var baseAddress = new Uri("http://localhost:10870/");
        
        using (var host = new ServiceHost(typeof(Service), baseAddress))
        {
            var endpoint = host.AddServiceEndpoint(
                typeof(IService),
                new WebHttpBinding(),
                ""
            );
            
            endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
            
            host.Open();
            Console.WriteLine($"REST Service running at {baseAddress}");
            Console.WriteLine("Press ENTER to stop...");
            Console.ReadLine();
        }
    }
}

// self-hosting option for a WCF REST service in an existing C# application (using .NET Framework).
//  Steps for self-hosting:
//  1. Define the service contract (interface) with [ServiceContract] and methods with [OperationContract] and REST-specific attributes (like [WebGet]).
//  2. Implement the service contract in a class (the service class) that uses the existing application logic.
//  3. In the host (e.g., a console application, Windows service, or even a Windows Forms application), create a `ServiceHost` instance.
//  4. Configure the endpoint with `WebHttpBinding` and add the `WebHttpBehavior` to the endpoint.
//  5. Open the host and keep it running.
//  Let me summarize the self-hosting steps in a concise manner.
// Self-Hosting WCF REST Service: Summary
// Here's a concise overview of self-hosting a REST service in your existing C# app using WCF:

// 1. Key Components
// ServiceHost: Runtime container for your service

// WebHttpBinding: Binding for REST/HTTP communication

// WebHttpBehavior: Enables REST-style endpoints (URIs, JSON/XML)

// 2. Implementation Steps
// Step 1: Define service contract (interface)
[ServiceContract]
public interface IService
{
    [WebGet(UriTemplate = "/GetVersion", ResponseFormat = WebMessageFormat.Json)]
    string GetSoftwareVersion();
}

// Step 2: Implement service
public class Service : IService
{
    public string GetSoftwareVersion() => ExistingApp.GetVersion(); // Call your existing logic
}

// Step 3: Host in application
var baseUri = new Uri("http://localhost:10870/");
using (var host = new ServiceHost(typeof(Service), baseUri))
{
    var endpoint = host.AddServiceEndpoint(
        typeof(IService),
        new WebHttpBinding(),  // REST binding
        ""                     // Relative address (appends to baseUri)
    );
    
    endpoint.Behaviors.Add(new WebHttpBehavior()); // Critical for REST
    
    host.Open();  // Start listening
    Console.WriteLine($"REST API live at {baseUri}");
    Console.ReadLine();  // Keep host running
}

// 3. Key Configuration Details
// Element	Purpose
// baseUri	Root address (e.g., http://localhost:10870/)
// WebHttpBinding()	Enables HTTP/HTTPS communication with REST semantics
// WebHttpBehavior	Translates URI templates to method calls (e.g., /GetVersion → GetSoftwareVersion())
// host.Open()	Activates the service endpoint
// 4. Endpoint Examples
// GET Request:
// http://localhost:10870/GetVersion → Returns "1.0.3" (JSON)

// POST Request (add to interface):

[WebInvoke(Method="POST", UriTemplate="/Update", RequestFormat=WebMessageFormat.Json)]
bool UpdateItem(ItemData data);
// Call via: POST http://localhost:10870/Update with JSON body

// 5. When to Use Self-Hosting
// ✅ Console apps (dev/debugging)

// ✅ Windows Services (production)

// ✅ Desktop apps (WinForms/WPF with background hosting)

// ❌ IIS-hosted apps (use .svc file instead)

// 6. Critical Notes
// Run as Admin: Required for binding to HTTP ports (or reserve URL with netsh)

// Thread Safety: Ensure your existing app logic is thread-safe

// Lifetime Management: Wrap ServiceHost in using or handle shutdown properly

// Firewall: Allow incoming connections on your port (10870 in example)

// 7. Shutdown Sequence
// Graceful shutdown in Windows Service/Desktop app
protected override void OnStop()
{
    host?.Close();  // Release resources
}
// Self-hosting adds REST capabilities to your existing codebase with minimal refactoring – 
// just wrap existing logic in a WCF service class and configure the host. For .NET Core/5+, 
// consider transitioning to ASP.NET Core minimal APIs for modern REST support.

// First, create a new public interface for the service contract:                      
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace bob
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/GetSoftwareVersion", ResponseFormat=WebMessageFormat.Json)]
        String GetSoftwareVersion();
    }
}
// Next, I define a class to implement this interface and therefore my REST services
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace bob
{
    public class RestService : IService
    {
        /// <summary>
        /// Gets the version
        /// </summary>
        /// <returns>The Version String</returns>
        public String GetSoftwareVersion()
        {
            return App.Instance.GetSoftwareVersion();
        }
    }
}                      

// Next, the app.config needs to be tweaked:

// XML
// <system.serviceModel>
//     <services>
//         <service name="bob.RestService">
//             <endpoint binding="webHttpBinding" contract="bob.IService"
//                       behaviorConfiguration="webHttp"/>
//         </service>
//     </services>
//     <behaviors>
//         <endpointBehaviors>
//             <behavior name="webHttp">
//                 <webHttp/>
//             </behavior>
//         </endpointBehaviors>
//     </behaviors>
// </system.serviceModel>
// Things of note are the service name (bob.RestService) and endpoint contract (bob.IService). 
//Point the service name at the REST implementation class and the contract at the interface.

// I then "borrowed" the ThreadedServiceHost class from 
        //http://www.codeproject.com/Articles/16299/Hosting-WCF-services-in-a-Windows-Forms-Applicatio, 
//changed the ServiceHost class reference to WebServiceHost, 
//updated the project reference, removed the endpoint and associated code (simplified things), 
// and made a call to start it from the App() (constructor) ala:

// C#
// Fire up REST web services.
restHost = new ThreadedWebServiceHost<RestService>(Properties.Settings.Default.REST_ENDPOINT);
// REST_ENDPOINT is simply the base URI where my services are located (http://localhost:10870/App). 
// Compile and point your browser at http://localhost:10870/App/GetSoftwareVersion. You should see the result on screen.

// Refs:
// http://www.codeproject.com/Articles/148762/NET-4-0-RESTful-Web-Service-Introduction
// http://www.codeproject.com/Articles/16299/Hosting-WCF-services-in-a-Windows-Forms-Applicatio
// http://msdn.microsoft.com/en-us/library/system.servicemodel.servicehost.aspx
// http://msdn.microsoft.com/en-us/library/dd203052.aspx
// http://msdn.microsoft.com/en-us/library/system.servicemodel.web.webservicehost.aspx


