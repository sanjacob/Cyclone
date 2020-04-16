using System;
using System.Collections.Generic;

namespace Cyclone.Objects {
    public class Rent {
        public static int RentalsCount;

        public int paymentType;
        public DateTime rentStartDate;
        private ClientData clientData;
        public List<Bike> rentedBikes;
        public double rentEarnings;
        
        public const int CASH = 0;
        public const int CARD = 1;
        public const int PAYPAL = 2;

        // Hourly rent rate per bike
        private static int hourlyRate = 20;

        public Rent(List<Bike> rentedBikes, ClientData client, int payment) {
            this.rentedBikes = rentedBikes;
            clientData = client;
            paymentType = payment;
            rentStartDate = DateTime.Now;
            RentalsCount++;
        }
        
        public double CalculateTotal {
            get {
                // Get span of time in hours, multiply by hourly rate and number of bikes
                TimeSpan rentTime = DateTime.Now.Subtract(rentStartDate);
                rentEarnings = rentTime.TotalDays * rentedBikes.Count * hourlyRate;
                return rentEarnings;
            }
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

        public string Renter{
            get {
                return string.Format("{0} {1}", clientData.Name, clientData.Surname);
            }
        }
    }
}
