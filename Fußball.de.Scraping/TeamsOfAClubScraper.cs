using HtmlAgilityPack;

namespace Fußball.de.Scraping;

public class TeamsOfAClubScraper(string clubId, string season)
{
    private string ClubId { get; } = clubId;
    private string Season { get; } = season;

    private string Url => $"https://www.fussball.de/ajax.club.teams/-/id/{ClubId}/saison/{Season}";
    private string LogoUrl => $"https://www.fussball.de/export.media/-/action/getLogo/format/12/id/{ClubId}";

    public async Task<List<Team>> Scrape()
    {
        var teams = new List<Team>();
        
        var client = new HttpClient();
        var response = await client.GetStringAsync(Url);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        
        foreach (var h4Node in doc.DocumentNode.SelectNodes("//h4/a"))
        {
            // Den Link-Text und die URL extrahieren
            var name = h4Node.InnerText.Trim();
            var link = h4Node.GetAttributeValue("href", string.Empty);

            // Team-ID aus dem Link extrahieren, wenn im Link enthalten
            var id = ExtractTeamId(link);

            // Neues Team-Objekt erstellen und zur Liste hinzufügen
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(link))
            {
                teams.Add(new Team(id, ClubId, name, link, LogoUrl));
            }
        }

        return teams;
    }

    private static string ExtractTeamId(string link)
    {
        var parts = link.Split(["team-id/"], StringSplitOptions.None);
        if (parts.Length <= 1) return string.Empty;
        var idPart = parts[1].Split('/')[0];
        return idPart;
    }
    
}