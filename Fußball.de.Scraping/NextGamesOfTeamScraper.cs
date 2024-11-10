using System.Globalization;
using HtmlAgilityPack;

namespace Fußball.de.Scraping;

public class NextGamesOfTeamScraper(string teamId)
{
    private string Url => $"https://www.fussball.de/ajax.team.next.games/-/team-id/{teamId}";

    public async Task<List<Game>> Scrape()
    {
        var gamesScraper = new GamesScraper(Url);
        return await gamesScraper.Scrape();
    }
}