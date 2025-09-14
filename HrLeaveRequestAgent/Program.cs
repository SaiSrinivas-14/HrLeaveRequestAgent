using HrLeaveRequestAgent.Data;
using HrLeaveRequestAgent.Models;
using HrLeaveRequestAgent.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HrDbContext>(options =>
    options.UseInMemoryDatabase("HrLeaveDb"));

builder.Services.AddScoped<ILeaveService, LeaveService>();

builder.Services.AddHttpClient<GptService>();

builder.Services.AddTransient(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var config = provider.GetRequiredService<IConfiguration>();
    var authToken = config["PerplexityApi:AuthToken"];
    return new GptService(httpClient, authToken);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HrDbContext>();

    if (!context.Employees.Any())
    {
        var emp = new Employee { Id = 1, Name = "John Doe", Email = "john.doe@example.com" };
        context.Employees.Add(emp);
        context.LeaveBalances.Add(new LeaveBalance { EmployeeId = 1, CasualLeaveRemaining = 7 });
        context.SaveChanges();
    }
}

app.UseStaticFiles();

app.MapDefaultControllerRoute();

app.Run();
