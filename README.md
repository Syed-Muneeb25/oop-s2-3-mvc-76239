ASP.NET Core MVC web application for a **multi-branch college management system**.  
The solution uses **ASP.NET Core Identity** for authentication and roles, **Entity Framework Core with SQLite** for data access, and includes a separate **test project**.

## Tech Stack

- ASP.NET Core MVC
- .NET 9
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- xUnit

## User Credentials

The application seeds three roles: **Admin**, **Faculty**, and **Student**.

### Admin
- **Email:** `admin@vgc.com`
- **Password:** `Admin@123`

### Faculty
- **Email:** `faculty1@vgc.com`
- **Password:** `Faculty@123`

- **Email:** `faculty2@vgc.com`
- **Password:** `Faculty@123`

### Students
- **Email:** `student1@vgc.com`
- **Password:** `Student@123`

- **Email:** `student2@vgc.com`
- **Password:** `Student@123`

- **Email:** `student3@vgc.com`
- **Password:** `Student@123`

## Seeded Roles

- Admin
- Faculty
- Student

## Main Features

- Role-based authentication and authorization
- Branch management
- Course management
- Faculty profile management
- Student profile management
- Attendance tracking
- Assignment management and assignment results
- Exam management and exam results
- Admin area for user administration

## Database

The application uses a **SQLite** database through the `DefaultConnection` connection string.

`ApplicationDbContext` includes these main entities:

- Branches
- Courses
- StudentProfiles
- FacultyProfiles
- CourseEnrolments
- AttendanceRecords
- Assignments
- AssignmentResults
- Exams
- ExamResults

## Project Structure

```text
oop-s2-3-mvc-76239/
├── VgcCollege.Domain/
│   ├── Assignment.cs
│   ├── AssignmentResult.cs
│   ├── AttendanceRecord.cs
│   ├── Branch.cs
│   ├── Course.cs
│   ├── CourseEnrollment.cs
│   ├── Exam.cs
│   ├── ExamResult.cs
│   ├── FacultyProfile.cs
│   ├── GradeCalculater.cs
│   ├── StudentProfile.cs
│   └── VgcCollege.Domain.csproj
│
├── VgcCollege.Tests/
│   ├── UnitTest1.cs
│   └── VgcCollege.Tests.csproj
│
├── VgcCollege.Web/
│   ├── Areas/
│   │   └── Identity/
│   │       └── Pages/
│   ├── Controllers/
│   │   ├── AdminController.cs
│   │   ├── AssignmentController.cs
│   │   ├── AttendanceController.cs
│   │   ├── BranchController.cs
│   │   ├── CourseController.cs
│   │   ├── ExamController.cs
│   │   ├── FacultyProfileController.cs
│   │   ├── HomeController.cs
│   │   └── StudentProfileController.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── DataSeeder.cs
│   ├── ExceptionalHandling/
│   │   └── ExceptionHandling.cs
│   ├── Models/
│   │   ├── CreateAdminViewModel.cs
│   │   ├── CreateFacultyViewModel.cs
│   │   ├── CreateStudentViewModel.cs
│   │   └── ErrorViewModel.cs
│   ├── Properties/
│   │   ├── launchSettings.json
│   │   ├── serviceDependencies.json
│   │   └── serviceDependencies.local.json
│   ├── Views/
│   │   ├── Admin/
│   │   ├── Assignment/
│   │   ├── Attendance/
│   │   ├── Branch/
│   │   ├── Course/
│   │   ├── Exam/
│   │   ├── FacultyProfile/
│   │   ├── Home/
│   │   ├── Shared/
│   │   ├── StudentProfile/
│   │   ├── _ViewImports.cshtml
│   │   └── _ViewStart.cshtml
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   ├── lib/
│   │   └── favicon.ico
│   ├── Program.cs
│   ├── VgcCollege.Web.csproj
│   ├── appsettings.Development.json
│   └── appsettings.json
│
├── .gitattributes
├── .gitignore
└── oop-s2-3-mvc-76239.sln
```

## How to Run

1. Clone the repository
2. Open the solution in Visual Studio
3. Apply migrations / update the database
4. Run the application
5. Log in using one of the seeded accounts above

## Notes

- Identity is configured with roles for **Admin**, **Faculty**, and **Student**
- The app calls `DataSeeder.SeedAsync(...)` on startup to create roles, users, and sample data
- Razor Pages are enabled for Identity pages
