﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Proto2.Areas.Teacher.Models;


namespace Proto2.Areas.Student.Models
{

    public class StudentModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        // list of classIDs
        public Guid[] ClassIDs { get; set; }
    }

    public class StudentAddClass
    {
        public string classCode { get; set; }
    }

    public class StudentClassModel
    {
        public string TeacherName { get; set; }
        public string ClassName { get; set; }
        public Guid courseId { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class StudentReviewView
    {
        public string Title { get; set; }
        public StoryReviewView ReviewOne { get; set; }
        public StoryReviewView ReviewTwo { get; set; }
    }

    public class SubmitDetails
    {
        public HtmlString Story { get; set; }
        public string SubmissionId { get; set; }
        public string AssignmentName { get; set; }
        public string Description { get; set; }
    }

    public class SubmissionView
    {
        public string Id { get; set; }
        public Guid classId { get; set; }
        public Guid AssignmentId { get; set; }
        public DateTime DueDate { get; set; }
        public string StudentId { get; set; }
        public string AssignmentName { get; set; }
        public string Description { get; set; }
        public DateTime SubmissionDate { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The story title is required, but may be changed at any time")]
        [Display(Name = "Story Title (Required)")]
        public string StoryTitle { get; set; }

        public string Story { get; set; }
        public int NumReviews { get; set; }
        public string reviewer1 { get; set; }
        public string reviewer2 { get; set; }
    }

    public class StoryReviewView
    {
        public int ScorePlot { get; set; }
        public int ScoreCharacter { get; set; }
        public int ScoreSetting { get; set; }
        public string Comments { get; set; }
        public int reviewNum { get; set; }
    }

    public class AssignmentsView
    {
        public AssignmentInputView[] Current { get; set; }
        public SubmissionView[] Submitted { get; set; }
    }

}