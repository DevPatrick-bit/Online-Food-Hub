using Microsoft.AspNetCore.Mvc;
using OnlineFoodHub.Data;
using OnlineFoodHub.Models;
using OnlineFoodHub.Services;
using System.Diagnostics;

namespace OnlineFoodHub.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IproductoService _productoService;

        private readonly ICategoriaService _categoriaService;




        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IproductoService productoService,
            ICategoriaService categoriaSevice

        )

            : base(context)
        {
            _logger = logger;
            _productoService = productoService;
            _categoriaService = categoriaSevice;

        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categorias = await _categoriaService.GetCategorias();

            try
            {
                List<Producto> productosDestacados = await _productoService.GetProductosDestacados();
                return View(productosDestacados);
            }
            catch (Exception e)
            {

                return HandleDbError(e);
            }

        }

        public IActionResult DetalleProducto(int id)
        {
            var producto = _productoService.GetProducto(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        public async Task<IActionResult> Productos(
        int? categoriaId,
        string? busqueda,
        int pagina = 1
)
        {
            try
            {
                int productosPorPagina = 9;
                // Llama al servicio para obtener los productos paginados según los parámetros
                var model = await _productoService.GetProductoPaginados(
                    categoriaId,
                    busqueda,
                    pagina,
                    productosPorPagina);

                // Obtiene las categorías y las almacena en ViewBag para usarlas en la vista
                ViewBag.Categorias = await _categoriaService.GetCategorias();

                // Comprueba si la solicitud es una petición AJAX
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Si es una petición AJAX, devuelve solo la vista parcial
                    return PartialView("_ProductosPartial", model);
                }
                // Si no es una petición AJAX, devuelve la vista completa
                return View(model);
            }
            catch (Exception e)
            {
                // Maneja cualquier excepción que ocurra y redirige a una página de error
                return HandleError(e);
            }
        }



        public async Task<IActionResult> AgregarProducto(
            int id,
            int cantidad,
            int? categoriaId,
            string? busqueda,
            int pagina = 1)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                return RedirectToAction(
                    "Producto",
                    new
                    {
                        id,
                        categoriaId,
                        busqueda,
                        pagina
                    }
                );
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProductoIndex(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                TempData["ErrorMessage"] = "No se pudo agregar el producto al carrito.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Producto agregado al carrito con éxito.";
            return RedirectToAction("Index"); // O la acción que desees
        }

        public async Task<IActionResult> AgregarProductoDetalle(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                return RedirectToAction("DetalleProducto", new { id });

            }
            else
            {
                return NotFound();
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
