# Email Builder + Progress Tracking Service (C# / ASP.NET)

A clean, reusable email sending service built with **MailKit +
MimeKit**, designed using the **Builder Pattern**, and extended with a
**general-purpose progress tracking system**.

A modular design with clear separation of concerns, making the system easier to maintain, extend, and adapt to different email-sending scenarios.

# Features

## Email Service

* Fluent Email Builder Pattern
* Multiple recipients (To, CC, BCC)
* Attachments support
* HTML \& plain text content
* Async email sending
* Clean and readable syntax
* Cancellation Token to terminate an execution
* Dynamic SMTP configurations

## Progress Tracking Service

* General-purpose (not email-specific)
* Tracks total, processed, success, failed, completion
* Thread-safe using Interlocked
* Supports multiple concurrent jobs

# Installation Guide

## 1\. Install Packages

dotnet add package MailKit  
dotnet add package MimeKit

## 2\. Copy Service Files

Copy: Src/Services
place it in your project

## 3\. Configure Multiple SMTP provider

appsettings.json:
```
"SmtpSettings": {
  "DefaultProvider": "office365",
  "Providers": {
    "office365": {
      "SmtpServer": "smtp-legacy.office365.com",
      "Port": 587,
      "SenderName": "Your name",
      "SenderEmail": "no-reply@outlook.ac.id",
      "Username": "no-reply@outlook.ac.id",
      "Password": "password",
      "UseSsl": false
    },
    "gmail": {
      "SmtpServer": "smtp.gmail.com",
      "Port": 587,
      "SenderName": "Gmail Sender",
      "SenderEmail": "gmail@gmail.com",
      "Username": "gmail@gmail.com",
      "Password": "password",
      "UseSSL": false
    }
  }
}
```
## 4\. Register Services

```C#
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddSingleton<IJobProgressService, JobProgressService>();
```
```C#
var app = builder.Build();

app.UseStaticFiles();
```

# Usage

# Controller Set Up
```C#
public class EmailController : Controller
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

## Simple Email
```C#
public async Task<IActionResult> SendSimpleEmail()
{
    var result = await _emailService
        .AddTo("user@mail.com", "User")
        .SetSubject("Hello")
        .SetBody("This is a simple email")
        .SendEmailAsync();

    return Ok(result);
}
```
When you need to change the SMPT Provider

```
public async Task<IActionResult> SendSimpleEmail()
{
    var result = await _emailService
        .AddTo("user@mail.com", "User")
        .SetSubject("Hello")
        .SetBody("This is a simple email")
        .UseSmtpProvider("gmail")
        .SendEmailAsync();

    return Ok(result);
}
```
place 
```
.UseSmtpProvider("gmail")
```
before
```
.SendEmailAsync();
```

## Email with Attachment \& CC

```C#
public async Task<IActionResult> SendWithAttachment()
{
    var result = await _emailService
        .AddTo("user@mail.com", "User")
        .AddCc("manager@mail.com", "Manager")
        .AddCc("admin@mail.com", "Admin")
        .AddAttachment("files/report.pdf")
        .SetSubject("Monthly Report")
        .SetBody("<h2>Report Attached</h2>")
        .SendEmailAsync();

    return Ok(result);
}
```

## Loop Sending
```C#
public async Task<IActionResult> SendBulkEmail()
{
    var users = GetUsers();

    for (int i = 0; i < users.Count; i++)
    {
        await _emailService
            .AddTo(users[i].Email, users[i].Name)
            .SetSubject("Bulk Email")
            .SetBody("Hello from bulk sender")
            .SendEmailAsync();
    }

    return Ok("Bulk email sent");
}
```
Applied with progress tracking
```C#

public async Task<IActionResult> StartSending(CancellationToken cancellationToken)
{
    List<EmailSendResult> results = new List<EmailSendResult>();
    List<Task> tasks = new List<Task>();

    string sendTo = "sender@gmail.com";
    string recipientName = "Bernadus Widaryanto";
    string emailBody = "This is the Email content";

    var semaphore = new SemaphoreSlim(2); // Limit to 2 concurrent sends

    int nTest = 5;

    _progress.Start(nTest, "this_is_the_progress_id");
    for (int i = 1; i <= nTest; i++)
    {
        int emailIndex = i; // Capture the value to avoid closure issues
        tasks.Add(Task.Run(async () =>
        {
            await semaphore.WaitAsync();
            try
            {
                var result = await _emailService.AddTo(sendTo, recipientName)
                    .SetSubject($"Test Email {emailIndex}")
                    .SetBody(emailBody)
                    .SendEmailAsync(cancellationToken);
                lock (results)
                {
                    results.Add(result);
                }
                if(result.Success)
                {
                    _progress.ReportSuccess();
                }
                else
                {
                    _progress.ReportFailed();
                }
            }
            catch (Exception ex)
            {
                lock (results)
                 {
                    results.Add(EmailSendResult.Fail($"Failed to send email {emailIndex} {sendTo}: {ex.Message}"));
                }
                _progress.ReportFailed();
            }
            finally
            {
                semaphore.Release();
            }
        }));
    }

    await Task.WhenAll(tasks);

    _progress.MarkCompleted();

    return Ok(results);
}

public IActionResult EmailProgressNotification()
{
    var progress = _progress.GetProgress("this_is_the_progress_id");
    return Ok(progress);
}
```

