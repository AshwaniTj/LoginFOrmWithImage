using LoginFOrmWithImage.Data;
using LoginFOrmWithImage.Models;
using LoginFOrmWithImage.ViewModel;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LoginFOrmWithImage.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public UserController(ApplicationContext context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var items = _context.Users.ToList();
            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            var hashsalt = EncryptPassword(user.Password);
            user.Password = hashsalt.Hash;
            user.StoredSalt = hashsalt.Salt;
            _context.Users.Add(user);
            _context.SaveChanges();
            return View();
        }

        [HttpPost]
        public IActionResult Login(User loginUser)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == loginUser.UserName);
            var isPasswordMatched = VerifyPassword(loginUser.Password, user.StoredSalt, user.Password);
            if (isPasswordMatched)
            {
                ViewBag.message = "Login successfull";
            }
            else
            {
                //Login Failed
                ViewBag.message = "Login Fail";
            }

            return View();
        }
        public HashSalt EncryptPassword(string password)
        {
            byte[] salt = new byte[128 / 8]; // Generate a 128-bit salt using a secure PRNG
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string encryptedPassw = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));
            return new HashSalt { Hash = encryptedPassw, Salt = salt };
        }

        public bool VerifyPassword(string enteredPassword, byte[] salt, string storedPassword)
        {
            string encryptedPassw = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));
            return encryptedPassw == storedPassword;
        }


        [HttpPost]
        public IActionResult Create(UserViewModel vm)
        {
            string stringFileName = UploadFile(vm);
            var user = new User
            {
                UserName = vm.UserName,
                Password = vm.Password,
                Image = stringFileName
            };
            var hashsalt = EncryptPassword(user.Password);
            user.Password = hashsalt.Hash;
            user.StoredSalt = hashsalt.Salt;
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        private string UploadFile(UserViewModel vm)
        {
            string fileName = null;
            if (vm.Image != null)
            {
                string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "Images");
                fileName = Guid.NewGuid().ToString() + "-" + vm.Image.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    vm.Image.CopyTo(fileStream);
                }
            }
            return fileName;
        }
    }
}
