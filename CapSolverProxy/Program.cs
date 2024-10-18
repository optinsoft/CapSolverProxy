using CapSolverProxy;
using System.Reflection.PortableExecutable;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/createTask", async delegate(HttpContext context)
{
    using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
    string requestJson = await reader.ReadToEndAsync();    
    return await CapSolverService.CreateTask(requestJson);
});

app.Run();
