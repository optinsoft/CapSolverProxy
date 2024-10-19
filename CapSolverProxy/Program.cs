using CapSolverProxy;
using Microsoft.Extensions.Configuration;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var settings = builder.Configuration.GetSection("CapSolverProxy").Get<CapSolverSettings>();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
CapSolverService capsolver = new(settings, factory);

app.MapGet("/", () => "Hello World!");

app.MapPost("/createTask", async delegate(HttpContext context)
{
    using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
    string requestJson = await reader.ReadToEndAsync();    
    return await capsolver.CreateTask(requestJson);
});

app.MapGet("/stats", () => capsolver.GetStats());

app.Run();
