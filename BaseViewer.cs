using System;
using Gtk;

namespace Cyclone {
    public abstract class BaseViewer : Window {
        protected VBox windowBox = new VBox();
        public abstract MenuBar ViewerMenu { get; }
        public abstract Frame ViewerFrame { get; }
        public abstract TreeView ViewerTree { get; }
        public abstract Statusbar ViewerStatusbar { get; }
        public TreeView BaseTree;
        public Statusbar BaseStatusbar;

        public abstract Toolbar EditingToolbar { get; }
        public Label ItemAmount;

        protected abstract string IconFilename { get; }
        protected abstract string TitleProperty { get; }
        protected abstract string FrameTitle { get; }
        protected abstract int WIN_W { get; }
        protected abstract int WIN_H { get; }

        protected abstract int ICON_W{ get; }
        protected abstract int ICON_H { get; }

        public const uint DefaultPadding = 20;
        public const int TOOL_SIDE = 24;

        protected BaseViewer() : base (WindowType.Toplevel) {
            WindowProperties();
        }

        private void WindowProperties() {
            SetDefaultSize(WIN_W, WIN_H);
            SetSizeRequest(WIN_W, WIN_H);

            //Resizable = false;
            Title = TitleProperty;
            Modal = true;

            // Set window icon
            Gdk.Pixbuf windowIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(),
                     IconFilename, ICON_W, ICON_H);
            Icon = windowIcon;
            
            // Create uppermost container
            windowBox = new VBox(false, 0);
            Add(windowBox);
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

            yearError.Title = "Info - Cyclone";

            ResponseType yearRes = (ResponseType) yearError.Run();
    
            if (yearRes == ResponseType.Close) {
                yearError.Destroy();
            }
        }

        public abstract void ItemCount();
    }
}
