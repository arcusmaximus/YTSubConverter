using System.Drawing;

namespace Arc.YTSubConverter
{
    internal class Section
    {
        public Section(string text)
        {
            Text = text;
        }

        public string Text
        {
            get;
            set;
        }

        public Color Color
        {
            get;
            set;
        }

        public bool Bold
        {
            get;
            set;
        }

        public bool Italic
        {
            get;
            set;
        }

        public bool Underline
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
