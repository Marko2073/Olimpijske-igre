using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OlimpijskeIgre
{
    public static class Loader
    {
        public static Dictionary<string, Group> LoadGroups(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Groups file not found at path: {path}");
            }

            var json = File.ReadAllText(path);
            var groups = JsonConvert.DeserializeObject<Dictionary<string, List<Tim>>>(json);

            
            var result = new Dictionary<string, Group>();
            foreach (var group in groups)
            {
                result[group.Key] = new Group { Teams = group.Value };
            }

            return result;
        }

        public static Dictionary<string, List<ExhibitionMatch>> LoadExhibitions(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Exhibitions file not found at path: {path}");
            }

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<string, List<ExhibitionMatch>>>(json);
        }

        public static void AssignExhibitionMatches(Dictionary<string, Group> groups, Dictionary<string, List<ExhibitionMatch>> exhibitions)
        {
            
            var allTeams = groups.SelectMany(g => g.Value.Teams).ToDictionary(t => t.ISOCode);

            foreach (var exhibitionEntry in exhibitions)
            {
                var teamISOCode = exhibitionEntry.Key;
                if (allTeams.TryGetValue(teamISOCode, out var team))
                {
                    foreach (var exhibition in exhibitionEntry.Value)
                    {
                        if (allTeams.TryGetValue(exhibition.Opponent, out var opponent))
                        {
                           
                            team.ExhibitionMatches.Add(new ExhibitionMatch
                            {
                                Date = exhibition.Date,
                                Opponent = exhibition.Opponent,
                                Result = exhibition.Result
                            });
                        }
                    }
                }
            }
        }
    }
}
