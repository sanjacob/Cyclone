using System;
using System.Collections.Generic;
using Gtk;
namespace Cyclone {
    public class ModelEditor : Window {
        protected Table fields;
        public Entry bikeMake;
        public Entry bikeModel;
        public Entry bikeType;

        public ComboBox bikeMakeC;
        public ListStore makeStore;
        public ComboBox bikeModelC;
        public ListStore modelStore;
        private Label bikeTypeL;

        public Button saveBike;

        public int errorType;
        public const int ERROR_NONE = 0;
        public const int ERROR_EMPTY = 2;

        private uint _EDITOR_ROWS = 4;
        protected uint EDITOR_COLUMNS = 2;
        protected const uint EDITOR_BORDER_WIDTH = 18;
        protected const uint CELL_PAD = 6;

        protected const uint ENTRY_SPAN = 1;
        protected const uint LABEL_SPAN = 1;
        protected const uint LABEL_COL = 0;
        protected const uint ENTRY_COL = 1;
        protected uint LABEL_END = LABEL_COL + LABEL_SPAN;
        protected uint ENTRY_END = ENTRY_COL + ENTRY_SPAN;

        protected const uint SAVE_SPAN = 1;
        private uint _SAVE_START = 3;

        protected uint defPadding = 20;
        protected int WIN_W = 400;
        protected int WIN_H = 200;
        protected const int ICON_SIDE = 32;

        public ModelEditor() : base(WindowType.Toplevel) {
            createEditor();
        }

        public ModelEditor(string make, string model, string type) : base(WindowType.Toplevel) {
            createEditor();

            Title = "Modify model";
            bikeMake.Text = make;
            bikeModel.Text = model;
            bikeType.Text = type;
        }

        public void createEditor() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);
            //Resizable = false;
            Title = "Create model";

            // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", ICON_SIDE, ICON_SIDE);
            Icon = windowIcon;

            fields = new Table(EDITOR_ROWS, EDITOR_COLUMNS, false);
            fields.BorderWidth = EDITOR_BORDER_WIDTH;

            Label makeLabel = new Label("Make: ");
            Label modelLabel = new Label("Model: ");
            Label typeLabel = new Label("Type: ");

            Widget makeWidget;
            Widget modelWidget;
            Widget typeWidget;

            bikeMake = new Entry();
            bikeModel = new Entry();
            bikeType = new Entry();

            makeWidget = bikeMake;
            modelWidget = bikeModel;
            typeWidget = bikeType;

            if (!entryEnabled) {
                CellRendererText comboCell = new CellRendererText();

                bikeMakeC = new ComboBox();
                bikeMakeC.Clear();

                bikeMakeC.PackStart(comboCell, false);
                bikeMakeC.AddAttribute(comboCell, "text", 0);

                makeStore = new ListStore(typeof(string));
                bikeMakeC.Model = makeStore;

                bikeModelC = new ComboBox();
                bikeModelC.Clear();

                bikeModelC.PackStart(comboCell, false);
                bikeModelC.AddAttribute(comboCell, "text", 0);

                modelStore = new ListStore(typeof(string));
                bikeModelC.Model = modelStore;

                bikeTypeL = new Label();

                makeWidget = bikeMakeC;
                modelWidget = bikeModelC;
                typeWidget = bikeTypeL;
            }

            saveBike = new Button();
            Label saveNemo = new Label("_Save");
            saveBike.Label = "Save";
            saveBike.AddMnemonicLabel(saveNemo);

            fields.Attach(makeLabel, LABEL_COL, LABEL_END, 0, 1, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(modelLabel, LABEL_COL, LABEL_END, 1, 2, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(typeLabel, LABEL_COL, LABEL_END, 2, 3, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            fields.Attach(makeWidget, ENTRY_COL, ENTRY_END, 0, 1, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(modelWidget, ENTRY_COL, ENTRY_END, 1, 2, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            fields.Attach(typeWidget, ENTRY_COL, ENTRY_END, 2, 3, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            uint SAVE_END = SAVE_START + SAVE_SPAN;
            fields.Attach(saveBike, LABEL_COL, ENTRY_END, SAVE_START, SAVE_END, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD * 2, CELL_PAD * 2);
            Add(fields);
        }

        public BikeModel getModel() {
            bool fieldsEmpty = string.IsNullOrWhiteSpace(bikeMake.Text) || string.IsNullOrWhiteSpace(bikeModel.Text)
                || string.IsNullOrWhiteSpace(bikeType.Text);

            if (fieldsEmpty) {
                errorType = ERROR_EMPTY;
            }

            if (errorType != ERROR_NONE) {
                throw new FormatException();
            }

            BikeModel editedModel = new BikeModel(bikeMake.Text, bikeType.Text, bikeModel.Text);
            return editedModel;
        }

        protected virtual uint EDITOR_ROWS {
            get {
                return _EDITOR_ROWS;
            }
        }

        protected virtual uint SAVE_START {
            get {
                return _SAVE_START;
            }
        }

        protected virtual bool entryEnabled {
            get {
                return true;
            }
        }

        public virtual void makeCombo(string[] makeList) {
            makeStore.Clear();

            foreach (string make in makeList) {
                makeStore.AppendValues(make);
            }
        }

        public virtual void modelCombo(string[] modelList) {
            modelStore.Clear();

            foreach (string model in modelList) {
                modelStore.AppendValues(model);
            }
        }

        public string type {
            get {
                return bikeTypeL.Text;
            } set {
                bikeTypeL.Text = value;
            }
        }
    }
}
