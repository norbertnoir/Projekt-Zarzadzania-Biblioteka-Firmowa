namespace SystemZarzadzaniaBibliotekaFirmy.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalBooks { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public int TotalEmployees { get; set; }
    }
}
