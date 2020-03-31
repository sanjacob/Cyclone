using System;
namespace Cyclone.Objects {
    public class ClientData {
        private string Name { get; }
        private string Surname { get; }
        private string Phone { get; set; }
        public string Mail { get; set; }
        public Address ClientAddress { get; }

        public ClientData(string name, string surname, string phone, Address address) {
            Name = name;
            Surname = surname;
            Phone = phone;
            ClientAddress = address;
        }
    }
}
