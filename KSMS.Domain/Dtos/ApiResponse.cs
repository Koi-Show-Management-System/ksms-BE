namespace KSMS.Domain.Dtos
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        
        public ApiResponse(T? data, int statusCode, string message)
        {
            Data = data;
            StatusCode = statusCode;
            Message = message;
        }

        public static ApiResponse<T> Success(T? data, string message = "Success")
        {
            return new ApiResponse<T>(data, 200, message);
        }

        public static ApiResponse<T> Fail(string message, int statusCode = 400)
        {
            return new ApiResponse<T>(default, statusCode, message);
        }

        public static ApiResponse<T> Created(T? data, string message = "Created successfully")
        {
            return new ApiResponse<T>(data, 201, message);
        }
    }
} 