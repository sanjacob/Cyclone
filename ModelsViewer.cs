using System;
using Gtk;

namespace Cyclone {
    /// <summary>
    /// Models viewer window. to create, edit, remove valid models.
    /// </summary>
    public class ModelsViewer : Window {
        private VBox windowBox;
        public ModelsMenu modelsMenu;
        public ModelsTools modelsToolbar;
        public TreeView modelTree;
        public Label modelAmount;

        public const int WIN_W = 600;
        public const int WIN_H = 400;
        public const uint defPadding = 20;
        
        public const int STORE_MAKE_P = 0;
        public const int STORE_MODEL_P = 1;
        public const int STORE_TYPE_P = 2;
        
        public const int ICON_SIDE = 32;
        public const int TOOL_SIDE = 24;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cyclone.ModelsViewer"/> class.
        /// </summary>
        public ModelsViewer() : base (WindowType.Toplevel) {
            WindowProperties();

            // Create menu bar
            modelsMenu = new ModelsMenu();

            Statusbar statusbar = new Statusbar();
            modelAmount = new Label("0 models loaded");
            statusbar.PackEnd(modelAmount, false, true, 0);

            // Create toolbar and add it to window
            modelsToolbar = new ModelsTools();

            // Create frame, set border
            Frame mainFrame = new Frame("Valid Bike Models");

            // mainFrame.BorderWidth = defPadding;
            mainFrame.MarginTop = (int) defPadding / 2;
            mainFrame.MarginBottom = mainFrame.MarginLeft = 
                mainFrame.MarginRight = (int) defPadding;

            // Create scroller inside frame
            ScrolledWindow scrollWindow = new ScrolledWindow();
            mainFrame.Add(scrollWindow);

            // Tree View
            modelTree = new ModelsTree();
            scrollWindow.Add(modelTree);

            // Add frame to window
            windowBox.PackStart(modelsMenu, false, false, 0);
            windowBox.PackEnd(statusbar, false, false, 0);
            windowBox.PackEnd(modelsToolbar, false, true, 0);
            windowBox.PackEnd(mainFrame, true, true, 0);
        }

        /// <summary>
        /// Sets the window properties: size and icon.
        /// </summary>
        private void WindowProperties() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);

            //Resizable = false;
            Title = "Customise bike models";

            // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", ICON_SIDE, ICON_SIDE);
            Icon = windowIcon;
            
            // Create uppermost container
            windowBox = new VBox(false, 0);
            Add(windowBox);
        }

        /// <summary>
        /// Updates the model count.
        /// </summary>
        public void ModelCount() {
            ModelAmount = BikeModel.ModelCount;
        }
        
        public int ModelAmount {
            set {
                string pluralNoun = "s";
    
                if (value == 1) {
                    pluralNoun = "";
                }
    
                modelAmount.Text = string.Format("{0} model{1} in inventory", value, pluralNoun);    
            }
        }

        /// <summary>
        /// Simplify error sending.
        /// </summary>
        /// <param name="errorMsg">Error message.</param>
        public void SendError(string errorMsg) {
            MessageDialog yearError = new MessageDialog(this,
                DialogFlags.DestroyWithParent,
                MessageType.Error,
                ButtonsType.Close, errorMsg);
    
            ResponseType yearRes = (ResponseType) yearError.Run();
    
            if (yearRes == ResponseType.Close) {
                yearError.Destroy();
            }
        }

        /// <summary>
        /// Build models menu bar.
        /// </summary>
        public class ModelsMenu : MenuBar {
            public ImageMenuItem importItem;
            public ImageMenuItem exportItem;

            public ModelsMenu() {
                Append(FileMenu);
            }

            /// <summary>
            /// Gets the file menu.
            /// </summary>
            /// <value>The file menu.</value>
            MenuItem FileMenu {
                get {
                    MenuItem fileItem = new MenuItem("File");
                    Menu fileMenu = new Menu();
        
                    AccelGroup fileAccel = new AccelGroup();
            
                    // Create 'Import' button on 'File'
                    importItem = new ImageMenuItem(Stock.Add, fileAccel) {
                        Label = "Import"
                    };
                    
                    // Create 'Export' button on 'File'
                    exportItem = new ImageMenuItem(Stock.SaveAs, fileAccel) {
                        Label = "Export"
                    };
                    
                    importItem.AddAccelerator("activate", fileAccel,
                        new AccelKey(Gdk.Key.i, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
                        
                    exportItem.AddAccelerator("activate", fileAccel,
                        new AccelKey(Gdk.Key.e, Gdk.ModifierType.ControlMask, AccelFlags.Visible));

                    fileMenu.Append(importItem);
                    fileMenu.Append(exportItem);
                    fileItem.Submenu = fileMenu;

                    return fileItem;
                }
            }
        }

        /// <summary>
        /// Builds models toolbar.
        /// </summary>
        public class ModelsTools : Toolbar {
            public ToolButton addModelButton;
            public ToolButton editModelButton;
            public ToolButton removeModelButton;

            public ModelsTools() {
                MarginLeft = (int) defPadding;
                MarginRight = (int) defPadding;
    
                Gdk.Pixbuf addModelIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                         "Cyclone.Assets.Add_2.png", TOOL_SIDE, TOOL_SIDE);
                Image addBikeImg = new Image(addModelIcon);
    
                Gdk.Pixbuf editModelIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                         "Cyclone.Assets.Edit.png", TOOL_SIDE, TOOL_SIDE);
                Image editBikeImg = new Image(editModelIcon);
        
                Gdk.Pixbuf removeModelIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                         "Cyclone.Assets.Close.png", TOOL_SIDE, TOOL_SIDE);
                Image removeBikeImg = new Image(removeModelIcon);
                
                addModelButton = new ToolButton(addBikeImg, "Add Model");
                editModelButton = new ToolButton(editBikeImg, "Edit Model");
                removeModelButton = new ToolButton(removeBikeImg, "Remove Model");
               
                Insert(addModelButton, 0);
                Insert(editModelButton, 1);
                Insert(removeModelButton, 2);
            }
        }

        /// <summary>
        /// Builds models treeview.
        /// </summary>
        public class ModelsTree : TreeView {
            public ModelsTree() { 
                HeadersVisible = true;
                EnableGridLines = TreeViewGridLines.Both;

                AppendColumn("Make", new CellRendererText(), "text", STORE_MAKE_P);
                AppendColumn("Model", new CellRendererText(), "text", STORE_MODEL_P);
                AppendColumn("Type", new CellRendererText(), "text", STORE_TYPE_P);

                CanFocus = true;
                Selection.Mode = SelectionMode.Multiple;
        
                // Set attributes for all columns       
                foreach (TreeViewColumn column in Columns) {
                    column.Resizable = true;
                    column.Clickable = true;
                    column.Expand = true;
                }
            }
        }
    }
}
