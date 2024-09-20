using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineFoodHub.Models
{
    public class Direccion
    {
        [Key]
        public int DireccionID { get; set; }

        [Required]
        [StringLength(50)]
        public string Address { get; set; } = null;

        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = null;
    }
}
