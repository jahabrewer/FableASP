using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Fable.Controllers;
using Fable.Models;
using Moq;
using Xunit;

namespace Fable.Tests.Controllers
{
    public class ApplicationsControllerTest
    {
        [Fact]
        public async Task Accept_IdDoesNotExistInContext_ReturnsHttpNotFoundResult()
        {
            var absenteeId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var applications = new List<Application>();
            Mock<ApplicationDbContext> mockDbContext;
            ApplicationsController controller = PerTestSetup(applications, absenteeId, out mockDbContext);

            var result = await controller.Accept(applicationId);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public async Task Accept_UserDoesNotOwnAbsence_ReturnsHttpNotFoundResult()
        {
            var absenteeId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var applications = new List<Application>
            {
                new Application
                {
                    ApplicationId = applicationId,
                    ApplicationState = ApplicationState.WaitingForDecision,
                    Absence = new Absence
                    {
                        AbsenceId = 1,
                        Absentee = new ApplicationUser
                        {
                            Id = absenteeId,
                        },
                        State = AbsenceState.Open,
                    }
                }
            };
            Mock<ApplicationDbContext> mockDbContext;
            ApplicationsController controller = PerTestSetup(applications, "bogus-user", out mockDbContext);

            var result = await controller.Accept(applicationId);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Theory]
        [InlineData(ApplicationState.Accepted)]
        [InlineData(ApplicationState.Retracted)]
        [InlineData(ApplicationState.Rejected)]
        public async Task Accept_ApplicationIsNotWaitingForDecision_ReturnsHttpForbidden(ApplicationState state)
        {
            var absenteeId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var applications = new List<Application>
            {
                new Application
                {
                    ApplicationId = applicationId,
                    ApplicationState = state,
                    Absence = new Absence
                    {
                        AbsenceId = 1,
                        Absentee = new ApplicationUser
                        {
                            Id = absenteeId,
                        },
                        State = AbsenceState.Open,
                    }
                }
            };
            Mock<ApplicationDbContext> mockDbContext;
            ApplicationsController controller = PerTestSetup(applications, absenteeId, out mockDbContext);

            var result = await controller.Accept(applicationId);

            Assert.IsType<HttpStatusCodeResult>(result);
            Assert.Equal(((HttpStatusCodeResult) result).StatusCode, (int) HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData(AbsenceState.New)]
        [InlineData(AbsenceState.Assigned)]
        [InlineData(AbsenceState.InProgress)]
        [InlineData(AbsenceState.Closed)]
        public async Task Accept_AbsenceIsNotOpen_ReturnsHttpForbidden(AbsenceState state)
        {
            var absenteeId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var applications = new List<Application>
            {
                new Application
                {
                    ApplicationId = applicationId,
                    ApplicationState = ApplicationState.WaitingForDecision,
                    Absence = new Absence
                    {
                        AbsenceId = 1,
                        Absentee = new ApplicationUser
                        {
                            Id = absenteeId,
                        },
                        State = state,
                    }
                }
            };
            Mock<ApplicationDbContext> mockDbContext;
            ApplicationsController controller = PerTestSetup(applications, absenteeId, out mockDbContext);

            var result = await controller.Accept(applicationId);

            Assert.IsType<HttpStatusCodeResult>(result);
            Assert.Equal(((HttpStatusCodeResult)result).StatusCode, (int)HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Accept_OnValidCall_ModifiesExpectedData()
        {
            var absenteeId = Guid.NewGuid().ToString();
            var applicantId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var absence = new Absence
            {
                AbsenceId = 1,
                Absentee = new ApplicationUser
                {
                    Id = absenteeId,
                },
                State = AbsenceState.Open,
                School = new School
                {
                    Name = "ABC School",
                    SchoolId = 7,
                    StreetAddress = "124 Main St"
                }
            };
            var acceptedApplication = new Application
            {
                ApplicationId = applicationId,
                ApplicationState = ApplicationState.WaitingForDecision,
                Absence = absence,
                Applicant = new ApplicationUser
                {
                    Id = applicantId,
                }
            };
            var anotherApplicationForSameAbsence = new Application
            {
                ApplicationId = applicationId + 1,
                ApplicationState = ApplicationState.WaitingForDecision,
                Absence = absence,
                Applicant = new ApplicationUser
                {
                    Id = "some-other-user",
                }
            };
            var applications = new List<Application>
            {
                acceptedApplication,
                anotherApplicationForSameAbsence,
            };
            Mock<ApplicationDbContext> mockDbContext;
            ApplicationsController controller = PerTestSetup(applications, absenteeId, out mockDbContext);

            await controller.Accept(applicationId);
            
            mockDbContext.Verify(c => c.SaveChangesAsync(), Times.Once);
            Assert.Equal(AbsenceState.Assigned, absence.State);
            Assert.Equal(applicantId, absence.Fulfiller.Id);
            Assert.Equal(ApplicationState.Accepted, acceptedApplication.ApplicationState);
            Assert.Equal(ApplicationState.Rejected, anotherApplicationForSameAbsence.ApplicationState);
        }

        private ApplicationsController PerTestSetup(
            IList<Application> applications,
            string currentUserId,
            out Mock<ApplicationDbContext> mockDbContext)
        {
            Mock<DbSet<Application>> mockApplicationSet =
                AsyncMocker.WrapAsAsyncCompatible(applications.AsQueryable(), app => app.ApplicationId);
            mockDbContext = new Mock<ApplicationDbContext> { CallBase = true };
            mockDbContext.Setup(c => c.Applications).Returns(mockApplicationSet.Object);
            var controller = new ApplicationsController(mockDbContext.Object);
            var mockControllerContext = CreateMockControllerContextWithUser(currentUserId);
            mockControllerContext.Setup(cc => cc.RouteData).Returns((RouteData)null);
            controller.ControllerContext = mockControllerContext.Object;

            return controller;
        }

        private Mock<ControllerContext> CreateMockControllerContextWithUser(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId)
            };
            var genericIdentity = new GenericIdentity("");
            genericIdentity.AddClaims(claims);
            var genericPrincipal = new GenericPrincipal(genericIdentity, new string[0]);
            var mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.Setup(cc => cc.HttpContext.User).Returns(genericPrincipal);

            return mockControllerContext;
        }
    }
}
