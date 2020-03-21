using System;
using Gtk;
namespace Cyclone {
    public class BikeEditor : Window {
        public const int ERROR_YEAR = 1;
        public const int ERROR_EMPTY = 2;

        private const int EDITOR_ROWS = 10;
        private const int EDITOR_COLUMNS = 2;
        private const int EDITOR_BORDER_WIDTH = 18;
        public const uint CELL_PAD = 6;
        private const int MIN_YEAR = 1200;
        private const int MAX_YEAR = 9999;

        public uint defPadding = 20;
        public int winW = 400;
        public int winH = 200;
        public Button saveBike;
        public Bike savedBike;
        public int errorType;
     
        Entry bikeMake;
        Entry bikeModel;
        Entry bikeType;
        Entry bikeYear;
        Entry bikeWheels;
        Entry bikeForks;

        public BikeEditor() : base(WindowType.Toplevel) {
            createBikeEditor();
        }
        
        public BikeEditor(Bike bike) :  base(WindowType.Toplevel){
            createBikeEditor();
            Title = "Modify bike";
            bikeMake.Text = bike.Make;
            bikeModel.Text = bike.Model;
            bikeYear.Text = bike.Year.ToString();
            bikeType.Text = bike.Type;
            bikeWheels.Text = bike.WheelSize;
            bikeForks.Text = bike.Forks;
        }
        
        public void createBikeEditor() {
            SetDefaultSize(winW, winH);
            SetSizeRequest(winW, winH);
            //Resizable = false;
            Title = "Create bike";
               // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", 32, 32);
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

            bikeMake = new Entry();
            bikeModel = new Entry();
            bikeYear = new Entry();
            bikeType = new Entry();
            bikeWheels = new Entry();
            bikeForks = new Entry();

            saveBike = new Button();
            Label saveNemo = new Label("_Save");
            saveBike.Label = "Save";
            saveBike.AddMnemonicLabel(saveNemo);

            // LRTB
            fields.Attach(makeLabel, 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(modelLabel, 0, 1, 1, 2, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(yearLabel, 0, 1, 2, 3, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(typeLabel, 0, 1, 3, 4, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(wheelLabel, 0, 1, 4, 5, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(forksLabel, 0, 1, 5, 6, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            fields.Attach(bikeMake, 1, 2, 0, 1, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeModel, 1, 2, 1, 2, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeYear, 1, 2, 2, 3, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeType, 1, 2, 3, 4, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeWheels, 1, 2, 4, 5, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(bikeForks, 1, 2, 5, 6, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            fields.Attach(saveBike, 0, 2, 9, 10, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD*2, CELL_PAD*2);

            Add(fields);    
        }

        public Bike getBike() {
            int bikeYearTry;

            bool yearValid = int.TryParse(bikeYear.Text, out bikeYearTry);
            yearValid = yearValid && (bikeYearTry < MAX_YEAR) && (bikeYearTry > MIN_YEAR);

            bool fieldsEmpty = string.IsNullOrWhiteSpace(bikeMake.Text) || string.IsNullOrWhiteSpace(bikeModel.Text)
             || string.IsNullOrWhiteSpace(bikeYear.Text) || string.IsNullOrWhiteSpace(bikeType.Text)
             || string.IsNullOrWhiteSpace(bikeWheels.Text) || string.IsNullOrWhiteSpace(bikeForks.Text);

            if (fieldsEmpty) {
                errorType = 2;
            } if (!yearValid) {
               errorType = 1;
            }
            
            if (errorType != 0) { 
                throw new FormatException();
            } else { 
                savedBike = new Bike(bikeMake.Text, bikeType.Text, bikeModel.Text, bikeYearTry, bikeWheels.Text, bikeForks.Text, 0);
                return savedBike;
            }
        }
    }
}
