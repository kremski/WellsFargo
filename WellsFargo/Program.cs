using WellsFargo.DAL;
using Microsoft.EntityFrameworkCore;
using WellsFargo.Services;
using WellsFargo.DAL.Model;
using WellsFargo.Helpers;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = 
    new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Add services to the container.
builder.Services.AddRazorPages(); 
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<WellsFargoDbContext>(options =>
    options.UseSqlServer(configuration["ConnectionStrings:WellsFargoDatabase"]));

builder.Services.AddScoped<IDbService<Portfolio>, PortfolioService>();
builder.Services.AddScoped<IDbService<Security>, SecurityService>();
builder.Services.AddScoped<IDbTransactionService, DbTransactionService>();
builder.Services.AddScoped<ICsvParser, CsvParser>();
builder.Services.AddScoped<IOmsOutoutHelper, OmsOutoutHelper>();

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<WellsFargoDbContext>();
    context.Database.EnsureCreated();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
