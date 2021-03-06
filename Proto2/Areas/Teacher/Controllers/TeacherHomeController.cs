﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using WriteItUp2.Areas.Account;
using WriteItUp2.Areas.Reviewer.Models;
using WriteItUp2.Areas.Student.Models;
using WriteItUp2.Areas.SystemAdmin.Models;
using WriteItUp2.Areas.Teacher.Models;
using WriteItUp2.Areas.Teacher.Indexes;
using System.Linq;
using Raven.Abstractions.Linq;
using Raven.Client;
using RavenDB.AspNet.Identity;
using ClassModel = WriteItUp2.Areas.Teacher.Models.ClassModel;
using StoryView = WriteItUp2.Areas.Teacher.Models.StoryView;
using SubmissionView = WriteItUp2.Areas.Student.Models.SubmissionView;


namespace WriteItUp2.Areas.Teacher.Controllers
{

    public class TeacherHomeController : Controller
    {
        //This will get set by dependency injection. Look at DependencyResolution\RavenRegistry
        public IDocumentSession DocumentSession { get; set; }
        public TeacherHomeController()
        {
            this.UserManager = new UserManager<WriteItUpUser>(
                new UserStore<WriteItUpUser>(() => this.DocumentSession));
        }


        public UserManager<WriteItUpUser> UserManager { get; private set; }
        // This is also the classView, the teacher home defaults to viewing their classes
        
        [Authorize(Roles=WriteItUpRoles.Teacher)]
        public ActionResult Index()
        {
            var courses = DocumentSession.Query<ClassModel>()
                // How to make it pull based on teacherID
                               .Where(r => r.teacherId == User.Identity.GetUserId())
                               .ToList();

            return View(courses);
        }

                [Authorize(Roles = WriteItUpRoles.Teacher)]

        public ActionResult AddClass()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult AddClass(AddClassInput input)
        {
            var random = new Random();
            var code = random.Next(1000, 9999);
            var course = new ClassModel()
            {
                Id = Guid.NewGuid(),
                ClassName = input.ClassName,
                teacherId = User.Identity.GetUserId(),
                EndDate = input.EndDate,
                ConfirmCode = code.ToString(),
                Students =  new List<string>(),
                Reviewers = new List<string>(),
            };

            var teacher = DocumentSession.Load<TeacherModel>("Teacher/" +User.Identity.Name);
            teacher.Classes.Add(course.Id);
           
            DocumentSession.Store(course);
            DocumentSession.SaveChanges();

            return RedirectToAction("Index");

        }
        // Students are enrolling themselves by adding classCodes
        /* 
         public ActionResult AddStudent()
         {
             return View();
         }

        [HttpPost]
         public ActionResult AddStudent(AddStudentInput input)
         {
             var student = new StudentViewModel()
             {
                 Confirmed = "Not Confirmed",
                 Id = new Guid("ED88C65A-77D4-4C57-976D-7E3E5303E6CA"),
                 Name = input.FirstName + " " + input.LastName,
                 NumReviews = 0
             };
             DocumentSession.Store(student);
             DocumentSession.SaveChanges();

             return View(student);

         }*/

        //TODO:  Pulling data from the database using fake data until the class view for teacher is implemented
        [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewStudents(Guid classID)
        {
            var studentList = DocumentSession.Query<StudentModel>()
                // How to make it pull based on teacherID?
                                .Where(x => x.ClassIDs.Contains<Guid>(classID))// classID associated with the link from the class button
                                .ToList();
            var students = new StudentViewList()
            {
                StudentList = studentList,
                CourseId = classID
            };
            return View(students);

        }
        
        [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewReviewer(Guid classID)
        {
            var reviewerList = DocumentSession.Query<ReviewerModel>()
                // How to make it pull based on teacherID?
                              .Where(x => x.ClassIDs.Contains<Guid>(classID))// classID associated with the link from the class button
                              .ToList();
            var reviewers = new ReviewerViewList()
            {
                ReviewerList = reviewerList,
                CourseId = classID
            };
            return View(reviewers);
        }

       [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewClasses(string teacherId)
        {
            var courses = DocumentSession.Query<ClassModel, ViewStudentsIndex>()
                // How to make it pull based on teacherID?
                                .Where(r => r.teacherId == teacherId)// classID associated with the link from the class button
                                .ToList();

            return View(courses);

        }

       [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewStory(Guid studentId)
        {
            var model = new List<StoryView>
            {
                new StoryView()
                {
                    Title = "Best Story Ever Written",
                    Stories ="This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title This is the greatest story I've ever read in my life. Can't you tell by the title",
                    Id = Guid.NewGuid()
                },
                new StoryView()
                {
                    Title ="Worst Story Ever",
                    Stories="Boo",
                    Id = Guid.NewGuid()
                }
            };
            return View(model);
        }

              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewReviews(Guid courseId)
        {
            var model = new List<ReviewView>
            {
                new ReviewView()
                {
                    ReviewerName = "Best Reviewer Ever",
                    Comment = "This was the greatest thing I've ever read",
                    ScoreCharacter = 6,
                    ScorePlot = 7,
                    ScoreSetting = 0,
                    Id = Guid.NewGuid()
                }
            };
            return View(model);
        }

              [Authorize(Roles = WriteItUpRoles.Teacher)]
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

              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewAddAssignments(Guid classid)
        {
           var assignments = DocumentSession.Query<AssignmentView>().ToList();
           

            var assignmentAdd = new AssignmentAddInput()
            {
                Assignments = assignments,
                CourseId = classid
            };
            return View(assignmentAdd);
        }

              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewAssignmentDetails(Guid classid)
        {
            throw new NotImplementedException();
        }

              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult AddAssignment(AssignmentAddInput course)
        {
            return View();
        }


              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewAssignmentDetailed(AssignmentView asgn)
        {

            return View(asgn);
        }

              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewAddAssignmentDetailed(Guid asgnId, Guid courseId)
        {
            var asgn = DocumentSession.Load<AssignmentView>(asgnId);
            var addAsgn = new AssignmentInputView()
            {
                AssignmentName = asgn.AssignmentName,
                Description = asgn.Description,
                CourseId = courseId,
                Id = Guid.NewGuid(),
                Link = asgn.Link,
                DueDate = DateTime.Now.AddDays(7)
            };

            return View(addAsgn);
        }
        [HttpPost]
        [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewAddAssignmentDetailed(AssignmentInputView asgn)
        {
            
            DocumentSession.Store(asgn);
            DocumentSession.SaveChanges();

            return RedirectToAction("ViewAssignments", new {classid = asgn.CourseId});
        }

             [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult DeleteStudent( Guid courseId, string dataId)
        {
            var courses = DocumentSession.Load<ClassModel>(courseId);
            if (courses != null)
            {
                courses.Students.Remove(dataId);// = courses.Students.Where(val => val != student);  //change from array to list of students, may need to be revised
                var random = new Random();
                var code = random.Next(1000, 9999);
                courses.ConfirmCode = code.ToString();
            }

            var studentName = DocumentSession.Load<StudentModel>(dataId);
            if (studentName != null)
            {
                studentName.ClassIDs = studentName.ClassIDs.Where(val => val != courseId).ToArray();
            }
       
            DocumentSession.SaveChanges();

            return RedirectToAction("ViewStudents", new {classid = courseId});
        }

              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult DeleteReviewer(Guid courseId, string dataId)
        {
            var courses = DocumentSession.Load<ClassModel>(courseId);
            if (courses != null)
            {
                courses.Reviewers.Remove(dataId);// = courses.Students.Where(val => val != student);  //change from array to list of students, may need to be revised
                var random = new Random();
                var code = random.Next(1000, 9999);
                courses.ConfirmCode = code.ToString();
            }

            var reviewer = DocumentSession.Load<ReviewerModel>(dataId);
            if (reviewer != null)
            {
                reviewer.ClassIDs.Remove(courseId);
            }

            DocumentSession.SaveChanges();

            return RedirectToAction("ViewReviewer", new { classid = courseId });
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult RegisterAsReviewer()
        {
            if (UserManager.IsInRole(User.Identity.GetUserId(), WriteItUpRoles.Reviewer))
            {
                return RedirectToAction("Index", "ReviewerHome", new {area = "Reviewer"});

            }
         
            else
            {
                return View();
            }
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult RegisterTeacherAsReviewer()
        {
            UserManager.AddToRole(User.Identity.GetUserId(), WriteItUpRoles.Reviewer);

            var r = new ReviewerModel()
            {
                Id = User.Identity.Name,
                Name = User.Identity.Name,
                ClassIDs = new List<Guid>(),
                Reviews = new List<PastReviewView>()
            };
            DocumentSession.Store(r);
            DocumentSession.SaveChanges();
            while (true)
            {
                var tmpUser = DocumentSession.Load<WriteItUpUser>(User.Identity.GetUserId());
                if (tmpUser.Roles.Contains(WriteItUpRoles.Reviewer))
                    break;
            }

            return RedirectToAction("LogOff", "Account", new { area = "Account" });
          //  while (!User.IsInRole(WriteItUpRoles.Reviewer)) { }

           // return RedirectToAction("Index", "ReviewerHome", new { area = "Reviewer" });
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ExtendDueDate(DateTime date, Guid id)
        {
            ViewData.Add("CurrentDueDate", date);
            
            return View(new AssignmentInputView{Id = id, DueDate = date.AddDays(1)});
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        [HttpPost]
        public ActionResult ExtendDueDate(AssignmentInputView assignmentInputView)
        {
            var assign = DocumentSession.Load<AssignmentInputView>(assignmentInputView.Id);
                assign.DueDate = assignmentInputView.DueDate;

                DocumentSession.SaveChanges();
                return RedirectToAction("ViewAssignments", new {classId = assign.CourseId});
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult DeleteCourse(Guid courseId)
        {
            var classModel = DocumentSession.Load<ClassModel>(courseId);
            foreach (var s in classModel.Students)
            {
                var studentName = DocumentSession.Load<StudentModel>(s);
                if (studentName != null)
                {
                    studentName.ClassIDs = studentName.ClassIDs.Where(val => val != courseId).ToArray();
                }

            }

            foreach (var r in classModel.Reviewers)
            {
                var reviewer = DocumentSession.Load<ReviewerModel>(r);
                if (reviewer != null)
                {
                    reviewer.ClassIDs.Remove(courseId);
                }
            }
            DocumentSession.Delete<ClassModel>(courseId);
            DocumentSession.SaveChanges();
            return RedirectToAction("Index");
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult ViewStudentAssignments(string studentid)
        {
            var userName = "StudentModels/" + User.Identity.Name;
      
                // Get SubmissionView that matches this assignment id and user
                var prev = DocumentSession.Query<SubmissionView>()
                        .Where(a => a.StudentId == studentid)
                        .ToList();
            return View(prev);
                                // If first time loading write page, make a StoryInput Model and return it
               }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult CurrentAssignment(string story)
        {
            var assign = DocumentSession.Load<SubmissionView>(story);
            assign.Story = Regex.Replace(assign.Story, @"<[^>]+>|&nbsp;", "").Trim();
            return View(assign);
        }
              [Authorize(Roles = WriteItUpRoles.Teacher)]
        public ActionResult CurrentReview(string revId, string assId)
        {
            var review = DocumentSession.Query<ReviewInputDatabase>().
                Where(x => x.SubmitId == assId && x.Username == revId).ToList().FirstOrDefault();
                  if (review != null)
                  {
                      var assignment = DocumentSession.Load<SubmissionView>(review.SubmitId);
                          ViewData.Add("AssignmentName", assignment.AssignmentName);
                  }

              

            return View(review);

        }
    }

}