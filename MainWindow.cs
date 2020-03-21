using System;
using System.Collections.Generic;
using Gtk;

public partial class MainWindow : Gtk.Window {
    public TreeView bikeTree;
    public ImageMenuItem importItem;
    public ImageMenuItem exportItem;
    public MenuItem modelsItem;
    public CheckMenuItem changeView;

    public bool allSelected = false;
    public Label bikeAmount;

    public ToolButton addBikeButton;
    public ToolButton removeBikeButton;

    public const uint defPadding = 20;
    public const int WIN_W = 800;
    public const int WIN_H = 500;

    public MainWindow() : base(Gtk.WindowType.Toplevel) {
        // Set size
        SetDefaultSize(WIN_W, WIN_H);
        SetSizeRequest(WIN_W, WIN_H);

        // Set window icon
        Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                 "Cyclone.Assets.Cyclone.png", 32, 32);
        Icon = windowIcon;

        // Create uppermost container
        VBox windowList = new VBox(false, 0);
        Add(windowList);

        // Create menu bar
        MenuBar menuBar = new MenuBar();
        
        // Create tabs of menu
        Menu fileMenu = new Menu();
        MenuItem fileItem = new MenuItem("File");
        
        Menu editMenu = new Menu();
        MenuItem editItem = new MenuItem("Edit");

        Menu viewMenu = new Menu();
        MenuItem viewItem = new MenuItem("View");
        
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

        modelsItem = new MenuItem("Valid Bike Models");

        changeView = new CheckMenuItem("Group by model");
        changeView.Toggle();
        
        // Place button on menu, on bar
        fileMenu.Append(importItem);
        fileMenu.Append(exportItem);
        editMenu.Append(modelsItem);
        viewMenu.Append(changeView);

        fileItem.Submenu = fileMenu;
        editItem.Submenu = editMenu;
        viewItem.Submenu = viewMenu;

        menuBar.Append(fileItem);
        menuBar.Append(editItem);
        menuBar.Append(viewItem);

        // Add menu bar to window
        windowList.PackStart(menuBar, false, false, 0);

        // Create toolbar and add it to window
        Toolbar bikeToolbar = new Toolbar();

        Gdk.Pixbuf addBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                 "Cyclone.Assets.AddBike.png", 32, 32);
        Image addBikeImg = new Image(addBikeIcon);

        Gdk.Pixbuf removeBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                 "Cyclone.Assets.Close.png", 32, 32);
        Image removeBikeImg = new Image(removeBikeIcon);
        
        addBikeButton = new ToolButton(addBikeImg, "Add Bike");
        removeBikeButton = new ToolButton(removeBikeImg, "Remove Bike");
       
        bikeToolbar.Insert(addBikeButton, 0);
        bikeToolbar.Insert(removeBikeButton, 1);
        windowList.PackStart(bikeToolbar, false, false, 0);

        Separator toolSeparator = new Separator(Orientation.Horizontal);
        windowList.PackStart(toolSeparator, false, false, 0);

        Statusbar statusbar = new Statusbar();
        bikeAmount = new Label("0 bikes in inventory");
        statusbar.PackEnd(bikeAmount, false, true, 0);

        windowList.PackEnd(statusbar, false, false, 0);

        // Create frame, set border
        Frame mainFrame = new Frame("Bike Inventory");
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
        bikeTree = new TreeView();
        bikeTree.HeadersVisible = true;
        bikeTree.EnableGridLines = TreeViewGridLines.Both;

        // Toggle selection cell renderer
        CellRendererToggle selectBikes = new CellRendererToggle();
        selectBikes.Activatable = true;
        
        // Edit row cell renderer
        CellRendererPixbuf editCell = new CellRendererPixbuf();
        Gdk.Pixbuf editIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                 "Cyclone.Assets.edit.png", 32, 32);
        editCell.Pixbuf = editIcon;

        // Construct columns of tree
        bikeTree.AppendColumn("Make", new CellRendererText(), "text", 1);
        bikeTree.AppendColumn("Model", new CellRendererText(), "text", 2);
        bikeTree.AppendColumn("Year", new CellRendererText(), "text", 3);
        bikeTree.AppendColumn("Type", new CellRendererText(), "text", 4);     
        //bikeTree.AppendColumn("Actions", editCell);

        bikeTree.CanFocus = true;
        bikeTree.Selection.Mode = SelectionMode.Multiple;
        bikeTree.Selection.Changed += changeRowSelection;

        // Set attributes for all columns
        int colCounter = 1;

        foreach (TreeViewColumn column in bikeTree.Columns) {
            column.Resizable = true;
            column.Clickable = true;
            column.SortColumnId = colCounter;

            // Don't add sort mechanism to selecting column (which is the first one)
            if (colCounter > 0) {
                column.Expand = true;
            }
            colCounter++;
        }
        
        // Add tree to scrollable
        scrollWindow.Add(bikeTree);

        // Attach the label over the entire first row

        //Icon = Properties.Resources.RedIcon;
        Build();
    }

    private void changeRowSelection(object sender, EventArgs e) {
            Console.WriteLine(String.Format("{0} rows are selected", bikeTree.Selection.GetSelectedRows().Length));
            TreePath[] newSelection = bikeTree.Selection.GetSelectedRows();
    }

    private void selectAll(object sender, EventArgs e) {
         bikeTree.Selection.UnselectAll();
        if (!allSelected) {
            bikeTree.Selection.SelectAll();
        }

        allSelected = !allSelected;
    }
    
    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }

    void aboutDialog(object sender, EventArgs args) {
        AboutDialog about = new AboutDialog();
        about.ProgramName = "Cyclone";
        about.Version = "0.01";
        about.Copyright = "(c) Sánchez Industries";
        about.Comments = @"Inventory manager for bike rental business";
        about.Website = "http://rusos.uk";
        about.Logo = new Gdk.Pixbuf("battery.png");
        about.Run();
        about.Destroy();
    }
}