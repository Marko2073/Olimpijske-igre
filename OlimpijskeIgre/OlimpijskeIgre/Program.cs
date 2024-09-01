using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OlimpijskeIgre;

class Program
{
    static void Main(string[] args)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string groupsPath = Path.Combine(basePath, "groups.json");
        string exhibitionsPath = Path.Combine(basePath, "exibitions.json");

        var groups = Loader.LoadGroups(groupsPath);
        var exhibitions = Loader.LoadExhibitions(exhibitionsPath);

        foreach (var groupEntry in groups)
        {
            SimulateGroupMatches(groupEntry.Key, groupEntry.Value);
        }

        var allTeams = GetRankedTeams(groups);
        var topTeams = GetTopTeams(allTeams);

        Console.WriteLine("Šeširi:");
        PrintSeedings(topTeams);

        
        var knockoutStage = CreateKnockoutPairs(topTeams);
        SimulateKnockoutStage(knockoutStage);
    }

    static void SimulateGroupMatches(string groupName, Group group)
    {
        Console.WriteLine($"Grupna faza - I kolo za grupu {groupName}:");

        for (int i = 0; i < group.Teams.Count; i++)
        {
            for (int j = i + 1; j < group.Teams.Count; j++)
            {
                var result = SimulateMatch(group.Teams[i], group.Teams[j]);
                Console.WriteLine($"{group.Teams[i].ISOCode} - {group.Teams[j].ISOCode} ({result.Score})");

                
                UpdateTeamStats(group.Teams[i], group.Teams[j], result);
            }
        }

       
        SortTeamsByRank(group);
        PrintGroupStandings(groupName, group);
    }

    static void UpdateTeamStats(Tim teamA, Tim teamB, MatchResult result)
    {
        if (result.Winner == teamA)
        {
            teamA.Points += 2;
        }
        else
        {
            teamB.Points += 2;
        }

        teamA.ScoredPoints += int.Parse(result.Score.Split(':')[0]);
        teamA.ConcededPoints += int.Parse(result.Score.Split(':')[1]);
        teamB.ScoredPoints += int.Parse(result.Score.Split(':')[1]);
        teamB.ConcededPoints += int.Parse(result.Score.Split(':')[0]);
    }

    static void SortTeamsByRank(Group group)
    {
        group.Teams.Sort((t1, t2) =>
        {
            int comparison = t2.Points.CompareTo(t1.Points);
            if (comparison == 0)
            {
                comparison = (t2.ScoredPoints - t2.ConcededPoints).CompareTo(t1.ScoredPoints - t1.ConcededPoints);
                if (comparison == 0)
                {
                    comparison = t2.ScoredPoints.CompareTo(t1.ScoredPoints);
                }
            }
            return comparison;
        });
    }

    static void PrintGroupStandings(string groupName, Group group)
    {
        Console.WriteLine($"Konačan plasman u grupi {groupName}:");
        for (int i = 0; i < group.Teams.Count; i++)
        {
            var team = group.Teams[i];
            Console.WriteLine($"{i + 1}. {team.ISOCode} {team.Points} / {team.ScoredPoints} / {team.ConcededPoints} / {team.PointDifference}");
        }
    }

    static Dictionary<string, Tim> GetRankedTeams(Dictionary<string, Group> groups)
    {
        var rankedTeams = new Dictionary<string, Tim>();

        foreach (var group in groups)
        {
            foreach (var team in group.Value.Teams)
            {
                rankedTeams.Add($"{group.Key}-{team.ISOCode}", team);
            }
        }

        return rankedTeams;
    }

    static List<Tim> GetTopTeams(Dictionary<string, Tim> rankedTeams)
    {
        var topTeams = rankedTeams.Values
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.PointDifference)
            .ThenByDescending(t => t.ScoredPoints)
            .Take(9)
            .ToList();

        return topTeams;
    }

    static void PrintSeedings(List<Tim> topTeams)
    {
        var pots = new Dictionary<string, List<Tim>>()
        {
            { "D", topTeams.Take(2).ToList() }, 
            { "E", topTeams.Skip(2).Take(2).ToList() },
            { "F", topTeams.Skip(4).Take(2).ToList() },
            { "G", topTeams.Skip(6).Take (2).ToList() }
        };

        Console.WriteLine("Šešir D");
        foreach (var team in pots["D"]) Console.WriteLine($"    {team.ISOCode}");
        Console.WriteLine("Šešir E");
        foreach (var team in pots["E"]) Console.WriteLine($"    {team.ISOCode}");
        Console.WriteLine("Šešir F");
        foreach (var team in pots["F"]) Console.WriteLine($"    {team.ISOCode}");
        Console.WriteLine("Šešir G");
        foreach (var team in pots["G"]) Console.WriteLine($"    {team.ISOCode}");
    }

    static List<MatchResult> CreateKnockoutPairs(List<Tim> topTeams)
    {
        var pots = new Dictionary<string, List<Tim>>()
        {
            { "D", topTeams.Take(2).ToList() },
            { "E", topTeams.Skip(2).Take(2).ToList() },
            { "F", topTeams.Skip(4).Take(2).ToList() },
            { "G", topTeams.Skip(6).Take(2).ToList() }
        };

        var matches = new List<MatchResult>();

        foreach (var d in pots["D"])
        {
            var g = pots["G"].FirstOrDefault(t => t.ISOCode != d.ISOCode && !matches.Any(m => m.Winner.ISOCode == t.ISOCode || m.Loser.ISOCode == t.ISOCode));
            if (g != null)
            {
                matches.Add(new MatchResult { Winner = d, Loser = g, Score = SimulateMatch(d, g).Score });
            }
        }

        foreach (var e in pots["E"])
        {
            var f = pots["F"].FirstOrDefault(t => t.ISOCode != e.ISOCode && !matches.Any(m => m.Winner.ISOCode == t.ISOCode || m.Loser.ISOCode == t.ISOCode));
            if (f != null)
            {
                matches.Add(new MatchResult { Winner = e, Loser = f, Score = SimulateMatch(e, f).Score });
            }
        }

        return matches;
    }

    static void SimulateKnockoutStage(List<MatchResult> matches)
    {
        Console.WriteLine("Četvrtfinale:");
        foreach (var match in matches)
        {
            Console.WriteLine($"{match.Winner.ISOCode} vs {match.Loser.ISOCode} ({match.Score})");
        }

        var semiFinals = SimulateNextRound(matches);
        Console.WriteLine("Polufinale:");
        foreach (var match in semiFinals)
        {
            Console.WriteLine($"{match.Winner.ISOCode} vs {match.Loser.ISOCode} ({match.Score})");
        }

        var final = SimulateNextRound(semiFinals);
        Console.WriteLine("Finale:");
        foreach (var match in final)
        {
            Console.WriteLine($"{match.Winner.ISOCode} vs {match.Loser.ISOCode} ({match.Score})");
        }

        var thirdPlaceMatch = new List<MatchResult> { new MatchResult { Winner = semiFinals[1].Loser, Loser = semiFinals[0].Loser, Score = SimulateMatch(semiFinals[1].Loser, semiFinals[0].Loser).Score } };
        Console.WriteLine("Utakmica za treće mesto:");
        foreach (var match in thirdPlaceMatch)
        {
            Console.WriteLine($"{match.Winner.ISOCode} vs {match.Loser.ISOCode} ({match.Score})");
        }
    }

    static List<MatchResult> SimulateNextRound(List<MatchResult> matches)
    {
        var nextRoundMatches = new List<MatchResult>();

        for (int i = 0; i < matches.Count; i += 2)
        {
            var winnerA = matches[i].Winner;
            var winnerB = matches[i + 1].Winner;

            nextRoundMatches.Add(SimulateMatch(winnerA, winnerB));
        }

        return nextRoundMatches;
    }

    static double CalculateTeamForm(Tim team)
    {
        double totalForm = 0;
        int matchCount = team.ExhibitionMatches.Count;

        foreach (var match in team.ExhibitionMatches)
        {
            var resultParts = match.Result.Split('-');
            if (resultParts.Length == 2)
            {
                int scoredPoints = int.Parse(resultParts[0]);
                int concededPoints = int.Parse(resultParts[1]);
                totalForm += (scoredPoints - concededPoints); 
            }
        }

        
        return matchCount > 0 ? totalForm / matchCount : 0;
    }

    static MatchResult SimulateMatch(Tim teamA, Tim teamB)
    {
        Random rnd = new Random();

        
        int rankingDifference = teamA.FIBARanking - teamB.FIBARanking;

       
        double formA = CalculateTeamForm(teamA);
        double formB = CalculateTeamForm(teamB);

        int scoreA = rnd.Next(80, 100) + rankingDifference / 10 + (int)formA;
        int scoreB = rnd.Next(80, 100) - rankingDifference / 10 + (int)formB;

       
        while (scoreA == scoreB)
        {
            scoreA += rnd.Next(1, 5);
        }

        return scoreA > scoreB
            ? new MatchResult { Winner = teamA, Loser = teamB, Score = $"{scoreA}:{scoreB}" }
            : new MatchResult { Winner = teamB, Loser = teamA, Score = $"{scoreB}:{scoreA}" };
    }






}
