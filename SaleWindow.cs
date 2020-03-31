using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cyclone.Objects;
using Gtk;
using Newtonsoft.Json;

namespace Cyclone {
    public class SaleWindow : Window {
        private VBox windowBox;
        private Entry nameEntry;
        private Entry surnameEntry;
        private Entry emailEntry;
        private Entry phoneEntry;
        private ComboBox paymentCombo;
        private ComboBox countryCombo;
        private Entry regionEntry;
        private Entry cityEntry;
        private Entry postCodeEntry;
        private Entry addressOneEntry;
        private Entry addressTwoEntry;

        private List<Bike> soldBikes;
        private List<SpinButton> bikePrices = new List<SpinButton>();
        private List<string> countryList;
        string[] paymentOptions = { "Cash", "Card", "Paypal" };

        public const int WIN_W = 800;
        public const int WIN_H = 600;
        public const uint defPadding = 20;

        public const int ICON_SIDE = 32;
        public const int TOOL_SIDE = 24;

        private uint _EDITOR_ROWS = 4;
        protected uint EDITOR_COLUMNS = 2;
        protected const uint EDITOR_BORDER_WIDTH = 4;
        protected const uint CELL_PAD = 4;

        protected const uint LABEL_SPAN = 1;

        public SaleWindow(List<Bike> saleBikes) : base(WindowType.Toplevel) {
            soldBikes = saleBikes;

            WindowProperties();
            Frame clientDataFrame = new Frame("Personal Information");
            SetFrame(ref clientDataFrame);

            Table pInfoTable = PersonalTable;
            clientDataFrame.Add(pInfoTable);
            windowBox.Add(clientDataFrame);

            Frame paymentFrame = new Frame("Payment");
            SetFrame(ref paymentFrame);

            Table paymentTable = PaymentTable;
            paymentFrame.Add(paymentTable);
            windowBox.Add(paymentFrame);

            Frame addressFrame = new Frame("Address");
            SetFrame(ref addressFrame);

            Table addressTable = AddressTable;
            addressFrame.Add(addressTable);
            windowBox.Add(addressFrame);
        }

        /// <summary>
        /// Sets the window properties: size and icon.
        /// </summary>
        private void WindowProperties() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);

            //Resizable = false;
            Title = "Sell bikes";

            // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Money.png", ICON_SIDE, ICON_SIDE);
            Icon = windowIcon;

            // Create uppermost container
            windowBox = new VBox(false, 0);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(windowBox);
            Add(scrolledWindow);
        }

        private void SetFrame(ref Frame frame) {
            frame.MarginTop = frame.MarginBottom = (int)defPadding;
            frame.MarginLeft = frame.MarginRight = (int)defPadding;
        }

        protected Table PersonalTable {
            get {
                Table pInfoTable = new Table(EDITOR_ROWS, 3, true);
                pInfoTable.BorderWidth = EDITOR_BORDER_WIDTH;
                pInfoTable.Expand = true;
                pInfoTable.Halign = Align.Center;

                Label nameLabel = new Label("Name*");
                nameLabel.SetAlignment(0, (float)0.5);

                Label surnameLabel = new Label("Surname*");
                surnameLabel.SetAlignment(0, (float)0.5);

                Label emailLabel = new Label("E-Mail");
                emailLabel.SetAlignment(0, (float)0.5);

                Label phoneLabel = new Label("Phone Number*");
                phoneLabel.SetAlignment(0, (float)0.5);

                nameEntry = new Entry();
                surnameEntry = new Entry();
                emailEntry = new Entry();
                phoneEntry = new Entry();

                pInfoTable.Attach(nameLabel, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                pInfoTable.Attach(surnameLabel, 2, 3, 0, 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                pInfoTable.Attach(emailLabel, 0, 1, 2, 3, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                pInfoTable.Attach(phoneLabel, 2, 3, 2, 3, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                pInfoTable.Attach(nameEntry, 0, 1, 1, 2, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                pInfoTable.Attach(surnameEntry, 2, 3, 1, 2, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                pInfoTable.Attach(emailEntry, 0, 1, 3, 4, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                pInfoTable.Attach(phoneEntry, 2, 3, 3, 4, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                return pInfoTable;
            }
        }

        protected Table AddressTable {
            get {
                Table addressTable = new Table(8, EDITOR_COLUMNS, true);
                addressTable.BorderWidth = EDITOR_BORDER_WIDTH;
                addressTable.Halign = Align.Center;
                addressTable.Expand = true;

                Label countryLabel = new Label("Country*");
                countryLabel.SetAlignment(0, (float)0.5);

                Label regionLabel = new Label("County*");
                regionLabel.SetAlignment(0, (float)0.5);

                Label cityLabel = new Label("City*");
                cityLabel.SetAlignment(0, (float)0.5);

                Label postCodeLabel = new Label("Post Code*");
                postCodeLabel.SetAlignment(0, (float)0.5);

                Label addressOneLabel = new Label("Street and House No.*");
                addressOneLabel.SetAlignment(0, (float)0.5);

                Label addressTwoLabel = new Label("Apartment, Suite, Unit, Floor*");
                addressTwoLabel.SetAlignment(0, (float)0.5);

                countryList = GetCountries();
                countryCombo = new ComboBox(countryList.ToArray());
                countryCombo.Active = 240;
                regionEntry = new Entry();
                cityEntry = new Entry();
                postCodeEntry = new Entry();
                addressOneEntry = new Entry();
                addressTwoEntry = new Entry();

                addressTable.Attach(countryLabel, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(regionLabel, 1, 2, 0, 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(cityLabel, 0, 1, 2, 3, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(postCodeLabel, 1, 2, 2, 3, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(addressOneLabel, 0, 1, 4, 5, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(addressTwoLabel, 0, 1, 6, 7, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                addressTable.Attach(countryCombo, 0, 1, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, CELL_PAD, CELL_PAD);
                addressTable.Attach(regionEntry, 1, 2, 1, 2, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(cityEntry, 0, 1, 3, 4, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(postCodeEntry, 1, 2, 3, 4, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(addressOneEntry, 0, 2, 5, 6, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                addressTable.Attach(addressTwoEntry, 0, 2, 7, 8, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                return addressTable;
            }
        }

        protected Table PaymentTable {
            get {
                Table paymentTable = new Table(3, 2, true);
                paymentCombo = new ComboBox(paymentOptions);
                paymentTable.Attach(paymentCombo, 0, 2, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 60, 12);

                Label bikeTitle = new Label("<b>BIKE MODEL:</b>");
                bikeTitle.UseMarkup = true;
                Label priceTitle = new Label("<b>COST (GBP):</b>");
                priceTitle.UseMarkup = true;

                paymentTable.Attach(bikeTitle, 0, 1, 1, 2, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                paymentTable.Attach(priceTitle, 1, 2, 1, 2, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                uint tableRowCounter = 2;
                uint labelSpan = 1;

                foreach (Bike soldBike in soldBikes) {
                    Label bikeName = new Label(string.Format("{0}, {1}", soldBike.Model, soldBike.Make));
                    Adjustment priceAdj = new Adjustment(399, 0, 1000000, 1, 10, 10);
                    SpinButton costSpin = new SpinButton(priceAdj, 50, 2);
                    bikePrices.Add(costSpin);

                    paymentTable.Attach(bikeName, 0, 1, tableRowCounter, tableRowCounter + labelSpan, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                    paymentTable.Attach(costSpin, 1, 2, tableRowCounter, tableRowCounter + labelSpan, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                    tableRowCounter++;
                }

                return paymentTable;
            }
        }

        protected virtual uint EDITOR_ROWS {
            get {
                return _EDITOR_ROWS;
            }
        }

        private List<string> GetCountries() {
            Dictionary<string, string> countryDict = new Dictionary<string, string>();
            try {
                countryDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("countries.json"));
            } catch (FileNotFoundException) {
                Console.WriteLine("Could not get list of countries");
            }

            return countryDict.Values.ToList();
        }

        public Sale ParseSale() {
            Address clientAddress = new Address(AddressCountry, AddressRegion, AddressCity, AddressOne, AddressTwo, AddressCode);
            ClientData clientData = new ClientData(CustomerName, CustomerSurname, CustomerPhone, clientAddress);
            Sale newSale = new Sale(soldBikes, BikeEarnings, clientData, PaymentMethod);
            return newSale;
        }
        
        protected string AddressCountry {
            get {
                if (countryCombo.Active == -1) {
                    throw new IndexOutOfRangeException();
                }

                return countryList[countryCombo.Active];
            }
        }
        
        protected int PaymentMethod {
            get {
                if(paymentCombo.Active == -1) {
                    throw new IndexOutOfRangeException();
                }
                return paymentCombo.Active;
            }
        }

        public string CustomerName {
            get {
                if (string.IsNullOrWhiteSpace(nameEntry.Text)) {
                    throw new FormatException();
                }
                return nameEntry.Text;
            } set {
                nameEntry.Text = value;
            }
        }

        public string CustomerSurname {
            get {
                if (string.IsNullOrWhiteSpace(surnameEntry.Text)) {
                    throw new FormatException();
                }
                return surnameEntry.Text;
            } set {
                surnameEntry.Text = value;
            }
        }

        public string CustomerPhone {
            get {
                if (string.IsNullOrWhiteSpace(phoneEntry.Text)) {
                    throw new FormatException();
                }
                return phoneEntry.Text;
            } set {
                phoneEntry.Text = value;
            }
        }

        public string CustomerEmail {
            get {
                if (string.IsNullOrWhiteSpace(emailEntry.Text)) {
                    throw new FormatException();
                }
                return emailEntry.Text;
            } set {
                emailEntry.Text = value;
            }
        }

        public string AddressRegion {
            get {
                if (string.IsNullOrWhiteSpace(regionEntry.Text)) {
                    throw new FormatException();
                }
                return regionEntry.Text;
            } set {
                regionEntry.Text = value;
            }
        }

        public string AddressCity {
            get {
                if (string.IsNullOrWhiteSpace(cityEntry.Text)) {
                    throw new FormatException();
                }
                return cityEntry.Text;
            } set {
                cityEntry.Text = value;
            }
        }

        public string AddressCode {
            get {
                if (string.IsNullOrWhiteSpace(postCodeEntry.Text)) {
                    throw new FormatException();
                }
                return postCodeEntry.Text;
            } set {
                postCodeEntry.Text = value;
            }
        }

        public string AddressOne {
            get {
                if (string.IsNullOrWhiteSpace(addressOneEntry.Text)) {
                    throw new FormatException();
                }
                return addressOneEntry.Text;
            } set {
                addressOneEntry.Text = value;
            }
        }

        public string AddressTwo {
            get {
                if (string.IsNullOrWhiteSpace(addressTwoEntry.Text)) {
                    throw new FormatException();
                }
                return addressTwoEntry.Text;
            } set {
                addressTwoEntry.Text = value;
            }
        }
        
        public List<int> BikeEarnings {
            get {
                List<int> bikeEarnings = new List<int>();
                foreach (SpinButton costSelector in bikePrices) {
                    bikeEarnings.Add(costSelector.ValueAsInt);
                }
                return bikeEarnings;
            }
        }
    }
}
