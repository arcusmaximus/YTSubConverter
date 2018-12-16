using System;

namespace Arc.YTSubConverter.Ass
{
    internal class AssDialogue
    {
        public AssDialogue(AssItem item)
        {
            Start = item.GetTimestamp("Start");
            End = item.GetTimestamp("End");
            Style = item.GetString("Style");
            Text = item.GetString("Text");
        }

        public DateTime Start
        {
            get;
        }

        public DateTime End
        {
            get;
        }

        public string Style
        {
            get;
        }

        public string Text
        {
            get;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
