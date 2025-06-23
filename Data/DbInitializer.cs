using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using school_diary.Models;

namespace school_diary.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();

            var ctx     = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await ctx.Database.MigrateAsync();

            string[] roles = { "Admin", "Director", "Teacher", "Parent", "Student" };
            foreach (var role in roles)
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));

            const string adminEmail = "admin@school.bg";
            const string adminPass  = "Admin123!";

            if (await userMgr.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new User
                {
                    UserName = adminEmail,         
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    Role = Role.Admin,
                    EmailConfirmed = true
                };

                var result = await userMgr.CreateAsync(admin, adminPass);
                if (result.Succeeded)
                    await userMgr.AddToRoleAsync(admin, "Admin");
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($" Admin creation failed: {errors}");
                }
            }
            if (!ctx.Schools.Any())
            {
                ctx.Schools.Add(new School { Name = "School", Address = "Main Street 1" });
                await ctx.SaveChangesAsync();
            }

        }
    }
}