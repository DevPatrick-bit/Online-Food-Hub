using OnlineFoodHub.Models;

namespace OnlineFoodHub.Services
{
    // La interfaz ICategoriaService define el contrato para los servicios que manejan categorías.
    public interface ICategoriaService
    {
        // Método asíncrono para obtener una lista de todas las categorías.
        // Retorna una tarea que representa la operación asíncrona y produce una lista de objetos Categoria.
        Task<List<Categoria>> GetCategorias();
    }

}
