using System;
using Gtk;

namespace Cyclone {
    /// <summary>
    /// Models viewer window. to create, edit, remove valid models.
    /// </summary>
    public class ModelsViewer : BaseViewer {
        public const int STORE_MAKE_P = 0;
        public const int STORE_MODEL_P = 1;
        public const int STORE_TYPE_P = 2;

        public ToolButton addModelButton;
        public ToolButton editModelButton;
        public ToolButton removeModelButton;
        
        public ImageMenuItem importItem;
        public ImageMenuItem exportItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cyclone.ModelsViewer"/> class.
        /// </summary>
        public ModelsViewer() {
           uint basePadding = 0;

            // Add frame to window
            windowBox.PackStart(ViewerMenu, false, false, basePadding);
            windowBox.PackEnd(ViewerStatusbar, false, false, basePadding);
            windowBox.PackEnd(EditingToolbar, false, true, basePadding);
            windowBox.PackEnd(ViewerFrame, true, true, basePadding);
        }

        public override Statusbar ViewerStatusbar {
            get {
                Statusbar statusbar = new Statusbar();
                ItemAmount = new Label("0 models loaded");
                statusbar.PackEnd(ItemAmount, false, true, 0);
                return statusbar;
            }
        }

        /// <summary>
        /// Updates the model count.
        /// </summary>
        public override void ItemCount() {
            ModelAmount = BikeModel.ModelCount;
        }
        
        public int ModelAmount {
            set {
                string pluralNoun = "s";
    
                if (value == 1) {
                    pluralNoun = "";
                }
    
                ItemAmount.Text = string.Format("{0} model{1} in inventory", value, pluralNoun);    
            }
        }

        public override Frame ViewerFrame {
            get {
                // Create frame, set border
                Frame mainFrame = new Frame(FrameTitle);
                //mainFrame.BorderWidth = DefaultPadding;
                mainFrame.MarginTop = (int) DefaultPadding / 2;
                mainFrame.MarginBottom = mainFrame.MarginLeft = mainFrame.MarginRight = (int) DefaultPadding;

                // Create scroller inside frame
                ScrolledWindow scrollWindow = new ScrolledWindow();
                mainFrame.Add(scrollWindow);

                BaseTree = ViewerTree;

                // Tree View
                scrollWindow.Add(BaseTree);

                return mainFrame;
            }
        }

        /// <summary>
        /// Build models menu bar.
        /// </summary>
        public override MenuBar ViewerMenu {
            get {
                MenuBar viewMenu = new MenuBar();
                viewMenu.Append(FileMenu);
                return viewMenu;
            }
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

        /// <summary>
        /// Builds models toolbar.
        /// </summary>
        public override Toolbar EditingToolbar {
            get {
                Toolbar editToolbar = new Toolbar();
                editToolbar.MarginLeft = (int) DefaultPadding;
                editToolbar.MarginRight = (int) DefaultPadding;

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

                editToolbar.Insert(addModelButton, 0);
                editToolbar.Insert(editModelButton, 1);
                editToolbar.Insert(removeModelButton, 2);
                return editToolbar;
            }
        }

        /// <summary>
        /// Builds models treeview.
        /// </summary>
        public override TreeView ViewerTree {
            get {
                TreeView viewTree = new TreeView();
                viewTree.HeadersVisible = true;
                viewTree.EnableGridLines = TreeViewGridLines.Both;

                viewTree.AppendColumn("Make", new CellRendererText(), "text", STORE_MAKE_P);
                viewTree.AppendColumn("Model", new CellRendererText(), "text", STORE_MODEL_P);
                viewTree.AppendColumn("Type", new CellRendererText(), "text", STORE_TYPE_P);

                viewTree.CanFocus = true;
                viewTree.Selection.Mode = SelectionMode.Multiple;
        
                // Set attributes for all columns       
                foreach (TreeViewColumn column in viewTree.Columns) {
                    column.Resizable = true;
                    column.Clickable = true;
                    column.Expand = true;
                }
                return viewTree;
            }
        }
        
        protected override int WIN_H {
            get {
                return 400;
            }
        }

        protected override int WIN_W {
            get {
                return 600;
            }
        }

        protected override int ICON_W {
            get {
                return 32;
            }
        }

        protected override int ICON_H {
            get {
                return 32;
            }
        }

        protected override string TitleProperty {
            get {
                return "Customise Valid Bike Models - Cyclone";
            }
        }

        protected override string IconFilename {
            get {
                return "Cyclone.Assets.Bike_Yellow.png";
            }
        }

        protected override string FrameTitle {
            get {
                return "Valid Bike Models";
            }
        }
    }
}