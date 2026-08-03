using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;


namespace Todo.Api.Common.Exceptions
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ValidationException validationException)
            {
                return false;
            }

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path // Cho Frontend biết API nào đang bị lỗi
            };

            var errors = validationException.Errors
            .Select(e => new
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            });

            problemDetails.Extensions.Add("errors", errors);

            // 4. TRẢ KẾT QUẢ VỀ CHO FRONTEND
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // Trả về true để báo cáo: "Lỗi này tui đã xử lý êm đẹp rồi, hệ thống không cần crash đâu!"
            return true;
        }
    }
}
