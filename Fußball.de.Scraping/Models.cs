namespace Fußball.de.Scraping;

public record Team(
    string Id,
    string ClubId,
    string Name,
    string Link,
    string LogoUrl);
public record Game(
    string Id, 
    DateTime KickOff, 
    string Link, 
    Team HomeSide, 
    Team AwaySide, 
    string League, 
    string Squad, 
    string SquadId, 
    string Address, 
    string DfbnetId,
    string GoalsHome,
    string GoalsAway,
    string GoalsHomeHalf,
    string GoalsAwayHalf);