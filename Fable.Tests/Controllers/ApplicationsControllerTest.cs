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
            var data = new List<Application>().AsQueryable();
            Mock<DbSet<Application>> mockSet = AsyncMocker.WrapAsAsyncCompatible(data, app => app.ApplicationId);
            var mockDbContext = new Mock<ApplicationDbContext>();
            mockDbContext.Setup(c => c.Applications).Returns(mockSet.Object);
            var controller = new ApplicationsController(mockDbContext.Object);

            var result = await controller.Accept(1);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public async Task Accept_UserDoesNotOwnAbsence_ReturnsHttpNotFoundResult()
        {
            var userId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var data = new List<Application>
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
                            Id = userId,
                        },
                        State = AbsenceState.Open,
                    }
                }
            }.AsQueryable();
            Mock<DbSet<Application>> mockSet = AsyncMocker.WrapAsAsyncCompatible(data, app => app.ApplicationId);
            var mockDbContext = new Mock<ApplicationDbContext>();
            mockDbContext.Setup(c => c.Applications).Returns(mockSet.Object);
            var controller = new ApplicationsController(mockDbContext.Object);
            controller.ControllerContext = CreateMockControllerContextWithUser("bogus-user").Object;

            var result = await controller.Accept(applicationId);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Theory]
        [InlineData(ApplicationState.Accepted)]
        [InlineData(ApplicationState.Retracted)]
        [InlineData(ApplicationState.Rejected)]
        public async Task Accept_ApplicationIsNotWaitingForDecision_ReturnsHttpForbidden(ApplicationState state)
        {
            var userId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var data = new List<Application>
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
                            Id = userId,
                        },
                        State = AbsenceState.Open,
                    }
                }
            }.AsQueryable();
            Mock<DbSet<Application>> mockSet = AsyncMocker.WrapAsAsyncCompatible(data, app => app.ApplicationId);
            var mockDbContext = new Mock<ApplicationDbContext>();
            mockDbContext.Setup(c => c.Applications).Returns(mockSet.Object);
            var controller = new ApplicationsController(mockDbContext.Object);
            controller.ControllerContext = CreateMockControllerContextWithUser(userId).Object;

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
            var userId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var data = new List<Application>
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
                            Id = userId,
                        },
                        State = state,
                    }
                }
            }.AsQueryable();
            Mock<DbSet<Application>> mockSet = AsyncMocker.WrapAsAsyncCompatible(data, app => app.ApplicationId);
            var mockDbContext = new Mock<ApplicationDbContext>();
            mockDbContext.Setup(c => c.Applications).Returns(mockSet.Object);
            var controller = new ApplicationsController(mockDbContext.Object);
            controller.ControllerContext = CreateMockControllerContextWithUser(userId).Object;

            var result = await controller.Accept(applicationId);

            Assert.IsType<HttpStatusCodeResult>(result);
            Assert.Equal(((HttpStatusCodeResult)result).StatusCode, (int)HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Accept_OnValidCall_ModifiesExpectedData()
        {
            var userId = Guid.NewGuid().ToString();
            const int applicationId = 12;
            var absence = new Absence
            {
                AbsenceId = 1,
                Absentee = new ApplicationUser
                {
                    Id = userId,
                },
                State = AbsenceState.Open,
                School = new School
                {
                    Name = "ABC School",
                    SchoolId = 7,
                    StreetAddress = "124 Main St"
                }
            };
            var absences = new List<Absence> {absence}.AsQueryable();
            var applications = new List<Application>
            {
                new Application
                {
                    ApplicationId = applicationId,
                    ApplicationState = ApplicationState.WaitingForDecision,
                    Absence = absence,
                }
            }.AsQueryable();
            Mock<DbSet<Application>> mockApplicationSet =
                AsyncMocker.WrapAsAsyncCompatible(applications, app => app.ApplicationId);
            Mock<DbSet<Absence>> mockAbsenceSet =
                AsyncMocker.WrapAsAsyncCompatible(absences, ab => ab.AbsenceId);
            //var users = new List<ApplicationUser>
            //{
            //    new ApplicationUser
            //    {
            //        UserName = "fake@example.com",
            //        Id = "1",
            //    }
            //}.AsQueryable();
            //Mock<DbSet<ApplicationUser>> mockUserSet = AsyncMocker.WrapAsAsyncCompatible(users, u => u.Id);
            var mockDbContext = new Mock<ApplicationDbContext> {CallBase = true};
            mockDbContext.Setup(c => c.Applications).Returns(mockApplicationSet.Object);
            mockDbContext.Setup(c => c.Absences).Returns(mockAbsenceSet.Object);
            //mockDbContext.Setup(c => c.Entry(It.IsAny<Absence>()).Reference(ab => ab.School).LoadAsync()).Verifiable();
            //mockDbContext.Setup(c => c.Users).Returns(mockUserSet.Object);
            var controller = new ApplicationsController(mockDbContext.Object);
            var mockControllerContext = CreateMockControllerContextWithUser(userId);
            mockControllerContext.Setup(cc => cc.RouteData).Returns((RouteData) null);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.Accept(applicationId);

            mockDbContext.Verify(c => c.SaveChangesAsync(), Times.Once);
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
