using Deslumbra.Data;
using Deslumbra.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deslumbra.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductosController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Catálogo público
        [AllowAnonymous]
        public IActionResult Index(string? buscar, string? categoria)
        {
            var productos = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                productos = productos.Where(p =>
                    p.Nombre.Contains(buscar));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                productos = productos.Where(p =>
                    p.Categoria == categoria);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Categoria = categoria;

            return View(productos.ToList());
        }

        // Formulario para agregar
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // Guardar producto
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            Producto producto,
            IFormFile? archivoImagen)
        {
            ValidarImagen(archivoImagen);

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                producto.Imagen = GuardarImagen(archivoImagen);
            }

            _context.Productos.Add(producto);
            _context.SaveChanges();

            TempData["Mensaje"] = "Producto agregado correctamente.";

            return RedirectToAction("Index");
        }

        // Formulario para editar
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // Guardar cambios y reemplazar fotografía
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(
            Producto producto,
            IFormFile? archivoImagen)
        {
            ValidarImagen(archivoImagen);

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            var productoGuardado =
                _context.Productos.Find(producto.IdProducto);

            if (productoGuardado == null)
            {
                return NotFound();
            }

            productoGuardado.Nombre = producto.Nombre;
            productoGuardado.Descripcion = producto.Descripcion;
            productoGuardado.Categoria = producto.Categoria;
            productoGuardado.Precio = producto.Precio;
            productoGuardado.Tallas = producto.Tallas;
            productoGuardado.Cantidad = producto.Cantidad;

            // Cambiar fotografía solo si se selecciona una nueva
            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                productoGuardado.Imagen = GuardarImagen(archivoImagen);
            }

            _context.SaveChanges();

            TempData["Mensaje"] = "Producto actualizado correctamente.";

            return RedirectToAction("Index");
        }

        // Eliminar producto
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            TempData["Mensaje"] = "Producto eliminado correctamente.";

            return RedirectToAction("Index");
        }

        // Validar tamaño y tipo básico de imagen
        private void ValidarImagen(IFormFile? archivoImagen)
        {
            if (archivoImagen == null || archivoImagen.Length == 0)
            {
                return;
            }

            string extension = Path.GetExtension(
                archivoImagen.FileName).ToLowerInvariant();

            string[] permitidas =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!permitidas.Contains(extension))
            {
                ModelState.AddModelError(
                    "archivoImagen",
                    "Selecciona una imagen JPG, PNG o WEBP.");
            }

            if (archivoImagen.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "archivoImagen",
                    "La imagen no debe superar los 5 MB.");
            }
        }

        // Guardar fotografía
        private string GuardarImagen(IFormFile archivoImagen)
        {
            string carpeta = Path.Combine(
                _environment.WebRootPath,
                "imagenes",
                "productos");

            Directory.CreateDirectory(carpeta);

            string extension = Path.GetExtension(
                archivoImagen.FileName).ToLowerInvariant();

            string nombreImagen =
                Guid.NewGuid().ToString() + extension;

            string ruta = Path.Combine(carpeta, nombreImagen);

            using (var archivo = new FileStream(ruta, FileMode.CreateNew))
            {
                archivoImagen.CopyTo(archivo);
            }

            return "/imagenes/productos/" + nombreImagen;
        }
    }
}
