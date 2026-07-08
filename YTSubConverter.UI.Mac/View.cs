using System;
using AppKit;
using Foundation;

namespace YTSubConverter.UI.Mac
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

        public override NSDragOperation DraggingEntered(INSDraggingInfo sender)
        {
            return Controller.GetDragOperation(sender);
        }

        public override NSDragOperation DraggingUpdated(INSDraggingInfo sender)
        {
            return Controller.GetDragOperation(sender);
        }

        public override bool PrepareForDragOperation(INSDraggingInfo sender)
        {
            return Controller.GetDragOperation(sender) != NSDragOperation.None;
        }

        public override bool PerformDragOperation(INSDraggingInfo sender)
        {
            return Controller.PerformDrag(sender);
        }
    }
}
