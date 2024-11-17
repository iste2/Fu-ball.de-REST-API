using Serilog.Core;

namespace Fußball.de.Scraping;

public class NextGamesOfTeamScraper(string teamId, Logger log)
{
    private string Url => $"https://www.fussball.de/ajax.team.next.games/-/team-id/{teamId}";

    public async Task<List<Game>> Scrape()
    {
        var gamesScraper = new GamesScraper(Url, log);
        return await gamesScraper.Scrape();
    }
}