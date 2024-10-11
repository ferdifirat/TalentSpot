using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSpot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IniateSeedDatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // WorkTypes
            migrationBuilder.InsertData(
                table: "WorkTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "Full-Time" },
                    { Guid.NewGuid(), "Part-Time" },
                    { Guid.NewGuid(), "Freelance" },
                    { Guid.NewGuid(), "Internship" }
                });

            // Benefits
            migrationBuilder.InsertData(
                table: "Benefits",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "Health Insurance" },
                    { Guid.NewGuid(), "Paid Vacation" },
                    { Guid.NewGuid(), "Remote Work" },
                    { Guid.NewGuid(), "Flexible Hours" }
                });

            // ForbiddenWords
            migrationBuilder.InsertData(
                table: "ForbiddenWords",
                columns: new[] { "Id", "Word" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "Prohibited" },
                    { Guid.NewGuid(), "Illegal" },
                    { Guid.NewGuid(), "Banned" },
                    { Guid.NewGuid(), "Restricted" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove inserted data on rollback
            migrationBuilder.Sql("DELETE FROM WorkTypes");
            migrationBuilder.Sql("DELETE FROM Benefits");
            migrationBuilder.Sql("DELETE FROM ForbiddenWords");
        }
    }
}
