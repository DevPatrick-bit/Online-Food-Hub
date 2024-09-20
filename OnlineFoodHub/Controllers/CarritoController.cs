using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodHub.Data;
using OnlineFoodHub.Models.ViewModels;
using System.Security.Claims;

namespace OnlineFoodHub.Controllers
{
    public class CarritoController : BaseController
    {
        public CarritoController(ApplicationDbContext context)
         : base(context) { }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var carritoViewModel = await GetCarritoViewModelAsync();

            foreach (var item in carritoViewModel.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto != null)
                {
                    item.Producto = producto;

                    if (!producto.Activo)
                    {
                        item.Cantidad = 0;
                    }
                    else
                        item.Cantidad = Math.Min(item.Cantidad, producto.Stock);
                    if (item.Cantidad == 0)
                    {
                        item.Cantidad = 1;
                    }
                }
                else
                {
                    item.Cantidad = 0;
                }
            }

            var usuarioId = User.Identity.IsAuthenticated == true
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                : 0;

            var direcciones =
                User.Identity.IsAuthenticated == true
                ? _context.Direcciones.Where(d => d.UsuarioId == usuarioId).ToList()
                : new List<Models.Direccion>();

            var procederConCompraViewModel = new ProcederConCompraViewModel
            {
                Carrito = carritoViewModel,
                Direcciones = direcciones,
            };

            return View(procederConCompraViewModel);
        }

        [HttpPost]

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int id, int cantidad)
        {
            var carritoViewModel = await GetCarritoViewModelAsync();
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == id);

            if (carritoItem != null)
            {
                carritoItem.Cantidad = cantidad;
                var producto = await _context.Productos.FindAsync(id);
                if (producto != null && producto.Activo && producto.Stock > 0)
                {
                    carritoItem.Cantidad = Math.Min(cantidad, producto.Stock);
                }
                await UpdateCarritoViewModelAsync(carritoViewModel);
            }

            return RedirectToAction("Index", "Carrito");
        }



        [HttpPost]

        public async Task<IActionResult> EliminarCantidad(int id, int cantidad)
        {
            var carritoViewModel = await GetCarritoViewModelAsync();
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == id);

            if (carritoItem != null)
            {
                carritoViewModel.Items.Remove(carritoItem);
                await UpdateCarritoViewModelAsync(carritoViewModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> VaciarCarrito()
        {
            await RemoveCarritoViewModelAsync();
            return RedirectToAction("Index");
        }

        private async Task RemoveCarritoViewModelAsync()
        {
            await Task.Run(() => Response.Cookies.Delete("carrito"));
        }
    }
}
