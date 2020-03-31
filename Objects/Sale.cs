using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyclone.Objects {
    public class Sale {
        public static int Balance;

        public int paymentType;
        public DateTime saleDate;
        public ClientData clientData;
        public List<Bike> soldBikes;
        public List<int> bikeEarnings;

        public const int CASH = 0;
        public const int CARD = 1;
        public const int PAYPAL = 2;

        public Sale(List<Bike> soldBikes, List<int> earnings, ClientData client, int payment) {
            this.soldBikes = soldBikes;
            bikeEarnings = earnings;
            clientData = client;
            paymentType = payment;
            saleDate = DateTime.Now;

            Balance += Total;
        }

        public int Total {
            get {
                return bikeEarnings.AsParallel().Sum();
            }
        }
    }
}
