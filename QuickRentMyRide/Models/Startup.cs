using System; // TimeSpan வகைக்காக
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        // Session service சேர்க்கவும்
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // session காலாவதியான நேரம்
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/Home/Error");

        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();  // Middleware பட்டியலில் Routingக்கு பிறகு, Endpointக்கு முன் வர வேண்டும்

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=LoginRegister}/{id?}");
        });
    }
}
