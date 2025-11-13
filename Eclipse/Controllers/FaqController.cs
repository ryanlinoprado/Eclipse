using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers {
    public class FaqController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
