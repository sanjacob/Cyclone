using System;
namespace Cyclone.Objects {
    public class Address {
        public string country;
        public string region;
        public string city;
        public string addressOne;
        public string addressTwo;
        public string postalCode;

        public Address(string country, string region, string city, string addressOne, string addressTwo, string postalCode) {
            this.country = country;
            this.region = region;
            this.city = city;
            this.addressOne = addressOne;
            this.addressTwo = addressTwo;
            this.postalCode = postalCode;
        }
    }
}
