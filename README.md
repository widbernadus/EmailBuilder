# Email Builder + Progress Tracking Service (C# / ASP.NET)

A clean, reusable email sending service built with **MailKit +
MimeKit**, designed using the **Builder Pattern**, and extended with a
**general-purpose progress tracking system**.

\---

# Features

## Email Service

* Fluent Email Builder Pattern
* Multiple recipients (To, CC, BCC)
* Attachments support
* HTML \& plain text content
* Async email sending
* Clean and readable syntax
* Cancelation Token to terminate an execution

## Progress Tracking Service

* General-purpose (not email-specific)
* Tracks total, processed, success, failed, completion
* Thread-safe using Interlocked
* Supports multiple concurrent jobs

\---

# Installation Guide

## 1\. Install Packages

dotnet add package MailKit  
dotnet add package MimeKit

## 2\. Copy Service Files

Copy: Services/, Models/

## 3\. Configure SMTP

appsettings.json:

{ "EmailSettings": { "Host": "smtp.gmail.com", "Port": 587, "Username":
"your@email.com", "Password": "yourpassword", "UseSSL": true } }

## 4\. Register Services

builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddSingleton<IJobProgressService,
JobProgressService>();

\---

# Usage

## Simple Email

await \_emailService .AddTo("user@mail.com", "User")
.SetSubject("Hello") .SetBody("Simple email") .SendEmailAsync();

## Email with Attachment \& CC

await \_emailService .AddTo("user@mail.com", "User")
.AddCc("admin@mail.com", "Admin") .AddAttachment("file.pdf")
.SetSubject("Report") .SendEmailAsync();

## Loop Sending

for (int i = 0; i < users.Count; i++) { await \_emailService
.AddTo(users\[i].Email, users\[i].Name) .SetSubject("Bulk")
.SendEmailAsync(); }

\---

# Notes

Production-ready design with clean architecture and extensibility.

