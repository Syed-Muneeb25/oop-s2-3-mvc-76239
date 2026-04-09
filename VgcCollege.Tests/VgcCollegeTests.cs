using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using VgcCollege.Domain;
using VgcCollege.Web.Controllers;

namespace VgcCollege.Tests
{
    public class Tests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, context, results, true);

            return results;
        }

        // -----------------------------
        // GradeCalculator tests
        // -----------------------------

        [Fact]
        public void GradeCalculator_ReturnsA_WhenScoreIs70Percent()
        {
            // Arrange
            int score = 70;
            int maxScore = 100;

            // Act
            string result = GradeCalculator.Calculate(score, maxScore);

            // Assert
            Assert.Equal("A", result);
        }

        [Fact]
        public void GradeCalculator_ReturnsB_WhenScoreIs60Percent()
        {
            // Arrange
            int score = 60;
            int maxScore = 100;

            // Act
            string result = GradeCalculator.Calculate(score, maxScore);

            // Assert
            Assert.Equal("B", result);
        }

        [Fact]
        public void GradeCalculator_ReturnsC_WhenScoreIs50Percent()
        {
            // Arrange
            int score = 50;
            int maxScore = 100;

            // Act
            string result = GradeCalculator.Calculate(score, maxScore);

            // Assert
            Assert.Equal("C", result);
        }

        [Fact]
        public void GradeCalculator_ReturnsE_WhenScoreIsBelow40Percent()
        {
            // Arrange
            int score = 30;
            int maxScore = 100;

            // Act
            string result = GradeCalculator.Calculate(score, maxScore);

            // Assert
            Assert.Equal("E", result);
        }

        [Fact]
        public void GradeCalculator_ReturnsNA_WhenMaxScoreIsZero()
        {
            // Arrange
            int score = 25;
            int maxScore = 0;

            // Act
            string result = GradeCalculator.Calculate(score, maxScore);

            // Assert
            Assert.Equal("N/A", result);
        }

        // -----------------------------
        // Model validation tests
        // -----------------------------

        [Fact]
        public void Branch_ShouldFailValidation_WhenNameIsMissing()
        {
            // Arrange
            var branch = new Branch
            {
                Name = "",
                Address = "Dublin"
            };

            // Act
            var results = ValidateModel(branch);

            // Assert
            Assert.Contains(results, r => r.ErrorMessage == "Branch name is required");
        }

        [Fact]
        public void StudentProfile_ShouldFailValidation_WhenEmailIsInvalid()
        {
            // Arrange
            var student = new StudentProfile
            {
                StudentNumber = "ST001",
                Name = "Muneeb",
                Email = "wrong-email",
                Phone = "0871234567",
                Address = "Dublin",
                DateOfBirth = new DateOnly(2004, 1, 1)
            };

            // Act
            var results = ValidateModel(student);

            // Assert
            Assert.Contains(results, r => r.ErrorMessage == "Invalid email format");
        }

        [Fact]
        public void Assignment_ShouldFailValidation_WhenMaxScoreIsOutOfRange()
        {
            // Arrange
            var assignment = new Assignment
            {
                Title = "CA Project",
                MaxScore = 0,
                DueDate = new DateOnly(2026, 5, 1)
            };

            // Act
            var results = ValidateModel(assignment);

            // Assert
            Assert.Contains(results, r => r.ErrorMessage == "Max score must be between 1 and 1000");
        }

        [Fact]
        public void Exam_ShouldFailValidation_WhenTitleIsMissing()
        {
            // Arrange
            var exam = new Exam
            {
                Title = "",
                Date = new DateOnly(2026, 6, 10),
                MaxScore = 100
            };

            // Act
            var results = ValidateModel(exam);

            // Assert
            Assert.Contains(results, r => r.ErrorMessage == "Title is required");
        }

        // -----------------------------
        // Controller test
        // -----------------------------

        [Fact]
        public void HomeController_Index_ReturnsViewResult()
        {
            // Arrange
            var logger = new LoggerFactory().CreateLogger<HomeController>();
            var controller = new HomeController(logger);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}