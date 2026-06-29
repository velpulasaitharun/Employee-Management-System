using HRMS.Models;
using Microsoft.EntityFrameworkCore;
namespace HRMS.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext
        (
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Designation> Designations { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<LeaveType> LeaveTypes { get; set; }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        public DbSet<Payroll> Payrolls { get; set; }

        public DbSet<Holiday> Holidays { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          modelBuilder.Entity<User>(entity =>
          {
            entity.Property(user => user.UserName)
              .HasMaxLength(100)
              .IsRequired();

            entity.Property(user => user.RoleName)
              .HasMaxLength(50)
              .IsRequired();

            entity.HasIndex(user => user.UserName)
              .IsUnique();

            entity.HasIndex(user => user.EmployeeId)
              .IsUnique();
          });

           // modelBuilder.Entity<Employee>()
             //   .HasOne(e => e.Department)
               // .WithMany(d => d.Employees)
                //.HasForeignKey(e => e.DepartmentId);

            //modelBuilder.Entity<Employee>()
              //  .HasOne(e => e.Designation)
                //.WithMany(d => d.Employees)
                //.HasForeignKey(e => e.DesignationId);
        }
    }
}
