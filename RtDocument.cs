using System;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Arc.YTSubConverter
{
    internal class RtDocument : SubtitleDocument
    {
        public RtDocument()
        {
        }

        public RtDocument(SubtitleDocument doc)
            : base(doc)
        {
        }

        public override void Save(string filePath)
        {
            XmlWriterSettings settings = new XmlWriterSettings { OmitXmlDeclaration = true };

            using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartElement("window");

                foreach (Line line in Lines)
                {
                    WriteLine(line, writer);
                }

                writer.WriteEndElement();
            }
        }

        private static void WriteLine(Line line, XmlWriter writer)
        {
            writer.WriteStartElement("time");
            writer.WriteAttributeString("begin", FormatTimestamp(line.Start));
            writer.WriteAttributeString("end", FormatTimestamp(line.End));
            writer.WriteEndElement();

            foreach (Section section in line.Sections)
            {
                WriteSection(section, writer);
            }
        }

        private static void WriteSection(Section section, XmlWriter writer)
        {
            if (section.Bold)
                writer.WriteStartElement("b");

            if (section.Italic)
                writer.WriteStartElement("i");

            if (section.Underline)
                writer.WriteStartElement("u");

            if (section.ForeColor != Color.Empty)
            {
                writer.WriteStartElement("font");
                writer.WriteAttributeString("color", $"#{section.ForeColor.R:X02}{section.ForeColor.G:X02}{section.ForeColor.B:X02}");
            }

            WriteText(section.Text, writer);

            if (section.ForeColor != Color.Empty)
                writer.WriteEndElement();

            if (section.Underline)
                writer.WriteEndElement();

            if (section.Italic)
                writer.WriteEndElement();

            if (section.Bold)
                writer.WriteEndElement();

            writer.WriteCharEntity((char)8203);
        }

        private static void WriteText(string text, XmlWriter writer)
        {
            int start = 0;
            while (start < text.Length)
            {
                int end = text.IndexOf("\r\n", start);
                if (end < 0)
                    end = text.Length;

                writer.WriteString(text.Substring(start, end - start));
                if (end < text.Length)
                {
                    writer.WriteStartElement("br");
                    writer.WriteEndElement();
                }

                start = end + 2;
            }
        }

        private static string FormatTimestamp(DateTime timestamp)
        {
            return $"{timestamp.Hour}:{timestamp.Minute:00}:{timestamp.Second:00}.{timestamp.Millisecond:000}";
        }
    }
}
