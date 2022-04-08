using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdministrationController(ApplicationDbContext context,
                                        UserManager<ApplicationUser> userManager,
                                        RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult DashBoard()
        {
            DashBoardViewModel viewModel = new DashBoardViewModel();

            //this for get count of User in System 
            int countforAllUser = _context.Users.Count();

            //this for get count of roles in system
            int countforAllRoles = roleManager.Roles.Count();

            //this to get list of Users 
            ViewBag.users = _context.Users.ToList();

            //this to get list of roles 
            ViewBag.roles = roleManager.Roles;

            viewModel.CountforAllUser = countforAllUser;
            viewModel.CountforAllRoles = countforAllRoles;
            return View(viewModel);
        }

        // for edit user in role 
        // this meaning add user to role or remover user to role
        // but this to show all user and who id member in this role and who is not
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound();
            }
            var model = new List<UserRoleViewModel>();

            foreach (var user in userManager.Users)
            {
                var userRoleViewMModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.RequstingParty,
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewMModel.IsSelected = true;
                }
                else
                {
                    userRoleViewMModel.IsSelected = false;
                }
                model.Add(userRoleViewMModel);
            }
            return View(model);
        }

        // to add user to role or remove him 
        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);
                IdentityResult result = null;

                //user is selected and he is not in role 
                //add him to role
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                // user not selected and he is already in member in this role 
                // Remove him from role
                else if (!(model[i].IsSelected) && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                // user is selected and he is already in role do nothing
                // user is not selected and he is not member in role do nothing
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    // we have more user to process
                    if (i < model.Count - 1)
                        continue;
                    else
                        return RedirectToAction("DashBoard", "Administration");
                }
            }

            return RedirectToAction("DashBoard", "Administration");

        }
        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewData["Type"] = new SelectList(bindListforType(), "Value", "Text");
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel viewModel)
        {
            ViewData["Type"] = new SelectList(bindListforType(), "Value", "Text");
            try
            {
                if (ModelState.IsValid)
                {
                    if (checkForEmail(viewModel.Email))
                    {
                        ViewBag.errorMassage = "هذا البريد الالكتروني موجود بالفعل حاول مجددا";
                        return View(viewModel);
                    }
                    var user = new ApplicationUser()
                    {
                        UserName = viewModel.Email,
                        Email = viewModel.Email,
                        RequstingParty = viewModel.RequestingParty,
                        Type = viewModel.Type,
                    };
                    var result = await userManager.CreateAsync(user, viewModel.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, viewModel.Type);
                        return RedirectToAction("DashBoard", "Administration");
                    }
                }
            }
            catch
            {
                throw;
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var editUserViewModel = new EditUserViewModel()
                {
                    UserID = user.Id,
                    RequestingParty = user.RequstingParty,
                    Email = user.Email,
                    Type = user.Type,
                };
                ViewData["Type"] = new SelectList(bindListforType(), "Value", "Text");
                return View(editUserViewModel);
            }
            catch
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel viewModel, string id)
        {
            if (id != viewModel.UserID)
            {
                return NotFound();
            }
            ViewData["Type"] = new SelectList(bindListforType(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    if (checkForEmail(viewModel.Email, viewModel.UserID))
                    {
                        ViewBag.errorMassage = "هذا البريد موجود بالفعل لمستخدم آخر";
                        return View(viewModel);
                    }
                    ApplicationUser user = await userManager.FindByIdAsync(viewModel.UserID);
                    user.Email = viewModel.Email;
                    user.RequstingParty = viewModel.RequestingParty;
                    user.Type = viewModel.Type;
                    user.UserName = user.Email;
                    await userManager.UpdateAsync(user);
                    return RedirectToAction("DashBoard", "Administration");
                }
                catch
                {
                    throw;
                }
            }
            ModelState.AddModelError("", "زبط حقولك");
            return View(viewModel);

        }
        ///
        ///
        ///
        ///end of all action related for users




        private List<SelectListItem> bindListforType()
        {
            List<SelectListItem> list = new();
            list.Add(new SelectListItem { Text = "مدير التطبيق", Value = "Admin" });
            list.Add(new SelectListItem { Text = "الجهة الطالبة", Value = "Requester" });
            list.Add(new SelectListItem { Text = "نائب رئيس الجامعة", Value = "VicePris" });
            list.Add(new SelectListItem { Text = "أمين المستودع", Value = "StoreKeep" });

            return list;
        }

        /// <summary>
        /// this for not duplicate the email for any user
        /// this for create action
        /// </summary>
        /// <param name="email"></param>
        /// <returns> true if duplacate email false if not </returns>
        private bool checkForEmail(string email) => _context.Users.Any(t => t.Email == email);

        /// <summary>
        /// this for not duplicate the email for any user
        /// this for Update action
        /// </summary>
        /// <param name="email"></param>
        /// <returns> true if duplacate email false if not </returns>
        private bool checkForEmail(string email, string id) => _context.Users.Any(t => t.Email == email && t.Id != id);

    }
}
