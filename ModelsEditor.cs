using System;
using Gtk;
namespace Cyclone {
    public class ModelsEditor : Window {
        public const int WIN_W = 600;
        public const int WIN_H = 400;
        public const uint defPadding = 20;
        
        public const int STORE_MAKE_P = 0;
        public const int STORE_MODEL_P = 1;
        public const int STORE_TYPE_P = 2;
        
        public const int ICON_SIDE = 32;
        public const int TOOL_SIDE = 24;

        public TreeView modelTree;
        public ImageMenuItem importItem;
        public ImageMenuItem exportItem;
        public Label modelAmount;
        
        public ToolButton addModelButton;
        public ToolButton editModelButton;
        public ToolButton removeModelButton;
    

        public ModelsEditor() : base (WindowType.Toplevel) {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);
            //Resizable = false;
            Title = "Customise bike models";
               // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     "Cyclone.Assets.Bike_Yellow.png", ICON_SIDE, ICON_SIDE);
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
            
            // Create toolbar and add it to window
            Toolbar bikeToolbar = new Toolbar();
            bikeToolbar.MarginLeft = (int) defPadding;
            bikeToolbar.MarginRight = (int) defPadding;

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
           
            bikeToolbar.Insert(addModelButton, 0);
            bikeToolbar.Insert(editModelButton, 1);
            bikeToolbar.Insert(removeModelButton, 2);
            windowList.PackEnd(bikeToolbar, false, true, 0);
            
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
            
            modelTree.AppendColumn("Make", new CellRendererText(), "text", STORE_MAKE_P);
            modelTree.AppendColumn("Model", new CellRendererText(), "text", STORE_MODEL_P);
            modelTree.AppendColumn("Type", new CellRendererText(), "text", STORE_TYPE_P);
            
            modelTree.CanFocus = true;
            modelTree.Selection.Mode = SelectionMode.Multiple;
            //modelTree.Selection.Changed += changeRowSelection;
    
            // Set attributes for all columns
    
            foreach (TreeViewColumn column in modelTree.Columns) {
                column.Resizable = true;
                column.Clickable = true;
                column.Expand = true;
            }

            // Add tree to scrollable
            scrollWindow.Add(modelTree);
        }
    }
}
