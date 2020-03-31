using System;
using System.Collections.Generic;
using Gtk;
using Cyclone;

/// <summary>
/// Main window for Cyclone app.
/// </summary>
public partial class MainWindow : Window {
    private VBox windowBox;
    public MainMenu mainMenu;
    public MainTools bikeToolbar;
    public TreeView bikeTree;
    public Statusbar mainStatus;
    public Label bikeAmount;
    public Label storeBalance;

    public const uint defPadding = 20;
    public const int WIN_W = 800;
    public const int WIN_H = 500;
    public const int ICON_SIDE = 40;

    public const int ICON_H = 42;
    public const int ICON_W = 38;

    public const int STORE_MAKE_P = 1;
    public const int STORE_MODEL_P = 2;
    public const int STORE_TYPE_P = 4;
    public const int STORE_YEAR_P = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:MainWindow"/> class.
    /// </summary>
    public MainWindow() : base(WindowType.Toplevel) {
        WindowProperties();

        // Create menu bar
        mainMenu = new MainMenu();
        AddAccelGroup(mainMenu.fileAccel);
        mainMenu.aboutItem.Activated += AboutDialog;

        // Create toolbar and add it to window
        bikeToolbar = new MainTools();

        Separator toolSeparator = new Separator(Orientation.Horizontal);

        mainStatus = MainStatusbar;

        // Create frame, set border
        Frame mainFrame = new Frame("Bike Inventory");
        // mainFrame.BorderWidth = defPadding;
        mainFrame.MarginTop = (int) defPadding / 2;
        mainFrame.MarginLeft = mainFrame.MarginRight = (int) defPadding;

        // Create scroller inside frame
        ScrolledWindow scrollWindow = new ScrolledWindow();
        mainFrame.Add(scrollWindow);

        // Tree View
        bikeTree = new MainTree();
        scrollWindow.Add(bikeTree);

        // Add all elements to window vertical container
        windowBox.PackStart(mainMenu, false, false, 0);
        windowBox.PackStart(bikeToolbar, false, false, 0);
        windowBox.PackStart(toolSeparator, false, false, 0);

        windowBox.PackEnd(mainStatus, false, false, 0);
        windowBox.PackEnd(mainFrame, true, true, 0);
    }

    /// <summary>
    /// Sets the window properties, size and icon.
    /// </summary>
    private void WindowProperties() {
        // Set size
        SetDefaultSize(WIN_W, WIN_H);
        SetSizeRequest(WIN_W, WIN_H);

        // Set window icon
        Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                 "Cyclone.Assets.Cyclone.png", ICON_W, ICON_H);
        Icon = windowIcon;

        // Create uppermost container
        windowBox = new VBox(false, 0);
        Add(windowBox);
    }

    /// <summary>
    /// Handles the delete event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="a">The alpha component.</param>
    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }

    /// <summary>
    /// Shows the About dialog.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    void AboutDialog(object sender, EventArgs args) {
        AboutDialog about = new MainAbout();
        about.Run();
        about.Destroy();
    }

    /// <summary>
    /// Updates the bike count.
    /// </summary>
    public void BikeCount() {
        BikeAmount = Bike.BikeCount;
    }

    public int BikeAmount {
        set {
            string pluralNoun = "s";

            if (value == 1) {
                pluralNoun = "";
            }

            bikeAmount.Text = string.Format("{0} model{1} in inventory", value, pluralNoun);
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

        ResponseType yearRes = (ResponseType)yearError.Run();

        if (yearRes == ResponseType.Close) {
            yearError.Destroy();
        }
    }

    /// <summary>
    /// Class for the menu bar.
    /// </summary>
    public class MainMenu : MenuBar {
        public ImageMenuItem importItem;
        public ImageMenuItem exportItem;
        public ImageMenuItem saveItem;
        public MenuItem modelsItem;
        public MenuItem clearItem;
        public CheckMenuItem changeView;
        public MenuItem aboutItem;

        public AccelGroup fileAccel;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainWindow.MainMenu"/> class.
        /// </summary>
        public MainMenu() {
            Append(FileMenu);
            Append(EditMenu);
            Append(ViewMenu);
            Append(HelpMenu);
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
    }

    /// <summary>
    /// Builds main toolbar.
    /// </summary>
    public class MainTools : Toolbar {
        public ToolButton addBikeButton;
        public ToolButton removeBikeButton;
        public ToolButton buyBikeButton;
        public ToolButton sellBikeButton;

        public MainTools() {
            Gdk.Pixbuf addBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.AddBike.png", ICON_SIDE, ICON_SIDE);
            Image addBikeImg = new Image(addBikeIcon);

            Gdk.Pixbuf removeBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Close.png", ICON_SIDE, ICON_SIDE);
            Image removeBikeImg = new Image(removeBikeIcon);

            Gdk.Pixbuf buyBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Cart.png", ICON_SIDE, ICON_SIDE);
            Image buyBikeImg = new Image(buyBikeIcon);

            Gdk.Pixbuf sellBikeIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.SellBike.png", ICON_SIDE, ICON_SIDE);
            Image sellBikeImg = new Image(sellBikeIcon);

            addBikeButton = new ToolButton(addBikeImg, "Add Bike");
            removeBikeButton = new ToolButton(removeBikeImg, "Remove Bike");
            buyBikeButton = new ToolButton(buyBikeImg, "Buy Bike");
            sellBikeButton = new ToolButton(sellBikeImg, "Sell Bike");

            SeparatorToolItem commerceActionsSeparator = new SeparatorToolItem();


            Insert(addBikeButton, 0);
            Insert(removeBikeButton, 1);
            Insert(commerceActionsSeparator, 2);
            Insert(buyBikeButton, 3);
            Insert(sellBikeButton, 4);
        }
    }

    /// <summary>
    /// Builds main treeview.
    /// </summary>
    public class MainTree : TreeView {
        public MainTree() {
            HeadersVisible = true;
            EnableGridLines = TreeViewGridLines.Both;

            // Toggle selection cell renderer
            CellRendererToggle selectBikes = new CellRendererToggle();
            selectBikes.Activatable = true;

            // Construct columns of tree
            AppendColumn("Make", new CellRendererText(), "text", STORE_MAKE_P);
            AppendColumn("Model", new CellRendererText(), "text", STORE_MODEL_P);
            AppendColumn("Year", new CellRendererText(), "text", STORE_YEAR_P);
            AppendColumn("Type", new CellRendererText(), "text", STORE_TYPE_P);
            //bikeTree.AppendColumn("Actions", editCell);

            CanFocus = true;
            Selection.Mode = SelectionMode.Multiple;

            // Set attributes for all columns
            int colCounter = 1;

            foreach (TreeViewColumn column in Columns) {
                column.Resizable = true;
                column.Clickable = true;
                column.SortColumnId = colCounter;
                column.Expand = true;

                colCounter++;
            }
        }
    }

    /// <summary>
    /// Builds about dialog.
    /// </summary>
    private class MainAbout : AboutDialog {
        public MainAbout() {
            ProgramName = "Cyclone";
            Version = "0.3";
            Copyright = "by Sánchez Industries";
            Comments = @"Inventory manager for bike rental business";
            Website = "https://github.com/jacobszpz/Cyclone";

            Gdk.Pixbuf aboutWIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Bike_Yellow.png", ICON_SIDE, ICON_SIDE);
            Icon = aboutWIcon;

            Gdk.Pixbuf aboutIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                "Cyclone.Assets.Cyclone.png", ICON_W, ICON_H);
            Logo = aboutIcon;
        }
    }

    private Statusbar MainStatusbar {
        get {
            Statusbar statusbar = new Statusbar();

            bikeAmount = new Label("0 bikes in inventory");
            storeBalance = new Label("Balance: $402.09");
            Separator balanceSeparator = new Separator(Orientation.Vertical);
            balanceSeparator.MarginLeft = (int) defPadding;
            balanceSeparator.MarginRight = (int) defPadding;

            statusbar.PackEnd(storeBalance, false, true, 0);
            statusbar.PackStart(bikeAmount, false, true, 0);
            statusbar.PackStart(balanceSeparator, false, false, 0);

            return statusbar;
        }
    }
}
