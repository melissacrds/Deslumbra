namespace Deslumbra.Models
{
    public class CarritoItem
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; } = "";

        public string? Imagen { get; set; }

        public string Talla { get; set; } = "";

        public decimal Precio { get; set; }

        public int Cantidad { get; set; }

        public decimal Subtotal
        {
            get
            {
                return Precio * Cantidad;
            }
        }
    }
}
