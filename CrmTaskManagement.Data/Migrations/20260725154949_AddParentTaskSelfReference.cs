using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmTaskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentTaskSelfReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_task_id",
                table: "work_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_work_tasks_parent_task_id",
                table: "work_tasks",
                column: "parent_task_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_work_tasks_parent_task_not_self",
                table: "work_tasks",
                sql: "\"parent_task_id\" <> \"id\" OR \"parent_task_id\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_work_tasks_work_tasks_parent_task_id",
                table: "work_tasks",
                column: "parent_task_id",
                principalTable: "work_tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_work_tasks_work_tasks_parent_task_id",
                table: "work_tasks");

            migrationBuilder.DropIndex(
                name: "ix_work_tasks_parent_task_id",
                table: "work_tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_work_tasks_parent_task_not_self",
                table: "work_tasks");

            migrationBuilder.DropColumn(
                name: "parent_task_id",
                table: "work_tasks");
        }
    }
}
