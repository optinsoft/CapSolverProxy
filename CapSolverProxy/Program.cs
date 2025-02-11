using CapSolverProxy;
using Microsoft.Extensions.Configuration;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var AllowAllCors = "AllowAllCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAllCors,
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseMiddleware<LocalhostMiddleware>();

app.UseCors(AllowAllCors);

var settings = builder.Configuration.GetSection("CapSolverProxy").Get<CapSolverSettings>();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
CapSolverService capsolver = new(settings, factory);

app.MapGet("/", () => "Hello World!");

app.MapPost("/getBalance", async delegate (HttpContext context)
{
    using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
    string requestJson = await reader.ReadToEndAsync();
    var responseJson = await capsolver.GetBalance(requestJson);
    context.Response.ContentType = "application/json";
    return responseJson;
});

app.MapPost("/createTask", async delegate(HttpContext context)
{
    using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
    string requestJson = await reader.ReadToEndAsync();
    var responseJson = await capsolver.CreateTask(requestJson);
    context.Response.ContentType = "application/json";
    return responseJson;
});

app.MapGet("/ProxyStats", (HttpContext context) => {
    var responseJson = capsolver.GetStats();
    context.Response.ContentType = "application/json";
    return responseJson;
});

app.Run();
