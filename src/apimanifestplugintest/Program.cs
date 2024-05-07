#pragma warning disable IDE0059, SKEXP0001, SKEXP0040, SKEXP0043, SKEXP0060	

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Spectre.Console;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

// Azure OpenAI keys
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL-GPT4"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];


// Create a chat completion service
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

// builder.AddOpenAIChatCompletion(
//     modelId: "phi3",
//     endpoint: new Uri("http://w11-eb20asus-docker-desktop:8080"),
//     apiKey: "apikey");

Kernel kernel = builder.Build();

var plugInName = "sklabs";
var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var plugInFilepath = Path.Combine(currentAssemblyDirectory, "apimanifest.json");

// specify auth callbacks for each API dependency
var apiManifestPluginParameters = new ApiManifestPluginParameters
{
    FunctionExecutionParameters = new()
    {
        { "petssearch", new OpenApiFunctionExecutionParameters(ignoreNonCompliantErrors: true) },
        { "superheroapi", new OpenApiFunctionExecutionParameters(ignoreNonCompliantErrors: true) }
    }
};

// import api manifest plugin
KernelPlugin plugin = await kernel.ImportPluginFromApiManifestAsync
    (plugInName, plugInFilepath, apiManifestPluginParameters)
    .ConfigureAwait(false);

// set goal

// execute plan
var planGoal = @"Find pets in the pets catalog that have super hero names. 
With the results of the search, show the information for each pet including the pet name, pet type, pet breed and pet age, the pet's owner information, and the super hero details that match the pet's name.
Show the result of the pets with super hero names as a indented list in plain text. 
Do not generate HTML or MARKDOWN, just text.";

AnsiConsole.Write(new FigletText("SK - API Manifest Test"));
// show the current prompt in the spectre console
AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
AnsiConsole.MarkupLine("[bold green]CURRENT PROMPT[/]");
Spectre.Console.AnsiConsole.MarkupLine(planGoal);
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

// show current used services urls
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
Spectre.Console.AnsiConsole.MarkupLine("[bold green]CURRENT SERVICES URLS[/]");
Spectre.Console.AnsiConsole.MarkupLine($"Super Hero API: http://localhost:5188/swagger/v1/swagger.yaml");
Spectre.Console.AnsiConsole.MarkupLine($"Pet Store API: http://localhost:5100/swagger/v1/swagger.yaml");
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");


// confirm in the spectre console to run this prompt
var confirm = AnsiConsole.Confirm("Do you want to run this prompt?");
if (!confirm)
{
    AnsiConsole.MarkupLine("[bold red]Prompt execution cancelled[/]");
    return;
}
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

// create planner
var planner = new FunctionCallingStepwisePlanner(
    new FunctionCallingStepwisePlannerOptions
    {
        MaxIterations = 10,
        MaxTokens = 32000
    }
);

var result = await planner.ExecuteAsync(kernel, planGoal);

// display the goal in the spectre console, with a title GOAL
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
Spectre.Console.AnsiConsole.MarkupLine("[bold green]GOAL[/]");
Spectre.Console.AnsiConsole.MarkupLine(planGoal);
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

// display the final answer in the spectre console, with a title FINAL ANSWER
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
Spectre.Console.AnsiConsole.MarkupLine("[bold green]FINAL ANSWER[/]");
Spectre.Console.AnsiConsole.MarkupLine(result.FinalAnswer);
Spectre.Console.AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

DisplayPlan(result);

void DisplayPlan(FunctionCallingStepwisePlannerResult result)
{
    // Create a table
    var table = new Table();
    table.Border(TableBorder.Ascii);

    // Add some columns
    table.AddColumn(new TableColumn("Role").Centered());
    table.AddColumn(new TableColumn("ModelId").Centered());
    table.AddColumn(new TableColumn("Type").Centered());
    table.AddColumn(new TableColumn("Step").Centered());

    // Add some rows
    foreach (var step in result.ChatHistory)
    {
        var row = new List<Spectre.Console.Rendering.Renderable>
        {
            new Markup($"[bold]{step.Role}[/]"),
            new Markup($"[bold]{step.ModelId}[/]")
        };

        var stepType = "";
        var lines = new List<string>();
        // get the last item from step.Items
        var line = step.Items.LastOrDefault();

        if (line is TextContent)
        {
            var currentLine = line as TextContent;
            stepType = "TextContent";

            // if currentLine.Text lenght is longer than 250 characters, get the 1st 250 characters
            if (currentLine.Text.Length > 250)
                lines.Add(currentLine.Text.Substring(0, 250)+ " ...");
            else
                lines.Add(currentLine.Text);
        }
        else if (line is FunctionCallContent)
        {
            var currLine = line as FunctionCallContent;
            stepType = "FunctionCallContent";
            lines.Add($"PlugIn Name: {currLine.PluginName} {Environment.NewLine} Function Name :{currLine.FunctionName}");
        }


        row.Add(new Markup($"[bold]{stepType}[/]"));

        // create a single string containing all the elements in the variable lines
        lines.Add(Environment.NewLine);
        var rowString = string.Join(Environment.NewLine, lines);
        row.Add(new Text(rowString));

        table.AddRow(row);

    }



    AnsiConsole.Write(table);
}