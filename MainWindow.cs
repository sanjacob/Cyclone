using System;
using System.Collections.Generic;
using Gtk;
using Cyclone;
using Cyclone.Objects;

/// <summary>
/// Main window for Cyclone app.
/// </summary>
public sealed class MainWindow : BaseViewer {
    public Label BalanceLabel;

    public const int STORE_MAKE_P = 1;
    public const int STORE_MODEL_P = 2;
    public const int STORE_TYPE_P = 4;
    public const int STORE_YEAR_P = 3;
    
    public ToolButton addBikeButton;
    public ToolButton removeBikeButton;
    public ToolButton sellBikeButton;
    public ToolButton rentBikeButton;
    public ToolButton rentalsButton;
    
    public ImageMenuItem importItem;
    public ImageMenuItem exportItem;
    public ImageMenuItem saveItem;
    public MenuItem modelsItem;
    public MenuItem clearItem;
    public CheckMenuItem changeView;
    public MenuItem aboutItem;

    public AccelGroup fileAccel;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:MainWindow"/> class.
    /// </summary>
    public MainWindow() {
        uint basePadding = 0;

        // Create menu bar
        MenuBar mainMenu = ViewerMenu;
        AddAccelGroup(fileAccel);
        aboutItem.Activated += AboutDialog;

        // Create toolbar and add it to window
        Toolbar bikeToolbar = EditingToolbar;

        Separator toolSeparator = new Separator(Orientation.Horizontal);

        BaseStatusbar = ViewerStatusbar;

        // Add all elements to window vertical container
        windowBox.PackStart(mainMenu, false, false, basePadding);
        windowBox.PackStart(bikeToolbar, false, false, basePadding);
        windowBox.PackStart(toolSeparator, false, false, basePadding);

        windowBox.PackEnd(BaseStatusbar, false, false, basePadding);
        windowBox.PackEnd(ViewerFrame, true, true, basePadding);
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
    /// Handles the delete event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="a">The alpha component.</param>
    private void OnDeleteEvent(object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }

    /// <summary>
    /// Shows the About dialog.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    void AboutDialog(object sender, EventArgs args) {
        AboutDialog about = MainAbout;
        about.Run();
        about.Destroy();
    }

    /// <summary>
    /// Updates the bike count.
    /// </summary>
    public override void ItemCount() {
        BikeAmount = Bike.BikeCount;
    }

    public int BikeAmount {
        set {
            string pluralNoun = "s";

            if (value == 1) {
                pluralNoun = "";
            }

            ItemAmount.Text = string.Format("{0} model{1} in inventory", value, pluralNoun);
        }
    }

    /// <summary>
    /// Updates the bike count.
    /// </summary>
    public void Balance() {
        StoreBalance = Sale.Balance;
    }

    public double StoreBalance {
        set {
            string sign = "";
            if (value < 0) {
                sign = "-";
            }
            BalanceLabel.Text = string.Format("Balance: {0}${1}", sign, Math.Abs(value));
        }
    }

     /// <summary>
    /// Build models menu bar.
    /// </summary>
    public override MenuBar ViewerMenu {
        get {
            MenuBar viewMenu = new MenuBar();
            viewMenu.Append(FileMenu);
            viewMenu.Append(EditMenu);
            viewMenu.Append(ViewMenu);
            viewMenu.Append(HelpMenu);
            return viewMenu;
        }
    }

    /// <summary>
    /// Builds the file menu.
    /// </summary>
    /// <value>The file menu.</value>
    MenuItem FileMenu {
        get {
            MenuItem fileItem = new MenuItem("File");
            Menu fileMenu = new Menu();

            // File Tab
            fileAccel = new AccelGroup();

            // Create 'Import' button on 'File'
            importItem = new ImageMenuItem(Stock.Add, fileAccel) {
                Label = "Import"
            };

            // Create 'Export' button on 'File'
            exportItem = new ImageMenuItem(Stock.SaveAs, fileAccel) {
                Label = "Export"
            };

            // Create 'Export' button on 'File'
            saveItem = new ImageMenuItem(Stock.Save, fileAccel) {
                Label = "Save"
            };

            importItem.AddAccelerator("activate", fileAccel,
                new AccelKey(Gdk.Key.i, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            exportItem.AddAccelerator("activate", fileAccel,
                new AccelKey(Gdk.Key.e, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            saveItem.AddAccelerator("activate", fileAccel,
                new AccelKey(Gdk.Key.s, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            // Append all items to each tab
            fileMenu.Append(saveItem);
            fileMenu.Append(importItem);
            fileMenu.Append(exportItem);
            fileItem.Submenu = fileMenu;

            return fileItem;
        }
    }

    /// <summary>
    /// Builds the edit menu.
    /// </summary>
    /// <value>The edit menu.</value>
    MenuItem EditMenu {
        get {
            MenuItem editItem = new MenuItem("Edit");
            Menu editMenu = new Menu();

            // Edit Tab
            modelsItem = new MenuItem("Valid Bike Models");
            clearItem = new MenuItem("Clear Inventory");

            editMenu.Append(modelsItem);
            editMenu.Append(clearItem);
            editItem.Submenu = editMenu;

            return editItem;
        }
    }

    /// <summary>
    /// Builds the view menu.
    /// </summary>
    /// <value>The view menu.</value>
    MenuItem ViewMenu {
        get {
            MenuItem viewItem = new MenuItem("View");
            Menu viewMenu = new Menu();

            // View Tab
            changeView = new CheckMenuItem("Group by model");
            changeView.Toggle();

            viewMenu.Append(changeView);
            viewItem.Submenu = viewMenu;

            return viewItem;
        }
    }

    /// <summary>
    /// Builds the help menu.
    /// </summary>
    /// <value>The help menu.</value>
    MenuItem HelpMenu {
        get {
            MenuItem helpItem = new MenuItem("Help");
            Menu helpMenu = new Menu();

            // Help Tab
            aboutItem = new MenuItem("About");

            helpMenu.Append(aboutItem);
            helpItem.Submenu = helpMenu;

            return helpItem;
        }
    }

    /// <summary>
    /// Builds main toolbar.
    /// </summary>
    public override Toolbar EditingToolbar {
        get {
            Toolbar editToolbar = new Toolbar();
            Gdk.Pixbuf addBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.AddBike.png", ICON_W, ICON_W);
            Image addBikeImg = new Image(addBikeIcon);

            Gdk.Pixbuf removeBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Close.png", ICON_W, ICON_W);
            Image removeBikeImg = new Image(removeBikeIcon);

            Gdk.Pixbuf sellBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.SellBike.png", ICON_W, ICON_W);
            Image sellBikeImg = new Image(sellBikeIcon);
            
            Gdk.Pixbuf rentBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.RentBike.png", ICON_W, ICON_W);
            Image rentBikeImg = new Image(rentBikeIcon);
            
            Gdk.Pixbuf rentalsIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Clock.png", ICON_W, ICON_W);
            Image rentalsImg = new Image(rentalsIcon);

            addBikeButton = new ToolButton(addBikeImg, "Add Bike");
            removeBikeButton = new ToolButton(removeBikeImg, "Remove Bike");
            sellBikeButton = new ToolButton(sellBikeImg, "Sell Bike");
            rentBikeButton = new ToolButton(rentBikeImg, "Rent Bike");
            rentalsButton = new ToolButton(rentalsImg, "Active Rentals");

            SeparatorToolItem commerceActionsSeparator = new SeparatorToolItem();
            SeparatorToolItem activeListsSeparator = new SeparatorToolItem();

            editToolbar.Insert(addBikeButton, 0);
            editToolbar.Insert(removeBikeButton, 1);
            editToolbar.Insert(commerceActionsSeparator, 2);
            editToolbar.Insert(sellBikeButton, 3);
            editToolbar.Insert(rentBikeButton, 4);
            editToolbar.Insert(activeListsSeparator, 5);
            editToolbar.Insert(rentalsButton, 6);
            return editToolbar;
        }
    }

    /// <summary>
    /// Builds main treeview.
    /// </summary>
    public override TreeView ViewerTree {
        get {
            TreeView viewTree = new TreeView();

            viewTree.HeadersVisible = true;
            viewTree.EnableGridLines = TreeViewGridLines.Both;

            // Construct columns of tree
            viewTree.AppendColumn("Make", new CellRendererText(), "text", STORE_MAKE_P);
            viewTree.AppendColumn("Model", new CellRendererText(), "text", STORE_MODEL_P);
            viewTree.AppendColumn("Year", new CellRendererText(), "text", STORE_YEAR_P);
            viewTree.AppendColumn("Type", new CellRendererText(), "text", STORE_TYPE_P);
            //bikeTree.AppendColumn("Actions", editCell);

            viewTree.CanFocus = true;
            viewTree.Selection.Mode = SelectionMode.Multiple;

            // Set attributes for all columns
            int colCounter = 1;

            foreach (TreeViewColumn column in viewTree.Columns) {
                column.Resizable = true;
                column.Clickable = true;
                column.SortColumnId = colCounter;
                column.Expand = true;

                colCounter++;
            }

            return viewTree;
        }
    }

    /// <summary>
    /// Builds about dialog.
    /// </summary>
    private AboutDialog MainAbout {
        get {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ProgramName = "Cyclone";
            aboutDialog.Version = "0.6";
            aboutDialog.Copyright = "by Sánchez Industries";
            aboutDialog.Comments = @"Inventory manager for bike rental business";
            aboutDialog.Website = "https://github.com/jacobszpz/Cyclone";

            Gdk.Pixbuf aboutWIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Bike_Yellow.png", ICON_W, ICON_H);
            aboutDialog.Icon = aboutWIcon;

            Gdk.Pixbuf aboutIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Cyclone.png", ICON_W, ICON_H);
            aboutDialog.Logo = aboutIcon;
            return aboutDialog;
        }
    }

    public override Statusbar ViewerStatusbar {
        get {
            Statusbar statusbar = new Statusbar();

            ItemAmount = new Label("0 bikes in inventory");
            BalanceLabel = new Label("Balance: $0");
            Separator balanceSeparator = new Separator(Orientation.Vertical);
            balanceSeparator.MarginLeft = (int) DefaultPadding;
            balanceSeparator.MarginRight = (int) DefaultPadding;

            statusbar.PackEnd(BalanceLabel, false, true, 0);
            statusbar.PackStart(ItemAmount, false, true, 0);
            statusbar.PackStart(balanceSeparator, false, false, 0);

            return statusbar;
        }
    }

    protected override int WIN_H {
        get {
            return 500;
        }
    }

    protected override int WIN_W {
        get {
            return 800;
        }
    }

    protected override int ICON_W {
        get {
            return 38;
        }
    }

    protected override int ICON_H {
        get {
            return 42;
        }
    }

    protected override string TitleProperty {
        get {
            return "Bike Business Management - Cyclone";
        }
    }

    protected override string IconFilename {
        get {
            return "Cyclone.Assets.Cyclone.png";
        }
    }

    protected override string FrameTitle {
        get {
            return "Bike Inventory";
        }
    }
}
