using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // User & Role
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }

    // Term
    public DbSet<Term> Terms { get; set; }

    // Programme
    public DbSet<Programme> Programmes { get; set; }
    public DbSet<ProgrammeVersion> ProgrammeVersions { get; set; }
    public DbSet<StudentProgramme> StudentProgrammes { get; set; }
    public DbSet<ProgrammeCategory> ProgrammeCategories { get; set; }

    // Category & Group
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryGroup> CategoryGroups { get; set; }
    public DbSet<CourseGroup> CourseGroups { get; set; }
    public DbSet<GroupCourse> GroupCourses { get; set; }

    // Course
    public DbSet<Code> Codes { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseDepartment> CourseDepartments { get; set; }
    public DbSet<CourseVersion> CourseVersions { get; set; }
    public DbSet<CourseSection> CourseSections { get; set; }
    public DbSet<CourseMeeting> CourseMeetings { get; set; }
    public DbSet<CoursePreReq> CoursePreReqs { get; set; }
    public DbSet<CourseAntiReq> CourseAntiReqs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure CoursePreReq relationships
        modelBuilder.Entity<CoursePreReq>()
            .HasOne(cp => cp.CourseVersion)
            .WithMany(cv => cv.Prerequisites)
            .HasForeignKey(cp => cp.CourseVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoursePreReq>()
            .HasOne(cp => cp.RequiredCourseVersion)
            .WithMany(cv => cv.PrerequisiteFor)
            .HasForeignKey(cp => cp.RequiredCourseVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CourseAntiReq relationships
        modelBuilder.Entity<CourseAntiReq>()
            .HasOne(ca => ca.CourseVersion)
            .WithMany(cv => cv.AntiRequisites)
            .HasForeignKey(ca => ca.CourseVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseAntiReq>()
            .HasOne(ca => ca.ExcludedCourseVersion)
            .WithMany(cv => cv.AntiRequisiteFor)
            .HasForeignKey(ca => ca.ExcludedCourseVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CategoryGroup relationships
        modelBuilder.Entity<CategoryGroup>()
            .HasOne(cg => cg.Category)
            .WithMany(c => c.CategoryGroups)
            .HasForeignKey(cg => cg.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryGroup>()
            .HasOne(cg => cg.Group)
            .WithMany(g => g.CategoryGroups)
            .HasForeignKey(cg => cg.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure GroupCourse relationships
        modelBuilder.Entity<GroupCourse>()
            .HasOne(gc => gc.Group)
            .WithMany(g => g.GroupCourses)
            .HasForeignKey(gc => gc.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupCourse>()
            .HasOne(gc => gc.Course)
            .WithMany(c => c.GroupCourses)
            .HasForeignKey(gc => gc.CourseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupCourse>()
            .HasOne(gc => gc.Code)
            .WithMany(c => c.GroupCourses)
            .HasForeignKey(gc => gc.CodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Course-Code relationship
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Code)
            .WithMany(code => code.Courses)
            .HasForeignKey(c => c.CodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure CourseDepartment relationships (many-to-many between Course and Department)
        modelBuilder.Entity<CourseDepartment>()
            .HasOne(cd => cd.Course)
            .WithMany(c => c.CourseDepartments)
            .HasForeignKey(cd => cd.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseDepartment>()
            .HasOne(cd => cd.Department)
            .WithMany(d => d.CourseDepartments)
            .HasForeignKey(cd => cd.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ProgrammeCategory relationships
        modelBuilder.Entity<ProgrammeCategory>()
            .HasOne(pc => pc.ProgrammeVersion)
            .WithMany(pv => pv.ProgrammeCategories)
            .HasForeignKey(pc => pc.ProgrammeVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProgrammeCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProgrammeCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure StudentProgramme relationships
        modelBuilder.Entity<StudentProgramme>()
            .HasOne(sp => sp.Student)
            .WithMany(u => u.StudentProgrammes)
            .HasForeignKey(sp => sp.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentProgramme>()
            .HasOne(sp => sp.ProgrammeVersion)
            .WithMany(pv => pv.StudentProgrammes)
            .HasForeignKey(sp => sp.ProgrammeVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
