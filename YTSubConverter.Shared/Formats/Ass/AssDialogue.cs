using System;

namespace YTSubConverter.Shared.Formats.Ass
{
    internal class AssDialogue
    {
        public AssDialogue(AssDocumentItem item)
        {
            Layer = item.GetInt("Layer");
            Start = item.GetTimestamp("Start");
            End = item.GetTimestamp("End");
            Style = item.GetString("Style");
            Effect = item.GetString("Effect");
            Text = item.GetString("Text");
        }

        public int Layer
        {
            get;
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

        public string Effect
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
