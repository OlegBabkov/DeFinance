using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusToMandatoryPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentStatusId",
                table: "MandatoryPayments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryPayments_PaymentStatusId",
                table: "MandatoryPayments",
                column: "PaymentStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryPayments_PaymentStatuses_PaymentStatusId",
                table: "MandatoryPayments",
                column: "PaymentStatusId",
                principalTable: "PaymentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryPayments_PaymentStatuses_PaymentStatusId",
                table: "MandatoryPayments");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryPayments_PaymentStatusId",
                table: "MandatoryPayments");

            migrationBuilder.DropColumn(
                name: "PaymentStatusId",
                table: "MandatoryPayments");
        }
    }
}
