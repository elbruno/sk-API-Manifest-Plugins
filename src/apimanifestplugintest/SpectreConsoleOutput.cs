#pragma warning disable SKEXP0001, SKEXP0003, SKEXP0010, SKEXP0011, SKEXP0050, SKEXP0052
#pragma warning disable IDE0059, SKEXP0001, SKEXP0040, SKEXP0043, SKEXP0060	

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.TextGeneration;
using Spectre.Console;
using Spectre.Console.Rendering;

public static class SpectreConsoleOutput
{
    public static void DisplayTitle(string title)
    {
        AnsiConsole.Write(new FigletText(title).Centered().Color(Color.Purple));
    }

    public static void DisplayTitleH2(string subtitle)
    {
        AnsiConsole.MarkupLine($"[bold][blue]=== {subtitle} ===[/][/]");
        AnsiConsole.MarkupLine($"");
    }

    public static void DisplayTitleH3(string subtitle)
    {
        AnsiConsole.MarkupLine($"[bold]>> {subtitle}[/]");
        AnsiConsole.MarkupLine($"");
    }

    public static void DisplaySection(string sectionTitle, string sectionContent)
    {
        DisplaySection(sectionTitle, new string[] { sectionContent });
    }
    public static void DisplaySection(string sectionTitle, string[] sectionContent)
    {
        AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
        AnsiConsole.MarkupLine($"[bold green]{sectionTitle}[/]");
        // add a line for each section content
        foreach (var line in sectionContent)
        {
            AnsiConsole.MarkupLine($"{line}");
        }
        AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
    }
    
    public static int AskForNumber(string question)
    {
        var number = AnsiConsole.Ask<int>(@$"[green]{question}[/]");
        return number;
    }

    public static string AskForString(string question)
    {
        var response = AnsiConsole.Ask<string>(@$"[green]{question}[/]");
        return response;
    }

    public static void DisplayKernels(Kernel mainKernel)
    {
        // Create a table
        var table = new Table();

        // Add columns
        table.AddColumn("kernel name");
        table.AddColumn("service");
        table.AddColumn("Key - Value");

        DisplayKernelInfo(mainKernel, "Main Kernel", table);

        // Render the table to the console
        AnsiConsole.Write(table);
    }

    public static void DisplayKernelInfo(Kernel kernel, string kernelName, Table table)
    {
        foreach (var service in kernel.GetAllServices<IChatCompletionService>().ToList())
        {
            AddRow(table, kernelName, "IChatCompletionService", service.Attributes);
        }

        foreach (var service in kernel.GetAllServices<ITextEmbeddingGenerationService>().ToList())
        {
            AddRow(table, kernelName, "ITextEmbeddingGenerationService", service.Attributes);
        }

        foreach (var service in kernel.GetAllServices<ITextGenerationService>().ToList())
        {
            AddRow(table, kernelName, "ITextGenerationService", service.Attributes);
        }
    }

    private static void AddRow(Table table, string kernelName, string serviceName, IReadOnlyDictionary<string, object?> services)
    {
        foreach (var atr in services)
        {
            List<Renderable> row = [new Markup($"[bold]< {kernelName} >[/]"), new Text(serviceName), new Text($"{atr.Key} - {atr.Value}")];
            table.AddRow(row.ToArray());
        }
    }

    public static void DisplayPlan(FunctionCallingStepwisePlannerResult result)
    {
        // Create a table
        var table = new Table();
        table.Border(TableBorder.Ascii);

        // Add some columns
        table.AddColumn(new TableColumn("Role").Centered());
        table.AddColumn(new TableColumn("ModelId").Centered());
        table.AddColumn(new TableColumn("Type").Centered());
        table.AddColumn(new TableColumn("Step").LeftAligned());

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
                    lines.Add(currentLine.Text.Substring(0, 250) + " ...");
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
}
