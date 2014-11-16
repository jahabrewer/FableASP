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

            const string userEmail = "ja@nz.com";
            const string userPassword = "glasslegstring";
            const string canCreateAbsenceRoleName = "canCreateAbsence";

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists(canCreateAbsenceRoleName))
            {
                roleManager.Create(new IdentityRole("canCreateAbsence"));
            }

            var user = new ApplicationUser
            {
                Email = userEmail,
                UserName = userEmail,
            };
            var result = userManager.Create(user, userPassword);

            if (result.Succeeded)
            {
                userManager.AddToRole(user.Id, canCreateAbsenceRoleName);
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

            #endregion

            //context.Absences.AddOrUpdate(a => a.Description,
            //    new Absence
            //    {

            //    });
        }
    }
}
