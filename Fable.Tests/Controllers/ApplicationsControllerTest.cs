using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
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
            var data = new List<Application>
            {
                new Application
                {
                    ApplicationId = 10,
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
            var mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.Setup(cc => cc.HttpContext.User.Identity.Name).Returns(Guid.NewGuid().ToString());
            var controller = new ApplicationsController(mockDbContext.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.Accept(10);

            Assert.IsType<HttpNotFoundResult>(result);
        }
    }
}
