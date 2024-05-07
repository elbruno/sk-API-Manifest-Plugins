﻿#pragma warning disable IDE0059, SKEXP0001, SKEXP0040, SKEXP0043, SKEXP0060	

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Spectre.Console;
using System.Reflection;

SpectreConsoleOutput.DisplayTitle(".NET - SK APIs);

// execute plan
var planGoal = @"Find pets in the pets catalog that have super hero names. 
With the results of the search, show the information for each pet including the pet name, pet type, pet breed and pet age, the pet's owner information, and the super hero details that match the pet's name.
Show the result of the pets with super hero names as a indented list in plain text. 
Do not generate HTML or MARKDOWN, just text.";

SpectreConsoleOutput.DisplaySection("CURRENT PROMPT", planGoal);
SpectreConsoleOutput.DisplaySection("CURRENT SERVICES URLS", new string[] {
    $"Super Hero API: http://localhost:5188/swagger/v1/swagger.yaml",
    $"Pet Store API: http://localhost:5100/swagger/v1/swagger.yaml"
});

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

SpectreConsoleOutput.DisplayKernels(kernel);

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



// confirm in the spectre console to run this prompt
var confirm = AnsiConsole.Confirm("Do you want to run this prompt?");
if (!confirm)
{
    AnsiConsole.MarkupLine("[bold red]Prompt execution cancelled[/]");
    return;
}
AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

// create planner
var planner = new FunctionCallingStepwisePlanner(
    new FunctionCallingStepwisePlannerOptions
    {
        MaxIterations = 10,
        MaxTokens = 32000
    }
);

var result = await planner.ExecuteAsync(kernel, planGoal);

SpectreConsoleOutput.DisplaySection("CURRENT PROMPT", planGoal);
SpectreConsoleOutput.DisplaySection("FINAL ANSWER", result.FinalAnswer);

// confirm to see plan details
confirm = AnsiConsole.Confirm("Do you want to see the plan details?");
if (!confirm)
{
    AnsiConsole.MarkupLine("[bold red]Prompt execution cancelled[/]");
    return;
}
AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

SpectreConsoleOutput.DisplayPlan(result);