using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemZarzadzaniaBibliotekaFirmy.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Procedura 1: GetOverdueLoans
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.GetOverdueLoans', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.GetOverdueLoans;
                GO

                CREATE PROCEDURE [dbo].[GetOverdueLoans]
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT 
                        l.Id,
                        l.LoanDate,
                        l.ReturnDate,
                        l.DueDate,
                        l.IsReturned,
                        l.Notes,
                        l.BookId,
                        b.Title AS BookTitle,
                        l.EmployeeId,
                        e.FirstName + ' ' + e.LastName AS EmployeeName
                    FROM Loans l
                    INNER JOIN Books b ON l.BookId = b.Id
                    INNER JOIN Employees e ON l.EmployeeId = e.Id
                    WHERE l.IsReturned = 0 AND l.DueDate < GETUTCDATE()
                    ORDER BY l.DueDate ASC;
                END
                GO
            ");

            // Procedura 2: GetEmployeeLoanStatistics
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.GetEmployeeLoanStatistics', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.GetEmployeeLoanStatistics;
                GO

                CREATE PROCEDURE [dbo].[GetEmployeeLoanStatistics]
                    @EmployeeId INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT 
                        COUNT(*) AS TotalLoans,
                        SUM(CASE WHEN IsReturned = 0 THEN 1 ELSE 0 END) AS ActiveLoans,
                        SUM(CASE WHEN IsReturned = 0 AND DueDate < GETUTCDATE() THEN 1 ELSE 0 END) AS OverdueLoans
                    FROM Loans
                    WHERE EmployeeId = @EmployeeId;
                END
                GO
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetOverdueLoans]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetEmployeeLoanStatistics]");
        }
    }
}

