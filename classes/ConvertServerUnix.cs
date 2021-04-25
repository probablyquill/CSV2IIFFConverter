using System;  
using System.IO;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Threading; 
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSV2FLL {
    class ConvertServerUnix {
        HttpListener server;
        String url = "http://localhost:2002/";
        String path;
        String[] webFiles;
        public ConvertServerUnix() {
            this.path = getPath();
            this.webFiles = cacheHTML(path);
            server = new HttpListener();
            server.Prefixes.Add(url);
            server.Start();
            Console.WriteLine("Listening for connections on " + url);
            startBrowser(url);
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
                        String pageInfo = loadPageData(path + "webapp/index.html");
                        byte[] data = Encoding.UTF8.GetBytes(pageInfo);

                        clientResp.ContentType = "text/html";
                        clientResp.ContentEncoding = Encoding.UTF8;
                        clientResp.ContentLength64 = data.LongLength;

                        await clientResp.OutputStream.WriteAsync(data, 0, data.Length);
                        clientResp.Close();
                    } else {
                        String[] temp = clientReq.Url.AbsolutePath.Split(".");
                        String extension = temp[temp.Length - 1];
                        String fileToServe = "";
                        int fileLocation = -1;

                        switch (extension) {
                            case "ico":
                            break;
                            case "js":
                            fileLocation = searchArray(this.webFiles, clientReq.Url.AbsolutePath);
                            clientResp.ContentType = "text/js";
                            break;
                            case "css":
                            fileLocation = searchArray(this.webFiles, clientReq.Url.AbsolutePath);
                            clientResp.ContentType = "text/css";
                            break;
                            default:
                            fileLocation = searchArray(this.webFiles, clientReq.Url.AbsolutePath + ".html");
                            clientResp.ContentType = "text/html";
                            break;
                        }

                        if (fileLocation == -1) {
                            fileToServe = "Error: 404";
                            clientResp.ContentType = "text/html";
                        } else {
                            fileToServe = loadPageData(path + "/webapp/" + this.webFiles[fileLocation]);
                        }

                        byte[] data = Encoding.UTF8.GetBytes(fileToServe);
                        clientResp.ContentEncoding = Encoding.UTF8;
                        clientResp.ContentLength64 = data.LongLength;

                        await clientResp.OutputStream.WriteAsync(data, 0, data.Length);
                        clientResp.Close();
                        //Dynamically handles pulling html, css, ico, and js files from the server by
                        //Caching a list of files in the webapp folder and searching the GET queries
                        //against it.
                    }
                } else if (clientReq.HttpMethod == "POST") {
                    if (clientReq.Url.AbsolutePath == "/convert_data") { 
                        Console.WriteLine("Convert request recieved.");
                        StreamReader reader = new StreamReader(clientReq.InputStream, clientReq.ContentEncoding);
                        String dataSent = reader.ReadToEnd();
                        
                        String[] splitData = dataSent.Split("|");
                        String csvText = splitData[splitData.Length - 1];
                        String[] splitCSV = csvText.Split("\n");
                        String filename = splitData[0].Replace(" ", "").Replace(",", "").ToLower();
                        filename += splitData[1].Replace("/", "") + "-" + splitData[2].Replace("/", "") + ".iff";

                        Console.WriteLine("LOOK HERE");
                        for (int i = 0; i < splitData.Length; i++) {
                            Console.WriteLine(splitData[i]);
                        }

                        CSVParser parser = new CSVParser();
                        TotalData dataFound = parser.parseData(splitCSV);
                        EditedData culledData = parser.cullData(splitData[1], splitData[2], dataFound);
                        String textResponse = parser.generateIFF(culledData, filename, splitData[0]);
                        
                        byte[] data = Encoding.UTF8.GetBytes(textResponse);
                        clientResp.ContentType = "text";
                        clientResp.ContentEncoding = Encoding.UTF8;
                        clientResp.ContentLength64 = data.LongLength;
                        clientResp.StatusCode = (int) HttpStatusCode.OK;
                        clientResp.StatusDescription = "OK";
                        clientResp.KeepAlive = false;
                        Console.WriteLine("Sending response.");
                        await clientResp.OutputStream.WriteAsync(data, 0, data.Length);
                    } else {
                        Console.WriteLine(clientReq.Url.AbsolutePath);
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
            filePath = filePath + "/webapp/";
            String[] files = System.IO.Directory.GetFiles(filePath);
            String[] output = new String[files.Length];
            for (int i = 0; i < files.Length; i++) {
                String[] temp = files[i].Split("/");
                output[i] = "/" + temp[temp.Length - 1];
            }

            return output;
        }

        private String getPath() {
            String tempPath = System.IO.Path.GetFullPath("Program.cs");
            String[] updatedPath = tempPath.Split("/");
            tempPath = "";

            for (int i = 0; i < updatedPath.Length - 1; i++) {
                tempPath += updatedPath[i] + "/";
            }
            return tempPath;
        }

        private int searchArray(String[] items, String query) {
            for (int i = 0; i < items.Length; i++) {
                if (items[i] == query) {
                    return i;
                }
            }
            Console.WriteLine("Could not find " + query + " in:");
            String temp = "";
            foreach (String item in items) {
                temp += item + " ";
            }
            Console.WriteLine(temp);
            return -1;
        }

        private void startBrowser(String url) {
            Process myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = url;
            myProcess.Start();
        }
    }
}