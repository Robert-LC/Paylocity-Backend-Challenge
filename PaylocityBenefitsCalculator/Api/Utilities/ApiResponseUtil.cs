using Api.Models;

namespace Api.Utilities
{
    /// <summary>
    /// Helper class with methods to streamline creating <see cref="ApiResponse{T}"./>
    /// The methods enforce that data cannot be null if success is true and
    /// you cannot have an error if success is true
    /// </summary>
    public static class ApiResponseUtil
    {
        public static ApiResponse<T> CreateResponse<T>(bool success, T? data = default, string? message = null, string? error = null)
        {
            if (success && data == null)
            {
                throw new ArgumentException("Data cannot be null when success is true.");
            }

            if (success && !string.IsNullOrEmpty(error))
            {
                throw new ArgumentException("Cannot have an error when success is true.");
            }

            return new ApiResponse<T>
            {
                Success = success,
                Data = data,
                Message = message ?? string.Empty,
                Error = error ?? string.Empty
            };
        }
    }
}
