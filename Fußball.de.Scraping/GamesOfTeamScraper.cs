using Serilog.Core;

namespace Fußball.de.Scraping;

public class GamesOfTeamScraper(string teamId, string start, string end, Logger log)
{
    private string TeamId { get; } = teamId;
    
    private string Url => $"https://www.fussball.de/ajax.team.matchplan/-/team-id/{TeamId}/max/100/offset/0/datum-von/{start}/datum-bis/{end}";

    public async Task<List<Game>> Scrape()
    {
        var gamesScraper = new GamesScraper(Url, log);
        return await gamesScraper.Scrape();
    }
}