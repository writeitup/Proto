﻿using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Proto2.Areas.Account;
using Proto2.Areas.Reviewer.Models;
using Proto2.Areas.Student.Models;
using Proto2.Areas.SystemAdmin.Models;
using Proto2.Areas.Teacher.Models;
using Raven.Client;
using Raven.Client.Document;
using StoryView = Proto2.Areas.SystemAdmin.Models.StoryView;
using VideoView = Proto2.Areas.SystemAdmin.Models.VideoView;

namespace Proto2.Areas.SystemAdmin.Controllers
{
    public class SystemAdminHomeController : Controller
    {
        //This will get set by dependency injection. Look at DependencyResolution\RavenRegistry
        public IDocumentSession DocumentSession { get; set; }

        //TODO: Feel free to add 'fake data' anywhere an empty
        //list is being returned see Students below for an example
        //[Authorize(Roles = ProtoRoles.SystemAdmin)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Students()
        {
            var students = DocumentSession.Query<StudentModel>().ToList();
            return View(students);
        }

        public ActionResult Teachers()
        {
            //need to make teacher model to save data
            var teachers = DocumentSession.Query<TeacherModel>().ToList();

            return View(teachers);
        }

        public ActionResult Reviewers()
        {
            var reviewers = DocumentSession.Query<ReviewerModel>().ToList();

            return View(reviewers);
        }

        public ActionResult ViewStudentsByTeacher()
        {
            var students = new List<StudentView>();
            return View(students);
        }

        public ActionResult ConfirmTeacher(Guid id)
        {
            return RedirectToAction("Teachers");
        }

        public ActionResult DeleteTeacher(string id)
        {
            //delete proto user, teacher model, courses associated with teacher,
            //for each course, remove it from associated students and reviewers

            var teacher = DocumentSession.Load<TeacherModel>(id);

            var courses = teacher.Classes;
            foreach (var c in courses)
            {
                var classModel = DocumentSession.Load<ClassModel>(c);
                foreach (var s in classModel.Students)
                {
                    var studentName = DocumentSession.Load<StudentModel>(s);
                    if (studentName != null)
                    {
                        studentName.ClassIDs = studentName.ClassIDs.Where(val => val != c).ToArray();
                    }

                }

                foreach (var r in classModel.Reviewers)
                {
                    var reviewer = DocumentSession.Load<ReviewerModel>(r);
                    if (reviewer != null)
                    {
                        reviewer.ClassIDs.Remove(c);
                    }
                }
                DocumentSession.Delete<ClassModel>(c);


            }
            var teacherId = teacher.Id.Split('/');
            var protoUser = DocumentSession.Load<ProtoUser>("ProtoUsers/" + teacherId[1]);
            protoUser.Roles.Remove(ProtoRoles.Teacher);
            if (!protoUser.Roles.Any())
            {
                DocumentSession.Delete(protoUser.Id);
            }
            DocumentSession.Delete(id);

            DocumentSession.SaveChanges();
            return RedirectToAction("Teachers");
        }

        public ActionResult StoryView()
        {
            var stories = new List<StoriesView>
            {
                new StoriesView()
                {
                    Title = "The Magnificent Unicorn",
                    Story =
                        "The magnificent unicorn (TMU) is the rarest of all creatures on earth. This beast stands over 6 feet tall, has a mane of rainbow colored hair, eyes that shine like two amethysts, and a horn of pure gold. TMU has been spotted in regions of the world such as Atlantis, The North Pole, and Imagination Land.",
                    Author = "Unicorn Cat"
                }
            };
            return View(stories);
        }

        public ActionResult StoryReviewsView()
        {
            var reviews = new List<StoryReviewsView>
            {
                new StoryReviewsView()
                {
                    ScorePlot = 5,
                    ScoreCharacter = 4,
                    ScoreSetting = 5,
                    Comments = "Develop a stronger plot and invest more thought to character development."
                }
            };
            return View(reviews);
        }

        public ActionResult ViewReviewsByReviewers(Guid id)
        {
            var reviews = new List<ReviewsView>()
            {
                new ReviewsView()
                {
                    Title = "The Best Story Ever",
                    ReviewOne = new StoryReviewsView()
                    {
                        ScorePlot = 5,
                        ScoreCharacter = 4,
                        ScoreSetting = 5,
                        Comments = "Develop a stronger plot and invest more thought to character development."
                    },
                    //ReviewTwo = new StoryReviewView(){
                    //    ScorePlot = 6,
                    //    ScoreCharacter = 4,
                    //    ScoreSetting = 6,
                    //    Comments = "Further develop the characters of your story."
                    //}
                }
            };
            return View(reviews);
        }

        public ActionResult ConfirmReviewer(Guid id)
        {
            return RedirectToAction("Reviewers");
        }

        public ActionResult DeleteReviewer(Guid id)
        {
            return RedirectToAction("Reviewers");
        }

        public ActionResult EditReviewerVideos()
        {
            var videos = new List<VideoView>();
            return View(videos);
        }

        public ActionResult AddReviewerVideo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddReviewerVideo(AddVideoInput input)
        {
            var splitLink = input.Link.Split('=');
            var videoID = splitLink[1];
            var video = new VideoView()
            {
                Link = videoID,
                Title = input.Title
            };

            DocumentSession.Store(video);
            DocumentSession.SaveChanges();
            var videos = new List<VideoView> {video};
            return View("EditReviewerVideos", videos);
        }

        public ActionResult DeleteVideo(Guid id)
        {
            return RedirectToAction("EditStudentVideos");
        }

        public ActionResult ViewStoriesByStudent(Guid id)
        {
            var stories = new List<StoryView>()
            {
                new StoryView()
                {
                    Author = "Alan Turing",
                    StoryOne = new StoriesView()
                    {
                        Title = "The Magnificent Unicorn",
                        Story =
                            "The magnificent unicorn (TMU) is the rarest of all creatures on earth. This beast stands over 6 feet tall, has a mane of rainbow colored hair, eyes that shine like two amethysts, and a horn of pure gold. TMU has been spotted in regions of the world such as Atlantis, The North Pole, and Imagination Land.",
                        Author = "Unicorn Cat"
                    }
                }
            };
            return View(stories);
        }

        public ActionResult ConfirmStudent(Guid id)
        {
            return RedirectToAction("Students");
        }

        public ActionResult DeleteStudent(Guid id)
        {
            return RedirectToAction("Students");
        }

        public ActionResult EditStudentVideos()
        {
            var videos = new List<VideoView>();
            return View(videos);
        }

        public ActionResult AddStudentVideo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddStudentVideo(AddVideoInput input)
        {
            var video = new VideoView()
            {
                Link = input.Link,
                Title = input.Title
            };
            var videos = new List<VideoView> {video};
            return View("EditStudentVideos", videos);
        }


        //public ActionResult DeleteStudentVideo()
        //{
        //    return RedirectToAction("EditStudentVideos");
        //}

        public ActionResult AddPass()
        {
            var codes =
                DocumentSession.Query<AddPassView>()
                    .Customize(x => x.WaitForNonStaleResultsAsOf(DateTime.Now.AddSeconds(1)))
                    .OrderByDescending(RetrieveDate)
                    .ToList();
            return View(codes);
        }


        public ActionResult AddPassGenerate()
        {
            var random = new Random();
            var code = random.Next(1000, 9999);
            var codeView = new AddPassView
            {
                Id = code,
                DateAdded = DateTime.Now,
                InUse = false,
            };

            DocumentSession.Store(codeView);
            DocumentSession.SaveChanges();
            return RedirectToAction("AddPass");
        }

        public ActionResult MarkCodeAsUsed(int code)
        {
            var codeModel = DocumentSession.Load<AddPassView>(code);
            codeModel.InUse = !codeModel.InUse;
            DocumentSession.SaveChanges();

            return RedirectToAction("AddPass");
        }

        public ActionResult DeleteCode(int code)
        {
            DocumentSession.Delete<AddPassView>(code);
            DocumentSession.SaveChanges();

            return RedirectToAction("AddPass");
        }

        public ActionResult RemoveStudent(string studentID, string dataID)
        {
            var random = new Random();
            var courses = DocumentSession.Query<ClassModel>()
                .Where(r => r.Students.Contains(studentID))
                .ToList();
            foreach (ClassModel course in courses)
            {
                course.ConfirmCode = random.Next(1000, 9999).ToString();
            }
            DocumentSession.Delete(dataID);
            DocumentSession.SaveChanges();
            return RedirectToAction("Students");
        }

        public ActionResult AddAssignment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddAssignment(AssignmentView input)
        {
            var splitLink = input.Link.Split('=');
            var videoID = splitLink[1];
            var asgn = new AssignmentView()
            {
                Id = Guid.NewGuid(),
                AssignmentName = input.AssignmentName,
                Description = input.Description,
                Link = videoID
            };

            DocumentSession.Store(asgn);
            DocumentSession.SaveChanges();
            return RedirectToAction("ViewAssignment");
        }

        public ActionResult ViewAssignment()
        {
            var assignment = DocumentSession.Query<AssignmentView>()
                .Where(r => r.Id != null)
                .ToList();

            return View(assignment);
        }

        public ActionResult ViewAssignmentDetailed(AssignmentView asgn)
        {
            return View(asgn);
        }

        public DateTime RetrieveDate(AddPassView input)
        {
            return input.DateAdded;
        }

        public ActionResult ViewClasess(string teacherid)
        {
            var teacher = teacherid.Split('/');
           teacherid = "ProtoUsers/" + teacher[1];
            var courses = DocumentSession.Query<ClassModel>()
                // How to make it pull based on teacherID
                .Where(r => r.teacherId == teacherid)
                .ToList();

            return View(courses);
        }

        public ActionResult ViewAssignments(Guid classid)
        {
            var assignments = DocumentSession.Query<AssignmentInputView>()
                .Where(x => x.CourseId == classid).ToList();
            var assignmentList = new AssignmentViewList()
            {
                Assignments = assignments,
                CourseId = classid
            };

            return View(assignmentList);
        }

        public ActionResult ViewReviewer(Guid classID)
        {
            var reviewerList = DocumentSession.Query<ReviewerModel>()
                // How to make it pull based on teacherID?
                .Where(x => x.ClassIDs.Contains<Guid>(classID))
                // classID associated with the link from the class button
                .ToList();
            var reviewers = new ReviewerViewList()
            {
                ReviewerList = reviewerList,
                CourseId = classID
            };
            return View(reviewers);
        }

        public ActionResult ViewStudents(Guid classID)
        {
            var studentList = DocumentSession.Query<StudentModel>()
                // How to make it pull based on teacherID?
                .Where(x => x.ClassIDs.Contains<Guid>(classID))
                // classID associated with the link from the class button
                .ToList();
            var students = new StudentViewList()
            {
                StudentList = studentList,
                CourseId = classID
            };
            return View(students);

        }
    }
}