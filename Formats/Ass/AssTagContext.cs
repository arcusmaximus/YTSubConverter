using System.Collections.Generic;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssTagContext
    {
        public delegate List<AssLine> PostProcessor();

        public AssDocument Document;
        public AssDialogue Dialogue;
        public AssStyle Style;
        public AssStyleOptions StyleOptions;
        public AssLine Line;
        public AssSection Section;
        public readonly List<PostProcessor> PostProcessors = new List<PostProcessor>();
    }
}
