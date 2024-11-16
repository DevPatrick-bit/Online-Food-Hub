using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OnlineFoodHub.Data;
using OnlineFoodHub.Models;
using OnlineFoodHub.Models.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

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
                        carritoViewModel.Items.Remove(item);
                    }
                    else
                    {
                        item.Cantidad = Math.Min(item.Cantidad, producto.Stock);
                    }
                }
                else
                {
                    item.Cantidad = 0;
                }
            }
            carritoViewModel.Total = carritoViewModel.Items.Sum(item => item.Subtotal);

            var usuarioId = User.Identity.IsAuthenticated == true
                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0
                : 0;

            var direcciones = usuarioId > 0
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
        public async Task<IActionResult> EliminarCantidad(int id)
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

        [HttpPost]
        [Authorize]
        public IActionResult ProcederConCompra(decimal montoTotal, int direccionIdSeleccionada)
        {
            if (direccionIdSeleccionada <= 0)
            {
                ModelState.AddModelError("", "Debe seleccionar una dirección válida.");
                return RedirectToAction("Index");
            }

            var usuarioId = User.Identity?.IsAuthenticated == true
                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0
                : 0;

            if (usuarioId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Fecha = DateTime.UtcNow,
                Estado = "Pendiente",
                DireccionIdSeleccionada = direccionIdSeleccionada,
                Total = montoTotal
            };
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            var carritoJson = Request.Cookies["carrito"];
            var carritoItems = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);

            if (carritoItems != null)
            {
                foreach (var item in carritoItems)
                {
                    var producto = _context.Productos.Find(item.ProductoId);
                    if (producto != null && producto.Activo && producto.Stock >= item.Cantidad)
                    {
                        var detalle = new Detalle_Pedido
                        {
                            PedidoId = pedido.PedidoId,
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            Precio = producto.Precio
                        };
                        _context.DetallePedidos.Add(detalle);

                        producto.Stock -= item.Cantidad;
                    }
                }
            }

            _context.SaveChanges();

            Response.Cookies.Delete("carrito");

            return RedirectToAction("ConfirmacionPedido", new { id = pedido.PedidoId });
        }

        public IActionResult ConfirmacionPedido(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.DetallesPedido)
                .ThenInclude(dp => dp.Producto)
                .FirstOrDefault(p => p.PedidoId == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        [HttpPost]
        public IActionResult RealizarPago(int pedidoId)
        {
            var pedido = _context.Pedidos
                .Include(p => p.DetallesPedido)
                .ThenInclude(dp => dp.Producto)
                .FirstOrDefault(p => p.PedidoId == pedidoId);

            if (pedido == null)
            {
                return NotFound();
            }

            pedido.Estado = "Cancelado";
            _context.SaveChanges();

            return RedirectToAction("PagoCompletado", new { id = pedidoId });
        }

        public IActionResult PagoCompletado(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.DetallesPedido)
                .ThenInclude(dp => dp.Producto)
                .FirstOrDefault(p => p.PedidoId == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        [Authorize]
        public IActionResult PedidosPendientes()
        {
            var usuarioId = User.Identity.IsAuthenticated
                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0
                : 0;

            if (usuarioId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var pedidosPendientes = _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId && p.Estado == "Pendiente")
                .Include(p => p.DetallesPedido)
                .ThenInclude(dp => dp.Producto)
                .ToList();

            return View(pedidosPendientes);
        }
    }
}














///Error de carrito///

////Error de carrito
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
//using OnlineFoodHub.Data;
//using OnlineFoodHub.Models;
//using OnlineFoodHub.Models.ViewModels;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Linq;
//using System;

//namespace OnlineFoodHub.Controllers
//{
//    public class CarritoController : BaseController
//    {
//        public CarritoController(ApplicationDbContext context)
//         : base(context) { }

//        [AllowAnonymous]
//        public async Task<IActionResult> Index()
//        {
//            var carritoViewModel = await GetCarritoViewModelAsync();

//            foreach (var item in carritoViewModel.Items)
//            {
//                var producto = await _context.Productos.FindAsync(item.ProductoId);
//                if (producto != null)
//                {
//                    item.Producto = producto;

//                    if (!producto.Activo)
//                    {
//                        carritoViewModel.Items.Remove(item);
//                    }
//                    else
//                    {
//                        item.Cantidad = Math.Min(item.Cantidad, producto.Stock);
//                    }
//                }
//                else
//                {
//                    item.Cantidad = 0;
//                }
//            }
//            carritoViewModel.Total = carritoViewModel.Items.Sum(item => item.Subtotal);

//            var usuarioId = User.Identity.IsAuthenticated == true
//                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0
//                : 0;

//            if (usuarioId == 0)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var direcciones = usuarioId > 0
//                ? _context.Direcciones.Where(d => d.UsuarioId == usuarioId).ToList()
//                : new List<Models.Direccion>();

//            var procederConCompraViewModel = new ProcederConCompraViewModel
//            {
//                Carrito = carritoViewModel,
//                Direcciones = direcciones,
//            };

//            return View(procederConCompraViewModel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> ActualizarCantidad(int id, int cantidad)
//        {
//            var carritoViewModel = await GetCarritoViewModelAsync();
//            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == id);

//            if (carritoItem != null)
//            {
//                carritoItem.Cantidad = cantidad;
//                var producto = await _context.Productos.FindAsync(id);
//                if (producto != null && producto.Activo && producto.Stock > 0)
//                {
//                    carritoItem.Cantidad = Math.Min(cantidad, producto.Stock);
//                }
//                await UpdateCarritoViewModelAsync(carritoViewModel);
//            }

//            return RedirectToAction("Index", "Carrito");
//        }

//        [HttpPost]
//        public async Task<IActionResult> EliminarCantidad(int id)
//        {
//            var carritoViewModel = await GetCarritoViewModelAsync();
//            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == id);

//            if (carritoItem != null)
//            {
//                carritoViewModel.Items.Remove(carritoItem);
//                await UpdateCarritoViewModelAsync(carritoViewModel);
//            }
//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        public async Task<IActionResult> VaciarCarrito()
//        {
//            await RemoveCarritoViewModelAsync();
//            return RedirectToAction("Index");
//        }

//        private async Task RemoveCarritoViewModelAsync()
//        {
//            await Task.Run(() => Response.Cookies.Delete("carrito"));
//        }

//        [HttpPost]
//        public IActionResult ProcederConCompra(decimal montoTotal, int direccionIdSeleccionada)
//        {
//            if (direccionIdSeleccionada <= 0)
//            {
//                ModelState.AddModelError("", "Debe seleccionar una dirección válida.");
//                return RedirectToAction("Index");
//            }

//            var usuarioId = User.Identity?.IsAuthenticated == true
//                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0
//                : 0;

//            if (usuarioId == 0)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var pedido = new Pedido
//            {
//                UsuarioId = usuarioId,
//                Fecha = DateTime.UtcNow,
//                Estado = "Pendiente",
//                DireccionIdSeleccionada = direccionIdSeleccionada,
//                Total = montoTotal
//            };
//            _context.Pedidos.Add(pedido);
//            _context.SaveChanges();

//            var carritoJson = Request.Cookies["carrito"];
//            var carritoItems = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);

//            if (carritoItems != null)
//            {
//                foreach (var item in carritoItems)
//                {
//                    var producto = _context.Productos.Find(item.ProductoId);
//                    if (producto != null && producto.Activo && producto.Stock >= item.Cantidad)
//                    {
//                        var detalle = new Detalle_Pedido
//                        {
//                            PedidoId = pedido.PedidoId,
//                            ProductoId = item.ProductoId,
//                            Cantidad = item.Cantidad,
//                            Precio = producto.Precio
//                        };
//                        _context.DetallePedidos.Add(detalle);

//                        producto.Stock -= item.Cantidad;
//                    }
//                }
//            }

//            _context.SaveChanges();

//            Response.Cookies.Delete("carrito");

//            return RedirectToAction("ConfirmacionPedido", new { id = pedido.PedidoId });
//        }

//        public IActionResult ConfirmacionPedido(int id)
//        {
//            var pedido = _context.Pedidos
//                .Include(p => p.DetallesPedido)
//                .ThenInclude(dp => dp.Producto)
//                .FirstOrDefault(p => p.PedidoId == id);

//            if (pedido == null)
//            {
//                return NotFound();
//            }

//            return View(pedido);
//        }

//        [HttpPost]
//        public IActionResult RealizarPago(int pedidoId)
//        {
//            var pedido = _context.Pedidos
//                .Include(p => p.DetallesPedido)
//                .ThenInclude(dp => dp.Producto)
//                .FirstOrDefault(p => p.PedidoId == pedidoId);

//            if (pedido == null)
//            {
//                return NotFound();
//            }

//            pedido.Estado = "Cancelado";
//            _context.SaveChanges();

//            return RedirectToAction("PagoCompletado", new { id = pedidoId });
//        }

//        public IActionResult PagoCompletado(int id)
//        {
//            var pedido = _context.Pedidos
//                .Include(p => p.DetallesPedido)
//                .ThenInclude(dp => dp.Producto)
//                .FirstOrDefault(p => p.PedidoId == id);

//            if (pedido == null)
//            {
//                return NotFound();
//            }

//            return View(pedido);
//        }

//        [Authorize]
//        public IActionResult PedidosPendientes()
//        {
//            var usuarioId = User.Identity.IsAuthenticated
//                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0
//                : 0;

//            if (usuarioId == 0)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var pedidosPendientes = _context.Pedidos
//                .Where(p => p.UsuarioId == usuarioId && p.Estado == "Pendiente")
//                .Include(p => p.DetallesPedido)
//                .ThenInclude(dp => dp.Producto)
//                .ToList();

//            return View(pedidosPendientes);
//        }
//    }
//}
