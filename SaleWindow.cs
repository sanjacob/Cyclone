using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cyclone.Objects;
using Gtk;
using Newtonsoft.Json;

namespace Cyclone {
    public class SaleWindow : Assistant {
        private Entry nameEntry;
        private Entry surnameEntry;
        private Entry emailEntry;
        private Entry phoneEntry;

        protected ComboBox paymentCombo;
        private List<SpinButton> bikePrices = new List<SpinButton>();

        private ComboBox countryCombo;
        private List<string> countryList;
        private Entry regionEntry;
        private Entry cityEntry;
        private Entry postCodeEntry;
        private Entry addressOneEntry;
        private Entry addressTwoEntry;

        protected List<Bike> soldBikes;
        protected string[] paymentOptions = { "Cash", "Card", "Paypal" };

        public const int WIN_W = 180;
        public const int WIN_H = 120;
        public const uint defPadding = 20;

        public const int ICON_SIDE = 32;
        public const int TOOL_SIDE = 24;

        protected const uint EDITOR_BORDER_WIDTH = 4;
        protected const uint CELL_PAD = 4;

        protected const float V_MIDDLE = 0.5f;
        protected const float H_START = 0;

        public SaleWindow(List<Bike> saleBikes) {
            soldBikes = saleBikes;
            WindowProperties();

            Widget basicInfoPage = CustomerInfo;
            AppendPage(basicInfoPage);
            SetPageTitle(basicInfoPage, "Customer Information");
            SetPageType (basicInfoPage, AssistantPageType.Content);

            Widget paymentPage = PaymentInfo;
            AppendPage(paymentPage);
            SetPageTitle(paymentPage, bikeCostsTitle);
            SetPageType (paymentPage, AssistantPageType.Content);

            Widget addressPage = CustomerAddress;
            AppendPage(addressPage);
            SetPageTitle(addressPage, "Customer Address");
            SetPageType (addressPage, AssistantPageType.Content);

            Widget completeSale = ReviewSale;
            AppendPage (completeSale);

            SetPageTitle (completeSale, reviewTitle);
            SetPageType (completeSale, AssistantPageType.Confirm);
            SetPageComplete(completeSale, true);
        }

        /// <summary>
        /// Sets the window properties: size and icon.
        /// </summary>
        private void WindowProperties() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);

            //Resizable = false;
            Title = "Sale Assistant";

            // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     iconName, ICON_SIDE, ICON_SIDE);
            Icon = windowIcon;

            Cancel += AssistantCancel;
            Close += AssistantClose;
        }

        private void AssistantClose(object sender, EventArgs e) {
            Destroy();
        }

        protected Widget CustomerInfo {
            get {
                uint customerRows = 5;
                uint customerColumns = 2;

                // First Column
                uint fStart = 0;
                uint fSpan = 1;
                uint fEnd = fStart + fSpan;

                // Second Column
                uint sStart = 1;
                uint sSpan = 1;
                uint sEnd = sStart + sSpan;

                Table basicTable = new Table(customerRows, customerColumns, true);
                basicTable.BorderWidth = EDITOR_BORDER_WIDTH;
                basicTable.Halign = Align.Center;
                basicTable.Expand = true;
                
                Label customerLabel = new Label("<b>CUSTOMER INFORMATION:</b>");
                customerLabel.UseMarkup = true;
                customerLabel.SetAlignment(H_START, V_MIDDLE);

                Label nameLabel = new Label("Name*");
                nameLabel.SetAlignment(H_START, V_MIDDLE);

                Label surnameLabel = new Label("Surname*");
                surnameLabel.SetAlignment(H_START, V_MIDDLE);

                Label emailLabel = new Label("E-Mail");
                emailLabel.SetAlignment(H_START, V_MIDDLE);

                Label phoneLabel = new Label("Phone Number*");
                phoneLabel.SetAlignment(H_START, V_MIDDLE);

                nameEntry = new Entry();
                nameEntry.Changed += ValidateBasic;
                surnameEntry = new Entry();
                surnameEntry.Changed += ValidateBasic;
                emailEntry = new Entry();
                emailEntry.Changed += ValidateBasic;
                phoneEntry = new Entry();
                phoneEntry.Changed += ValidateBasic;

                List<Widget> basicWgtList = new List<Widget> { null, null, nameLabel, surnameLabel, 
                nameEntry, surnameEntry, emailLabel, phoneLabel, emailEntry, phoneEntry};

                uint row = 0;
                basicTable.Attach(customerLabel, fStart, sEnd, row, ++row, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                for (; row < customerRows; row++) {
                    basicTable.Attach(basicWgtList[(int)row*2], fStart, fEnd, row, row + 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                    basicTable.Attach(basicWgtList[((int)row*2)+1], sStart, sEnd, row, row + 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                }

                return basicTable;
            }
        }

        protected Widget CustomerAddress {
            get {
                uint addressRows= 9;
                uint addressColumns = 2;

                // First Column
                uint genStart = 0;
                uint genSpan = 1;
                uint genEnd = genStart + genSpan;

                // Second Column
                uint regionStart = 1;
                uint regionSpan = 1;
                uint regionEnd = regionStart + regionSpan;

                Table addressTable = new Table(addressRows, addressColumns, true);
                addressTable.BorderWidth = EDITOR_BORDER_WIDTH;
                addressTable.Halign = Align.Center;
                addressTable.Expand = true;
                
                Label addressLabel = new Label("<b>CUSTOMER ADDRESS:</b>");
                addressLabel.UseMarkup = true;
                addressLabel.SetAlignment(H_START, V_MIDDLE);

                Label countryLabel = new Label("Country*");
                countryLabel.SetAlignment(H_START, V_MIDDLE);

                Label regionLabel = new Label("County*");
                regionLabel.SetAlignment(H_START, V_MIDDLE);

                Label cityLabel = new Label("City*");
                cityLabel.SetAlignment(H_START, V_MIDDLE);

                Label postCodeLabel = new Label("Post Code*");
                postCodeLabel.SetAlignment(H_START, V_MIDDLE);

                Label addressOneLabel = new Label("Street and House No.*");
                addressOneLabel.SetAlignment(H_START, V_MIDDLE);

                Label addressTwoLabel = new Label("Apartment, Suite, Unit, Floor");
                addressTwoLabel.SetAlignment(H_START, V_MIDDLE);

                countryList = GetCountries();
                countryCombo = new ComboBox(countryList.ToArray());
                countryCombo.Active = 240;

                regionEntry = new Entry();
                regionEntry.Changed += ValidateAddress;
                cityEntry = new Entry();
                cityEntry.Changed += ValidateAddress;
                postCodeEntry = new Entry();
                postCodeEntry.Changed += ValidateAddress;
                addressOneEntry = new Entry();
                addressOneEntry.Changed += ValidateAddress;
                addressTwoEntry = new Entry();

                uint row = 0;
                uint wgtSpan = 1;
                int areaRows = 5;
                int rowWgts = 2;

                addressTable.Attach(addressLabel, genStart, regionEnd, row, ++row, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                List<Widget> addressWgts = new List<Widget> { null, null, countryLabel, regionLabel, countryCombo, regionEntry, 
                    cityLabel, postCodeLabel, cityEntry, postCodeEntry};

                for (; row < areaRows; row++) {
                    addressTable.Attach(addressWgts[(int)row * rowWgts], genStart, genEnd, row, row+ wgtSpan, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                    addressTable.Attach(addressWgts[(int)((row * rowWgts) + wgtSpan)], regionStart, regionEnd, row, row + wgtSpan, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                }

                addressTable.Attach(addressOneLabel, genStart, genEnd, row, row + wgtSpan, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                row++;
                addressTable.Attach(addressOneEntry, genStart, regionEnd, row, row + wgtSpan, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                row++;
                addressTable.Attach(addressTwoLabel, genStart, genEnd, row, row + wgtSpan, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                row++;
                addressTable.Attach(addressTwoEntry, genStart, regionEnd, row, row + wgtSpan, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                return addressTable;
            }
        }

        protected virtual Widget PaymentInfo {
            get {
                // First Column
                uint modelStart = 0;
                uint modelSpan = 1;
                uint modelEnd = modelStart + modelSpan;

                // Second Column
                uint costStart = 1;
                uint costSpan = 1;
                uint costEnd = costStart + costSpan;

                uint paymentRows =  3 + (uint) soldBikes.Count;
                uint paymentColumns = 2;
                ScrolledWindow bikeCostScroll = new ScrolledWindow();
                Table paymentTable = new Table(paymentRows, paymentColumns, true);

                Label paymentLabel = new Label("<b>PAYMENT VIA:</b>");
                paymentLabel.UseMarkup = true;
                paymentLabel.SetAlignment(H_START, V_MIDDLE);
                paymentCombo = new ComboBox(paymentOptions);
                paymentCombo.Changed += ValidatePayment;

                paymentTable.Attach(paymentLabel, modelStart, modelEnd, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 60, 12);
                paymentTable.Attach(paymentCombo, modelStart, costEnd, 1, 2, AttachOptions.Fill, AttachOptions.Fill, 60, 12);

                Label bikeTitle = new Label("<b>BIKE MODEL:</b>");
                bikeTitle.UseMarkup = true;
                Label priceTitle = new Label("<b>COST (GBP):</b>");
                priceTitle.UseMarkup = true;

                paymentTable.Attach(bikeTitle, modelStart, modelEnd, 2, 3, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                paymentTable.Attach(priceTitle, costStart, costEnd, 2, 3, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                uint tableRowCounter = 3;
                uint labelSpan = 1;

                foreach (Bike soldBike in soldBikes) {
                    Label bikeName = new Label(string.Format("{0}, {1} ({2})", soldBike.Model, soldBike.Make, soldBike.SecurityCode));
                    bikeName.Selectable = true;
                    Adjustment priceAdj = new Adjustment(399, 0, 1000000, 1, 10, 10);
                    SpinButton costSpin = new SpinButton(priceAdj, 50, 2);
                    bikePrices.Add(costSpin);

                    paymentTable.Attach(bikeName, modelStart, modelEnd, tableRowCounter, tableRowCounter + labelSpan, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                    paymentTable.Attach(costSpin, costStart, costEnd, tableRowCounter, tableRowCounter + labelSpan, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                    tableRowCounter++;
                }

                bikeCostScroll.Add(paymentTable);
                return bikeCostScroll;
            }
        }
        
        protected virtual Widget ReviewSale {
            get {
                VBox reviewBox = new VBox();

                string pluralBikes = "";
                if (soldBikes.Count > 1) {
                    pluralBikes = "s";
                }

                Label completeSale = new Label (string.Format("Proceed with the sale of {0} bike{1}?", soldBikes.Count, pluralBikes));
                reviewBox.PackStart(completeSale, true, true, defPadding);

                return reviewBox;
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
            clientData.Mail = CustomerEmail;

            Sale newSale = new Sale(soldBikes, BikeEarnings, clientData, PaymentMethod);
            return newSale;
        }

        protected void ValidateBasic (object o, EventArgs args) {
            bool basicEmpty = string.IsNullOrWhiteSpace(nameEntry.Text);
            basicEmpty = basicEmpty || string.IsNullOrWhiteSpace(surnameEntry.Text);
            basicEmpty = basicEmpty || string.IsNullOrWhiteSpace(phoneEntry.Text);

            bool emailValid = true;
            if (!string.IsNullOrWhiteSpace(emailEntry.Text)) {
                emailValid = emailEntry.Text.Contains('@');
            }

            bool phoneValid = phoneEntry.Text.Any(char.IsDigit);
            bool basicValid = !basicEmpty && emailValid && phoneValid;

            SetPageComplete(GetNthPage (CurrentPage), basicValid);
        }

        protected void ValidatePayment(object o, EventArgs args) {
            bool paymentEmpty = paymentCombo.Active == -1;
            
            SetPageComplete(GetNthPage (CurrentPage), !paymentEmpty);
        }

        protected void ValidateAddress(object o, EventArgs args) {
            bool addressEmpty = countryCombo.Active == -1;
            addressEmpty = addressEmpty || string.IsNullOrWhiteSpace(regionEntry.Text);
            addressEmpty = addressEmpty || string.IsNullOrWhiteSpace(cityEntry.Text);
            addressEmpty = addressEmpty || string.IsNullOrWhiteSpace(postCodeEntry.Text);
            addressEmpty = addressEmpty || string.IsNullOrWhiteSpace(addressOneEntry.Text);

            SetPageComplete(GetNthPage (CurrentPage), !addressEmpty);
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
                if (paymentCombo.Active == -1) {
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

        void AssistantCancel (object o, EventArgs args) {
            Console.WriteLine ("Sale assistant cancelled.");
            Destroy();
        }
 
        protected virtual string iconName {
            get { 
                return "Cyclone.Assets.Money.png";
            }
        }
        
        protected virtual string reviewTitle {
            get { 
                return "Review Sale";
            }
        }

        protected virtual string bikeCostsTitle {
            get { 
                return "Bike Costs And Payment";
            }
        }
    }
}
