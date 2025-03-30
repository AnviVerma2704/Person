using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ExceptionFilter
{
    public class HandleExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;
        public HandleExceptionFilter(ILogger<HandleExceptionFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "HandleExceptionFilter.OnException method");
        }
    }
}
