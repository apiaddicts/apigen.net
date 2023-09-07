using Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Test.Api.Helpers
{
    public class HttpResponseExceptionFilterTest
    {
        [Fact]
        public void FilterBadRequest()
        {
            HttpResponseExceptionFilter addHeaderAttribute = new HttpResponseExceptionFilter();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");

            var actionContext = new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>(), modelState);

            var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), Mock.Of<Controller>());

            addHeaderAttribute.OnActionExecuting(actionExecutingContext);

            var result = (ObjectResult?)actionExecutingContext.Result;

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, result?.StatusCode);

        }

        [Fact]
        public void FilterException()
        {
            HttpResponseExceptionFilter addHeaderAttribute = new HttpResponseExceptionFilter();

            var actionContext = new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>(), new ModelStateDictionary());

            var actionExecutedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

            actionExecutedContext.Exception = new System.Exception();

            addHeaderAttribute.OnActionExecuted(actionExecutedContext);

            var result = (ObjectResult?)actionExecutedContext.Result;

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, result?.StatusCode);

        }
    }
}
