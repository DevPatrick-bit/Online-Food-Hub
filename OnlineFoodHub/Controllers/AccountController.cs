
using OnlineFoodHub.Data;
using OnlineFoodHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data.Common;
using System.Security.Claims;


namespace OnlineFoodHub.Controllers
{
    // Controlador para gestionar las operaciones relacionadas con la cuenta de usuario
    public class AccountController : BaseController
    {

        private const int MaxLoginAttempts = 3;
        public AccountController(ApplicationDbContext context) : base(context)
        { }

        // Acción para mostrar la vista de registro de usuario (permitida para todos)
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // Acción para registrar un nuevo usuario (permite la solicitud POST)
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            try
            {
                if (usuario != null)
                {
                    // Verifica si el nombre de usuario ya está en uso
                    if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
                    {
                        ModelState.AddModelError(nameof(usuario.NombreUsuario), "El nombre de usuario ya esta en uso.");
                        return View(usuario);
                    }

                    // Obtiene el rol de cliente
                    var clienteRol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");

                    if (clienteRol != null)
                    {
                        usuario.RolID = clienteRol.RolId;
                    }

                    // Crea una nueva dirección para el usuario
                    usuario.Direcciones = new List<Direccion>
                    {
                        new Direccion
                        {
                            Address = usuario.Direccion,
                        }
                    };

                    // Agrega el usuario a la base de datos y guarda los cambios
                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();

                    // Crea una identidad de usuario con autenticación de cookies
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuario.NombreUsuario));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Cliente"));

                    // Firma al usuario y crea un cookie de autenticación
                    await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity)
                    );

                    return RedirectToAction("Index", "Home");
                }

                return View(usuario);

            }
            catch (DbException dbException)
            {
                // Maneja errores de base de datos
                return HandleDbError(dbException);
            }
        }

        // Acción para mostrar la vista de inicio de sesión (permitida para todos)
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Redirige al dashboard si el usuario es Administrador o Staff, de lo contrario, redirige al Home
                if (User.IsInRole("Administrador") || User.IsInRole("Staff"))
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        // Acción para iniciar sesión (permite la solicitud POST)
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                // Busca al usuario con el nombre de usuario y contraseña proporcionados
                var user = await _context.Usuarios.FirstOrDefaultAsync(
                    u => u.NombreUsuario == username && u.Contrasenia == password);

                if (user != null)
                {
                    // Crea una identidad de usuario con autenticación de cookies
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.Name, username));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()));

                    // Obtiene el rol del usuario y añade la reclamación de rol a la identidad
                    var rol = await _context.Roles.FirstOrDefaultAsync(r => r.RolId == user.RolID);
                    if (rol != null)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, rol.Nombre));
                    }

                    // Firma al usuario y crea un cookie de autenticación
                    await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity)
                    );

                    // Redirige a Dashboard si el rol es Administrador o Staff, de lo contrario, redirige a Home
                    if (rol != null)
                    {
                        if (rol.Nombre == "Administrador" || rol.Nombre == "Staff")
                        {
                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                // Agrega un error de modelo si las credenciales son inválidas
                ModelState.AddModelError("LoginError", "Usuario o contraseña incorrectos. Inténtelo de nuevo.");

                // Puedes optar por redirigir a la misma vista con el mensaje de error
                // en lugar de usar solo 'return View();'
                TempData["LoginError"] = "Usuario o contraseña incorrectos. Inténtelo de nuevo.";
                return View();

            }
            catch (Exception e)
            {
                // Maneja errores generales
                return HandleError(e);
            }

        }

        // Acción para cerrar sesión y redirigir a la vista de inicio de sesión
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Acción para mostrar una vista de acceso denegado (permitida para todos)
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
