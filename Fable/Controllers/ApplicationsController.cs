using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Fable.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

namespace Fable.Controllers
{
    public class ApplicationsController : Controller
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public ApplicationsController() : this(new ApplicationDbContext())
        {
        }

        public ApplicationsController(ApplicationDbContext context)
        {
            ApplicationDbContext = context;
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        }

        // POST: Applications/Accept
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Accept(int applicationId)
        {
            var application = await ApplicationDbContext.Applications.FindAsync(applicationId);
            if (application == null)
            {
                return HttpNotFound();
            }

            // verify that this user owns the absence that was applied for
            if (application.Absence.Absentee.Id != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            // verify that the application is in a valid state for acceptance
            if (application.ApplicationState != ApplicationState.WaitingForDecision)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                    "Application is not in a valid state to be Accepted");
            }

            // verify that the absence is open
            if (application.Absence.State != AbsenceState.Open)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                    "Absence is not in valid state to have an Application Retracted");
            }

            // set the applicant as fulfiller on the absence and transition absence to assigned state
            application.Absence.Fulfiller = application.Applicant;
            application.Absence.State = AbsenceState.Assigned;

            // accept this application and reject all other applications
            ApplicationDbContext.Applications
                .Where(app =>
                    app.Absence.AbsenceId == application.Absence.AbsenceId
                    && app.ApplicationState == ApplicationState.WaitingForDecision)
                .ForEach(app =>
                {
                    app.ApplicationState = app.ApplicationId == application.ApplicationId
                        ? ApplicationState.Accepted
                        : ApplicationState.Rejected;
                    app.ApplicationStateModified = DateTime.UtcNow;
                });

            await ApplicationDbContext.SaveChangesAsync();
            return RedirectToAction("Details", "Absences", new {id = application.Absence.AbsenceId});
        }

        // POST: Applications/Retract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Retract(int applicationId)
        {
            var application = await ApplicationDbContext.Applications.FindAsync(applicationId);
            if (application == null)
            {
                return HttpNotFound();
            }

            // verify that this user owns the application
            if (application.Applicant.Id != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            // verify that the application is in a valid state for retracting
            if (application.ApplicationState != ApplicationState.WaitingForDecision)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                    "Application is not in a valid state to be Retracted");
            }

            // verify that the absence is open
            if (application.Absence.State != AbsenceState.Open)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                    "Absence is not in valid state to have an Application Retracted");
            }

            // retract the application
            application.ApplicationState = ApplicationState.Retracted;
            application.ApplicationStateModified = DateTime.UtcNow;

            await ApplicationDbContext.SaveChangesAsync();
            return RedirectToAction("Details", "Absences", new { id = application.Absence.AbsenceId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ApplicationDbContext.Dispose();
                UserManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
