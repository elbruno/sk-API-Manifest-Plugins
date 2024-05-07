using System.ComponentModel;
using Microsoft.SemanticKernel;

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
        // using spectre console display the email information
        SpectreConsoleOutput.DisplaySection("EMAIL SENT", new string[] {
            $"Recipient Emails: {recipientEmails}",
            $"Subject: {subject}",
            $"Body: {body}"
        });
    }
}