using MCCC.Extensions.Configuration.Encryption;
using MCCC.Brainfuse.DataSync;
using MCCC.Brainfuse.DataSync.Models;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection(AppSettings.Configuration))
    .ValidateDataAnnotations()
    .ValidateOnStart()
    .Decrypt();

builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

var host = builder.Build();

host.Run();