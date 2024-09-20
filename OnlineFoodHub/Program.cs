using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineFoodHub.Data;
using OnlineFoodHub.Services;

var builder = WebApplication.CreateBuilder(args); // Inicializa el constructor de la aplicación web

// Agrega servicios al contenedor.
builder.Services.AddControllersWithViews(); // Agrega soporte para controladores MVC con vistas

// Configura el contexto de la base de datos para usar SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura las políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "RequiredAdminOrStaff", // Nombre de la política
        policy => policy.RequireRole("Administrador", "Staff") // Roles requeridos para esta política
    );
});

// Configura la autenticación con cookies
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true; // Asegura que la cookie solo sea accesible por HTTP
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Establece el tiempo de expiración de la cookie
        options.LoginPath = "/Account/Login"; // Ruta de redirección para el login
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta de redirección para acceso denegado
    });

// Configura servicios personalizados
builder.Services.AddScoped<IproductoService, ProductoService>(); // Registra el servicio de producto
builder.Services.AddScoped<ICategoriaService, CategoriaService>(); // Registra el servicio de categoría

var app = builder.Build(); // Construye la aplicación

// Configura el pipeline de solicitud HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Configura el manejo de excepciones en producción
    app.UseHsts(); // Activa HSTS para mayor seguridad
}

app.UseHttpsRedirection(); // Redirige las solicitudes HTTP a HTTPS
app.UseStaticFiles(); // Sirve archivos estáticos (CSS, JS, imágenes, etc.)

app.UseRouting(); // Habilita el enrutamiento

app.UseAuthentication(); // Habilita la autenticación
app.UseAuthorization(); // Habilita la autorización

// Configura la ruta predeterminada para los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Ruta predeterminada con controlador Home y acción Index

app.Run(); // Ejecuta la aplicación
