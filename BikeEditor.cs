using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
namespace Cyclone {
    public class BikeEditor : ModelEditor {
        private uint _SAVE_START = 8;
        private uint _EDITOR_ROWS = 10;

        private const int MIN_YEAR = 1200;
        private const int FUTURE_MARGIN = 10;
        public const int ERROR_YEAR = 1;
        public const int ERROR_CODE = 3;

        public Bike savedBike;
     
        public Entry bikeYear;
        public Entry bikeWheels;
        public Entry bikeForks;
        public Entry bikeCode;
        public Label bikeCodeDisp;

        private int securityCode;
        private bool codeDisplay;

        public BikeEditor() {
            createBikeEditor();
            Title = "Create bike";
        }
        
        public BikeEditor(string make, string model, int year, string type, string wheelSize, string forks, int code) {
            codeDisplay = true;
            createBikeEditor();

            Title = "Modify bike";
            bikeMake.Text = make;
            bikeModel.Text = model;
            bikeYear.Text = year.ToString();
            bikeType.Text = type;
            bikeWheels.Text = wheelSize;
            bikeForks.Text = forks;
            bikeCodeDisp.Text = code.ToString();

            securityCode = code;
        }
        
        public void createBikeEditor() {
            Label yearLabel = new Label("Year: ");
            Label wheelLabel = new Label("Wheel Size: ");
            Label forksLabel = new Label("Forks: ");
            Label priceLabel = new Label("Retail Price: ");
            Label codeLabel = new Label("Security Code: ");

            bikeYear = new Entry();
            bikeWheels = new Entry();
            bikeForks = new Entry();
            bikeCode = new Entry();
            bikeCodeDisp = new Label();
            
            Widget codeWidget = bikeCode;
            
            if (codeDisplay) {
                codeWidget = bikeCodeDisp;
            }

            fields.Attach(yearLabel, LABEL_COL, LABEL_END, 3, 4, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(wheelLabel, LABEL_COL, LABEL_END, 4, 5, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(forksLabel, LABEL_COL, LABEL_END, 5, 6, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(codeLabel, LABEL_COL, LABEL_END, 6, 7, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            fields.Attach(bikeYear, ENTRY_COL, ENTRY_END, 3, 4, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeWheels, ENTRY_COL, ENTRY_END, 4, 5, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeForks, ENTRY_COL, ENTRY_END, 5, 6, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(codeWidget, ENTRY_COL, ENTRY_END, 6, 7, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
        }

        public Bike getBike(Dictionary<string, List<string>> modelsDict) {

            int parseCode;
            int bikeYearTry;
            bool yearValid = int.TryParse(bikeYear.Text, out bikeYearTry);
            yearValid = yearValid && (bikeYearTry < (DateTime.Now.Year + FUTURE_MARGIN)) && (bikeYearTry > MIN_YEAR);

            bool fieldsEmpty = string.IsNullOrWhiteSpace(bikeYear.Text)
             || string.IsNullOrWhiteSpace(bikeWheels.Text) 
             || string.IsNullOrWhiteSpace(bikeForks.Text);

            int makeSelected = bikeMakeC.Active;
            int modelSelected = bikeModelC.Active;

            fieldsEmpty = fieldsEmpty
             || makeSelected == -1
             || modelSelected == -1;

            parseCode = securityCode;
            if (!codeDisplay) {
                bool codeParsed = int.TryParse(bikeCode.Text, out parseCode);
                fieldsEmpty = fieldsEmpty || string.IsNullOrWhiteSpace(bikeCode.Text);
                
                if (!codeParsed) {
                    errorType = ERROR_CODE;
                }
            }

            if (fieldsEmpty) {
                errorType = ERROR_EMPTY;
            } if (!yearValid) {
               errorType = ERROR_YEAR;
            }

            if (securityCode == 0) {

            }

            if (errorType != ERROR_NONE) { 
                throw new FormatException();
            }
            
            string[] makeArray = modelsDict.Keys.ToArray();
            string makeParsed = makeArray[makeSelected];

            string[] currentModels = modelsDict[makeParsed].ToArray();
            string modelParsed = currentModels[modelSelected];
            
            savedBike = new Bike(makeParsed, type, modelParsed, bikeYearTry, bikeWheels.Text, bikeForks.Text, parseCode);
            return savedBike;
        }

        protected override uint EDITOR_ROWS {
            get {
                return _EDITOR_ROWS;
            }
        }

        protected override uint SAVE_START {
            get {
                return _SAVE_START;
            }
        }

        protected override bool entryEnabled {
            get {
                return false;
            }
        }
    }
}
