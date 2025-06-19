namespace ExpenseTrackerApi.DTOs
{
    public class ExpenseRequestDto
    {
        public string Title { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string CategoryName { get; set; } = string.Empty;

    }
}