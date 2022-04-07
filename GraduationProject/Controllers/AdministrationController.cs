using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.Administration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
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
        ///begin of all action related for roles
        ///
        ///
        ///

        //get all roles
        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        //create role post method
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IdentityRole identityRole = new()
                    {
                        Name = viewModel.RoleName
                    };

                    IdentityResult result = await roleManager.CreateAsync(identityRole);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles", "Administration");
                    }

                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            catch
            {
                throw;
            }
            return View(viewModel);
        }

        //view  EditRole/roleId
        [HttpGet]
        public async Task<IActionResult> EditRole(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }
            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
            };

            foreach (var user in userManager.Users)
            {
                // to show user in this role 
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.RequstingParty);
                }
            }
            return View(model);
        }

        //Edit role post method
        //here edit is just edit name of role 
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var role = await roleManager.FindByIdAsync(model.Id);
                    if (role == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        role.Name = model.RoleName;
                        var result = await roleManager.UpdateAsync(role);

                        if (result.Succeeded)
                        {
                            return RedirectToAction("ListRoles");
                        }
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }

                return View(model);
            }
            catch
            {
                throw;
            }
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
                        return RedirectToAction("EditRole", new { roleId = role.Id });
                }
            }

            return RedirectToAction("EditRole", "Administration", new { roleId = role.Id });

        }
        ///
        ///
        ///
        ///end of all action related for roles



        ///begin of all action related for users
        ///
        ///
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = _context.Users.ToList();
            return View(users);
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
                        userManager.AddToRoleAsync(user, viewModel.Type).Wait();
                        return RedirectToAction("ListUsers", "Administration");
                    }
                }
            }
            catch
            {
                throw;
            }
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
    }
}
