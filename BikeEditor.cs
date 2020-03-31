using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
namespace Cyclone {
    public class BikeEditor : ModelEditor {
        private ComboBox makeCombo;
        private ComboBox modelCombo;
        private Label typeLabel;

        private CellRendererText ComboCell = new CellRendererText();
        private ListStore makeStore;
        private ListStore modelStore;

        public Dictionary<string, List<BikeModel>> comboModels = new Dictionary<string, List<BikeModel>>();

        private uint _SAVE_START = 8;
        private uint _EDITOR_ROWS = 10;

        private const int MIN_YEAR = 1200;
        private const int FUTURE_MARGIN = 10;
        public const int ERROR_YEAR = 1;
        public const int ERROR_CODE = 3;

        public Entry yearEntry;
        public Entry wheelsEntry;
        public Entry forksEntry;
        public Entry codeEntry;
        public Label codeDisp;

        private int currentCode;
        private bool codeDisplay;

        public BikeEditor(Dictionary<string, List<BikeModel>> validModels) {
            comboModels = validModels;
            createBikeEditor();
            Title = "Create bike";
        }

        public BikeEditor(Dictionary<string, List<BikeModel>> validModels, Bike bike) {

            comboModels = validModels;
            codeDisplay = true;
            createBikeEditor();

            Title = "Modify bike";
            MakeEdit = bike.Make;
            ModelEdit = bike.Model;
            TypeEdit = bike.Type;

            YearEdit = bike.Year;
            WheelSizeEdit = bike.WheelSize;
            ForksEdit = bike.Forks;
            CodeEdit = bike.SecurityCode;

            currentCode = bike.SecurityCode;
        }

        public void createBikeEditor() {
            BuildTable();
            makeStore.Clear();

            foreach (string make in comboModels.Keys) {
                makeStore.AppendValues(make);
            }

            makeCombo.Changed += populateModelCombo;
            modelCombo.Changed += updateBikeType;
        }

        private void updateBikeType(object sender, EventArgs e) {
            List<BikeModel> availableModels = comboModels[MakeEdit];
            if (modelCombo.Active != -1) {
                typeLabel.Text = availableModels[modelCombo.Active].Type;
            }
        }

        private void populateModelCombo(object sender, EventArgs e) {
            List<BikeModel> availableModels = comboModels[MakeEdit];
            modelStore.Clear();
            typeLabel.Text = "";

            foreach (BikeModel validModel in availableModels) {
                modelStore.AppendValues(validModel.Model);
            }
        }

        public Bike ParseBike() {
            int SecurityCode = currentCode;

            if (!codeDisplay) {
                SecurityCode = CodeEdit;
            }

            Bike savedBike = new Bike(MakeEdit, TypeEdit, ModelEdit, YearEdit, WheelSizeEdit, ForksEdit, SecurityCode);
            return savedBike;
        }

        private void BuildTable() {
            Label yearLabel = new Label("Year: ");
            Label wheelLabel = new Label("Wheel Size: ");
            Label forksLabel = new Label("Forks: ");
            Label priceLabel = new Label("Retail Price: ");
            Label codeLabel = new Label("Security Code: ");

            yearEntry = new Entry();
            wheelsEntry = new Entry();
            forksEntry = new Entry();
            codeEntry = new Entry();
            codeDisp = new Label();
            
            Widget codeWidget = codeEntry;
            
            if (codeDisplay) {
                codeWidget = codeDisp;
            }

            editorTable.Attach(yearLabel, LABEL_COL, LABEL_END, 3, 4, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            editorTable.Attach(wheelLabel, LABEL_COL, LABEL_END, 4, 5, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            editorTable.Attach(forksLabel, LABEL_COL, LABEL_END, 5, 6, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            editorTable.Attach(codeLabel, LABEL_COL, LABEL_END, 6, 7, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);

            editorTable.Attach(yearEntry, ENTRY_COL, ENTRY_END, 3, 4, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            editorTable.Attach(wheelsEntry, ENTRY_COL, ENTRY_END, 4, 5, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            editorTable.Attach(forksEntry, ENTRY_COL, ENTRY_END, 5, 6, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            editorTable.Attach(codeWidget, ENTRY_COL, ENTRY_END, 6, 7, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
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

        protected override Widget MakeWidget {
            get {
                makeCombo = new ComboBox();
                makeCombo.Clear();
                makeCombo.PackStart(ComboCell, false);
                makeCombo.AddAttribute(ComboCell, "text", 0);
                
                makeStore = new ListStore(typeof(string));
                makeCombo.Model = makeStore;
                return makeCombo;
            }
        }
        
        protected override string MakeEdit {
            get {
                string[] makeArray = comboModels.Keys.ToArray();
                int makeIndex = makeCombo.Active;

                if (makeIndex == -1) {
                    throw new IndexOutOfRangeException();
                }

                return makeArray[makeIndex];
            } set {
                string[] makeArray = comboModels.Keys.ToArray();
                int makeIndex = Array.IndexOf(makeArray, value);
                makeCombo.Active = makeIndex;
            }
        }

        protected override Widget ModelWidget {
            get {
                modelCombo = new ComboBox();
                modelCombo.Clear();
                modelCombo.PackStart(ComboCell, false);
                modelCombo.AddAttribute(ComboCell, "text", 0);
                
                modelStore = new ListStore(typeof(string));
                modelCombo.Model = modelStore;
                return modelCombo;
            }
        }

        protected override string ModelEdit {
            get {
                List<BikeModel> availableModels = comboModels[MakeEdit];

                if (modelCombo.Active == -1) {
                    throw new IndexOutOfRangeException();
                }

                return availableModels[modelCombo.Active].Model;
            } set {
                List<BikeModel> availableModels = comboModels[MakeEdit];

                int setModel = 0;
                int currentModelIndex = 0;

                modelStore.Clear();

                foreach (BikeModel validModel in availableModels) {
                    modelStore.AppendValues(validModel.Model);

                    if (validModel.Model == value) {
                        setModel = currentModelIndex;
                    }

                    currentModelIndex++;
                }

                modelCombo.Active = setModel;
            }
        }

        protected override Widget TypeWidget {
            get {
                typeLabel = new Label();
                return typeLabel;
            }
        }
        
        protected override string TypeEdit {
            get {
                if (string.IsNullOrWhiteSpace(typeLabel.Text)) {
                    throw new FormatException();
                }
                return typeLabel.Text;
            } set {
                typeLabel.Text = value;
            }
        }
        
        private int YearEdit {
            get {
                bool yearParsed = int.TryParse(yearEntry.Text, out int parsedYear);

                if (!yearParsed) {
                    errorType = ERROR_YEAR;
                    throw new FormatException();
                }

                return parsedYear;
            } set {
                yearEntry.Text = value.ToString();
            }
        }
        
        private string WheelSizeEdit {
            get {
                if (string.IsNullOrWhiteSpace(wheelsEntry.Text)) {
                    throw new FormatException();
                }
                return wheelsEntry.Text;
            } set {
                wheelsEntry.Text = value;
            }
        }
        
        private string ForksEdit {
            get {
                if (string.IsNullOrWhiteSpace(forksEntry.Text)) {
                    throw new FormatException();
                }
                return forksEntry.Text;
            } set {
                forksEntry.Text = value;
            }
        }
        
        private int CodeEdit {
            get {
                bool codeParsed = int.TryParse(codeEntry.Text, out int parsedCode);

                if (!codeParsed) {
                    errorType = ERROR_CODE;
                    throw new FormatException();
                }

                return parsedCode;
            } set {
                codeDisp.Text = value.ToString();
            }
        }
    }
}
