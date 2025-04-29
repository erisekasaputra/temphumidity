var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddOpenApi();

var app = builder.Build();
 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
 
app.MapGet("/api/timezone/utc", () =>
{ 
    return new 
    {
        Time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
        TimeZone = "UTC"       
    };
});

app.Run();
 