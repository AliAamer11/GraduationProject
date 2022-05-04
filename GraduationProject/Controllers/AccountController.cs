using GraduationProject.Data;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GraduationProject.Service;

namespace GraduationProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUserService userService;


        public AccountController(ApplicationDbContext context,
                                 UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 IUserService userService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userService = userService;
            _context = context;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            //this for check if we have user login so we dont return it to Login view 
            //inested of that we return him to view based on who is he
            //I use Sharingan *-*
            var userid = userService.GetUserId();
            if (userid!=null)
            {
                var user = await userManager.FindByIdAsync(userid);
                if (user!=null)
                {
                    if (user.Type == "Admin")
                    {
                        return RedirectToAction("DashBoard", "Administration");
                    }
                    else if (user.Type == "Requester")
                    {
                        return RedirectToAction("Home", "RPOrder");
                    }
                    else if (user.Type == "VicePris")
                    {
                        return RedirectToAction("Index", "VPOrder"); 
                    }
                    else if (user.Type == "StoreKeep")
                    {
                        return RedirectToAction("Index", "Items");
                    }
                }
            }
            //if he was not login return him to view for login
            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByEmailAsync(viewModel.Email);
                    if (user != null)
                    {
                        var result = await signInManager.PasswordSignInAsync(user.Email, viewModel.Password, true, false);
                        if (result.Succeeded)
                        {                   
                            if (user.Type == "Admin")
                            {
                                return RedirectToAction("DashBoard", "Administration");
                            }
                            else if (user.Type == "Requester")
                            {
                                return RedirectToAction("Home", "RPOrder");
                            }
                            else if (user.Type == "VicePris")
                            {
                                return RedirectToAction("Index", "AnnualOrder"); //o Be fixed
                            }
                            else if (user.Type == "StoreKeep")
                            {
                                return RedirectToAction("Index", "Items");
                            }
                        }
                        else
                        {
                            ViewBag.errorMessage = "Bad Password. Good Luck Next Time";
                            return View(viewModel);
                        }
                    }

                }
                return View(viewModel);

            }
            catch
            {

            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
