
using System.Collections.Generic;

namespace Watch.Toolkit.Processing.Recognizers
{
    public class Template
    {
        public string Label { get; set; }
        public double[] Front { get; set; }
        public double[] Left { get; set; }
        public double[] Right { get; set; }
        public double[] Light { get; set; }

        public double[] Vector
        {
            get
            {
                var vec = new List<double>();
          
                vec.AddRange(Left);
                vec.AddRange(Right);
                return vec.ToArray();
            }
        }

        public Template(string label, double[] front, double[] left, double[] right, double[] light)
        {
            Label = label;
            Front = front;
            Left = left;
            Right = right;
            Light = light;
        }
    }


    public static class TemplateData
    {
        public static Template LeftToRight = 
            new Template(
                "SwipeRight",
                new double[]{5,302,359,359,4,4,},
                new double[]{476,476,320,80,45,45,},
                new double[]{47,47,355,355,316,55,},
                new double[]{784,784,675,675,777,777,}
                );

        public static Template RightToLeft = 
            new Template(
                "SwipeLeft",
                new double[]{6,6,5,511,532,532,},
                new double[]{5,104,37,53,413,413,},
                new double[]{304,40,304,516,495,67,},
                new double[]{674,742,737,678,678,6}
                );

        public static Template Cover = 
            new Template(
                "Cover",
                new double[]{9,427,340,340,198,153,},
                new double[]{48,48,6,6,6,162,},
                new double[]{505,440,379,280,280,144,},
                new double[]{698,653,557,557,530,590,}
                );

        public static Template BottomToTop = 
            new Template(
                "SwipeUp",
                new double[]{5,24,432,376,376,376,},
                new double[]{13,60,53,298,298,298,},
                new double[]{337,189,46,35,331,331,},
                new double[]{676,737,778,563,563,615,}
                );

        public static Template TopToBottom = 
            new Template(
                "SwipeDown",
                new double[]{191,191,191,191,419,191,},
                new double[]{52,381,24,24,44,44,},
                new double[]{405,510,450,43,43,54,},
                new double[]{746, 695, 649, 661, 736, 747, }
                );
 
    }
}
