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

        private uint _SAVE_START = 22;
        private uint _EDITOR_ROWS = 23;
        private int _WIN_H = 400;

        private const int MIN_YEAR = 1200;
        private const int FUTURE_MARGIN = 10;
        public const int ERROR_YEAR = 1;
        public const int ERROR_CODE = 3;
        public const int ERROR_COST = 4;

        public Entry yearEntry;
        public Entry wheelsEntry;
        public Entry forksEntry;
        public Entry codeEntry;
        public Entry frontDEntry;
        public Entry rearDEntry;
        public Entry gearsEntry;
        public Entry shiftersEntry;
        public Entry chainSetEntry;
        public Entry chainEntry;
        public Entry brakeSetEntry;
        public Entry handleBarsEntry;
        public Entry stemEntry;
        public Entry rimsEntry;
        public Entry tyresEntry;
        public Entry saddleEntry;
        public Entry weightEntry;
        public Label codeDisp;
        public Entry costEntry;

        public CheckButton buyCheck;

        private int currentCode;
        private bool codeDisplay;

        public BikeEditor(Dictionary<string, List<BikeModel>> validModels) {
            comboModels = validModels;
            createBikeEditor();
            Title = "Create Bike - Cyclone";
        }

        public BikeEditor(Dictionary<string, List<BikeModel>> validModels, Bike bike) {

            comboModels = validModels;
            codeDisplay = true;
            createBikeEditor();

            Title = "Modify Bike - Cyclone";
            MakeEdit = bike.Make;
            ModelEdit = bike.Model;
            TypeEdit = bike.Type;

            YearEdit = bike.Year;
            WheelSizeEdit = bike.WheelSize;
            ForksEdit = bike.Forks;
            CodeEdit = bike.SecurityCode;

            currentCode = bike.SecurityCode;

            frontDEntry.Text = bike.FrontDerailleur;
            rearDEntry.Text = bike.RearDerailleur;
            shiftersEntry.Text = bike.Shifters;

            if (bike.Gears != 0) {
                gearsEntry.Text = bike.Gears.ToString();
            }

            chainSetEntry.Text = bike.ChainSet;
            chainEntry.Text = bike.Chain;
            brakeSetEntry.Text = bike.BrakeSet;
            handleBarsEntry.Text = bike.Handlebars;
            stemEntry.Text = bike.Stem;
            rimsEntry.Text = bike.Rims;
            tyresEntry.Text = bike.Tyres;
            saddleEntry.Text = bike.Saddle;

            if (bike.Weight > 0) {
                weightEntry.Text = bike.Weight.ToString();
            }

            if (bike.Cost > 0) {
                costEntry.Text = bike.Cost.ToString();
            }
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

            if (!string.IsNullOrWhiteSpace(frontDEntry.Text)) {
                savedBike.FrontDerailleur = frontDEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(rearDEntry.Text)) {
                savedBike.RearDerailleur = rearDEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(shiftersEntry.Text)) {
                savedBike.Shifters = shiftersEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(chainSetEntry.Text)) {
                savedBike.ChainSet = chainSetEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(chainEntry.Text)) {
                savedBike.Chain= chainEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(brakeSetEntry.Text)) {
                savedBike.BrakeSet = brakeSetEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(handleBarsEntry.Text)) {
                savedBike.Handlebars = handleBarsEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(stemEntry.Text)) {
                savedBike.Stem = stemEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(rimsEntry.Text)) {
                savedBike.Rims = rimsEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(tyresEntry.Text)) {
                savedBike.Tyres = tyresEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(saddleEntry.Text)) {
                savedBike.Saddle= saddleEntry.Text;
            }

            if (!string.IsNullOrWhiteSpace(gearsEntry.Text)) {
                if (int.TryParse(gearsEntry.Text, out int gearsNumber)) {
                    savedBike.Gears = gearsNumber;
                }
            }

            if (!string.IsNullOrWhiteSpace(weightEntry.Text)) {
                if (double.TryParse(weightEntry.Text, out double weightNumber)) {
                    savedBike.Weight = weightNumber;
                }
            }

            if (!string.IsNullOrWhiteSpace(costEntry.Text) && buyCheck.Active) {
                if (double.TryParse(costEntry.Text, out double costNumber)) {
                    savedBike.Cost = costNumber;
                }
            }

            return savedBike;
        }

        private void BuildTable() {
            Label yearLabel = new Label("Year: ");
            Label wheelLabel = new Label("Wheel Size: ");
            Label forksLabel = new Label("Forks: ");
            Label priceLabel = new Label("Retail Price: ");
            Label codeLabel = new Label("Security Code: ");
            Label frontDLabel = new Label("Front Derailleur: ");
            Label rearDLabel = new Label("Rear Derailleur: ");
            Label gearsLabel = new Label("Gears: ");
            Label shiftersLabel = new Label("Shifters: ");
            Label chainSetLabel = new Label("Chain Set: ");
            Label chainLabel = new Label("Chain: ");
            Label brakeSetLabel = new Label("Brake Set: ");
            Label handleBarsLabel = new Label("Handlebars: ");
            Label stemLabel = new Label("Stem: ");
            Label rimsLabel = new Label("Rims: ");
            Label tyresLabel = new Label("Tyres: ");
            Label saddleLabel = new Label("Saddle: ");
            Label weightLabel = new Label("Weight (KG): ");

            yearEntry = new Entry();
            wheelsEntry = new Entry();
            forksEntry = new Entry();
            codeEntry = new Entry();
            codeDisp = new Label();
            frontDEntry = new Entry();
            rearDEntry = new Entry();
            gearsEntry = new Entry();
            shiftersEntry = new Entry();
            chainSetEntry = new Entry();
            chainEntry = new Entry();
            brakeSetEntry = new Entry();
            handleBarsEntry = new Entry();
            stemEntry = new Entry();
            rimsEntry = new Entry();
            tyresEntry = new Entry();
            saddleEntry = new Entry();
            weightEntry = new Entry();
            costEntry = new Entry();

            Widget codeWidget = codeEntry;

            if (codeDisplay) {
                codeWidget = codeDisp;
            }

            List<Widget> editorWidgets = new List<Widget> { yearEntry, codeWidget, wheelsEntry, forksEntry,
                frontDEntry, rearDEntry, gearsEntry, shiftersEntry, chainSetEntry, chainEntry, brakeSetEntry,
                handleBarsEntry, stemEntry, rimsEntry, tyresEntry, saddleEntry, weightEntry };
            List<Widget> labelWidgets = new List<Widget> { yearLabel, codeLabel, wheelLabel, forksLabel, 
                frontDLabel, rearDLabel, gearsLabel, shiftersLabel, chainSetLabel, chainLabel, brakeSetLabel, 
                handleBarsLabel, stemLabel, rimsLabel, tyresLabel, saddleLabel, weightLabel };

            uint offset = 3;
            uint span = 1;
            int row = 0;
            for (; row < editorWidgets.Count; row++) {
                editorTable.Attach(labelWidgets[row], LABEL_COL, LABEL_END, offset + (uint) row, offset + (uint) row + span, 
                    AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
                editorTable.Attach(editorWidgets[row], ENTRY_COL, ENTRY_END, offset + (uint) row, offset + (uint) row + span, 
                    AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            }

            if (!codeDisplay) {
                buyCheck = new CheckButton("Register as purchase?");
                buyCheck.StateChanged += ToggleCostLabel;
                editorTable.Attach(buyCheck, LABEL_COL, ENTRY_END, offset + (uint)row, offset + (uint)row + span,
                    AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
                row++;
            }

            Label costLabel = new Label("Cost: ");
            editorTable.Attach(costLabel, LABEL_COL, LABEL_END, offset + (uint) row, offset + (uint) row + span, 
                AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
            costEntry.Sensitive = false;
            editorTable.Attach(costEntry, ENTRY_COL, ENTRY_END, offset + (uint) row, offset + (uint) row + span, 
                AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
        }

        private void ToggleCostLabel(object o, StateChangedArgs args) {
            costEntry.Sensitive = buyCheck.Active;
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

        private int CostEdit {
            get {
                bool costParsed = int.TryParse(costEntry.Text, out int parsedCost);

                if (!costParsed) {
                    errorType = ERROR_COST;
                    throw new FormatException();
                }

                return parsedCost;
            } set {
                costEntry.Text = value.ToString();
            }
        }

        protected override int WIN_H {
            get {
                return _WIN_H;
            }
        }
    }
}
