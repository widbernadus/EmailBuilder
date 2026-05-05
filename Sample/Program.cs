using Src.Services.Email.Implementations;
using Src.Services.Email.Interfaces;
using Src.Services.Email.Models;
using Src.Services.Progress.Implementations;
using Src.Services.Progress.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddSingleton<IJobProgressService, JobProgressService>();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Email/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Email}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
