
using System.Text;
using System.Text.Json;
using Deslumbra.Data;
using Deslumbra.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deslumbra.Controllers
{
    [AllowAnonymous]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string ClaveCarrito = "CarritoDeslumbra";

        // WhatsApp de Deslumbra, con código de Costa Rica.
        private const string NumeroWhatsApp = "50687136383";

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener el carrito de la sesión.
        private List<CarritoItem> ObtenerCarrito()
        {
            string? datos =
                HttpContext.Session.GetString(ClaveCarrito);

            if (string.IsNullOrEmpty(datos))
            {
                return new List<CarritoItem>();
            }

            return JsonSerializer.Deserialize<List<CarritoItem>>(datos)
                   ?? new List<CarritoItem>();
        }

        // Guardar el carrito en la sesión.
        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            string datos = JsonSerializer.Serialize(carrito);

            HttpContext.Session.SetString(ClaveCarrito, datos);
        }

        // Mostrar el carrito.
        [HttpGet]
        public IActionResult Index()
        {
            return View(ObtenerCarrito());
        }

        // Agregar productos al carrito.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(
            int idProducto,
            string talla,
            int cantidad = 1)
        {
            var producto = _context.Productos.Find(idProducto);

            if (producto == null)
            {
                return NotFound();
            }

            if (producto.Cantidad <= 0)
            {
                TempData["Error"] = "Este producto está agotado.";

                return RedirectToAction("Index", "Productos");
            }

            if (cantidad < 1 || cantidad > producto.Cantidad)
            {
                TempData["Error"] =
                    "La cantidad solicitada no está disponible.";

                return RedirectToAction("Index", "Productos");
            }

            var tallasDisponibles = (producto.Tallas ?? "")
                .Split(',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            if (tallasDisponibles.Length > 0)
            {
                var tallaValida = tallasDisponibles.FirstOrDefault(t =>
                    string.Equals(
                        t,
                        talla,
                        StringComparison.OrdinalIgnoreCase));

                if (tallaValida == null)
                {
                    TempData["Error"] =
                        "Selecciona una talla válida.";

                    return RedirectToAction("Index", "Productos");
                }

                talla = tallaValida;
            }
            else
            {
                talla = "Única";
            }

            var carrito = ObtenerCarrito();

            int cantidadActual = carrito
                .Where(p => p.IdProducto == idProducto)
                .Sum(p => p.Cantidad);

            if (cantidadActual + cantidad > producto.Cantidad)
            {
                TempData["Error"] =
                    "No hay suficientes unidades disponibles.";

                return RedirectToAction("Index", "Productos");
            }

            var existente = carrito.FirstOrDefault(p =>
                p.IdProducto == idProducto &&
                p.Talla.Equals(
                    talla,
                    StringComparison.OrdinalIgnoreCase));

            if (existente != null)
            {
                existente.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    IdProducto = producto.IdProducto,
                    Nombre = producto.Nombre,
                    Imagen = producto.Imagen,
                    Talla = talla,
                    Precio = producto.Precio,
                    Cantidad = cantidad
                });
            }

            GuardarCarrito(carrito);

            TempData["Mensaje"] =
                "Producto agregado al carrito correctamente.";

            return RedirectToAction("Index");
        }

        // Modificar cantidades.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Actualizar(
            int idProducto,
            string talla,
            int cantidad)
        {
            var carrito = ObtenerCarrito();

            var item = carrito.FirstOrDefault(p =>
                p.IdProducto == idProducto &&
                p.Talla == talla);

            if (item == null)
            {
                TempData["Error"] =
                    "No se encontró el producto en el carrito.";

                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(idProducto);

            if (producto == null)
            {
                TempData["Error"] =
                    "Este producto ya no está disponible.";

                return RedirectToAction("Index");
            }

            if (cantidad < 1)
            {
                TempData["Error"] =
                    "La cantidad debe ser mayor que cero.";

                return RedirectToAction("Index");
            }

            int otrasCantidades = carrito
                .Where(p =>
                    p.IdProducto == idProducto &&
                    p != item)
                .Sum(p => p.Cantidad);

            if (cantidad + otrasCantidades > producto.Cantidad)
            {
                TempData["Error"] =
                    "No hay suficientes unidades disponibles. " +
                    "Existencias actuales: " + producto.Cantidad;

                return RedirectToAction("Index");
            }

            item.Cantidad = cantidad;
            item.Nombre = producto.Nombre;
            item.Imagen = producto.Imagen;
            item.Precio = producto.Precio;

            GuardarCarrito(carrito);

            TempData["Mensaje"] =
                "Cantidad actualizada correctamente.";

            return RedirectToAction("Index");
        }

        // Eliminar una prenda.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(
            int idProducto,
            string talla)
        {
            var carrito = ObtenerCarrito();

            var item = carrito.FirstOrDefault(p =>
                p.IdProducto == idProducto &&
                p.Talla == talla);

            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarrito(carrito);

                TempData["Mensaje"] =
                    "Producto eliminado del carrito.";
            }

            return RedirectToAction("Index");
        }

        // Vaciar el carrito.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vaciar()
        {
            HttpContext.Session.Remove(ClaveCarrito);

            TempData["Mensaje"] =
                "El carrito se ha vaciado.";

            return RedirectToAction("Index");
        }

        // Preparar el pedido para WhatsApp.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizarPedido()
        {
            var carrito = ObtenerCarrito();

            if (carrito.Count == 0)
            {
                TempData["Error"] =
                    "Agrega productos antes de finalizar el pedido.";

                return RedirectToAction("Index");
            }

            decimal total = 0;

            // Comprobar todos los productos en SQL Server.
            foreach (var item in carrito)
            {
                var producto =
                    _context.Productos.Find(item.IdProducto);

                if (producto == null)
                {
                    TempData["Error"] =
                        "Uno de los productos ya no está disponible. " +
                        "Elimínalo del carrito antes de continuar.";

                    return RedirectToAction("Index");
                }

                var tallasDisponibles = (producto.Tallas ?? "")
                    .Split(',',
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries);

                bool tallaValida;

                if (tallasDisponibles.Length == 0)
                {
                    tallaValida = item.Talla == "Única";
                }
                else
                {
                    tallaValida = tallasDisponibles.Any(t =>
                        string.Equals(
                            t,
                            item.Talla,
                            StringComparison.OrdinalIgnoreCase));
                }

                if (!tallaValida)
                {
                    TempData["Error"] =
                        "La talla seleccionada para " +
                        producto.Nombre +
                        " ya no está disponible.";

                    return RedirectToAction("Index");
                }

                int cantidadSolicitada = carrito
                    .Where(p => p.IdProducto == item.IdProducto)
                    .Sum(p => p.Cantidad);

                if (item.Cantidad < 1 ||
                    cantidadSolicitada > producto.Cantidad)
                {
                    TempData["Error"] =
                        "No hay suficientes unidades de " +
                        producto.Nombre +
                        ". Revisa las cantidades del carrito.";

                    return RedirectToAction("Index");
                }

                // Utilizar los precios actuales de la base de datos.
                item.Nombre = producto.Nombre;
                item.Precio = producto.Precio;
                item.Imagen = producto.Imagen;

                total += item.Precio * item.Cantidad;
            }

            // Conservar los precios actualizados en el carrito.
            GuardarCarrito(carrito);

            // Construir el mensaje.
            var mensaje = new StringBuilder();

            mensaje.AppendLine("¡Hola, DESLUMBRA!");
            mensaje.AppendLine();
            mensaje.AppendLine(
                "Deseo realizar el siguiente pedido:");
            mensaje.AppendLine();

            int numero = 1;

            foreach (var item in carrito)
            {
                mensaje.AppendLine(
                    numero + ". " + item.Nombre);

                mensaje.AppendLine(
                    "Talla: " + item.Talla);

                mensaje.AppendLine(
                    "Cantidad: " + item.Cantidad);

                mensaje.AppendLine(
                    "Precio unitario: ₡" +
                    item.Precio.ToString("N2"));

                mensaje.AppendLine(
                    "Subtotal: ₡" +
                    item.Subtotal.ToString("N2"));

                mensaje.AppendLine();

                numero++;
            }

            mensaje.AppendLine("------------------------");

            mensaje.AppendLine(
                "TOTAL DEL PEDIDO: ₡" +
                total.ToString("N2"));

            mensaje.AppendLine("------------------------");
            mensaje.AppendLine();

            mensaje.AppendLine(
                "Quedo pendiente de la confirmación " +
                "de disponibilidad y entrega.");

            mensaje.AppendLine("¡Muchas gracias!");

            // Codificar el mensaje para incluirlo en una URL.
            string textoCodificado =
                Uri.EscapeDataString(mensaje.ToString());

            string enlaceWhatsApp =
                "https://wa.me/" +
                NumeroWhatsApp +
                "?text=" +
                textoCodificado;

            // Abrir la página de WhatsApp.
            return Redirect(enlaceWhatsApp);
        }
    }
}
