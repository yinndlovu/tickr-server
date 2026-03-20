namespace Core.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Details { get; set; }
    }
}
