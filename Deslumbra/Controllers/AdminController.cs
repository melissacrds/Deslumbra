using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Deslumbra.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Mostrar inicio de sesión
        [HttpGet]
        public IActionResult Ingresar()
        {
            return View();
        }

        // Comprobar usuario y contraseña
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ingresar(
            string usuario, string clave)
        {
            string usuarioCorrecto =
                _configuration["Admin:Usuario"] ?? "";

            string claveCorrecta =
                _configuration["Admin:Clave"] ?? "";

            // Comparación de credenciales
            bool usuarioValido = Comparar(usuario, usuarioCorrecto);
            bool claveValida = Comparar(clave, claveCorrecta);

            if (!string.IsNullOrEmpty(usuarioCorrecto) &&
                !string.IsNullOrEmpty(claveCorrecta) &&
                usuarioValido && claveValida)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuarioCorrecto),
                    new Claim(ClaimTypes.Role, "Administrador")
                };

                var identidad = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identidad);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToAction("Index", "Productos");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        // Cerrar sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private static bool Comparar(string? recibido, string esperado)
        {
            byte[] a = SHA256.HashData(
                Encoding.UTF8.GetBytes(recibido ?? ""));

            byte[] b = SHA256.HashData(
                Encoding.UTF8.GetBytes(esperado));

            return CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}

