namespace Deslumbra.Models
{
    public class ResumenInventario
    {
        public int TotalProductos { get; set; }

        public int TotalUnidades { get; set; }

        public decimal ValorInventario { get; set; }

        public int ProductosAgotados { get; set; }
    }
}
