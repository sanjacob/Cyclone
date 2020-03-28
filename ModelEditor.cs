using System;
using Gtk;
namespace Cyclone {
    public class ModelEditor : Window {
        public BikeModel oldModel;
        protected Table editorTable;

        public Entry makeEntry;
        public Entry modelEntry;
        public Entry typeEntry;

        public Button saveEdit;

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

        public ModelEditor(BikeModel editedModel) : base(WindowType.Toplevel) {
            oldModel = editedModel;
            createEditor();

            Title = "Modify model";
            MakeEdit = editedModel.Make;
            ModelEdit = editedModel.Model;
            TypeEdit = editedModel.Type;
        }

        public void createEditor() {
            EditorProperties();
            editorTable = ModelTable;
            Add(editorTable);
        }
        
        private void EditorProperties() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);
            //Resizable = false;
            Title = "Create model";

            // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", ICON_SIDE, ICON_SIDE);
            Icon = windowIcon;
        }

        public BikeModel ParseModel() {
            BikeModel editedModel = new BikeModel(MakeEdit, ModelEdit, TypeEdit);
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

        protected virtual Widget MakeWidget {
            get {
                makeEntry = new Entry();
                return makeEntry;
            }
        }
        
        protected virtual string MakeEdit {
            get {
                if (string.IsNullOrWhiteSpace(makeEntry.Text)) {
                    throw new FormatException();
                }
                return makeEntry.Text;
            } set {
                makeEntry.Text = value;
            }
        }

        protected virtual Widget ModelWidget {
            get {
                modelEntry = new Entry();
                return modelEntry;
            }
        }

        protected virtual string ModelEdit {
            get {
                if (string.IsNullOrWhiteSpace(modelEntry.Text)) {
                    throw new FormatException();
                }
                return modelEntry.Text;
            } set {
                modelEntry.Text = value;
            }
        }

        protected virtual Widget TypeWidget {
            get {
                typeEntry = new Entry();
                return typeEntry;
            }
        }
        
        protected virtual string TypeEdit {
            get {
                if (string.IsNullOrWhiteSpace(typeEntry.Text)) {
                    throw new FormatException();
                }
                return typeEntry.Text;
            } set {
                typeEntry.Text = value;
            }
        }

        private Table ModelTable {
            get {
                Table newTable = new Table(EDITOR_ROWS, EDITOR_COLUMNS, false);
                newTable.BorderWidth = EDITOR_BORDER_WIDTH;
    
                Label makeLabel = new Label("Make: ");
                Label modelLabel = new Label("Model: ");
                Label typeLabel = new Label("Type: ");
    
                saveEdit = new Button();
                Label saveNemo = new Label("_Save");
                saveEdit.Label = "Save";
                saveEdit.AddMnemonicLabel(saveNemo);
    
                newTable.Attach(makeLabel, LABEL_COL, LABEL_END, 0, 1, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
                newTable.Attach(modelLabel, LABEL_COL, LABEL_END, 1, 2, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
                newTable.Attach(typeLabel, LABEL_COL, LABEL_END, 2, 3, AttachOptions.Shrink, AttachOptions.Expand, CELL_PAD, CELL_PAD);
    
                newTable.Attach(MakeWidget, ENTRY_COL, ENTRY_END, 0, 1, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
                newTable.Attach(ModelWidget, ENTRY_COL, ENTRY_END, 1, 2, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
                newTable.Attach(TypeWidget, ENTRY_COL, ENTRY_END, 2, 3, AttachOptions.Fill, AttachOptions.Expand, CELL_PAD, CELL_PAD);
    
                uint SAVE_END = SAVE_START + SAVE_SPAN;
                newTable.Attach(saveEdit, LABEL_COL, ENTRY_END, SAVE_START, SAVE_END, AttachOptions.Expand, AttachOptions.Fill, CELL_PAD * 2, CELL_PAD * 2);

                return newTable;
            }
        }
    }
}
