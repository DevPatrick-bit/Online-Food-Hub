namespace OnlineFoodHub.Models.ViewModels
{
    public class ProductoPaginadosViewModel
    {
        public List<Producto> Productos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int? CategoriaIdSelecionada { get; set; }
        public string Busqueda { get; set; }
        public bool MostrarMensajeSinResultados { get; set; }
        public string? NombreCategoriaSelecionada { get; set; }
    }
}
