using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
    public class ResponseHeaderActionFilter : IActionFilter
    {
        private readonly ILogger _logger;
        private readonly string key;
        private readonly string value;
        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value)
        {
            _logger = logger;
            this.key = key;
            this.value = value;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("ResponseHeaderActionFilter.OnActionExecuted method");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("ResponseHeaderActionFilter.OnActionExecuting method");
            context.HttpContext.Response.Headers[key] = value;
        }
    }
}
