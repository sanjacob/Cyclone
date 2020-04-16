using System;
namespace Cyclone.Objects {
    public class ClientData {
        public string Name { get; }
        public string Surname { get; }
        public string Phone { get; set; }
        public string Mail { get; set; }
        public Address ClientAddress { get; }

        public ClientData(string name, string surname, string phone, Address address) {
            Name = name;
            Surname = surname;
            Phone = phone;
            ClientAddress = address;
        }
        
        public ClientData(string name, string surname, string phone) {
            Name = name;
            Surname = surname;
            Phone = phone;
        }
    }
}
