using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Enter your API Key in the field below.",
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        logger.LogError($"Unhandled Exception: {ex.Message}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred.",
            Details = ex.Message
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(jsonResponse);
    }
});
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();


    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        await next();
        return;
    }
    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
    {
        logger.LogWarning("Unauthorized request - Missing API key");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("{\"StatusCode\": 401, \"Message\": \"Unauthorized - API Key is required\"}");
        return;
    }

    var validTokens = new List<string> { "secure-token" };

    if (!validTokens.Contains(apiKey))
    {
        logger.LogWarning($"Unauthorized request - Invalid API key: {apiKey}");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("{\"StatusCode\": 401, \"Message\": \"Unauthorized - Invalid API Key\"}");
        return;
    }

    await next();
});


app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation($"Incoming Request: {context.Request.Method} {context.Request.Path}");
    await next();
    logger.LogInformation($"Outgoing Response: {context.Response.StatusCode}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
