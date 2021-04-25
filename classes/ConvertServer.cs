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
        String path;
        String[] webFiles;
        public ConvertServer() {
            this.path = getPath();
            this.webFiles = cacheHTML(path);
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

                if (clientReq.HttpMethod == "GET") {
                    if (clientReq.Url.AbsolutePath == "/") {
                        String pageInfo = loadPageData(path + "webapp\\index.html");
                        byte[] data = Encoding.UTF8.GetBytes(pageInfo);

                        clientResp.ContentType = "text/html";
                        clientResp.ContentEncoding = Encoding.UTF8;
                        clientResp.ContentLength64 = data.LongLength;

                        await clientResp.OutputStream.WriteAsync(data, 0, data.Length);
                        clientResp.Close();
                    } else {
                        //Add dynamic handling of GET requests based on the files cached.
                        //Should allow for the files to easily be served based on what has 
                        //been written in the webapp folder without having to hardcode it all.
                    }
                }
            }
        }

        public String loadPageData(String filepath) {
            String[] file = System.IO.File.ReadAllLines(@filepath);
            String output = "";
            foreach (String item in file) {
                output += item + "\n";
            }
            return output;
        }

        public String[] cacheHTML(String filePath) {
            filePath = filePath + "\\webapp\\";
            String[] files = System.IO.Directory.GetFiles(filePath);
            String[] output = new String[files.Length];
            for (int i = 0; i < files.Length; i++) {
                String[] temp = files[i].Split("\\");
                output[i] = temp[temp.Length - 1];
            }

            return output;
        }

        private String getPath() {
            String tempPath = System.IO.Path.GetFullPath("Program.cs");
            String[] updatedPath = tempPath.Split("\\");
            tempPath = "";

            for (int i = 0; i < updatedPath.Length - 1; i++) {
                tempPath += updatedPath[i] + "\\";
            }
            return tempPath;
        }

    }
}