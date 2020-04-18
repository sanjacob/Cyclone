using System;
using System.Collections.Generic;
using Cyclone.Objects;
using Gtk;

namespace Cyclone {
    public class RentWindow : SaleWindow {
        private SpinButton rateSpin;

        public RentWindow(List<Bike> rentedBikes) : base(rentedBikes) {
        }

        protected override Widget PaymentInfo {
            get {
                // First Column
                uint modelStart = 0;
                uint modelSpan = 1;
                uint modelEnd = modelStart + modelSpan;

                // Second Column
                uint costStart = 1;
                uint costSpan = 1;
                uint costEnd = costStart + costSpan;

                uint paymentRows = 3 + (uint)soldBikes.Count;
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

                Label bikeTitle = new Label("<b>BIKES TO RENT:</b>");
                bikeTitle.UseMarkup = true;

                Label hourlyRateTitle = new Label("<b>HOURLY RENT RATE:</b>");
                hourlyRateTitle.UseMarkup = true;

                Adjustment rateAdj = new Adjustment(42, 0, 1000, 1, 10, 10);
                rateSpin = new SpinButton(rateAdj, 50, 2);

                uint tableRowCounter = 2;
                paymentTable.Attach(bikeTitle, modelStart, modelEnd, tableRowCounter, tableRowCounter + 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                paymentTable.Attach(hourlyRateTitle, costStart, costEnd, tableRowCounter, tableRowCounter + 1, AttachOptions.Fill, AttachOptions.Fill, CELL_PAD, CELL_PAD);
                tableRowCounter++;
                paymentTable.Attach(rateSpin, costStart, costEnd, tableRowCounter, tableRowCounter + 1, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                uint labelSpan = 1;

                foreach (Bike soldBike in soldBikes) {
                    Label bikeName = new Label(string.Format("{0}, {1} ({2})", soldBike.Model, soldBike.Make, soldBike.SecurityCode));
                    bikeName.Selectable = true;

                    paymentTable.Attach(bikeName, modelStart, modelEnd, tableRowCounter, tableRowCounter + labelSpan, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD, CELL_PAD);

                    tableRowCounter++;
                }

                bikeCostScroll.Add(paymentTable);
                return bikeCostScroll;
            }
        }

        protected override Widget ReviewSale {
            get {
                VBox reviewBox = new VBox();

                string pluralBikes = "";
                if (soldBikes.Count > 1) {
                    pluralBikes = "s";
                }

                Label completeSale = new Label (string.Format("Proceed with the rent of {0} bike{1}?", soldBikes.Count, pluralBikes));
                reviewBox.PackStart(completeSale, true, true, defPadding);

                return reviewBox;
            }
        }
        
        public Rent ParseRent() {
            Address clientAddress = new Address(AddressCountry, AddressRegion, AddressCity, AddressOne, AddressTwo, AddressCode);
            ClientData clientData = new ClientData(CustomerName, CustomerSurname, CustomerPhone, clientAddress);
            Rent newRent = new Rent(soldBikes, clientData, PaymentMethod, HourlyRate);
            return newRent;
        }

        private double HourlyRate {
            get {
                return rateSpin.Value;
            }
        }

        protected override string iconName { 
            get { 
                return "Cyclone.Assets.Clock.png";
            }
        }
        
        protected override string reviewTitle {
            get { 
                return "Review Rent";
            }
        }

        protected override string bikeCostsTitle {
            get { 
                return "Rent Payment";
            }
        }
    }
}
