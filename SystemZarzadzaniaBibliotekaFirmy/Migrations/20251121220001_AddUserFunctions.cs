using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemZarzadzaniaBibliotekaFirmy.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Funkcja użytkownika: CalculateLoanDays
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.CalculateLoanDays', 'FN') IS NOT NULL
                    DROP FUNCTION dbo.CalculateLoanDays;
                GO

                CREATE FUNCTION [dbo].[CalculateLoanDays]
                (
                    @LoanDate DATETIME,
                    @ReturnDate DATETIME
                )
                RETURNS INT
                AS
                BEGIN
                    DECLARE @Days INT;
                    IF @ReturnDate IS NULL
                        SET @Days = DATEDIFF(DAY, @LoanDate, GETUTCDATE());
                    ELSE
                        SET @Days = DATEDIFF(DAY, @LoanDate, @ReturnDate);
                    RETURN @Days;
                END
                GO
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS [dbo].[CalculateLoanDays]");
        }
    }
}

