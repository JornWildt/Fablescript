using Fablescript.Core;
using Fablescript.Core.Database;

namespace Fablescript.Web.Client
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.AddServiceDefaults();

      // Add services to the container.
      builder.Services.AddControllersWithViews();

      builder.Services.AddSession()
        .AddHttpContextAccessor()
        .AddCore(builder.Configuration)
        .AddCorePersistence(builder.Configuration)
        .AddMainWebClient(builder.Configuration);

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        // app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();
      app.UseSession();
      app.UseAuthorization();

      app.MapControllerRoute(
          name: "default",
          pattern: "app/{controller=Home}/{action=Index}/{id?}");

      app.Run();
    }
  }
}
