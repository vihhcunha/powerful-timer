using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerfulTimer.Api.Data;
using PowerfulTimer.Api.Middlewares;
using PowerfulTimer.Api.Models;
using PowerfulTimer.Api.Services;
using PowerfulTimer.Api.Services.WebSockets;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
    builder.WebHost.UseSentry();

// Add services to the container.
builder.Services.AddDbContext<PowerfulTimerContext>(c =>
{
    c.UseSqlServer(builder.Configuration.GetConnectionString("PowerfulTimer"),
        sqlServer =>
        {
            sqlServer.EnableRetryOnFailure();
        });
    c.LogTo(Console.WriteLine);
    c.EnableDetailedErrors();
});
builder.Services.AddScoped<ITimerService, TimerService>();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
builder.Services.AddCors();
builder.Services.AddHealthChecks();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => 
{
    options.InvalidModelStateResponseFactory = (errorContext) =>
    {
        var errors = errorContext.ModelState
        .Values
        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
        .ToList();

        var result = ResponseModelBuilder.CreateErrorResponse(errors);
        return new BadRequestObjectResult(result);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(option =>
{
    option.AllowAnyOrigin();
    option.AllowAnyHeader();
    option.AllowAnyMethod();
});

app.UseHttpsRedirection();

if (app.Environment.IsProduction())
{
    app.UseSentryTracing();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(40)});

app.MapHealthChecks("/health-check");
app.MapControllers();

app.Run();
