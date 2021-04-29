using System;
using System.Collections.Generic;

namespace CSV2FLL
{
    class EditedData
    {
        public List<DateTime> daysWorked;
        public List<Double> hoursPerDay;
        public Double totalHours;
        public EditedData() {
            this.daysWorked = new List<DateTime>();
            this.hoursPerDay = new List<Double>();
            this.totalHours = 0;
        }

        public void add(DateTime day, Double hours) {
            this.daysWorked.Add(day);
            this.hoursPerDay.Add(hours);
            this.totalHours += hours;
        }
        public Double hours() {
            return this.totalHours;
        }
        public String toString() {
            String output = "";

            foreach (DateTime date in this.daysWorked) {
                output += date.ToString("MM/dd/yyyy") + ", ";
            }
            output += "\n";

            foreach (Double hour in this.hoursPerDay) {
                output += "" + hour + ", ";
            }
            output += "\nTotal Hours: " + this.totalHours;
            return output;
        }
        public void calcHours() {
            this.totalHours = 0;
            foreach (Double hours in this.hoursPerDay) {
                this.totalHours += hours;
            }
        }
    }

    class TotalData {
            public List<String> dates;
            public List<String> arrived;
            public List<String> left;
            public List<Double> hours;
            public List<int> incorrectEntry;

            public TotalData() {
                dates = new List<String>();
                arrived = new List<String>();
                left = new List<String>();
                hours = new List<Double>();
                incorrectEntry = new List<int>();
            }
            public TotalData(List<String> dates, List<String> arrived, List<String> left, List<Double> hours) {
                this.dates = dates;
                this.arrived = arrived;
                this.left = left;
                this.hours = hours;
            }
    }
}