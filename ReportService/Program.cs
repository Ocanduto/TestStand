using Microsoft.EntityFrameworkCore;

using ReportService.DataContexts;

namespace ReportService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<KreativDBContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Results"), b => b.MigrationsAssembly("ReportService"));
        });

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();
    }
}
