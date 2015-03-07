﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Proto2.Areas.Teacher.Models
{

    public class ClassViewModel
    {
        public string teacherID { get; set; }
        public Guid id { get; set; }
        public int ConfirmCode { get; set; }
        public DateTime EndDate { get; set; }
        // List of students
        // When student adds guid on their page, it adds their names to this list
        public string[] Students { get; set; }
        // List of reviewers
        // When a reviewer agrees to review for this class it adds them to this list
        public string[] Reviewers { get; set; }
        public string className { get; set; }
    }

    public class TeacherModel
    {
        public string teacherID { get; set; }
        public string Name { get; set; }
        public string confirmCode { get; set; }
        // list of classIDs
        public string[] classIDs { get; set; }
    }


    public class StudentViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        // Add list of classes they are enrolled in, probably only one
        // ClassIDs are not the same as className, so knowing just the ID should be able to retrieve the teacherID
        public string classID { get; set; }
        public int NumReviews { get; set; }
        public string Confirmed { get; set; }
        public string teacherID { get; set; }

    }

    public class AddStudentInput
    {
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [DisplayName("Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public Guid Id { get; set; }
    }

    public class AddClassInput
    {
        [Required]
        [DisplayName("Class ID")]
        public string classID { get; set; }

        [Required]
        [DisplayName("Class Name")]
        public string className { get; set; }
        
        // This should not be required, we need to get the teacherID automatically by whos logged into the area
        [Required]
        [DisplayName("Your ID")]
        public String teacherID { get; set; }

    }

    public class StoryView
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Stories { get; set; }
    }

    public class ReviewView
    {
        public Guid Id { get; set; }
        public int ScorePlot { get; set; }
        public int ScoreCharacter { get; set; }
        public int ScoreSetting { get; set; }
        public string Comment { get; set; }
        public string ReviewerName { get; set; }
    }

}