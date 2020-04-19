using System;
using Newtonsoft.Json;

namespace Cyclone.Objects {
    public class ClientData {
        public string Name { get; }
        public string Surname { get; }
        public string Phone { get; set; }
        public string Mail { get; set; }
        public Address ClientAddress { get; }

        [JsonConstructor]
        public ClientData(string Name, string Surname, string Phone, Address ClientAddress) {
            this.Name = Name;
            this.Surname = Surname;
            this.Phone = Phone;
            this.ClientAddress = ClientAddress;
        }
        
        public ClientData(string name, string surname, string phone) {
            Name = name;
            Surname = surname;
            Phone = phone;
        }
    }
}
