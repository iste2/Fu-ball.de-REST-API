namespace Fußball.de.Scraping.Tests;

public class TeamsOfAClubScraperTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task TestThatAnyResultIsReturned()
    {
        // Arrange
        var teamId = "00ES8GN92C0000B1VV0AG08LVUPGND5I"; // Kornelimünster
        var season = "2021";
        var scraper = new TeamsOfAClubScraper(teamId, season);
        
        // Act
        var result = await scraper.Scrape();
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task TestThatCorrectNumberOfTeamsIsReturned()
    {
        // Arrange
        var teamId = "00ES8GN92C0000B1VV0AG08LVUPGND5I"; // Kornelimünster
        var season = "2021";
        var scraper = new TeamsOfAClubScraper(teamId, season);
        
        // Act
        var result = await scraper.Scrape();
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(16));
    }
}