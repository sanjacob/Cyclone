using System;
using Cyclone.Objects;
using Gtk;

namespace Cyclone {
    public class ActiveRentals : BaseViewer {
        ToolButton returnButton;

        public ActiveRentals()  {
            uint basePadding = 0;

            // Add frame to window
            windowBox.PackStart(ViewerMenu, false, false, basePadding);
            windowBox.PackEnd(ViewerStatusbar, false, false, basePadding);
            windowBox.PackEnd(EditingToolbar, false, true, basePadding);
            windowBox.PackEnd(ViewerFrame, true, true, basePadding);
        }

        /// <summary>
        /// Build models menu bar.
        /// </summary>
        public override MenuBar ViewerMenu {
            get {
                MenuBar viewMenu = new MenuBar();
                return viewMenu;
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

                Gdk.Pixbuf returnIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                         "Cyclone.Assets.Money.png", TOOL_SIDE, TOOL_SIDE);
                Image returnImg = new Image(returnIcon);

                returnButton = new ToolButton(returnImg, "Add Model");

                editToolbar.Insert(returnButton, 0);
                return editToolbar;
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
        /// Builds models treeview.
        /// </summary>
        public override TreeView ViewerTree {
            get {
                TreeView viewTree = new TreeView();
                viewTree.HeadersVisible = true;
                viewTree.EnableGridLines = TreeViewGridLines.Both;

                viewTree.AppendColumn("Renter", new CellRendererText(), "text", 0);
                viewTree.AppendColumn("Bikes Rented", new CellRendererText(), "text", 1);
                viewTree.AppendColumn("No. Of Bikes", new CellRendererText(), "text", 2);
                viewTree.AppendColumn("Date / Time", new CellRendererText(), "text", 3);

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

        public override Statusbar ViewerStatusbar {
            get {
                Statusbar statusbar = new Statusbar();
                ItemAmount = new Label("0 active rentals");
                statusbar.PackEnd(ItemAmount, false, true, 0);
                return statusbar;
            }
        }

        /// <summary>
        /// Updates the model count.
        /// </summary>
        public override void ItemCount() {
            RentalsAmount = Rent.RentalsCount;
        }

        public int RentalsAmount {
            set {
                string pluralNoun = "s";
    
                if (value == 1) {
                    pluralNoun = "";
                }
    
                ItemAmount.Text = string.Format("{0} active rental{1}", value, pluralNoun);    
            }
        }

        protected override int WIN_H {
            get {
                return 500;
            }
        }

        protected override int WIN_W {
            get {
                return 460;
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
                return "Active Rentals - Cyclone";
            }
        }

        protected override string IconFilename {
            get {
                return "Cyclone.Assets.Clock.png";
            }
        }

        protected override string FrameTitle {
            get {
                return "Current Rentals";
            }
        }
    }
}
