using Application.Common.Exceptions;
using Application.Common.Models;
using MediatR;

namespace Application.Common.Behaviors
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (ValidationException ex)
            {
                return CreateErrorResponse(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return CreateErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An error occurred: {ex.Message}");
            }
        }

        private TResponse CreateErrorResponse(string message)
        {
            var responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(BaseResponse<>))
            {
                var dataType = responseType.GetGenericArguments()[0];
                var baseResponseType = typeof(BaseResponse<>).MakeGenericType(dataType);
                var failureMethod = baseResponseType.GetMethod("FailureResponse");

                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object[] { message });
                    return result as TResponse;
                }
            }

            return default!;
        }
    }
}
