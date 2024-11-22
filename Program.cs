using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace StudentSubjectApp
{
    public class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public List<Subject> Subjects { get; set; } = new List<Subject>();
    }

    public class Subject
    {
        public int SubjectId { get; set; }
        public string Title { get; set; }
        public int MaximumCapacity { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }

    public class SchoolContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=school.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Subjects)
                .WithMany(s => s.Students)
                .UsingEntity(j => j.ToTable("StudentSubjects"));
        }
    }

    public class Repository
    {
        private readonly SchoolContext _context;

        public Repository()
        {
            _context = new SchoolContext();
            _context.Database.EnsureCreated();
        }

        public void AddSubject(Subject subject)
        {
            _context.Subjects.Add(subject);
            _context.SaveChanges();
        }

        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
        }

        public void EnrollStudentToSubject(int studentId, int subjectId)
        {
            var student = _context.Students.Find(studentId);
            var subject = _context.Subjects.Find(subjectId);

            if (student != null && subject != null)
            {
                student.Subjects.Add(subject);
                subject.Students.Add(student);
                _context.SaveChanges();
            }
        }

        public List<Subject> GetAllSubjects()
        {
            return _context.Subjects.Include(s => s.Students).ToList();
        }

        public List<Student> GetStudentsForSubject(int subjectId)
        {
            var subject = _context.Subjects
                .Include(s => s.Students)
                .FirstOrDefault(s => s.SubjectId == subjectId);

            return subject?.Students ?? new List<Student>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var repository = new Repository();

            // Add a new Subject
            var subject = new Subject { Title = "Mathematics", MaximumCapacity = 30 };
            repository.AddSubject(subject);

            // Add Students
            var student1 = new Student { Name = "Alice", EnrollmentDate = DateTime.Now };
            var student2 = new Student { Name = "Bob", EnrollmentDate = DateTime.Now };
            repository.AddStudent(student1);
            repository.AddStudent(student2);

            // Enroll Students in Subject
            repository.EnrollStudentToSubject(student1.StudentId, subject.SubjectId);
            repository.EnrollStudentToSubject(student2.StudentId, subject.SubjectId);

            // Retrieve and Display Subjects and Enrolled Students
            var subjects = repository.GetAllSubjects();
            foreach (var sub in subjects)
            {
                Console.WriteLine($"Subject: {sub.Title}");
                Console.WriteLine("Enrolled Students:");
                foreach (var stud in sub.Students)
                {
                    Console.WriteLine($"- {stud.Name}");
                }
            }
        }
    }
}
