using System;
namespace Cyclone.Objects {
    public class Address {
        public string country;
        public string region;
        public string city;
        public string addressOne;
        public string addressTwo;
        public string postalCode;

        public Address(string country, string region, string city, string aOne, string aTwo, string code) {
            this.country = country;
            this.region = region;
            this.city = city;
            addressOne = aOne;
            addressTwo = aTwo;
            postalCode = code;
        }
    }
}
