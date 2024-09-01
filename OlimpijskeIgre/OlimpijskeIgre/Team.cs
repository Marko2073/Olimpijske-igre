using System.Collections.Generic;

namespace OlimpijskeIgre
{
    public class Tim
    {
        public string Team { get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public int Points { get; set; } = 0;
        public int ScoredPoints { get; set; } = 0;
        public int ConcededPoints { get; set; } = 0;

        public int PointDifference => ScoredPoints - ConcededPoints;
        public List<ExhibitionMatch> ExhibitionMatches { get; set; } 

        public Tim()
        {
            ExhibitionMatches = new List<ExhibitionMatch>();
        }
    }

    public class ExhibitionMatch
    {
        public string Date { get; set; }
        public string Opponent { get; set; }
        public string Result { get; set; }
    }


    public class Group
    {
        public List<Tim> Teams { get; set; }
    }

    

    public class MatchResult
    {
        public Tim Winner { get; set; }
        public Tim Loser { get; set; }
        public string Score { get; set; }
    }
}
