using System.ComponentModel;
using Microsoft.SemanticKernel;
using Spectre.Console;

public class EmailPlugin
{
    [KernelFunction, Description("Sends an email to a recipient.")]
    public async Task SendEmailAsync(
        Kernel kernel,
        [Description("Semicolon delimitated list of emails of the recipients")] string recipientEmails,
        string subject,
        string body
    )
    {

        AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");
        AnsiConsole.MarkupLine($"[bold green]Send Emails[/]");
        AnsiConsole.MarkupLine($"");
        AnsiConsole.MarkupLine($"[bold green]Recipients:[/] {recipientEmails}");
        AnsiConsole.MarkupLine($"[bold green]Subject:[/] {subject}");
        AnsiConsole.MarkupLine($"[bold green]Body:[/]");
        AnsiConsole.MarkupLine($"{body}");
        AnsiConsole.MarkupLine("[bold green]--------------------------------------------------[/]");

    }
}