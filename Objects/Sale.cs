using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cyclone.Objects {
    public class Sale {
        public static double Balance;

        public int paymentType;
        public DateTime saleDate;
        public ClientData clientData;
        public List<Bike> soldBikes;
        public List<int> bikeEarnings;

        public const int CASH = 0;
        public const int CARD = 1;
        public const int PAYPAL = 2;

        public Sale(List<Bike> soldBikes, List<int> bikeEarnings, ClientData clientData, int paymentType) {
            this.soldBikes = soldBikes;
            this.bikeEarnings = bikeEarnings;
            this.clientData = clientData;
            this.paymentType = paymentType;
            saleDate = DateTime.Now;

            Balance += Total;
        }

        [JsonIgnore]
        public int Total {
            get {
                return bikeEarnings.AsParallel().Sum();
            }
        }

        [JsonIgnore]
        public string Buyer {
            get {
                return string.Format("{0} {1}", clientData.Name, clientData.Surname);
            }
        }
    }
}
