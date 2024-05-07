using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://chat.openai.com", "http://localhost:5188").AllowAnyHeader().AllowAnyMethod();
        policy.WithOrigins("http://localhost:3000", "http://localhost:5188").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Super Heroes API",
        Version = "v1",
        Description = "View super heroes information.",
        Contact = new OpenApiContact
        {
            Name = "El Bruno",
            Email = "elbruno@elbruno.com",
            Url = new Uri("https://elbruno.com/")
        }
    });
    // add servers to swagger, this is needed if testing with the Semantic Kernel Console App
    options.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5188"
    });
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();
app.UseCors("AllowAll");

// need to serialize this as V2 to work as ChatGPT API plugin
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Super Heroes v1");
});

// necessary public files for ChatGPT to get plugin logo
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "pluginInfo")),
    RequestPath = "/pluginInfo"
});

// publish the plugin manifest information, update the host with the current one
app.MapGet("/.well-known/ai-plugin.json", (HttpRequest request) =>
{
    Console.WriteLine($"GET PLUGIN MANIFEST");
    // get current url from request headers (codespaces dev) or app Urls (local dev)
    var userAgent = request.Headers.UserAgent;
    var customHeader = request.Headers["x-custom-header"];
    var currentUrl = request.Headers["x-forwarded-proto"] + "://" + request.Headers["x-forwarded-host"];
    if (currentUrl == "://")
        currentUrl = app.Urls.First();

    // update current host in manifest
    string aiPlugInManifest = File.ReadAllText("pluginInfo/ai-plugin.json");
    aiPlugInManifest = aiPlugInManifest.Replace("$host", currentUrl);
    return Results.Text(aiPlugInManifest);
})
.ExcludeFromDescription(); // exclude from swagger description;

// return the list of pets
app.MapGet("/GetAllHeroes", () =>
{
    Console.WriteLine($"{DateTime.Now} - GET ALL HEROES");
    var heroesFile = File.ReadAllText("data/superheroes.json");
    return Results.Json(heroesFile);
})
.WithName("GetAllHeroes")
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Gets the list of all super heroes available in El Bruno's Super Heroes catalog.";
    return generatedOperation;
});

// add a new super hero
app.MapPost("/AddSuperHero", async (Superhero newHero) =>
{
    Console.WriteLine($"{DateTime.Now} - ADDHERO / HERO info: {newHero.SuperHeroFullName}");
    var dataFile = File.ReadAllText("data/superheroes.json");
    var heroes = JsonSerializer.Deserialize<List<Superhero>>(dataFile);
    heroes.Add(newHero);
    dataFile = JsonSerializer.Serialize(heroes);
    File.WriteAllText("data/superheroes.json", dataFile);
    return Results.Ok();
})
.WithName("AddSuperHero")
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Add a new super hero to the catalog";
    return generatedOperation;
});

app.Run();

/// <summary>
/// Superhero class that contains information about a superhero.
/// </summary>
public class Superhero
{
    /// <summary>
    /// Full name of the superhero.
    /// </summary>
    public string SuperHeroFullName { get; set; }

    /// <summary>
    /// Alter egos of the superhero.
    /// </summary>
    public string AlterEgos { get; set; }

    /// <summary>
    /// List of aliases of the superhero.
    /// </summary>
    public List<string> Aliases { get; set; }

    /// <summary>
    /// Place of birth of the superhero.
    /// </summary>
    public string PlaceofBirth { get; set; }

    /// <summary>
    /// First appearance of the superhero.
    /// </summary>
    public string FirstAppearance { get; set; }

    /// <summary>
    /// Publisher of the superhero.
    /// </summary>
    public string Publisher { get; set; }

    /// <summary>
    /// Occupation of the superhero.
    /// </summary>
    public string Occupation { get; set; }

    /// <summary>
    /// Base of operation of the superhero.
    /// </summary>
    public string BaseofOperation { get; set; }
}
