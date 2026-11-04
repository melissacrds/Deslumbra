
using System.Diagnostics;
using Deslumbra.Models;
using Microsoft.AspNetCore.Mvc;

namespace Deslumbra.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Página principal
        public IActionResult Index()
        {
            return View();
        }

        // Página Quiénes somos
        public IActionResult QuienesSomos()
        {
            return View();
        }

        // Página de privacidad
        public IActionResult Privacy()
        {
            return View();
        }

        // Página de errores
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ??
                            HttpContext.TraceIdentifier
            });
        }
    }
}