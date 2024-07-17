using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Test.Models;

namespace Silmoon.AspNetCore.Test.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult UploadImage()
        {
            User user = new User()
            {
                Username = "silmoon"
            };
            return View(user);
        }
        public IActionResult UploadPicture()
        {
            User user = new User()
            {
                Username = "silmoon"
            };
            return View(user);
        }
        public IActionResult UploadFile()
        {
            User user = new User()
            {
                Username = "silmoon"
            };
            return View(user);
        }
    }
}
