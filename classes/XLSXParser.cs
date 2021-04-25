using System;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace CSV2FLL
{
    class CSVParser
    {
        public TotalData parseData(String[] filepath) {
            TotalData dataFound = new TotalData();

            Double totalHours = 0;
            int arrivedIndex = -1;
            int leftIndex = -1;
            int datesIndex = -1;


            //String[] fileLines = System.IO.File.ReadAllLines(@filepath);
            List<String[]> splitStrings = new List<String[]>();

            foreach (String line in filepath) {
                String[] split = line.Split(",");
                splitStrings.Add(split);
            }

            String[] indexFinder = splitStrings[0];
            for (int i = 0; i < indexFinder.Length; i++) {
                String item = indexFinder[i];
                switch (item) {
                    case "Started":
                    case "Started:":
                    case "Arrived":
                    case "Arrived:":
                        arrivedIndex = i;
                        break;
                    case "Left":
                    case "Left:":
                        leftIndex = i;
                        break;
                    case "Date":
                    case "Date:":
                        datesIndex = i;
                        break;
                }
            }

            if (arrivedIndex == -1 || datesIndex == -1 || leftIndex == -1) {
                Console.WriteLine("Error: Data is not tagged correctly. Assuming the following format:");
                Console.WriteLine("Date | Time Arrived | Time Left");
                arrivedIndex = 1;
                leftIndex = 2;
                datesIndex = 0;
            }

            for (int i = 1; i < splitStrings.Count; i++) {
                String[] info = splitStrings[i];
                dataFound.dates.Add(info[datesIndex]);
                dataFound.arrived.Add(info[arrivedIndex]);
                dataFound.left.Add(info[leftIndex]);
            }

            DateTime dt;
            TimeSpan ts;
            String[] hoursMinutesSeconds = new String[3];
            double timeSpent;
            for (int i = 0; i < dataFound.dates.Count; i++) {
                dt = DateTime.Parse(dataFound.left[i]);
                ts = dt.Subtract(DateTime.Parse(dataFound.arrived[i]));
                hoursMinutesSeconds = ts.ToString().Split(":");
                timeSpent = Double.Parse(hoursMinutesSeconds[0]) + (Double.Parse(hoursMinutesSeconds[1])/60);
                dataFound.hours.Add(timeSpent);
            }
            foreach (Double hour in dataFound.hours) {
                totalHours += hour;
            }
            return dataFound;
        }

        public EditedData cullData(String startDay, String endDay, TotalData totalData) {
            EditedData culledData = new EditedData();
            Boolean startFound = false;
            startFound = false;

            for (int j = 0; j < totalData.dates.Count; j++) {
                String[] readDataDateString = totalData.dates[j].Split("/");
                String[] startDateString = startDay.Split("/");
                String[] endDateString = endDay.Split("/");

                int[] readDataDate = Array.ConvertAll(readDataDateString, s => int.Parse(s));
                int[] startDate = Array.ConvertAll(startDateString, s => int.Parse(s));
                int[] endDate = Array.ConvertAll(endDateString, s => int.Parse(s));

                DateTime dataDate = new DateTime(readDataDate[2], readDataDate[0], readDataDate[1]);
                DateTime trueStartDate = new DateTime(startDate[2], startDate[0], startDate[1]);
                DateTime trueEndDate = new DateTime(endDate[2], endDate[0], endDate[1]);

                if (startFound == false) {
                    if (dataDate >= trueStartDate) {
                        //Console.WriteLine(dataDate.ToString() + " >= " + trueStartDate.ToString());
                        startFound = true;
                        culledData.add(dataDate, totalData.hours[j]);
                    }

                } else {
                    if (dataDate > trueEndDate) {
                        //Console.WriteLine(dataDate.ToString() + " > " + trueEndDate.ToString());
                        break;
                    } else if (dataDate == trueEndDate) {
                        //Console.WriteLine(dataDate.ToString() + " == " + trueEndDate.ToString());
                        culledData.add(dataDate, totalData.hours[j]);
                        break;
                    } else {
                        //Console.WriteLine(dataDate.ToString() + " < " + trueEndDate.ToString());
                        culledData.add(dataDate, totalData.hours[j]);
                    }
                }
                
            }
            
            culledData.calcHours();
            return culledData;
        }

        public String generateIFF(EditedData workedDaysHours, String filelocation, String employee) {
            DateTime currentDate = DateTime.Now;
            String outputFileText = "!HDR   PROD    VER REL IIFVER  DATE    TIME    ACCNTNT     ACCNTNTSPLITTIME";
            outputFileText += "\nHDR  QuickBooks  Version    Release R1  1   " + currentDate.ToString("MM/dd/yyyy") + "   0  N   0";
            outputFileText += "\n!TIMEACT DATE    JOB EMP ITEM    PITEM   DURATION    PROJ    XFERTOPAYROLL";
            
            for (int i = 0; i < workedDaysHours.daysWorked.Count; i++) {
                outputFileText += "\nTIMEACT    " + workedDaysHours.daysWorked[i].ToString("MM/dd/yyyy") + "    "; //Add date
                outputFileText += "Stream Station Inc.   Tate, Elijah   Labor   Employee    "; //Misc info for setup.
                outputFileText += Math.Round(workedDaysHours.hoursPerDay[i], 2) + "   In Office Work   N";
            }

            using (System.IO.FileStream fs = System.IO.File.Create(filelocation))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(outputFileText);
                fs.Write(info, 0, info.Length);
            }

            return outputFileText;
        }

    }
}