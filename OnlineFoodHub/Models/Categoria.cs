using System.ComponentModel.DataAnnotations;

namespace OnlineFoodHub.Models
{
    public class Categoria
    {
        public Categoria()
        {
            Productos = new List<Producto>();
        }
        [Key]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El campo Nombre es Olbigatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = null;

        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; } = null;

        [Required(ErrorMessage = "El campo Imagen es obligatorio")]
        [StringLength(255)]
        public string ImagenCategoria { get; set; }

        public ICollection<Producto> Productos { get; set; } = null;

    }
}

