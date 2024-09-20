using System.ComponentModel.DataAnnotations;

namespace OnlineFoodHub.Models
{
    public class Rol
    {
        [Key]
        public int RolId { get; set; }
        [Required(ErrorMessage = "El campo Nombre es Olbigatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = null;
    }
}
