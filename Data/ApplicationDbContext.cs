using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using school_diary.Models;

namespace school_diary.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> UsersExt { get; set; }

        public DbSet<School> Schools { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Director> Directors { get; set; }

        public DbSet<ParentStudent> ParentStudents { get; set; }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }

        public DbSet<Grade> Grades { get; set; }
        public DbSet<Absence> Absences => Set<Absence>();

        public DbSet<Curriculum> Curricula { get; set; }
        public DbSet<CurriculumEntry> CurriculumEntries { get; set; }

        public DbSet<Class> Classes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.School)
                .WithMany(s => s.Teachers)
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Teacher)
                .WithMany(t => t.TeacherSubjects)
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Subject)
                .WithMany()
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherSubject>()
                .HasIndex(ts => new { ts.TeacherId, ts.SubjectId })
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.School)
                .WithMany(sch => sch.Students)
                .HasForeignKey(s => s.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Parent>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Director>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Director>()
                .HasOne(d => d.School)
                .WithOne(s => s.Director)
                .HasForeignKey<Director>(d => d.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParentStudent>()
                .HasOne(ps => ps.Parent)
                .WithMany(p => p.ParentStudents)
                .HasForeignKey(ps => ps.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParentStudent>()
                .HasOne(ps => ps.Student)
                .WithMany(s => s.ParentStudents)
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParentStudent>()
                .HasIndex(ps => new { ps.ParentId, ps.StudentId })
                .IsUnique();

            modelBuilder.Entity<Absence>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Absences)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Absence>()
                .HasOne(a => a.Subject)
                .WithMany()
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Teacher)
                .WithMany(t => t.Grades)
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Subject)
                .WithMany()
                .HasForeignKey(g => g.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Curriculum>()
                .HasOne(c => c.Class)
                .WithMany(c => c.Curricula)
                .HasForeignKey(c => c.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Curriculum>()
                .HasIndex(c => new { c.ClassId, c.Term });

            modelBuilder.Entity<CurriculumEntry>()
                .HasOne(e => e.Curriculum)
                .WithMany(c => c.Entries)
                .HasForeignKey(e => e.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CurriculumEntry>()
                .HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurriculumEntry>()
                .HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurriculumEntry>()
                .HasIndex(e => new { e.CurriculumId, e.DayOfWeek, e.Period })
                .IsUnique();

            // Seed School
            modelBuilder.Entity<School>().HasData(
                new School
                {
                    Id = 1,
                    Name = "Default School",
                    Address = "Default Address"
                }
            );

            // Seed Classes
            var classes = new List<Class>();
            int id = 1;

            for (int grade = 1; grade <= 12; grade++)
            {
                foreach (var letter in new[] { "A", "B", "C", "D" })
                {
                    classes.Add(new Class
                    {
                        Id = id++,
                        Name = $"{grade}{letter}",
                        SchoolId = 1
                    });
                }
            }

            modelBuilder.Entity<Class>().HasData(classes);
        }
    }
}