using System.Globalization;
using HtmlAgilityPack;

namespace Fußball.de.Scraping;

public class GamesScraper(string url)
{
    public async Task<List<Game>> Scrape()
    {
        var games = new List<Game>();
        
        var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        
        var rows = doc.DocumentNode.SelectNodes("//tr");
        if(rows == null) return games;
        
        DateTime? currentKickOff = null;
        var currentLeague = "";
        
        foreach (var row in rows)
        {
            if (row.HasClass("row-competition"))
            {
                var dateEntries = row.SelectSingleNode(".//td[@class='column-date']").InnerText.Replace("|&nbsp;", "").Split(",");
                var dateText = dateEntries.Length > 1 ? dateEntries[1].Trim() : "";
                currentKickOff = string.IsNullOrEmpty(dateText) ? null : ParseDateTime(dateText);
                currentLeague = row.SelectSingleNode(".//td[@colspan='3']/a").InnerText.Trim();
            }
            else
            {
                var homeTeamNode = row.SelectSingleNode(".//td[@class='column-club'][1]/a");
                var awayTeamNode = row.SelectSingleNode(".//td[@class='column-club no-border']/a");
                var gameLinkNode = row.SelectSingleNode(".//td[@class='column-score']/a");

                if (homeTeamNode == null || awayTeamNode == null || gameLinkNode == null || !currentKickOff.HasValue) continue;
                
                var homeClubId = await GetClubIdFromLink(homeTeamNode.GetAttributeValue("href", ""));
                var homeSideKind = await GetHomeSideKind(homeTeamNode.GetAttributeValue("href", ""));
                
                var homeSide = new Team(
                    ExtractTeamId(homeTeamNode.GetAttributeValue("href", "")),
                    homeClubId,
                    homeTeamNode.SelectSingleNode(".//div[@class='club-name']").InnerText.Trim(),
                    homeTeamNode.GetAttributeValue("href", ""),
                    $"https://www.fussball.de/export.media/-/action/getLogo/format/12/id/{homeClubId}",
                    homeSideKind
                    
                );

                var awayClubId = await GetClubIdFromLink(awayTeamNode.GetAttributeValue("href", ""));
                var awaySideKind = await GetHomeSideKind(awayTeamNode.GetAttributeValue("href", ""));
                
                var awaySide = new Team(
                    ExtractTeamId(awayTeamNode.GetAttributeValue("href", "")),
                    awayClubId,
                    awayTeamNode.SelectSingleNode(".//div[@class='club-name']").InnerText.Trim(),
                    awayTeamNode.GetAttributeValue("href", ""),
                    $"https://www.fussball.de/export.media/-/action/getLogo/format/12/id/{awayClubId}",
                    awaySideKind
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
                
                var goalsHalfNode = gameDoc.DocumentNode.SelectSingleNode("//span[@class='half-result']");
                var goalsHalfText = goalsHalfNode?.InnerText.Trim();
                var goalsHalf = goalsHalfText?.Replace("[", "").Replace("]", "").Split(':') ?? ["-", "-"];
                var goalsHomeHalf = goalsHalf[0].Trim();
                var goalsAwayHalf = goalsHalf[1].Trim();
                
                var goalsHomeNodes = gameDoc.DocumentNode.SelectNodes("//div[contains(@class, 'event-left') and .//span[contains(@class, 'colon')]]\n");
                var goalsHome = goalsHomeNodes?.Count.ToString() ?? (goalsHomeHalf == "-" ? "-" : "0");
                var goalsAwayNodes = gameDoc.DocumentNode.SelectNodes("//div[contains(@class, 'event-right') and .//span[contains(@class, 'colon')]]\n");
                var goalsAway = goalsAwayNodes?.Count.ToString() ?? (goalsAwayHalf == "-" ? "-" : "0");
                
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
                    dfbnetId,
                    goalsHome,
                    goalsAway,
                    goalsHomeHalf,
                    goalsAwayHalf
                );

                games.Add(game);
            }
        }
        
        return games;
    }

    private static async Task<string> GetHomeSideKind(string link)
    {
        if (string.IsNullOrEmpty(link) || link == "#") return "";
        var client = new HttpClient();
        var response = await client.GetStringAsync(link);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        
        var kind = doc.DocumentNode.SelectSingleNode("//p[@class='subline']").InnerText.Split("\n\t\t\t\t&#124;\n\t\t\t\t")[0].Trim();
        return kind;
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
        return DateTime.TryParseExact(dateText, dateFormat, null, DateTimeStyles.None, out var date) ? date : null;
    }
    
    private static async Task<string> GetClubIdFromLink(string link)
    {
        if (string.IsNullOrEmpty(link) || link == "#") return "";
        var client = new HttpClient();
        var response = await client.GetStringAsync(link);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        
        var clubLink = doc.DocumentNode.SelectNodes("//p[@class='subline']//a")[1].GetAttributeValue("href", "");
        var clubId = clubLink?.Split(["id/"], StringSplitOptions.None)[1] ?? "";
        return clubId;
    }
}