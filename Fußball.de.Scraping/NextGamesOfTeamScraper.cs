using System.Globalization;
using HtmlAgilityPack;

namespace Fußball.de.Scraping;

public class NextGamesOfTeamScraper(string teamId)
{
    private string TeamId { get; } = teamId;
    
    private string Url => $"https://www.fussball.de/ajax.team.next.games/-/team-id/{TeamId}";

    public async Task<List<Game>> Scrape()
    {
        var games = new List<Game>();
        
        var client = new HttpClient();
        var response = await client.GetStringAsync(Url);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        
        var rows = doc.DocumentNode.SelectNodes("//tr");
        
        DateTime? currentKickOff = null;
        var currentLeague = "";
        
        foreach (var row in rows)
        {
            if (row.HasClass("row-competition"))
            {
                var dateText = row.SelectSingleNode(".//td[@class='column-date']").InnerText.Replace("|&nbsp;", "").Split(",")[1].Trim();
                currentKickOff = ParseDateTime(dateText);
                currentLeague = row.SelectSingleNode(".//td[@colspan='3']/a").InnerText.Trim();
            }
            else
            {
                var homeTeamNode = row.SelectSingleNode(".//td[@class='column-club'][1]/a");
                var awayTeamNode = row.SelectSingleNode(".//td[@class='column-club no-border']/a");
                var gameLinkNode = row.SelectSingleNode(".//td[@class='column-score']/a");

                if (homeTeamNode == null || awayTeamNode == null || gameLinkNode == null || !currentKickOff.HasValue) continue;
                
                var homeSide = new Team(
                    ExtractTeamId(homeTeamNode.GetAttributeValue("href", "")),
                    homeTeamNode.SelectSingleNode(".//div[@class='club-name']").InnerText.Trim(),
                    homeTeamNode.GetAttributeValue("href", "")
                );

                var awaySide = new Team(
                    ExtractTeamId(awayTeamNode.GetAttributeValue("href", "")),
                    awayTeamNode.SelectSingleNode(".//div[@class='club-name']").InnerText.Trim(),
                    awayTeamNode.GetAttributeValue("href", "")
                );

                var responseFromGameLink = await client.GetStringAsync(gameLinkNode.GetAttributeValue("href", ""));
                var gameDoc = new HtmlDocument();
                gameDoc.LoadHtml(responseFromGameLink);
                
                var staffelNameNode = gameDoc.DocumentNode.SelectSingleNode("//a[@class='competition']");
                var squad = staffelNameNode?.InnerText.Trim() ?? "";

                var staffelIdNode = gameDoc.DocumentNode.SelectSingleNode("//li[span[contains(text(),'Staffel-ID')]]/span[2]");
                var squadId = staffelIdNode?.InnerText.Trim() ?? "";

                var spielAdresseNode = gameDoc.DocumentNode.SelectSingleNode("//a[@class='location']");
                var address = spielAdresseNode?.InnerText.Trim() ?? "";

                var spielIdNode = gameDoc.DocumentNode.SelectSingleNode("//li[span[contains(text(),'Spiel:')]]/span[2]");
                var dfbnetId = spielIdNode?.InnerText.Trim().Split('/')[0].Trim() ?? "";

                var game = new Game(
                    ExtractGameId(gameLinkNode.GetAttributeValue("href", "")),
                    currentKickOff.Value,
                    gameLinkNode.GetAttributeValue("href", ""),
                    homeSide,
                    awaySide,
                    currentLeague,
                    squad,
                    squadId,
                    address,
                    dfbnetId
                );

                games.Add(game);
            }
        }
        
        return games;
    }
    
    private static string ExtractTeamId(string link)
    {
        var parts = link.Split(["team-id/"], StringSplitOptions.None);
        if (parts.Length > 1)
        {
            return parts[1].Split('/')[0];
        }
        return string.Empty;
    }

    private static string ExtractGameId(string link)
    {
        var parts = link.Split(["spiel/"], StringSplitOptions.None);
        return parts.Length > 2 ? parts[2] : string.Empty;
    }

    private static DateTime? ParseDateTime(string dateText)
    {
        const string dateFormat = "dd.MM.yy HH:mm";
        try
        {
            return DateTime.ParseExact(dateText, dateFormat, null, DateTimeStyles.None);
        }
        catch
        {
            return null;
        }
    }
}