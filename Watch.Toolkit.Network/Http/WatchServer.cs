using System;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;

namespace Watch.Toolkit.Network.Http
{
    public class Server
    {
        public Server(string url = "http://localhost:8080")
        {
            using (WebApp.Start<WebService>(url))
            {
                Console.WriteLine("Server running on {0}", url);
                while (true) ;
            }
        }
    }
  
    public class MyHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }
    }
    internal class WebService
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });


            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(500);

            app.UseWebApi(config);
            app.MapSignalR();

            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }));
        }
    }
}
