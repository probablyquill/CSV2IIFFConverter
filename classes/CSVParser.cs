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
            //Initialize values, items are set to -1 so that they can be checked later: if the items are 
            //still -1 the program knows that some problem has occured.
            TotalData dataFound = new TotalData();
            Double totalHours = 0;
            int arrivedIndex = -1;
            int leftIndex = -1;
            int datesIndex = -1;

            //List of String arrays which will store the information parsed from the CSV file.
            List<String[]> splitStrings = new List<String[]>();

            //Iterates through the given array of strings and creates a List of arrays, with the String 
            //arrays being split based on the commas from the CSV file. Importantly, this assumes that there are 
            //no commas used outside of item splits.
            foreach (String line in filepath) {
                String[] split = line.Split(",");
                splitStrings.Add(split);
            }

            //Iterates through the first line pulled from the CSV file to find what columns are the ones which
            //the program is interested in.
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

            //If any index is still set to -1, then some identifier could not be concretely found, and so
            //the program assumes a preset format for data entry.
            if (arrivedIndex == -1 || datesIndex == -1 || leftIndex == -1) {
                Console.WriteLine("Error: Data is not tagged correctly. Assuming the following format:");
                Console.WriteLine("Date | Time Arrived | Time Left");
                arrivedIndex = 1;
                leftIndex = 2;
                datesIndex = 0;
            }

            //Iterates through the lines pulled from the CSV file, starting with the second line. Saves the information
            //to the appropriate places in the TotalData object created at the beginning of the method. Also saves the
            //locations of incorrect data entries.
            for (int i = 1; i < splitStrings.Count; i++) {
                String[] info = splitStrings[i];
                if (info[datesIndex] != "" && info[arrivedIndex] != "" && info[leftIndex] != "") {
                    dataFound.dates.Add(info[datesIndex]);
                    dataFound.arrived.Add(info[arrivedIndex]);
                    dataFound.left.Add(info[leftIndex]);
                } else {
                    dataFound.incorrectEntry.Add(i);
                }
                
            }

            //Converts the data from the format "8:00AM" to "8:00:00" and then takes the difference between the time arrived
            //and the time left.
            DateTime dt;
            TimeSpan ts;
            String[] hoursMinutesSeconds = new String[3];
            double timeSpent;
            for (int i = 0; i < dataFound.dates.Count; i++) {
                dt = DateTime.Parse(dataFound.left[i]);
                ts = dt.Subtract(DateTime.Parse(dataFound.arrived[i]));
                hoursMinutesSeconds = ts.ToString().Split(":");
                timeSpent = Double.Parse(hoursMinutesSeconds[0]) + (Double.Parse(hoursMinutesSeconds[1])/60);
                if (timeSpent > 0) {
                    dataFound.hours.Add(timeSpent);
                } else {
                    dataFound.incorrectEntry.Add(i);
                }
                
            }
            foreach (Double hour in dataFound.hours) {
                totalHours += hour;
            }
            return dataFound;
        }

        //Takes the TotalData object created by the parser and converts it into an EditedData object based on the entered
        //start and end dates.
        public EditedData cullData(String startDay, String endDay, TotalData totalData) {
            EditedData culledData = new EditedData();
            Boolean startFound = false;

            for (int j = 0; j < totalData.dates.Count; j++) {
                String[] readDataDateString = totalData.dates[j].Split("/");
                String[] startDateString = startDay.Split("/");
                String[] endDateString = endDay.Split("/");

                int[] readDataDate = Array.ConvertAll(readDataDateString, s => int.Parse(s));
                int[] startDate = Array.ConvertAll(startDateString, s => int.Parse(s));
                int[] endDate = Array.ConvertAll(endDateString, s => int.Parse(s));

                DateTime dataDate = DateTime.Now;
                DateTime trueStartDate = DateTime.Now;
                DateTime trueEndDate = DateTime.Now;

                try {
                    dataDate = new DateTime(readDataDate[2], readDataDate[0], readDataDate[1]);
                } catch (Exception e) {
                    culledData.totalHours = -3;
                    return culledData;
                }
                try {
                    trueStartDate = new DateTime(startDate[2], startDate[0], startDate[1]);
                    trueEndDate = new DateTime(endDate[2], endDate[0], endDate[1]);
                } catch (Exception e) {
                    culledData.totalHours = -1;
                    return culledData;
                }
                if (trueEndDate < trueStartDate) {
                    culledData.totalHours = -2;
                    return culledData;
                }
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
            if (workedDaysHours.totalHours == -1) {
                return "Error1";
            }else if (workedDaysHours.totalHours == -2) {
                return "Error2";
            }
            switch (workedDaysHours.totalHours) {
                case -1:
                return "Error1";
                break;
                case -2:
                return "Error2";
                break;
                case -3:
                return "Error3";
                break;
            }
            DateTime currentDate = DateTime.Now;
            String outputFileText = "!HDR   PROD    VER REL IIFVER  DATE    TIME    ACCNTNT     ACCNTNTSPLITTIME";
            outputFileText += "\nHDR  QuickBooks  Version    Release R1  1   " + currentDate.ToString("MM/dd/yyyy") + "   0  N   0";
            outputFileText += "\n!TIMEACT DATE    JOB EMP ITEM    PITEM   DURATION    PROJ    XFERTOPAYROLL";
            
            for (int i = 0; i < workedDaysHours.daysWorked.Count; i++) {
                outputFileText += "\nTIMEACT    " + workedDaysHours.daysWorked[i].ToString("MM/dd/yyyy") + "    "; //Add date
                outputFileText += "Stream Station Inc.   " + employee + "   Labor   Employee    "; //Misc info for setup.
                outputFileText += Math.Round(workedDaysHours.hoursPerDay[i], 2) + "   In Office Work   N";
            }

            //Code which wrote to output file
            /*using (System.IO.FileStream fs = System.IO.File.Create(filelocation))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(outputFileText);
                fs.Write(info, 0, info.Length);
            }*/

            return outputFileText;
        }

    }
}