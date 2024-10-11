using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TalentSpot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReIniateSeedDatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Benefits",
                columns: new[] { "Id", "CreatedDate", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("356ad535-3c3c-4deb-8fd9-bfa2c7ce1a1f"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1191), "Paid Vacation", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1191) },
                    { new Guid("8f7ee1c3-c085-4816-8be8-2c4bb2ccb023"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1189), "Health Insurance", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1189) },
                    { new Guid("9088898e-32bf-4acc-b482-3083ce52f58d"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1193), "Flexible Hours", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1194) },
                    { new Guid("d934d410-c4e0-4a4e-8dcf-dccbef64d539"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1192), "Remote Work", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1192) }
                });

            migrationBuilder.InsertData(
                table: "ForbiddenWords",
                columns: new[] { "Id", "CreatedDate", "UpdatedDate", "Word" },
                values: new object[,]
                {
                    { new Guid("3db62487-eb51-4adf-9768-7aa50afa8f07"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1206), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1206), "Prohibited" },
                    { new Guid("8236abee-82c4-499a-9abe-205c88425774"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1207), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1208), "Illegal" },
                    { new Guid("a14ee31a-a2ae-4c49-9882-45943c1eb2d7"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1211), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1212), "Restricted" },
                    { new Guid("cb463d3f-235b-4613-93ac-58751d4c04d2"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1209), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1209), "Banned" }
                });

            migrationBuilder.InsertData(
                table: "WorkTypes",
                columns: new[] { "Id", "CreatedDate", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("5cd362bc-a6d4-4f87-b267-3f12ca8d1e41"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1094), "Part-Time", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1094) },
                    { new Guid("acfa3608-d91a-4aa1-82cc-ba7f5b962b84"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1126), "Internship", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1127) },
                    { new Guid("c8062f72-1951-423e-a619-2f3941be2f66"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1085), "Full-Time", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1085) },
                    { new Guid("c9c2b820-d805-401e-a7ee-492fc84e3eed"), new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1117), "Freelance", new DateTime(2024, 10, 11, 15, 11, 10, 129, DateTimeKind.Utc).AddTicks(1117) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Benefits",
                keyColumn: "Id",
                keyValue: new Guid("356ad535-3c3c-4deb-8fd9-bfa2c7ce1a1f"));

            migrationBuilder.DeleteData(
                table: "Benefits",
                keyColumn: "Id",
                keyValue: new Guid("8f7ee1c3-c085-4816-8be8-2c4bb2ccb023"));

            migrationBuilder.DeleteData(
                table: "Benefits",
                keyColumn: "Id",
                keyValue: new Guid("9088898e-32bf-4acc-b482-3083ce52f58d"));

            migrationBuilder.DeleteData(
                table: "Benefits",
                keyColumn: "Id",
                keyValue: new Guid("d934d410-c4e0-4a4e-8dcf-dccbef64d539"));

            migrationBuilder.DeleteData(
                table: "ForbiddenWords",
                keyColumn: "Id",
                keyValue: new Guid("3db62487-eb51-4adf-9768-7aa50afa8f07"));

            migrationBuilder.DeleteData(
                table: "ForbiddenWords",
                keyColumn: "Id",
                keyValue: new Guid("8236abee-82c4-499a-9abe-205c88425774"));

            migrationBuilder.DeleteData(
                table: "ForbiddenWords",
                keyColumn: "Id",
                keyValue: new Guid("a14ee31a-a2ae-4c49-9882-45943c1eb2d7"));

            migrationBuilder.DeleteData(
                table: "ForbiddenWords",
                keyColumn: "Id",
                keyValue: new Guid("cb463d3f-235b-4613-93ac-58751d4c04d2"));

            migrationBuilder.DeleteData(
                table: "WorkTypes",
                keyColumn: "Id",
                keyValue: new Guid("5cd362bc-a6d4-4f87-b267-3f12ca8d1e41"));

            migrationBuilder.DeleteData(
                table: "WorkTypes",
                keyColumn: "Id",
                keyValue: new Guid("acfa3608-d91a-4aa1-82cc-ba7f5b962b84"));

            migrationBuilder.DeleteData(
                table: "WorkTypes",
                keyColumn: "Id",
                keyValue: new Guid("c8062f72-1951-423e-a619-2f3941be2f66"));

            migrationBuilder.DeleteData(
                table: "WorkTypes",
                keyColumn: "Id",
                keyValue: new Guid("c9c2b820-d805-401e-a7ee-492fc84e3eed"));
        }
    }
}
