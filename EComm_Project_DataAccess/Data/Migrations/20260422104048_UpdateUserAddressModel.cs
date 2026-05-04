using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComm_Project_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAddressModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "State",
                table: "UserAddresses");
        }
    }
}
