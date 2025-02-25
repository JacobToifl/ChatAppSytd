using Microsoft.AspNetCore.SignalR;
using Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        policyBuilder.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();
app.UseCors("CorsPolicy");

app.MapHub<ChatHub>("/Chat");

app.Run();
