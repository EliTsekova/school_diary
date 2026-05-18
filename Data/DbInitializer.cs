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
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));
            }

            const string adminEmail = "admin@school.bg";
            const string adminPass  = "Admin123!";

            var existingAdmin = await userMgr.FindByEmailAsync(adminEmail);

            if (existingAdmin is null)
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
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Admin creation failed: {errors}");
                }

                
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                
                var rolesOfAdmin = await userMgr.GetRolesAsync(existingAdmin);
                if (!rolesOfAdmin.Contains("Admin"))
                    await userMgr.AddToRoleAsync(existingAdmin, "Admin");
            }

            
            if (!ctx.Schools.Any())
            {
                ctx.Schools.Add(new School { Name = "School", Address = "Main Street 1" });
                await ctx.SaveChangesAsync();
            }
           

            var teachers = await ctx.Teachers.Include(t => t.User).ToListAsync();
            foreach (var teacher in teachers)
            {
                if (teacher.User != null && !await userMgr.IsInRoleAsync(teacher.User, "Teacher"))
                    await userMgr.AddToRoleAsync(teacher.User, "Teacher");
            }

            var students = await ctx.Students.Include(s => s.User).ToListAsync();
            foreach (var student in students)
            {
                if (student.User != null && !await userMgr.IsInRoleAsync(student.User, "Student"))
                    await userMgr.AddToRoleAsync(student.User, "Student");
            }

            var parents = await ctx.Parents.Include(p => p.User).ToListAsync();
            foreach (var parent in parents)
            {
                if (parent.User != null && !await userMgr.IsInRoleAsync(parent.User, "Parent"))
                    await userMgr.AddToRoleAsync(parent.User, "Parent");
            }

            var directors = await ctx.Directors.Include(d => d.User).ToListAsync();
            foreach (var director in directors)
            {
                if (director.User != null && !await userMgr.IsInRoleAsync(director.User, "Director"))
                    await userMgr.AddToRoleAsync(director.User, "Director");
            }
        }
    }
}
