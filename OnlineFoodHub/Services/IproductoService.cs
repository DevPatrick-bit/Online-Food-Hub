using OnlineFoodHub.Models;
using OnlineFoodHub.Models.ViewModels;

namespace OnlineFoodHub.Services
{
    // La interfaz IproductoService define el contrato para los servicios que manejan productos.
    public interface IproductoService
    {
        // Método para obtener un producto por su ID.
        // Devuelve un objeto Producto.
        Producto GetProducto(int id);

        // Método asíncrono para obtener una lista de productos destacados.
        // Retorna una tarea que representa la operación asíncrona y produce una lista de objetos Producto.
        Task<List<Producto>> GetProductosDestacados();

        // Método asíncrono para obtener productos paginados con opciones de filtrado.
        // Retorna una tarea que representa la operación asíncrona y produce un objeto ProductoPaginadosViewModel.
        // Permite filtrar por categoría y búsqueda, y especificar la página actual y el número de productos por página.
        Task<ProductoPaginadosViewModel> GetProductoPaginados(int? categoriaId, string? busqueda, int pagina, int productosPorPagina);
    }
}
