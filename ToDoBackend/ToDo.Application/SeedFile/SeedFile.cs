using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.Constants;
using ToDo.Application.Options;
using ToDo.Domain.Models;

namespace ToDo.Application.SeedFile
{
    public static class SeedFile
    {

        public static async Task OnInitialiseAsync(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager , IOptions<AdminOptions> adminOptions)
        {

            await CreateRoleIfNotExistsAsync(roleManager, UserRoles.Admin);
            await CreateRoleIfNotExistsAsync(roleManager, UserRoles.User);

            var options = adminOptions.Value;
            var adminEmail = options.AdminEmail;
            var adminPassword = options.AdminPassword;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(newAdminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
                }
            }
        }

        private static async Task CreateRoleIfNotExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
