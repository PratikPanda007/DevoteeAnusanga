using Microsoft.AspNetCore.Mvc;

namespace DevoteeAnusanga.Controllers
{
    public class FilesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
