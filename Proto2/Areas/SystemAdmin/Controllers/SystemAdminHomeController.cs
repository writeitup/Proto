﻿using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Security;
using Proto2.Areas.Account;
using Proto2.Areas.SystemAdmin.Models;
using Raven.Client;
using Raven.Client.Document;

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
            var students = new List<StudentView>()
            {
                new StudentView()
                {
                    Name = "Alan Turing",
                    Email = "turing@email.com",
                    Confirmed = true,
                    Teacher = "Dr. Mocas"
                }
            };
            return View(students);
        }

        public ActionResult Teachers()
        {
            var teachers = new List<TeacherView>()
            {
                new TeacherView()
                {
                    Name = "Dr. Mocas",
                    Email = "mocas@email.com",
                    Confirmed = true,
                }
            };
            return View(teachers);
        }

        public ActionResult Reviewers()
        {
            var reviewers = new List<ReviewerView>()
            {
                new ReviewerView()
                {
                    Name = "The Best Reviewer",
                    Email = "reviewer@email.com",
                    Confirmed = true
                }
            };
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

        public ActionResult DeleteTeacher(Guid id)
        {
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
            var video = new VideoView()
            {
                Link = input.Link,
                Title = input.Title
            };
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
            var codes = DocumentSession.Query<AddPassView>().Customize(x =>x.WaitForNonStaleResultsAsOf(DateTime.Now.AddSeconds(1))).OrderByDescending(RetrieveDate).ToList();
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
    }
}