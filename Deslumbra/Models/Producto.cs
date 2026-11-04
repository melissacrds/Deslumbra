using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deslumbra.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        public string Nombre { get; set; } = "";

        public string? Descripcion { get; set; }

        [Required]
        public string Categoria { get; set; } = "";

        [Range(0.01, 99999999)]
        public decimal Precio { get; set; }

        public string? Tallas { get; set; }

        public string? Imagen { get; set; }

        [Range(0, 999999)]
        public int Cantidad { get; set; }
    }
}

