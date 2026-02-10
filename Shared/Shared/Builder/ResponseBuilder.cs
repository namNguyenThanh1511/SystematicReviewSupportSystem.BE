using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Models;

namespace Shared.Builder
{
    public static class ResponseBuilder
    {
        // Success responses
        public static ApiResponse<T> SuccessWithData<T>(T data, string message = "Thao tác thành công")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse SuccessWithMessage(string message = "Thao tác thành công")
        {
            return new ApiResponse
            {
                IsSuccess = true,
                Message = message,
            };
        }

        // Error responses
        public static ApiResponse<T> Error<T>(string message, List<ApiError>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResponse Error(string message, List<ApiError>? errors = null)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = errors
            };
        }

        // Specific error responses
        public static ApiResponse<T> BadRequest<T>(string message = "Yêu cầu không hợp lệ", List<ApiError>? errors = null)
        {
            return Error<T>(message, errors);
        }

        public static ApiResponse BadRequest(string message = "Yêu cầu không hợp lệ", List<ApiError>? errors = null)
        {
            return Error(message, errors);
        }

        public static ApiResponse<T> NotFound<T>(string message = "Không tìm thấy dữ liệu", List<ApiError>? errors = null)
        {
            return NotFound<T>(message, errors);
        }

        public static ApiResponse NotFound(string message = "Không tìm thấy dữ liệu")
        {
            return Error(message);
        }

        public static ApiResponse<T> Unauthorized<T>(string message = "Không có quyền truy cập")
        {
            return Error<T>(message);
        }

        public static ApiResponse Unauthorized(string message = "Không có quyền truy cập")
        {
            return Error(message);
        }

        public static ApiResponse<T> Forbidden<T>(string message = "Bị cấm truy cập")
        {
            return Error<T>(message);
        }

        public static ApiResponse Forbidden(string message = "Bị cấm truy cập")
        {
            return Error(message);
        }

        public static ApiResponse<T> InternalServerError<T>(string message = "Lỗi máy chủ nội bộ")
        {
            return Error<T>(message);
        }

        public static ApiResponse InternalServerError(string message = "Lỗi máy chủ nội bộ")
        {
            return Error(message);
        }

        public static ApiResponse<T> Conflict<T>(string message = "Xung đột dữ liệu")
        {
            return Error<T>(message);
        }
        public static ApiResponse Conflict(string message = "Xung đột dữ liệu", List<ApiError>? errors = null)
        {
            return Error(message, errors);
        }


        // Validation error response
        public static ApiResponse ValidationError(ModelStateDictionary modelState, string message = "Dữ liệu không hợp lệ")
        {
            var errors = new List<ApiError>();

            foreach (var (field, state) in modelState)
            {
                foreach (var error in state.Errors)
                {
                    errors.Add(new ApiError
                    {
                        Code = field,
                        Message = error.ErrorMessage,
                    });
                }
            }

            return BadRequest(message, errors);
        }

    }
}
