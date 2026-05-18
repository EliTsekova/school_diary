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

            modelBuilder.Entity<Class>().HasData(
                new Class { Id = 1, Name = "1A", SchoolId = 1 },
                new Class { Id = 2, Name = "1B", SchoolId = 1 },
                new Class { Id = 3, Name = "1C", SchoolId = 1 },
                new Class { Id = 4, Name = "1D", SchoolId = 1 },

                new Class { Id = 5, Name = "2A", SchoolId = 1 },
                new Class { Id = 6, Name = "2B", SchoolId = 1 },
                new Class { Id = 7, Name = "2C", SchoolId = 1 },
                new Class { Id = 8, Name = "2D", SchoolId = 1 },

                new Class { Id = 9, Name = "3A", SchoolId = 1 },
                new Class { Id = 10, Name = "3B", SchoolId = 1 },
                new Class { Id = 11, Name = "3C", SchoolId = 1 },
                new Class { Id = 12, Name = "3D", SchoolId = 1 },

                new Class { Id = 13, Name = "4A", SchoolId = 1 },
                new Class { Id = 14, Name = "4B", SchoolId = 1 },
                new Class { Id = 15, Name = "4C", SchoolId = 1 },
                new Class { Id = 16, Name = "4D", SchoolId = 1 },

                new Class { Id = 17, Name = "5A", SchoolId = 1 },
                new Class { Id = 18, Name = "5B", SchoolId = 1 },
                new Class { Id = 19, Name = "5C", SchoolId = 1 },
                new Class { Id = 20, Name = "5D", SchoolId = 1 },

                new Class { Id = 21, Name = "6A", SchoolId = 1 },
                new Class { Id = 22, Name = "6B", SchoolId = 1 },
                new Class { Id = 23, Name = "6C", SchoolId = 1 },
                new Class { Id = 24, Name = "6D", SchoolId = 1 },

                new Class { Id = 25, Name = "7A", SchoolId = 1 },
                new Class { Id = 26, Name = "7B", SchoolId = 1 },
                new Class { Id = 27, Name = "7C", SchoolId = 1 },
                new Class { Id = 28, Name = "7D", SchoolId = 1 },

                new Class { Id = 29, Name = "8A", SchoolId = 1 },
                new Class { Id = 30, Name = "8B", SchoolId = 1 },
                new Class { Id = 31, Name = "8C", SchoolId = 1 },
                new Class { Id = 32, Name = "8D", SchoolId = 1 },

                new Class { Id = 33, Name = "9A", SchoolId = 1 },
                new Class { Id = 34, Name = "9B", SchoolId = 1 },
                new Class { Id = 35, Name = "9C", SchoolId = 1 },
                new Class { Id = 36, Name = "9D", SchoolId = 1 },

                new Class { Id = 37, Name = "10A", SchoolId = 1 },
                new Class { Id = 38, Name = "10B", SchoolId = 1 },
                new Class { Id = 39, Name = "10C", SchoolId = 1 },
                new Class { Id = 40, Name = "10D", SchoolId = 1 },

                new Class { Id = 41, Name = "11A", SchoolId = 1 },
                new Class { Id = 42, Name = "11B", SchoolId = 1 },
                new Class { Id = 43, Name = "11C", SchoolId = 1 },
                new Class { Id = 44, Name = "11D", SchoolId = 1 },

                new Class { Id = 45, Name = "12A", SchoolId = 1 },
                new Class { Id = 46, Name = "12B", SchoolId = 1 },
                new Class { Id = 47, Name = "12C", SchoolId = 1 },
                new Class { Id = 48, Name = "12D", SchoolId = 1 }
            );
        }
    }
}