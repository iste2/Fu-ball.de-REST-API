using System.Collections.Concurrent;
using System.Globalization;
using Fußball.de.Rest.Api;
using Fußball.de.Scraping;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache();
builder.Services.AddCors();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

var log = LoggerBuilder.BuildLogger(app.Environment.IsDevelopment());

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(policyBuilder => policyBuilder.AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseOutputCache();

app.MapGet("teams/club/{id}/season/{season}", async (string id, string season) =>
    {
        log.Information($"Calling teams/club/{id}/season/{season}");
        try
        {
            var parser = new TeamsOfAClubScraper(id, season, log);
            var teams = await parser.Scrape();
            log.Information("Finished scraping teams of club {ClubId} in season {Season}\n{Teams}", id, season, JsonConvert.SerializeObject(teams));
            return Results.Json(teams);
        } catch (Exception e)
        {
            log.Error(e, $"Error in /teams/club/{id}/season/{season}");
            return Results.BadRequest(e);
        }
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/games/team/{id}", async ([FromRoute]string id, [FromQuery]string start, [FromQuery]string end) =>
    {
        try
        {
            var parser = new GamesOfTeamScraper(id, start, end, log);
            var games = await parser.Scrape();
            log.Information("Finished scraping games of team {TeamId}\n{Games}", id, JsonConvert.SerializeObject(games));
            return Results.Json(games);
        }
        catch (Exception e)
        {
            log.Error(e, $"Error in /games/team/{id}/start/{start}/end/{end}");
            return Results.BadRequest(e);
        }
    })
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(30)));

app.MapGet("/games/club/{id}", async ([FromRoute]string id, [FromQuery]string start, [FromQuery]string end, [FromQuery]bool homeGamesOnly = false) =>
    {
        try
        {
            var seasons = Utils.Seasons(DateTime.ParseExact(start, "dd.MM.yyyy", CultureInfo.InvariantCulture), DateTime.ParseExact(end, "dd.MM.yyyy", CultureInfo.InvariantCulture));
            var allTeams = new List<Team>();
            var allGames = new ConcurrentBag<Game>();
        
            foreach (var teamsParser in seasons.Select(season => new TeamsOfAClubScraper(id, season, log)))
            {
                var teamsOfSeason = await teamsParser.Scrape();
                allTeams.AddRange(teamsOfSeason);
            }
            var teams = allTeams.DistinctBy(team => team.Id).ToList();

            await Parallel.ForEachAsync(teams.Select(team => new GamesOfTeamScraper(team.Id, start, end, log, homeGamesOnly)), ScrapeGamesOfTeam);
            log.Information("Finished scraping games of all teams\n{Games}", JsonConvert.SerializeObject(allGames));
            return Results.Json(allGames.ToArray());

            async ValueTask ScrapeGamesOfTeam(GamesOfTeamScraper gamesParser, CancellationToken token)
            {
                var games = await gamesParser.Scrape();
                foreach (var game in games) allGames.Add(game);
            }
        }
        catch (Exception e)
        {
            log.Error(e, $"Error in /games/club/{id}/start/{start}/end/{end}");
            return Results.BadRequest(e);
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
            log.Error(e, $"Error in /gamesduration/teamkind/{teamkind}");
            return Results.BadRequest(e);
        }
    });

app.Run();