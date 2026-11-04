using Deslumbra.Data;
using Deslumbra.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deslumbra.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PanelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PanelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mostrar estadísticas del inventario
        [HttpGet]
        public IActionResult Index()
        {
            var resumen = new ResumenInventario
            {
                TotalProductos = _context.Productos.Count(),

                TotalUnidades = _context.Productos
                    .Sum(p => (int?)p.Cantidad) ?? 0,

                ValorInventario = _context.Productos
                    .Sum(p => (decimal?)(p.Precio * p.Cantidad))
                    ?? 0,

                ProductosAgotados = _context.Productos
                    .Count(p => p.Cantidad == 0)
            };

            return View(resumen);
        }
    }
}