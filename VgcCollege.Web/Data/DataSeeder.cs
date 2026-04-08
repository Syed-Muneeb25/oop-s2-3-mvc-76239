using Microsoft.AspNetCore.Identity;
using VgcCollege.Domain;

namespace VgcCollege.Web.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in new[] { "Admin", "Faculty", "Student" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await CreateUser(userManager, "admin@vgc.com", "Admin@123", "Admin");
            await CreateUser(userManager, "faculty1@vgc.com", "Faculty@123", "Faculty");
            await CreateUser(userManager, "faculty2@vgc.com", "Faculty@123", "Faculty");
            await CreateUser(userManager, "student1@vgc.com", "Student@123", "Student");
            await CreateUser(userManager, "student2@vgc.com", "Student@123", "Student");
            await CreateUser(userManager, "student3@vgc.com", "Student@123", "Student");

            if (context.Branches.Any()) return;

            var dublin = new Branch { Name = "Dublin Campus", Address = "8 Belvedere Palce, Dublin 2" };
            var cork = new Branch { Name = "Cork Campus", Address = "123 Cork Street, Cork" };
            var galway = new Branch { Name = "Galway Campus", Address = "525 Mount Street Rd, Galway" };
            context.Branches.AddRange(dublin, cork, galway);
            await context.SaveChangesAsync();

            var f1User = await userManager.FindByEmailAsync("faculty1@vgc.com");
            var f2User = await userManager.FindByEmailAsync("faculty2@vgc.com");

            var faculty1 = new FacultyProfile
            {
                IdentityUserId = f1User!.Id,
                Name = "Faculty1",
                Email = "faculty1@vgc.com",
                Phone = "087-1234567"
            };
            var faculty2 = new FacultyProfile
            {
                IdentityUserId = f2User!.Id,
                Name = "Faculty2",
                Email = "faculty2@vgc.com",
                Phone = "087-1122334"
            };
            context.FacultyProfiles.AddRange(faculty1, faculty2);
            await context.SaveChangesAsync();

            var courseStart = new DateOnly(2026, 1, 12);
            var courseEnd = new DateOnly(2026, 5, 31);

            var cSoftware = new Course
            {
                Name = "Software Development",
                BranchId = dublin.Id,
                FacultyProfileId = faculty1.Id,
                StartDate = courseStart,
                EndDate = courseEnd
            };
            var cData = new Course
            {
                Name = "Data Science",
                BranchId = cork.Id,
                FacultyProfileId = faculty1.Id,
                StartDate = courseStart,
                EndDate = courseEnd
            };
            var cCyber = new Course
            {
                Name = "Cybersecurity",
                BranchId = galway.Id,
                FacultyProfileId = faculty2.Id,
                StartDate = courseStart,
                EndDate = courseEnd
            };
            context.Courses.AddRange(cSoftware, cData, cCyber);
            await context.SaveChangesAsync();

            var s1User = await userManager.FindByEmailAsync("student1@vgc.com");
            var s2User = await userManager.FindByEmailAsync("student2@vgc.com");
            var s3User = await userManager.FindByEmailAsync("student3@vgc.com");

            var student1 = new StudentProfile
            {
                IdentityUserId = s1User!.Id,
                Name = "Student1",
                Email = "student1@vgc.ie",
                Phone = "086-1212123",
                Address = "10 Mount Joy, Dublin 2",
                DateOfBirth = new DateOnly(2000, 3, 15),
                StudentNumber = "STU001"
            };
            var student2 = new StudentProfile
            {
                IdentityUserId = s2User!.Id,
                Name = "Student2",
                Email = "student2@vgc.com",
                Phone = "086-7654321",
                Address = "25 Lay Ave, Cork",
                DateOfBirth = new DateOnly(1999, 7, 22),
                StudentNumber = "STU002"
            };
            var student3 = new StudentProfile
            {
                IdentityUserId = s3User!.Id,
                Name = "Student3",
                Email = "student3@vgc.com",
                Phone = "086-3421345",
                Address = "5 Shop Street, Galway",
                DateOfBirth = new DateOnly(2001, 11, 8),
                StudentNumber = "STU003"
            };
            context.StudentProfiles.AddRange(student1, student2, student3);
            await context.SaveChangesAsync();

            var enrol1 = new CourseEnrolment
            {
                StudentProfileId = student1.Id,
                CourseId = cSoftware.Id,
                EnrolDate = new DateOnly(2026, 1, 8),
                Status = EnrolmentStatus.Active
            };
            var enrol2 = new CourseEnrolment
            {
                StudentProfileId = student2.Id,
                CourseId = cSoftware.Id,
                EnrolDate = new DateOnly(2026, 1, 8),
                Status = EnrolmentStatus.Active
            };
            var enrol3 = new CourseEnrolment
            {
                StudentProfileId = student2.Id,
                CourseId = cData.Id,
                EnrolDate = new DateOnly(2026, 1, 9),
                Status = EnrolmentStatus.Active
            };
            var enrol4 = new CourseEnrolment
            {
                StudentProfileId = student3.Id,
                CourseId = cCyber.Id,
                EnrolDate = new DateOnly(2026, 1, 9),
                Status = EnrolmentStatus.Active
            };
            context.CourseEnrolments.AddRange(enrol1, enrol2, enrol3, enrol4);
            await context.SaveChangesAsync();

            var attendanceData = new List<(CourseEnrolment enrol, int week, DateOnly date, AttendanceStatus status)>
        {
            (enrol1, 1, new DateOnly(2026, 1, 12), AttendanceStatus.Present),
            (enrol1, 2, new DateOnly(2026, 1, 19), AttendanceStatus.Present),
            (enrol1, 3, new DateOnly(2026, 1, 26), AttendanceStatus.Present),
            (enrol1, 4, new DateOnly(2026, 2, 2),  AttendanceStatus.Absent),

            (enrol2, 1, new DateOnly(2026, 1, 12), AttendanceStatus.Present),
            (enrol2, 2, new DateOnly(2026, 1, 19), AttendanceStatus.NA),
            (enrol2, 3, new DateOnly(2026, 1, 26), AttendanceStatus.Present),
            (enrol2, 4, new DateOnly(2026, 2, 2),  AttendanceStatus.Present),

            (enrol4, 1, new DateOnly(2026, 1, 12), AttendanceStatus.Present),
            (enrol4, 2, new DateOnly(2026, 1, 19), AttendanceStatus.Present),
            (enrol4, 3, new DateOnly(2026, 1, 26), AttendanceStatus.NA),
            (enrol4, 4, new DateOnly(2026, 2, 2),  AttendanceStatus.Present),
        };
            foreach (var (enrol, week, date, status) in attendanceData)
            {
                context.AttendanceRecords.Add(new AttendanceRecord
                {
                    CourseEnrolmentId = enrol.Id,
                    WeekNumber = week,
                    Date = date,
                    Status = status
                });
            }
            await context.SaveChangesAsync();

            var assign1 = new Assignment
            {
                Title = "OOP Fundamentals",
                MaxScore = 100,
                DueDate = new DateOnly(2026, 2, 20),
                CourseId = cSoftware.Id
            };
            var assign2 = new Assignment
            {
                Title = "Web Application Project",
                MaxScore = 100,
                DueDate = new DateOnly(2026, 4, 20),
                CourseId = cSoftware.Id
            };
            var assign3 = new Assignment
            {
                Title = "Networking Analysis",
                MaxScore = 100,
                DueDate = new DateOnly(2026, 2, 27),
                CourseId = cCyber.Id
            };
            context.Assignments.AddRange(assign1, assign2, assign3);
            await context.SaveChangesAsync();

            context.AssignmentResults.AddRange(
                new AssignmentResult
                {
                    AssignmentId = assign1.Id,
                    StudentProfileId = student1.Id,
                    Score = 85,
                    Feedback = "Excellent understanding of OOP principles."
                },
                new AssignmentResult
                {
                    AssignmentId = assign1.Id,
                    StudentProfileId = student2.Id,
                    Score = 71,
                    Feedback = "Good work, needs more detail on inheritance."
                },
                new AssignmentResult
                {
                    AssignmentId = assign3.Id,
                    StudentProfileId = student3.Id,
                    Score = 88,
                    Feedback = "Very thorough security analysis."
                }
            );
            await context.SaveChangesAsync();

            var exam1 = new Exam
            {
                Title = "Software Development — Semester 1 Exam",
                Date = new DateOnly(2026, 3, 10),
                MaxScore = 100,
                ResultsReleased = true,
                CourseId = cSoftware.Id
            };
            var exam2 = new Exam
            {
                Title = "Cybersecurity — Semester 1 Exam",
                Date = new DateOnly(2026, 3, 15),
                MaxScore = 100,
                ResultsReleased = false,
                CourseId = cCyber.Id
            };
            context.Exams.AddRange(exam1, exam2);
            await context.SaveChangesAsync();

            context.ExamResults.AddRange(
                new ExamResult
                {
                    ExamId = exam1.Id,
                    StudentProfileId = student1.Id,
                    Score = 76,
                    Grade = "B"
                },
                new ExamResult
                {
                    ExamId = exam1.Id,
                    StudentProfileId = student2.Id,
                    Score = 63,
                    Grade = "C"
                },
                new ExamResult
                {
                    ExamId = exam2.Id,
                    StudentProfileId = student3.Id,
                    Score = 83,
                    Grade = "B+"
                }
            );
            await context.SaveChangesAsync();
        }

        private static async Task CreateUser(
            UserManager<IdentityUser> userManager,
            string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
