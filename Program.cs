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
