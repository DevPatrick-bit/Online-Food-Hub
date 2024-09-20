using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineFoodHub.Models
{
    public class Pedido
    {
        public int PedidoId { get; set; }
        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = null;
        [Required]
        public DateTime Fecha { get; set; }
        [Required]
        public string Estado { get; set; } = null;
        public int DireccionIdSeleccionada { get; set; }
        public Direccion Direccion { get; set; } = null;
        public decimal Total { get; set; }

        public ICollection<Detalle_Pedido> DetallesPedido { get; set; } = null;
    }
}
