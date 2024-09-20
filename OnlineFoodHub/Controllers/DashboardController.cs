using Microsoft.AspNetCore.Mvc;
using OnlineFoodHub.Data;

namespace OnlineFoodHub.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(ApplicationDbContext context)
         : base(context) { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
