using Microsoft.Net.Http.Headers;
using System.Net;
using System.Web.Http;
using WebApplication1.Authentication;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();
//builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/home/login");
//builder.Services
//         .AddAuthentication()
//         .AddCookie(options =>
//         {
//             options.LoginPath = "/home/login";
//             options.LogoutPath = "/logout";
//         });

builder.Services.AddHttpClient<ITestService, TestService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TestServiceURL"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();


// 2 create an HttpClient used for accessing the IDP
builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:IdentityServerURL"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (HttpResponseException ex)
    {
        if(ex.Response.StatusCode == HttpStatusCode.Unauthorized)
            ctx.Response.Redirect("/home/login");
    }
});

//app.UseStatusCodePages(async context =>
//{
//    var response = context.HttpContext.Response;

//    if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
//            response.StatusCode == (int)HttpStatusCode.Forbidden)
//        response.Redirect("/home/login");
//});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
