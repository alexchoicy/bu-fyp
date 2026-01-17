using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Data.Seed
{
    public class UserSeed
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {

            // Seed Admin User
            string adminEmail = "admin@bu.edu";
            User? adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    Name = "Admin GorGor",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "admin");

                Console.WriteLine(result);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
                }
            }

            // Seed Student User
            string studentEmail = "student@bu.edu";
            User? studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new User
                {
                    //This user year 2 student
                    UserName = "student",
                    Email = studentEmail,
                    Name = "Student A",
                    EmailConfirmed = true,
                    EntryAcedmicYear = 2024,
                    EntryYear = 1
                };

                var result = await userManager.CreateAsync(studentUser, "student");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, Roles.Student.ToString());
                }
            }
        }

    }
}