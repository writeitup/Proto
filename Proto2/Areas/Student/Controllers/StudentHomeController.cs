﻿using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Web.Mvc;
using Raven.Client;
using Microsoft.AspNet.Identity;
using Raven.Client.Document;
using Proto2.Areas.Teacher.Models;
using Proto2.Areas.Reviewer.Models;
using Proto2.Areas.Student.Indexes;
using Proto2.Areas.Student.Models;

namespace Proto2.Areas.Student.Controllers
{
    public class StudentHomeController : Controller
    {
        //This will get set by dependency injection. Look at DependencyResolution\RavenRegistry
        public IDocumentSession DocumentSession { get; set; }    

        //
        // GET: /Student/
        public ActionResult Index()
        {
            // Hardcoded for tesing because login is broken
            //string userID = "1234";
            var models = new List<ClassModel>();
            var courses = DocumentSession.Query<ClassModel>()
                         .ToList();

            // TODO: There has to be a better way than this
            // TODO: Think it would be better to pull classes from the student's profile
            // then find infor on the classes that pull...need the log in model to be fixed for that
            // If there are a lot of courses this could take a long time to run
            // Calling the .Contains in the query did not work though
            for (int i = 0; i < courses.Count; i++)
            {
                if(courses[i].Students.Contains(User.Identity.GetUserId())){
                    models.Add(courses[i]);
                }
            }
            return View(models);
        }

        public ActionResult StudentAddClass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StudentAddClass(StudentAddClass input)
        {
            //string hardcodedIDForTesting = "1234";
             //Query classes for this confCode and then add student to the list
             //If there is one, then add load that specific object and add the student to the array
            var courses = DocumentSession.Query<ClassModel, StudentAddClassIndex>()
                         .Where(c => c.ConfirmCode == input.classCode)
                         .ToList();


            // This not working because log in is not tracking who the actual logged in identity is.
            string stu = User.Identity.GetUserId();

            var student = DocumentSession.Query<StudentModel, AddClassToStudentIndex>()
                          .Where(s => s.StudentID == User.Identity.GetUserId())
                          .ToList();

            if (courses.Count != 0 && student.Count != 0)
            {

                string id = courses[0].Id;
                // Having this Id attribute that gets set by RavenDb 
                // allows for retrieval of the exact object that can be updated or deleted
                // by using the Load command that uses a document Id
                ClassModel course = DocumentSession.Load<ClassModel>(id);
                List<string> list = course.Students.ToList();
                list.Add(User.Identity.GetUserId());
                course.Students = list.ToArray();
                //DocumentSession.SaveChanges();

                string ids = student[0].Id;
                // Having this Id attribute that gets set by RavenDb 
                // allows for retrieval of the exact object that can be updated or deleted
                // by using the Load command that uses a document Id
                StudentModel st = DocumentSession.Load<StudentModel>(ids);
                List<Guid> listS = st.ClassIDs.ToList();
                listS.Add(course.id);
                st.ClassIDs = listS.ToArray();
                
                DocumentSession.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }

        }

        public ActionResult ViewAssignments(Guid classID)
        {
            var assigns = new List<AssignmentsView>();
            // TODO: Finish implementation after some sore of class relation is added 
            // after adding assignments to courses on teacher end
            //var courses = DocumentSession.Query<ClassModel>()
            //             .Where(c => c.id == classID)
            //             .ToList();

            var student = DocumentSession.Query<StudentModel>()
                               .Where(s => s.StudentID == User.Identity.GetUserId())
                               .ToList();

            if (/*courses.Count != 0 && */student.Count != 0)
            {
                AssignmentsView av = new AssignmentsView()
                {
                    // Guessing at this since assignments seem to be unrelated still
                    //Current = courses[0].Assignments;
                    Submitted = student[0].Submissions

                };
                assigns.Add(av);
            }

            return View(assigns);
        }

        /*public ActionResult CurrentAssignment(AssignmentView input)
        {
            var assignment = new List<AssignmentView>();
            assignment.Add(input);

            return View(assignment);
        }*/

        public ActionResult submissionView(SubmissionView input)
        {
            var submission = new List<SubmissionView>();
            submission.Add(input);

            return View(submission);
        }

        public ActionResult Write()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Write(StoryInput input)
        {
            string hardCodedId = "1234";
            StoryInput story = new StoryInput()
            {
                //StudentId = User.Identity.GetUserId(),
                StudentId = hardCodedId,
                Story = input.Story
            };
            DocumentSession.Store(story);
            DocumentSession.SaveChanges();

            return View();
        }

        public ActionResult Train()
        {
            return View();
        }

        public ActionResult BrainStorm()
        {
            return View();
        }

        public ActionResult Reviews()
        {
            //TODO: this should return a list of ReviewView modles
            //Default review, will pull reviews from database but will use this as default for now.
            var ReviewsList = new List<StudentReviewView>(){
                new StudentReviewView(){
                    Title = "The Best Story Ever",
                    ReviewOne = new StoryReviewView(){
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
            return View(ReviewsList);
        }

        public ActionResult StoryReview()
        {
            //TODO: this should return a list of StoryReviewViews
            //Default review, will pull reviews from database but will use this as default for now.
            var StoryReviewsList = new List<StoryReviewView>(){
                new StoryReviewView(){
                       ScorePlot = 5,
                       ScoreCharacter = 4,
                       ScoreSetting = 5,
                       Comments = "Develop a stronger plot and invest more thought to character development."
                }
            };
            return View(StoryReviewsList);
        }

    }
}