using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CrmTaskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "work_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    planned_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by_employee_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_to_employee_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_tasks", x => x.id);
                    table.CheckConstraint("CK_work_tasks_completed_at_requires_completed_status", "(\"status\" = 'Completed' AND \"completed_at\" IS NOT NULL) OR (\"status\" <> 'Completed' AND \"completed_at\" IS NULL)");
                    table.CheckConstraint("CK_work_tasks_due_at_after_planned_start", "\"due_at\" >= \"planned_start_at\"");
                    table.ForeignKey(
                        name: "fk_work_tasks_employees_assigned_to_employee_id",
                        column: x => x.assigned_to_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_work_tasks_employees_created_by_employee_id",
                        column: x => x.created_by_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_employees_email",
                table: "employees",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_work_tasks_assigned_to_employee_id",
                table: "work_tasks",
                column: "assigned_to_employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_work_tasks_created_by_employee_id",
                table: "work_tasks",
                column: "created_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_work_tasks_due_at",
                table: "work_tasks",
                column: "due_at");

            migrationBuilder.CreateIndex(
                name: "ix_work_tasks_status",
                table: "work_tasks",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_tasks");

            migrationBuilder.DropTable(
                name: "employees");
        }
    }
}
