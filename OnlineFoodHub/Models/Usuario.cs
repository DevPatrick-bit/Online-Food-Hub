using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineFoodHub.Models
{
    public class Usuario
    {
        public Usuario()
        {
            Pedidos = new List<Pedido>();
        }

        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombres { get; set; } = null;
        [Required]
        [StringLength(20)]
        public string Telefono { get; set; } = null;

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = null;

        [Required]
        [StringLength(50)]
        public string Contrasenia { get; set; } = null;
        [Required]
        [StringLength(255)]
        public string Correo { get; set; } = null;
        [Required]
        [StringLength(50)]
        public string Direccion { get; set; } = null;
        [Required]
        public int RolID { get; set; }

        [ForeignKey("RolID")]
        public Rol Rol { get; set; } = null;
        public ICollection<Pedido> Pedidos { get; set; }

        [InverseProperty("Usuario")]
        public ICollection<Direccion> Direcciones { get; set; } = null;
    }
}
