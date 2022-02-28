using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetNote.Models;
using NetNote.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetNote.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<NoteUser> UserManager { get; }
        public SignInManager<NoteUser> SignInManager { get; }
        private readonly ILogger<AccountController> _logger;
        public AccountController(UserManager<NoteUser> userManager,SignInManager<NoteUser> signInManager,ILogger<AccountController> logger)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Login(string resutnUrl=null)
        {
            ViewBag.ReturnUrl = resutnUrl;
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model,string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Logged in {model.UserName}");
                return RedirectToAction("Index", "Note");
            }
            else
            {
                _logger.LogWarning($"Failed to log in {model.UserName}");
                ModelState.AddModelError("", "用户名或密码错误");
                return View(model);
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new NoteUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.UserName} was created.");
                    return RedirectToAction("Login");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            var userName = HttpContext.User.Identity.Name;
            await SignInManager.SignOutAsync();

            _logger.LogInformation($"{userName} logged out.");
            return RedirectToAction("Index", "Home");
        }
    }
}
