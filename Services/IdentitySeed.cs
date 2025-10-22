using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StowawayStorage.Services
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var roles = new[] { "Admin" };
            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
            }

            var adminEmail = config["Admin:Email"];
            var adminPwd = config["Admin:Password"];

            if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPwd))
            {
                var user = await userMgr.FindByEmailAsync(adminEmail);
                if (user == null)
                {
                    user = new IdentityUser { Email = adminEmail, UserName = adminEmail, EmailConfirmed = true };
                    var res = await userMgr.CreateAsync(user, adminPwd);
                    if (res.Succeeded)
                    {
                        await userMgr.AddToRoleAsync(user, "Admin");
                    }
                }
            }
        }
    }
}
