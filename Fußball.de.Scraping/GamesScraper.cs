using System.Globalization;
using HtmlAgilityPack;
using Serilog.Core;

namespace Fußball.de.Scraping;

public class GamesScraper(string url, Logger log, string teamId, bool onlyHomeGames)
{
    private Dictionary<string, string> GetClubIdFromLinkCache { get; } = new();
    private Dictionary<string, string> GetKindCache { get; } = new();
    private Dictionary<string, string> ExtractTeamIdCache { get; } = new();
    private Dictionary<string, string> ExtractGameIdCache { get; } = new();
    
    public async Task<List<Game>> Scrape()
    {
        var games = new List<Game>();
        
        var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        
        var rows = doc.DocumentNode.SelectNodes("//tr");
        if (rows == null)
        {
            log.Warning("No games found in {Url}", url);
            return games;
        }
        
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
                if(onlyHomeGames && ExtractTeamId(homeTeamNode.GetAttributeValue("href", "")) != teamId) continue;
                
                var homeTeamLink = homeTeamNode.GetAttributeValue("href", "");
                var (homeClubId, homeSideKind) = await GetTeamValuesFromLink(homeTeamLink);
                
                var homeSide = new Team(
                    ExtractTeamId(homeTeamNode.GetAttributeValue("href", "")),
                    homeClubId,
                    homeTeamNode.SelectSingleNode(".//div[@class='club-name']").InnerText.Trim(),
                    homeTeamLink,
                    $"https://www.fussball.de/export.media/-/action/getLogo/format/12/id/{homeClubId}",
                    homeSideKind
                    
                );

                var awayTeamLink = awayTeamNode.GetAttributeValue("href", "");
                var (awayClubId, awaySideKind) = await GetTeamValuesFromLink(awayTeamLink);
                
                var awaySide = new Team(
                    ExtractTeamId(awayTeamNode.GetAttributeValue("href", "")),
                    awayClubId,
                    awayTeamNode.SelectSingleNode(".//div[@class='club-name']").InnerText.Trim(),
                    awayTeamLink,
                    $"https://www.fussball.de/export.media/-/action/getLogo/format/12/id/{awayClubId}",
                    awaySideKind
                );

                var gameLink = gameLinkNode.GetAttributeValue("href", "");
                var responseFromGameLink = await client.GetStringAsync(gameLink);
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
                    ExtractGameId(gameLink),
                    currentKickOff.Value,
                    gameLink,
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
                log.Information("Game {GameId} added", game.Id);
            }
        }
        
        return games;
    }

    private string ExtractTeamId(string link)
    {
        if (ExtractTeamIdCache.TryGetValue(link, out var value)) return value;
        var parts = link.Split(["team-id/"], StringSplitOptions.None);
        var linkTeamId = parts.Length > 1 ? parts[1].Split('/')[0] : string.Empty;
        ExtractTeamIdCache[link] = linkTeamId;
        return linkTeamId;
    }

    private string ExtractGameId(string link)
    {
        if (ExtractGameIdCache.TryGetValue(link, out var value)) return value;
        var parts = link.Split(["spiel/"], StringSplitOptions.None);
        var gameId = parts.Length > 2 ? parts[2] : string.Empty;
        ExtractGameIdCache[link] = gameId;
        return gameId;
    }

    private static DateTime? ParseDateTime(string dateText)
    {
        const string dateFormat = "dd.MM.yy HH:mm";
        return DateTime.TryParseExact(dateText, dateFormat, null, DateTimeStyles.None, out var date) ? date : null;
    }
    
    private async Task<(string, string)> GetTeamValuesFromLink(string link)
    {
        var clubId = "";
        var kind = "";
        if (GetClubIdFromLinkCache.TryGetValue(link, out var clubIdValue))
        {
            clubId = clubIdValue;
        }
        if(GetKindCache.TryGetValue(link, out var kindValue))
        {
            kind = kindValue;
        }
        if (!string.IsNullOrEmpty(clubId) && !string.IsNullOrEmpty(kind)) return (clubId, kind);
        
        if (string.IsNullOrEmpty(link) || link == "#") return ("", "");
        var client = new HttpClient();
        var response = await client.GetStringAsync(link);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        if (string.IsNullOrEmpty(clubId))
        {
            var clubLink = doc.DocumentNode.SelectNodes("//p[@class='subline']//a")[1].GetAttributeValue("href", "");
            clubId = clubLink?.Split(["id/"], StringSplitOptions.None)[1] ?? "";
            GetClubIdFromLinkCache[link] = clubId;
        }

        if (string.IsNullOrEmpty(kind))
        {
            kind = doc.DocumentNode.SelectSingleNode("//p[@class='subline']").InnerText.Split("\n\t\t\t\t&#124;\n\t\t\t\t")[0].Trim();
            GetKindCache[link] = kind;
        }
        
        return (clubId, kind);
    }
}