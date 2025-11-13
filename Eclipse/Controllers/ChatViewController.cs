// Controllers/ChatViewController.cs
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers {
    public class ChatViewController : Controller {
        [Route("chat")]
        public IActionResult Index() {

            return View();
        }
    }
}
