using Microsoft.AspNetCore.Mvc;
using OnlineFoodHub.Data;

namespace OnlineFoodHub.Controllers
{
    public class MenuController : BaseController
    {
        public MenuController(ApplicationDbContext context)
         : base(context) { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
