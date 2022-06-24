using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITestService _testService;
        private readonly IAuthenticationService _authenticationService;

        public HomeController(ITestService testService, IAuthenticationService authenticationService)
        {
            _testService = testService;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["resp"] = await _testService.GetData();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Login([FromQuery]string? returnUrl = null)
        {
            var (accessTokenModel, refreshTokenModel) = await _authenticationService.RetrieveNewToken();

            if (accessTokenModel == null)
                return StatusCode(503, "Authentication service is unavailable.");

            if (accessTokenModel.SetNewCookie) Response.Cookies.Append("access_token", accessTokenModel.Token, new CookieOptions() { MaxAge = TimeSpan.FromSeconds(accessTokenModel.ExpiresIn), HttpOnly = true }) ;
            if (refreshTokenModel.SetNewCookie) Response.Cookies.Append("refresh_token", refreshTokenModel.Token, new CookieOptions() { MaxAge = TimeSpan.FromDays(60), HttpOnly = true });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}