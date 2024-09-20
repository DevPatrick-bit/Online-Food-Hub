using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineFoodHub.Models
{
    public class Producto
    {
        [Key]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El campo Código es obligatorio")]
        [StringLength(255)]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(255)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Tipo es obligatorio")]
        [StringLength(255)]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "El campo Descripción es obligatorio")]
        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El campo Precio es obligatorio")]
        [Range(0.01, 1000000, ErrorMessage = "El precio debe estar entre 0.01 y 1000000.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El campo Imagen es obligatorio")]
        [StringLength(255)]
        public string Imagen { get; set; }

        [Required(ErrorMessage = "El campo Categoria es obligatorio")]
        public int CategoriaID { get; set; }

        [ForeignKey("CategoriaID")]
        public Categoria Categoria { get; set; }

        public int Stock { get; set; }

        [StringLength(100)]
        public string Marca { get; set; }

        [Required]
        public bool Activo { get; set; }

        public ICollection<Detalle_Pedido> DetallePedido { get; set; }
    }
}
