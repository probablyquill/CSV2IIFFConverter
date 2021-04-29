using System;

namespace CSV2FLL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {
                ConvertServer server = new ConvertServer();
            } else {
                ConvertServerUnix server = new ConvertServerUnix();
            }
           
        }

        
    }
}

//TODO:
//1.) Add checks to make sure that the data is all correctly entered into the csv.
//2.) Add checks to make sure that all dates entered into the CSV or webapp are reachable,
//i.e. make sure that there are no entires like "2/30/2020."
//3.) Make sure that the converter actually works -> can't do this one on my own.
//4.) Overload the generateIFF function to make one which returns a string and 
//one which outputs a file.