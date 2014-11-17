using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Fable.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Fable.Controllers
{
    public class AbsencesController : Controller
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public AbsencesController()
        {
            ApplicationDbContext = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }

        // GET: Absences
        public ActionResult Index()
        {
            return View(ApplicationDbContext.Absences.ToList());
        }

        // GET: Absences/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Absence absence = ApplicationDbContext.Absences.Find(id);
            if (absence == null)
            {
                return HttpNotFound();
            }
            return View(absence);
        }

        // GET: Absences/Create
        [Authorize(Roles = "canCreateAbsence")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Absences/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "canCreateAbsence")]
        public ActionResult Create([Bind(Include = "AbsenceId,Location,Start,End,Description")] Absence absence)
        {
            if (ModelState.IsValid)
            {
                ApplicationDbContext.Absences.Add(absence);
                ApplicationDbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(absence);
        }

        // GET: Absences/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Absence absence = ApplicationDbContext.Absences.Find(id);
            if (absence == null)
            {
                return HttpNotFound();
            }
            return View(absence);
        }

        // POST: Absences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AbsenceId,Location,Start,End,Description")] Absence absence)
        {
            if (ModelState.IsValid)
            {
                ApplicationDbContext.Entry(absence).State = EntityState.Modified;
                ApplicationDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(absence);
        }

        // GET: Absences/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Absence absence = ApplicationDbContext.Absences.Find(id);
            if (absence == null)
            {
                return HttpNotFound();
            }
            return View(absence);
        }

        // POST: Absences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Absence absence = ApplicationDbContext.Absences.Find(id);
            ApplicationDbContext.Absences.Remove(absence);
            ApplicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Absences/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "canCreateApplication")]
        public async Task<ActionResult> Apply(int absenceId)
        {
            if (ModelState.IsValid)
            {
                var absence = await ApplicationDbContext.Absences.FindAsync(absenceId);
                if (absence == null)
                {
                    return HttpNotFound();
                }
                var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (await ApplicationDbContext.Applications.AnyAsync(a => a.Applicant.Id == currentUser.Id && a.Absence.AbsenceId == absence.AbsenceId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                        "An Application for the specified Absence for the logged in User already exists");
                }
                ApplicationDbContext.Applications.Add(new Application
                {
                    Absence = absence,
                    ApplicationState = ApplicationState.WaitingForDecision,
                    ApplicationStateModified = DateTime.UtcNow,
                    Applicant = currentUser,
                });
                await ApplicationDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
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
