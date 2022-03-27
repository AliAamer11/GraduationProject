using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.DataSeed
{
    public class DataInitilizer
    {
        public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }
        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = new string[] {"Admin", "Requester", "VicePris", "StoreKeep"};
            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    IdentityRole identityRole = new IdentityRole();
                    identityRole.Name = role;
                    IdentityResult roleResult = roleManager.CreateAsync(identityRole).Result;
                }
            }
           
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager)
        {
            if (userManager.FindByEmailAsync("admin@admin.com").Result == null)
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "admin@admin.com";
                user.Email = "admin@admin.com";
                user.RequstingParty = "SuperHeroAdmin";
                user.Type = "Admin";
                var result = userManager.CreateAsync(user, "201710803").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user,"Admin").Wait();
                }
            }
        }
    }
}
