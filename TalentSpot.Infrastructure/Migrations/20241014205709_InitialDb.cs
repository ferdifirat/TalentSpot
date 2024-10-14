using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TalentSpot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForbiddenWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForbiddenWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    AllowedJobPostings = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    WorkTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Salary = table.Column<decimal>(type: "numeric", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_WorkTypes_WorkTypeId",
                        column: x => x.WorkTypeId,
                        principalTable: "WorkTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobBenefits",
                columns: table => new
                {
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobBenefits", x => new { x.JobId, x.BenefitId });
                    table.ForeignKey(
                        name: "FK_JobBenefits_Benefits_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "Benefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobBenefits_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Benefits",
                columns: new[] { "Id", "CreatedDate", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("11aaba7a-81df-4e90-a3e5-910afdfc4397"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(650), "Paid Vacation", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(651) },
                    { new Guid("5ad96154-7bd7-427e-b9e9-831020cc7ecf"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(646), "Health Insurance", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(646) },
                    { new Guid("a3f995ca-4b13-4645-8fe6-699287567eba"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(654), "Remote Work", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(654) },
                    { new Guid("b67f6992-c0af-41fe-a64a-80b36559b8cb"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(657), "Flexible Hours", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(658) }
                });

            migrationBuilder.InsertData(
                table: "ForbiddenWords",
                columns: new[] { "Id", "CreatedDate", "UpdatedDate", "Word" },
                values: new object[,]
                {
                    { new Guid("26fa4ba6-446a-4b37-b89d-a7a470124f3c"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(810), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(811), "Banned" },
                    { new Guid("b97d6818-3796-45f3-9094-993cba920846"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(794), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(795), "Prohibited" },
                    { new Guid("ef395537-2173-4f8a-bb49-119f82a3e95c"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(807), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(807), "Illegal" },
                    { new Guid("f65981a9-8b1d-4b8c-b1ad-0eb13b4a58b4"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(813), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(814), "Restricted" }
                });

            migrationBuilder.InsertData(
                table: "WorkTypes",
                columns: new[] { "Id", "CreatedDate", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("1529d4ac-8281-46bd-8fd3-c55858729883"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(338), "Full-Time", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(339) },
                    { new Guid("61b99ba1-9c03-498d-98d6-1ff6115e347b"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(412), "Freelance", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(413) },
                    { new Guid("a2a5552f-b753-44ba-9fa5-7936fa461737"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(407), "Part-Time", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(408) },
                    { new Guid("be9346ac-7f14-424d-9bc7-fa3656e19fa8"), new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(416), "Internship", new DateTime(2024, 10, 14, 20, 57, 8, 764, DateTimeKind.Utc).AddTicks(417) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_UserId",
                table: "Companies",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobBenefits_BenefitId",
                table: "JobBenefits",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyId",
                table: "Jobs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WorkTypeId",
                table: "Jobs",
                column: "WorkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForbiddenWords");

            migrationBuilder.DropTable(
                name: "JobBenefits");

            migrationBuilder.DropTable(
                name: "Benefits");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "WorkTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
