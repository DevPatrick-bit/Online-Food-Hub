using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OnlineFoodHub.Data;
using OnlineFoodHub.Models;
using OnlineFoodHub.Models.ViewModels;
using System.Diagnostics;

namespace OnlineFoodHub.Controllers
{
    public class BaseController : Controller
    {
        public readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Sobrescribe el método View para agregar la cantidad de productos en el carrito al ViewBag
        public override ViewResult View(string viewName, object? model)
        {
            ViewBag.numeroProductos = GetCarritoCount();
            return base.View(viewName, model);
        }

        // Obtiene la cantidad de productos en el carrito desde las cookies
        protected int GetCarritoCount()
        {
            int count = 0;
            string? carritoJson = Request.Cookies["carrito"];

            if (!string.IsNullOrEmpty(carritoJson))
            {
                var carrito = JsonConvert.DeserializeObject<List<Models.ProductoIdAndCantidad>>(carritoJson);
                if (carrito != null)
                {
                    count = carrito.Count();
                }
            }
            return count;
        }

        // Agrega un producto al carrito y actualiza la cantidad si ya existe
        public async Task<CarritoViewModel> AgregarProductoAlCarrito(int productoId, int cantidad)
        {
            var producto = await _context.Productos.FindAsync(productoId);

            if (producto != null)
            {
                var carritoViewModel = await GetCarritoViewModelAsync();

                var carritoItem = carritoViewModel.Items.FirstOrDefault(item => item.ProductoId == productoId);

                if (carritoItem != null)
                    carritoItem.Cantidad += cantidad;
                else
                {
                    carritoViewModel.Items.Add(new CarritoItemViewModel
                    {
                        ProductoId = producto.ProductoId,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Cantidad = cantidad
                    });
                }

                carritoViewModel.Total = carritoViewModel.Items.Sum(item => item.Cantidad * item.Precio);

                await UpdateCarritoViewModelAsync(carritoViewModel);
                return carritoViewModel;
            }

            return new CarritoViewModel();
        }

        // Actualiza el carrito guardando los cambios en las cookies
        public async Task UpdateCarritoViewModelAsync(CarritoViewModel carritoViewModel)
        {
            var productoIds = carritoViewModel.Items
                .Select(item => new ProductoIdAndCantidad
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad
                })
                .ToList();

            var carritoJson = await Task.Run(() => JsonConvert.SerializeObject(productoIds));
            Response.Cookies.Append(
                "carrito",
                carritoJson,
                new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7) }
            );
        }

        // Recupera el estado actual del carrito desde las cookies y lo convierte en un CarritoViewModel
        public async Task<CarritoViewModel> GetCarritoViewModelAsync()
        {
            var carritoJson = Request.Cookies["carrito"];

            if (string.IsNullOrEmpty(carritoJson))
            {
                return new CarritoViewModel();
            }

            var productoIdsAndCantidades = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);
            var carritoViewModel = new CarritoViewModel();

            if (productoIdsAndCantidades != null)
            {
                foreach (var item in productoIdsAndCantidades)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto != null)
                    {
                        carritoViewModel.Items.Add(new CarritoItemViewModel
                        {
                            ProductoId = producto.ProductoId,
                            Nombre = producto.Nombre,
                            Precio = producto.Precio,
                            Cantidad = item.Cantidad
                        });
                    }
                }
            }

            carritoViewModel.Total = carritoViewModel.Items.Sum(item => item.Subtotal);
            return carritoViewModel;
        }

        // Maneja los errores generales y muestra la vista de error
        protected IActionResult HandleError(Exception e)
        {
            return View(
                "Error",
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }

        // Maneja errores específicos de la base de datos y muestra una vista personalizada
        protected IActionResult HandleDbError(Exception dbException)
        {
            var viewModel = new DbErrorViewModel
            {
                ErrorMessage = "Error de base de datos",
                Details = dbException.Message
            };
            return View("DbError", viewModel);
        }

        // Maneja errores de actualización de la base de datos y muestra una vista personalizada
        protected IActionResult HandleDbError(DbUpdateException dbUpdateException)
        {
            var viewModel = new DbErrorViewModel
            {
                ErrorMessage = "Error de actualización de base de datos",
                Details = dbUpdateException.Message
            };
            return View("DbError", viewModel);
        }
    }
}

