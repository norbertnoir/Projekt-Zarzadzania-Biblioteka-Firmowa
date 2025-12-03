using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemZarzadzaniaBibliotekaFirmy.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Trigger: UpdateBookAvailabilityOnLoanReturn
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.UpdateBookAvailabilityOnLoanReturn', 'TR') IS NOT NULL
                    DROP TRIGGER dbo.UpdateBookAvailabilityOnLoanReturn;
                GO

                CREATE TRIGGER [dbo].[UpdateBookAvailabilityOnLoanReturn]
                ON [dbo].[Loans]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF UPDATE(IsReturned)
                    BEGIN
                        UPDATE Books
                        SET IsAvailable = 1
                        FROM Books b
                        INNER JOIN inserted i ON b.Id = i.BookId
                        WHERE i.IsReturned = 1;
                    END
                END
                GO
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[UpdateBookAvailabilityOnLoanReturn]");
        }
    }
}

