using CrmTaskManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrmTaskManagement.Data.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("work_tasks", t =>
        {
            t.HasCheckConstraint("CK_work_tasks_due_at_after_planned_start", "\"due_at\" >= \"planned_start_at\"");
            t.HasCheckConstraint(
                "CK_work_tasks_completed_at_requires_completed_status",
                "(\"status\" = 'Completed' AND \"completed_at\" IS NOT NULL) OR (\"status\" <> 'Completed' AND \"completed_at\" IS NULL)");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.PlannedStartAt)
            .IsRequired();

        builder.Property(t => t.DueAt)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.HasOne(t => t.CreatedBy)
            .WithMany(e => e.CreatedTasks)
            .HasForeignKey(t => t.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(t => t.AssignedToEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.AssignedToEmployeeId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueAt);
    }
}
