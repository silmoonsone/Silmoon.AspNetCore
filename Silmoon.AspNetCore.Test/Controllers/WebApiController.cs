using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Graphics.Extension;
using Silmoon.Extension;
using Silmoon.Runtime.Cache;
using Silmoon.Collections;
using Silmoon.Secure;
using System.Threading.Tasks;

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
            if (Username.IsNullOrEmpty() || Password.IsNullOrEmpty()) return (false, "用户名或密码为空").GetStateResultJson();
            if (Password != Repassword) return (false, "两次密码不一致").GetStateResultJson();
            var existUser = Core.GetUser(Username);
            if (existUser is null) return (false, "用户名已存在").GetStateResultJson();
            User user = new User()
            {
                Username = Username,
                Password = EncryptHelper.RsaEncrypt(Password),
            };
            return this.JsonStateResult(true, user);
        }

        [Authorize]
        public IActionResult DashboardApi()
        {
            var result = User.Identity.IsAuthenticated;
            return this.JsonStateResult(true, $"You IsAuthenticated is {result}.", data: result);
        }

        public async Task<IActionResult> UploadTempImage(string UserId, string fileName)
        {
            if (fileName.IsNullOrEmpty()) fileName = HashHelper.RandomChars(32);
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");
            var imageData = await Request.Form.Files[0].GetBytesAsync();
            using var image = imageData.GetSKImage();

            using var fixedImage = image.FixiPhoneOrientation();
            using var bitmap = fixedImage.ToSKBitmap();
            using var resizedBitmap = bitmap.ResizeWidth(800, true, true);
            using var resizedImage = resizedBitmap.ToSKImage();
            var compressedImage = resizedImage.Compress();
            var compressedImageData = compressedImage.GetBytes();

            if (files.Matched)
                files.Value[fileName] = compressedImageData;
            else
                GlobalCaching<string, NameObjectCollection<byte[]>>.Set(UserId + ":temp_images", new NameObjectCollection<byte[]>() { { fileName, compressedImageData } });

            return this.JsonStateResult(true);
        }
        public IActionResult GetTempImageNames(string UserId)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");
            if (files.Matched)
            {
                return this.JsonStateResult(true, data: files.Value.GetAllKeys());
            }
            else
                return this.JsonStateResult(true, data: 0);
        }
        public IActionResult DeleteTempImage(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_images");
            if (files.Matched)
            {
                files.Value.Remove(fileName);
                return this.JsonStateResult(true);
            }
            else
                return this.JsonStateResult(false);
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




        public async Task<IActionResult> UploadFile(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");

            var data = await Request.Form.Files[0].GetBytesAsync();

            if (files.Matched)
                files.Value.Set(fileName.IsNullOrEmpty() ? Request.Form.Files[0].FileName : fileName, data);
            else
                GlobalCaching<string, NameObjectCollection<byte[]>>.Set(UserId + ":temp_files", new NameObjectCollection<byte[]>() { { fileName.IsNullOrEmpty() ? Request.Form.Files[0].FileName : fileName, data } });

            return this.JsonStateResult(true);
        }
        public IActionResult GetTempFileNames(string UserId)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");
            if (files.Matched)
            {
                return this.JsonStateResult(true, data: files.Value.GetAllKeys());
            }
            else
                return this.JsonStateResult(true, data: 0);
        }
        public IActionResult DeleteTempFile(string UserId, string fileName)
        {
            var files = GlobalCaching<string, NameObjectCollection<byte[]>>.Get(UserId + ":temp_files");
            if (files.Matched)
            {
                files.Value.Remove(fileName);
                return this.JsonStateResult(true);
            }
            else
                return this.JsonStateResult(false);
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
