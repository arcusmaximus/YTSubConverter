using System;
using AppKit;
using Foundation;

namespace Arc.YTSubConverter.UI.Mac
{
    [Register("View")]
    public class View : NSView
    {
        public View(IntPtr handle)
            : base(handle)
        {
        }

        internal ViewController Controller
        {
            get;
            set;
        }

        public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
        {
            return Controller.GetDragOperation(sender);
        }

        public override NSDragOperation DraggingUpdated(NSDraggingInfo sender)
        {
            return Controller.GetDragOperation(sender);
        }

        public override bool PrepareForDragOperation(NSDraggingInfo sender)
        {
            return Controller.GetDragOperation(sender) != NSDragOperation.None;
        }

        public override bool PerformDragOperation(NSDraggingInfo sender)
        {
            return Controller.PerformDrag(sender);
        }
    }
}
