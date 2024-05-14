using AsseyLabMgt.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AsseyLabMgt.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region Models
        public DbSet<Staff> Staff { get; set; }
        public DbSet<LabRequest> LabRequests { get; set; }
        public DbSet<LabResults> LabResults { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Designation> Designation { get; set; }
        public DbSet<PlantSource> PlantSources { get; set; }
        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Configuring OnDelete behavior to prevent cascading delete issues 
            // Configure the FK relationships to not cascade on delete
            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.DeliveredBy)
                .WithMany()
                .HasForeignKey(l => l.DeliveredById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete

            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.ReceivedBy)
                .WithMany()
                .HasForeignKey(l => l.ReceivedById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete

            // Apply this pattern for other Staff-related foreign keys
            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.PreparedBy)
                .WithMany()
                .HasForeignKey(l => l.PreparedById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete

            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.WeighedBy)
                .WithMany()
                .HasForeignKey(l => l.WeighedById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete

            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.DigestedBy)
                .WithMany()
                .HasForeignKey(l => l.DigestedById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete

            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.TitratedBy)
                .WithMany()
                .HasForeignKey(l => l.TitratedById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete

            modelBuilder.Entity<LabRequest>()
                .HasOne(l => l.EnteredBy)
                .WithMany()
                .HasForeignKey(l => l.EnteredById)
                .OnDelete(DeleteBehavior.Restrict); // Set to restrict on delete
            #endregion

            #region Index for staff
            // Index for StaffNumber in the Staff table
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.StaffNumber)
                .HasDatabaseName("IX_Staff_StaffNumber")
                .IsUnique(); // Make the index unique if StaffNumber should be unique

            // Index for Email in the Staff table
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Email)
                .HasDatabaseName("IX_Staff_Email")
                .IsUnique(); // Emails are typically unique

            // Index for DepartmentId in the Staff table
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.DepartmentId)
                .HasDatabaseName("IX_Staff_DepartmentId");

            // Composite index for Surname and Firstname, if commonly used together in queries
            modelBuilder.Entity<Staff>()
                .HasIndex(s => new { s.Surname, s.Firstname })
                .HasDatabaseName("IX_Staff_Surname_Firstname");

            // Index for Fullname if it is commonly used in searches
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Fullname)
                .HasDatabaseName("IX_Staff_Fullname");

            #endregion

            #region Index for LabRequest
            // Index for JobNumber in the LabRequest table
            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.JobNumber)
                .HasDatabaseName("IX_LabRequests_JobNumber");

            // Index for Date in the LabRequest table
            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.Date)
                .HasDatabaseName("IX_LabRequests_Date");

            // Index for ProductionDate in the LabRequest table
            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.ProductionDate)
                .HasDatabaseName("IX_LabRequests_ProductionDate");

            // Indexes for foreign keys
            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.ClientId)
                .HasDatabaseName("IX_LabRequests_ClientId");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.DepartmentId)
                .HasDatabaseName("IX_LabRequests_DepartmentId");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.PlantSourceId)
                .HasDatabaseName("IX_LabRequests_PlantSourceId");

            // Indexes for Staff IDs if they are frequently used in queries
            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.DeliveredById)
                .HasDatabaseName("IX_LabRequests_DeliveredById");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.ReceivedById)
                .HasDatabaseName("IX_LabRequests_ReceivedById");

            // Apply similar patterns for other staff-related foreign keys
            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.PreparedById)
                .HasDatabaseName("IX_LabRequests_PreparedById");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.WeighedById)
                .HasDatabaseName("IX_LabRequests_WeighedById");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.DigestedById)
                .HasDatabaseName("IX_LabRequests_DigestedById");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.TitratedById)
                .HasDatabaseName("IX_LabRequests_TitratedById");

            modelBuilder.Entity<LabRequest>()
                .HasIndex(l => l.EnteredById)
                .HasDatabaseName("IX_LabRequests_EnteredById");

            #endregion

            #region Index for LabResults
            // Index for LabRequestId in the LabResults table
            modelBuilder.Entity<LabResults>()
                .HasIndex(lr => lr.LabRequestId)
                .HasDatabaseName("IX_LabResults_LabRequestId");

            // Index for SampleId in the LabResults table
            modelBuilder.Entity<LabResults>()
                .HasIndex(lr => lr.SampleId)
                .HasDatabaseName("IX_LabResults_SampleId");

            #endregion

        }

    }
}
