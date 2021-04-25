using System;  
using System.IO;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Threading; 
using System.Threading.Tasks;

namespace CSV2FLL {
    class ConvertServer {
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        HttpListener server;
        String url = "http://localhost:2002/";
        public ConvertServer() {
            server = new HttpListener();
            server.Prefixes.Add(url);
            server.Start();
            Console.WriteLine("Listening for connections on " + url);

            Task listenTask = incommingConnection();
            listenTask.GetAwaiter().GetResult();
        }

        public async Task incommingConnection() {
            Boolean running = true;
            
            while (running) {
                HttpListenerContext ctx = await server.GetContextAsync();
                HttpListenerRequest clientReq = ctx.Request;
                HttpListenerResponse clientResp = ctx.Response;

                Console.WriteLine(clientReq.Url.ToString());
                Console.WriteLine(clientReq.HttpMethod);
                Console.WriteLine(clientReq.UserHostName);
                Console.WriteLine(clientReq.UserAgent);
                Console.WriteLine();
            }
        }

    }
}