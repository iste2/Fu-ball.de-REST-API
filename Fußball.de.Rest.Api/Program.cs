using Fußball.de.Scraping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("teams/club/{id}/season/{season}", async (string id, string season) =>
{
    var parser = new TeamsOfAClubScraper(id, season);
    var teams = await parser.Scrape();
    return Results.Ok(teams);
});

app.MapGet("/nextgames/team/{id}", async (string id) =>
{
    var parser = new NextGamesOfTeamScraper(id);
    var games = await parser.Scrape();
    return Results.Ok(games);
});

app.Run();