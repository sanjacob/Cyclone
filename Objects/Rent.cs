using System;
using System.Collections.Generic;

namespace Cyclone.Objects {
    public class Rent {
        public static int RentalsCount;

        public int paymentType;
        public DateTime rentStartDate;
        public DateTime rentEndDate;

        public ClientData clientData;
        public List<Bike> rentedBikes;
        public double rentEarnings;
        public TimeSpan duration;
        
        public const int CASH = 0;
        public const int CARD = 1;
        public const int PAYPAL = 2;

        // Hourly rent rate per bike
        private double hourlyRate;
        public bool concluded;

        public Rent(List<Bike> rentedBikes, ClientData client, int payment, double rate) {
            this.rentedBikes = rentedBikes;
            clientData = client;
            paymentType = payment;
            rentStartDate = DateTime.Now;
            hourlyRate = rate;
            RentalsCount++;
        }
        
        public double CalculateTotal {
            get {
                // Get span of time in hours, multiply by hourly rate and number of bikes
                rentEndDate = DateTime.Now;
                TimeSpan rentTime = rentEndDate.Subtract(rentStartDate);
                rentEarnings = rentTime.TotalHours * rentedBikes.Count * hourlyRate;
                rentEarnings = Math.Round(rentEarnings, 2);
                return rentEarnings;
            }
        }

        public void Finish() {
            Sale.Balance += rentEarnings;
            duration = rentEndDate.Subtract(rentStartDate);
            concluded = true;
        }

        public string RentalSummary {
            get {
                string summary = "";
                foreach (Bike bike in rentedBikes) {
                    summary += string.Format("{1}: {0}, ", bike.Model, bike.Make);
                }
                return summary.Substring(0, summary.Length - 2);
            }
        }

        public int BikesCount {
            get {
                return rentedBikes.Count;
            }
        }

        public string Renter {
            get {
                return string.Format("{0} {1}", clientData.Name, clientData.Surname);
            }
        }
    }
}
