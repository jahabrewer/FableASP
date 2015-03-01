using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Fable.Controllers;
using Fable.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fable.Tests.Controllers
{
    [TestClass]
    public class ApplicationsControllerTest
    {
        [TestMethod]
        public async Task Accept_IdDoesNotExistInContext_ReturnsHttpNotFoundResult()
        {
            var data = new List<Application>().AsQueryable();
            var mockSet = AsyncMocker.WrapAsAsyncCompatible(data);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Applications).Returns(mockSet.Object);
            var controller = new ApplicationsController(mockContext.Object);

            var result = await controller.Accept(1);

            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
        }
    }
}
