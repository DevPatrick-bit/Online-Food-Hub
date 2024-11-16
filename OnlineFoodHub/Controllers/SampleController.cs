using Microsoft.AspNetCore.Mvc;
using OnlineFoodHub.Data;

namespace OnlineFoodHub.Controllers
{
    public class SampleController : BaseController
    {
        public SampleController(ApplicationDbContext context)
          : base(context) { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
