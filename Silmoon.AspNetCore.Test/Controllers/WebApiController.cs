using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.Drawing;
using Silmoon.Extension;
using Silmoon.Runtime.Cache;
using Silmoon.Collections;
using Silmoon.Secure;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Silmoon.AspNetCore.Test.Controllers
{
    public class WebApiController : Controller
    {
        Core Core { get; set; }
        ISilmoonAuthService SilmoonAuthService { get; set; }
        public WebApiController(Core core, ISilmoonAuthService silmoonAuthService)
        {
            Core = core;
            SilmoonAuthService = silmoonAuthService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateUser(string Username, string Password, string Repassword)
        {
            if (Username.IsNullOrEmpty() || Password.IsNullOrEmpty()) return (false, "用户名或密码为空").GetStateFlagResult();
            if (Password != Repassword) return (false, "两次密码不一致").GetStateFlagResult();
            var existUser = Core.GetUser(Username);
            if (existUser is null) return (false, "用户名已存在").GetStateFlagResult();
            User user = new User()
            {
                Username = Username,
                Password = EncryptHelper.RsaEncrypt(Password),
            };
            return this.JsonStateFlag(true, user);
        }

        [Authorize]
        public IActionResult DashboardApi()
        {
            var result = User.Identity.IsAuthenticated;
            return this.JsonStateFlag(true, $"You IsAuthenticated is {result}.", Data: result);
        }

        public IActionResult UploadTempImage(string UserId, string fileName)
        {
            if (fileName.IsNullOrEmpty()) fileName = HashHelper.RandomChars(32);
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");

            var image = new Bitmap(Request.Form.Files[0].OpenReadStream());
            var imageFormat = image.RawFormat;

            ImageHelper.FixiPhoneOrientation(image);
            var image2 = ImageHelper.ResizeWidth(image, 800, true, true);
            image.Dispose();
            var image3 = ImageHelper.Compress(image2, CompositingQuality.HighSpeed);
            image2.Dispose();
            var data = image3.GetBytes(imageFormat);
            image3.Dispose();

            if (files.Matched)
                files.Value[fileName] = data;
            else
                GlobalCaching<string, NameObjectCollection<byte[]>>.Set(UserId + ":temp_images", new NameObjectCollection<byte[]>() { { fileName, data } });

            return this.JsonStateFlag(true);
        }
        public IActionResult GetTempImageNames(string UserId)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");
            if (files.Matched)
            {
                return this.JsonStateFlag(true, Data: files.Value.GetAllKeys());
            }
            else
                return this.JsonStateFlag(true, Data: 0);
        }
        public IActionResult DeleteTempImage(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");
            if (files.Matched)
            {
                files.Value.Remove(fileName);
                return this.JsonStateFlag(true);
            }
            else
                return this.JsonStateFlag(false);
        }
        //[OutputCache(Duration = 3600)]
        public IActionResult ShowTempImage(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");
            if (files.Matched)
                return File(files.Value.Get(fileName) ?? Array.Empty<byte>(), "image/jpeg");
            else
                return new EmptyResult();
        }




        public IActionResult UploadFile(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");

            var data = Request.Form.Files[0].OpenReadStream().ToBytes();

            if (files.Matched)
                files.Value.Set(fileName.IsNullOrEmpty() ? Request.Form.Files[0].FileName : fileName, data);
            else
                GlobalCaching<string, NameObjectCollection<byte[]>>.Set(UserId + ":temp_files", new NameObjectCollection<byte[]>() { { fileName.IsNullOrEmpty() ? Request.Form.Files[0].FileName : fileName, data } });

            return this.JsonStateFlag(true);
        }
        public IActionResult GetTempFileNames(string UserId)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");
            if (files.Matched)
            {
                return this.JsonStateFlag(true, Data: files.Value.GetAllKeys());
            }
            else
                return this.JsonStateFlag(true, Data: 0);
        }
        public IActionResult DeleteTempFile(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");
            if (files.Matched)
            {
                files.Value.Remove(fileName);
                return this.JsonStateFlag(true);
            }
            else
                return this.JsonStateFlag(false);
        }
        public IActionResult ShowTempFile(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");
            new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

            if (files.Matched)
                return File(files.Value.Get(fileName) ?? Array.Empty<byte>(), contentType);
            else
                return new EmptyResult();
        }

    }
}
