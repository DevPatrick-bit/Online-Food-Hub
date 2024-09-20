using Microsoft.EntityFrameworkCore;
using OnlineFoodHub.Data;
using OnlineFoodHub.Models;
using static OnlineFoodHub.Services.CategoriaService;

namespace OnlineFoodHub.Services
{
    // La clase CategoriaService implementa la interfaz ICategoriaService
    // y se encarga de manejar las operaciones relacionadas con las categorías.
    public class CategoriaService : ICategoriaService
    {
        // El contexto de la base de datos para interactuar con la base de datos.
        private readonly ApplicationDbContext _context;

        // Constructor que recibe un ApplicationDbContext para inicializar el contexto.
        public CategoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método asíncrono que obtiene la lista de todas las categorías desde la base de datos.
        public async Task<List<Categoria>> GetCategorias()
        {
            // Devuelve la lista de categorías de forma asíncrona.
            return await _context.Categorias.ToListAsync();
        }
    }
}
