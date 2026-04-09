using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcCollege.Domain
{
    public static class GradeCalculator
    {
        public static string Calculate(int score, int maxScore)
        {
            if (maxScore <= 0) return "N/A";
            var ratio = (double)score / maxScore;
            return ratio switch
            {
                >= 0.7 => "A",
                >= 0.6 => "B",
                >= 0.5 => "C",
                >= 0.4 => "D",
                _ => "E"
            };
        }
    }
}
