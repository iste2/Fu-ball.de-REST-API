namespace Fußball.de.Scraping;

public record Team(string Id, string Name, string Link);
public record Game(string Id, DateTime KickOff, string Link, Team HomeSide, Team AwaySide, string League, string Squad, string SquadId, string Address, string DfbnetId);