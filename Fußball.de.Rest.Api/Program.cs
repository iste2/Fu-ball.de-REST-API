using System.Collections.Concurrent;
using System.Globalization;
using Fußball.de.Rest.Api;
using Fußball.de.Scraping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache();
builder.Services.AddCors();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(policyBuilder => policyBuilder.AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseOutputCache();

app.MapGet("teams/club/{id}/season/{season}", async (string id, string season) =>
    {
        try
        {
            var parser = new TeamsOfAClubScraper(id, season);
            var teams = await parser.Scrape();
            return Results.Json(teams);
        } catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/games/team/{id}/start/{start}/end/{end}", async (string id, string start, string end) =>
    {
        try
        {
            var parser = new GamesOfTeamScraper(id, start, end);
            var games = await parser.Scrape();
            return Results.Json(games);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/games/club/{id}/start/{start}/end/{end}", async (string id, string start, string end) =>
    {
        try
        {
            var seasons = Utils.Seasons(DateTime.ParseExact(start, "dd.MM.yyyy", CultureInfo.InvariantCulture), DateTime.ParseExact(end, "dd.MM.yyyy", CultureInfo.InvariantCulture));
            var allTeams = new List<Team>();
            var allGames = new ConcurrentBag<Game>();
        
            foreach (var teamsParser in seasons.Select(season => new TeamsOfAClubScraper(id, season)))
            {
                var teamsOfSeason = await teamsParser.Scrape();
                allTeams.AddRange(teamsOfSeason);
            }
            var teams = allTeams.DistinctBy(team => team.Id).ToList();

            await Parallel.ForEachAsync(teams.Select(team => new GamesOfTeamScraper(team.Id, start, end)), ScrapeGamesOfTeam);
        
            return Results.Json(allGames);

            async ValueTask ScrapeGamesOfTeam(GamesOfTeamScraper gamesParser, CancellationToken token)
            {
                var games = await gamesParser.Scrape();
                foreach (var game in games) allGames.Add(game);
            }
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/gamesduration/teamkind/{teamkind}", (string teamkind) =>
    {
        try
        {
            var duration = teamkind switch
            {
                TeamKinds.Herren => TeamKindGameDurations.Herren,
                TeamKinds.Frauen => TeamKindGameDurations.Frauen,
                TeamKinds.AJunioren => TeamKindGameDurations.AJunioren,
                TeamKinds.BJunioren => TeamKindGameDurations.BJunioren,
                TeamKinds.CJunioren => TeamKindGameDurations.CJunioren,
                TeamKinds.DJunioren => TeamKindGameDurations.DJunioren,
                TeamKinds.EJunioren => TeamKindGameDurations.EJunioren,
                TeamKinds.FJunioren => TeamKindGameDurations.FJunioren,
                TeamKinds.AJuniorinnen => TeamKindGameDurations.AJuniorinnen,
                TeamKinds.BJuniorinnen => TeamKindGameDurations.BJuniorinnen,
                TeamKinds.CJuniorinnen => TeamKindGameDurations.CJuniorinnen,
                TeamKinds.DJuniorinnen => TeamKindGameDurations.DJuniorinnen,
                _ => 0
            };
            return Results.Ok(duration);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    });

app.Run();