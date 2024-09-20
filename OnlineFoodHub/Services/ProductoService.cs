using Microsoft.EntityFrameworkCore;
using OnlineFoodHub.Data;
using OnlineFoodHub.Models;
using OnlineFoodHub.Models.ViewModels;

namespace OnlineFoodHub.Services
{
    // La clase ProductoService implementa la interfaz IproductoService
    // y se encarga de manejar las operaciones relacionadas con los productos.
    public class ProductoService : IproductoService
    {
        // El contexto de la base de datos para interactuar con la base de datos.
        private readonly ApplicationDbContext _context;

        // Constructor que recibe un ApplicationDbContext para inicializar el contexto.
        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para obtener un producto por su ID. Incluye la categoría del producto.
        public Producto GetProducto(int id)
        {
            // Busca el producto con el ID especificado e incluye la categoría asociada.
            var producto = _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.ProductoId == id);

            // Si el producto es encontrado, lo devuelve; de lo contrario, devuelve un nuevo producto.
            return producto ?? new Producto();
        }

        // Método asíncrono que actualmente no está implementado.
        public Task<List<Producto>> GetProductoAsync(int id)
        {
            throw new NotImplementedException();
        }

        // Método asíncrono para obtener una lista de productos destacados.
        public async Task<List<Producto>> GetProductosDestacados()
        {
            IQueryable<Producto> productosQuery = _context.Productos;

            // Filtra los productos activos y toma los primeros 9 productos.
            productosQuery = productosQuery.Where(p => p.Activo);
            List<Producto> productosDestacados = await productosQuery
                .Take(9)
                .ToListAsync();

            return productosDestacados;
        }

        // Método asíncrono para obtener productos paginados con opciones de filtrado.
        public async Task<ProductoPaginadosViewModel> GetProductoPaginados(
            int? categoriaId,
            string? busqueda,
            int pagina,
            int productosPorPagina)
        {
            IQueryable<Producto> query = _context.Productos;

            // Filtra los productos activos.
            query = query.Where(p => p.Activo);

            // Aplica filtros adicionales según la categoría y la búsqueda.
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaID == categoriaId);
            }
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(
                    p => p.Nombre.Contains(busqueda) || p.Descripcion.Contains(busqueda));
            }

            // Obtiene el número total de productos y calcula el número total de páginas.
            int totalProductos = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalProductos / productosPorPagina);

            // Ajusta la página actual para que esté dentro del rango válido.
            if (pagina < 1)
            {
                pagina = 1;
            }
            else if (pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            List<Producto> productos = new();
            if (totalPaginas > 0)
            {
                // Obtiene los productos para la página actual.
                productos = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((pagina - 1) * productosPorPagina)
                    .Take(productosPorPagina)
                    .ToListAsync();
            }

            // Determina si se debe mostrar un mensaje sin resultados.
            bool MostrarMensajeSinResultados = totalProductos == 0;

            // Crea el modelo de vista para los productos paginados.
            var model = new ProductoPaginadosViewModel
            {
                Productos = productos,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                CategoriaIdSelecionada = categoriaId,
                Busqueda = busqueda,
                MostrarMensajeSinResultados = MostrarMensajeSinResultados
            };

            return model;
        }
    }
}
