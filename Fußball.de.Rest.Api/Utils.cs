namespace Fußball.de.Rest.Api;

public static class Utils
{
    public static List<string> Seasons(DateTime start, DateTime end)
    {
        var seasons = new List<string>();
        
        var startYear = start.Year;
        var endYear = end.Year;

        if (start.Month < 8) startYear -= 1;
        
        if (end.Month < 8) endYear -= 1;

        for (var year = startYear; year <= endYear; year++)
        {
            var season = $"{year % 100:D2}{(year + 1) % 100:D2}";
            seasons.Add(season);
        }

        return seasons;
    }
}