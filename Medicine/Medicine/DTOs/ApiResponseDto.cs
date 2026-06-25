namespace Medicine.DTOs
{
    public class ApiResponseDto
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
        public int? DrugId { get; set; }
    }
}
