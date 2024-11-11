using System.Globalization;
using Fußball.de.Rest.Api;
using Fußball.de.Scraping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseOutputCache();

app.MapGet("teams/club/{id}/season/{season}", async (string id, string season) =>
    {
        var parser = new TeamsOfAClubScraper(id, season);
        var teams = await parser.Scrape();
        return Results.Ok(teams);
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/games/team/{id}/start/{start}/end/{end}", async (string id, string start, string end) =>
    {
        var parser = new GamesOfTeamScraper(id, start, end);
        var games = await parser.Scrape();
        return Results.Ok(games);
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/games/club/{id}/start/{start}/end/{end}", async (string id, string start, string end) =>
    {
        var seasons = Utils.Seasons(DateTime.ParseExact(start, "dd.MM.yyyy", CultureInfo.InvariantCulture), DateTime.ParseExact(end, "dd.MM.yyyy", CultureInfo.InvariantCulture));
        var allTeams = new List<Team>();
        var allGames = new List<Game>();
        
        foreach (var teamsParser in seasons.Select(season => new TeamsOfAClubScraper(id, season)))
        {
            var teamsOfSeason = await teamsParser.Scrape();
            allTeams.AddRange(teamsOfSeason);
        }
        var teams = allTeams.DistinctBy(team => team.Id).ToList();
        
        
        foreach (var gamesParser in teams.Select(team => new GamesOfTeamScraper(team.Id, start, end)))
        {
            var games = await gamesParser.Scrape();
            allGames.AddRange(games);
        }
        
        return Results.Ok(allGames);
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.Run();