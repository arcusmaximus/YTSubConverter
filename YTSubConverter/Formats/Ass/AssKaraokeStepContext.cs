using System;
using System.Collections.Generic;

namespace Arc.YTSubConverter.Formats.Ass
{
    public class AssKaraokeStepContext
    {
        public AssDocument Document;
        public AssLine OriginalLine;
        public SortedList<TimeSpan, int> ActiveSectionsPerStep;

        public AssLine StepLine;
        public int StepIndex;
        public int NumActiveSections;
        public List<AssSection> SingingSections;
    }
}
