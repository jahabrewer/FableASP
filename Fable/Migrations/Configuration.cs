using System;
using System.Linq;
using Fable.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Fable.Migrations
{
    using Microsoft.AspNet.Identity;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Fable.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            #region Seed users and roles

            const string adminEmail = "ava@example.com";
            const string teacherEmail = "tess@example.com";
            const string substitute1Email = "steve@example.com";
            const string substitute2Email = "sally@example.com";

            var users = new[]
            {
                new {Email = adminEmail, Password = "avaForAdmin", Roles = Roles.AdminRoles},
                new {Email = teacherEmail, Password = "tessForTeacher", Roles = Roles.TeacherRoles},
                new {Email = substitute1Email, Password = "steveForSubstitute", Roles = Roles.SubstituteRoles},
                new {Email = substitute2Email, Password = "sallyForSubstitute", Roles = Roles.SubstituteRoles},
            };

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            foreach (var role in Roles.AllRoles.Where(role => !roleManager.RoleExists(role)))
            {
                roleManager.Create(new IdentityRole(role));
            }

            foreach (var user in users)
            {
                var applicationUser = new ApplicationUser
                {
                    Email = user.Email,
                    UserName = user.Email,
                };
                var result = userManager.Create(applicationUser, user.Password);
                if (result.Succeeded)
                {
                    foreach (var role in user.Roles)
                    {
                        userManager.AddToRole(applicationUser.Id, role);
                    }
                }
            }

            #endregion

            #region Seed schools

            context.Schools.AddOrUpdate(s => s.Name,
                new School
                {
                    Name = "Albert Brenner Primary",
                    StreetAddress = "101 8th St",
                },
                new School
                {
                    Name = "Charles Dickens Elementary",
                    StreetAddress = "750 15th Ave",
                },
                new School
                {
                    Name = "Elizabeth Freeman Middle",
                    StreetAddress = "800 2nd St",
                });

            context.SaveChanges();

            #endregion

            #region Seed absences

            context.Absences.AddOrUpdate(a => a.Description,
                new Absence
                {
                    State = AbsenceState.Open,
                    Absentee = context.Users.First(u => u.Email == teacherEmail),
                    Start = new DateTime(2015, 12, 16, 8, 0, 0),
                    End = new DateTime(2015, 12, 16, 15, 0, 0),
                    School = context.Schools.First(),
                    Location = "Room 123",
                    Description = "do some stuff",
                });

            #endregion
        }
    }
}
