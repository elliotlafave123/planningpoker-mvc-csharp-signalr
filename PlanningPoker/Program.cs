// Program.cs
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Hubs;
using PlanningPoker.Interfaces;
using PlanningPoker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework and SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IVoteService, VoteService>();

// Add SignalR for WebSocket support
builder.Services.AddSignalR();

// Add session support
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Use static files, routing, and session
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map the SignalR hub
app.MapHub<GameHub>("/gamehub");

app.Run();
