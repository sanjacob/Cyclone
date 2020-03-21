using System;
using Gtk;
namespace Cyclone {
    public class ModelsEditor : Window {
        public const int WIN_W = 600;
        public const int WIN_H = 400;
        public const uint defPadding = 20;

        public TreeView modelTree;
        public ImageMenuItem importItem;
        public ImageMenuItem exportItem;
        public Label modelAmount;

        public ModelsEditor() : base (WindowType.Toplevel) {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);
            //Resizable = false;
            Title = "Customise bike models";
               // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", 32, 32);
            Icon = windowIcon;
            
            // Create uppermost container
            VBox windowList = new VBox(false, 0);
            Add(windowList);
    
            // Create menu bar
            MenuBar menuBar = new MenuBar();
            
            // Create tabs of menu
            Menu fileMenu = new Menu();
            MenuItem fileItem = new MenuItem("File");
    
            AccelGroup accel = new AccelGroup();
            AddAccelGroup(accel);
    
            // Create 'Import' button on 'File'
            importItem = new ImageMenuItem(Stock.Add, accel) {
                Label = "Import"
            };
            
            importItem.AddAccelerator("activate", accel,
                new AccelKey(Gdk.Key.i, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
    
            // Create 'Export' button on 'File'
            exportItem = new ImageMenuItem(Stock.SaveAs, accel) {
                Label = "Export"
            };
            exportItem.AddAccelerator("activate", accel,
                new AccelKey(Gdk.Key.e, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
                
            // Place button on menu, on bar
            fileMenu.Append(importItem);
            fileMenu.Append(exportItem);
    
            fileItem.Submenu = fileMenu;
    
            menuBar.Append(fileItem);
    
            // Add menu bar to window
            windowList.PackStart(menuBar, false, false, 0);
            
            Statusbar statusbar = new Statusbar();
            modelAmount = new Label("0 models loaded");
            statusbar.PackEnd(modelAmount, false, true, 0);
        
            windowList.PackEnd(statusbar, false, false, 0);
            
            // Create frame, set border
            Frame mainFrame = new Frame("Valid Bike Models");
            
            // mainFrame.BorderWidth = defPadding;
            mainFrame.MarginBottom = mainFrame.MarginLeft = 
            mainFrame.MarginRight = (int) defPadding;
            mainFrame.MarginTop = (int) defPadding / 2;
            
            // Create scroller inside frame
            ScrolledWindow scrollWindow = new ScrolledWindow();
            mainFrame.Add(scrollWindow);
        
            // Add frame to window
            windowList.PackEnd(mainFrame, true, true, 0);
            
            // Tree View
            modelTree= new TreeView();
            modelTree.HeadersVisible = true;
            modelTree.EnableGridLines = TreeViewGridLines.Both;
            
            modelTree.AppendColumn("Make", new CellRendererText(), "text", 0);
            modelTree.AppendColumn("Model", new CellRendererText(), "text", 1);
            modelTree.AppendColumn("Type", new CellRendererText(), "text", 2);
            
            modelTree.CanFocus = true;
            modelTree.Selection.Mode = SelectionMode.Multiple;
            //modelTree.Selection.Changed += changeRowSelection;
    
            // Set attributes for all columns
            int colCounter = 0;
    
            foreach (TreeViewColumn column in modelTree.Columns) {
                column.Resizable = true;
                column.Clickable = true;
                column.Expand = true;
                colCounter++;
            }

            // Add tree to scrollable
            scrollWindow.Add(modelTree);
        }
    }
}
