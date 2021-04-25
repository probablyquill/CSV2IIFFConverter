using System;

namespace CSV2FLL
{
    class Program
    {
        static void Main(string[] args)
        {
            ConvertServer server = new ConvertServer();
            //CSVParser finalParser = new CSVParser();
            //TotalData dataFound = finalParser.parseData("Example.csv");
            //EditedData culledData = finalParser.cullData("3/1/2021", "5/16/2021", dataFound);

            //Console.WriteLine(culledData.toString());

            //finalParser.generateIFF(culledData, "output.iff", "");
        }
    }
}
