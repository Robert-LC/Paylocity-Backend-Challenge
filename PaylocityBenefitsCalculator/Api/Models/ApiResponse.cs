namespace Api.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    /// <summary>
    /// The user-friendly message to be displayed to the client
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// An error code that can be traced back internally to more detailed exception details
    /// </summary>
    public string Error { get; set; } = string.Empty;
}
