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
            Mock<DbSet<Application>> mockSet = AsyncMocker.WrapAsAsyncCompatible(data);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Applications).Returns(mockSet.Object);
            var controller = new ApplicationsController(mockContext.Object);

            var result = await controller.Accept(1);

            Assert.IsType<HttpNotFoundResult>(result);
        }
    }
}
