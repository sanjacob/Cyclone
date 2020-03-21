using System;
using Gtk;
namespace Cyclone {
    public class BikeEditor : Window {
        public const int ERROR_NONE = 0;
        public const int ERROR_YEAR = 1;
        public const int ERROR_EMPTY = 2;

        private const int EDITOR_ROWS = 10;
        private const int EDITOR_COLUMNS = 2;
        private const int EDITOR_BORDER_WIDTH = 18;
        public const uint CELL_PAD = 6;
        private const int MIN_YEAR = 1200;
        private const int FUTURE_MARGIN = 10;

        private const int LABEL_COL = 0;
        private const int ENTRY_COL = 1;
        private const uint ENTRY_SPAN = 1;
        private const uint LABEL_SPAN = 1;

        public uint defPadding = 20;
        public int WIN_W = 400;
        public int WIN_H = 200;
        public const int ICON_SIDE = 32;

        public Button saveBike;
        public Bike savedBike;
        public int errorType;
     
        Entry bikeMake;
        Entry bikeModel;
        Entry bikeType;
        Entry bikeYear;
        Entry bikeWheels;
        Entry bikeForks;
        Entry bikeCode;
        Label bikeCodeDisp;

        private int securityCode;
        private bool codeDisplay = false;

        public BikeEditor() : base(WindowType.Toplevel) {
            createBikeEditor();
        }
        
        public BikeEditor(Bike bike) :  base(WindowType.Toplevel){
            codeDisplay = true;
            createBikeEditor();

            Title = "Modify bike";
            bikeMake.Text = bike.Make;
            bikeModel.Text = bike.Model;
            bikeYear.Text = bike.Year.ToString();
            bikeType.Text = bike.Type;
            bikeWheels.Text = bike.WheelSize;
            bikeForks.Text = bike.Forks;
            bikeCodeDisp.Text = bike.SecurityCode.ToString();

            securityCode = bike.SecurityCode;
        }
        
        public void createBikeEditor() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);
            //Resizable = false;
            Title = "Create bike";
               // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", ICON_SIDE, ICON_SIDE);
            Icon = windowIcon;

            Table fields = new Table(EDITOR_ROWS, EDITOR_COLUMNS, false);
            fields.BorderWidth = EDITOR_BORDER_WIDTH;

            Label makeLabel = new Label("Make: ");
            Label modelLabel = new Label("Model: ");
            Label yearLabel = new Label("Year: ");
            Label typeLabel = new Label("Type: ");
            Label wheelLabel = new Label("Wheel Size: ");
            Label forksLabel = new Label("Forks: ");
            Label priceLabel = new Label("Retail Price: ");
            Label codeLabel = new Label("Security Code: ");

            bikeMake = new Entry();
            bikeModel = new Entry();
            bikeYear = new Entry();
            bikeType = new Entry();
            bikeWheels = new Entry();
            bikeForks = new Entry();
            bikeCode = new Entry();
            bikeCodeDisp = new Label();


            saveBike = new Button();
            Label saveNemo = new Label("_Save");
            saveBike.Label = "Save";
            saveBike.AddMnemonicLabel(saveNemo);

            // LRTB
            uint LABEL_END = LABEL_COL + LABEL_SPAN;
            uint ENTRY_END = ENTRY_COL + ENTRY_SPAN;

            fields.Attach(makeLabel, LABEL_COL, LABEL_END, 0, 1, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(modelLabel, LABEL_COL, LABEL_END, 1, 2, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(yearLabel, LABEL_COL, LABEL_END, 2, 3, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(typeLabel, LABEL_COL, LABEL_END, 3, 4, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(wheelLabel, LABEL_COL, LABEL_END, 4, 5, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(forksLabel, LABEL_COL, LABEL_END, 5, 6, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(codeLabel, LABEL_COL, LABEL_END, 6, 7, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            fields.Attach(bikeMake, ENTRY_COL, ENTRY_END, 0, 1, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeModel, ENTRY_COL, ENTRY_END, 1, 2, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeYear, ENTRY_COL, ENTRY_END, 2, 3, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeType, ENTRY_COL, ENTRY_END, 3, 4, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeWheels, ENTRY_COL, ENTRY_END, 4, 5, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeForks, ENTRY_COL, ENTRY_END, 5, 6, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            Widget codeWidget = bikeCode;
            
            if (codeDisplay) {
                codeWidget = bikeCodeDisp;
            }

            fields.Attach(codeWidget, ENTRY_COL, ENTRY_END, 6, 7, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            fields.Attach(saveBike, LABEL_COL, ENTRY_END, 9, 10, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD*2, CELL_PAD*2);

            Add(fields);    
        }

        public Bike getBike() {

            int bikeYearTry;
            bool yearValid = int.TryParse(bikeYear.Text, out bikeYearTry);
            yearValid = yearValid && (bikeYearTry < (DateTime.Now.Year + FUTURE_MARGIN)) && (bikeYearTry > MIN_YEAR);

            bool fieldsEmpty = string.IsNullOrWhiteSpace(bikeMake.Text) || string.IsNullOrWhiteSpace(bikeModel.Text)
             || string.IsNullOrWhiteSpace(bikeYear.Text) || string.IsNullOrWhiteSpace(bikeType.Text)
             || string.IsNullOrWhiteSpace(bikeWheels.Text) || string.IsNullOrWhiteSpace(bikeForks.Text);
             
            if (!codeDisplay) {
                fieldsEmpty = fieldsEmpty || string.IsNullOrWhiteSpace(bikeCode.Text);
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
            } else { 
                savedBike = new Bike(bikeMake.Text, bikeType.Text, bikeModel.Text, bikeYearTry, bikeWheels.Text, bikeForks.Text, securityCode);
                return savedBike;
            }
        }
    }
}
