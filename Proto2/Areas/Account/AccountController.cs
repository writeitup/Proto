﻿using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Proto2.Areas.SystemAdmin.Models;
using Proto2.Areas.Teacher.Models;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using RavenDB.AspNet.Identity;
using Proto2.Areas.Student.Models;
using StructureMap.Pipeline;
using Proto2.Areas.Reviewer.Models;

namespace Proto2.Areas.Account
{
    [Authorize]
    public class AccountController : Controller
    {
        public IDocumentSession DocumentSession { get; set; }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            using (DocumentSession)
            {
                if (filterContext.Exception != null)
                    return;

                if (DocumentSession != null)
                    DocumentSession.SaveChanges();
            }
        }

        public AccountController()
        {
            this.UserManager = new UserManager<ProtoUser>(
                new UserStore<ProtoUser>(() => this.DocumentSession));
        }

        public UserManager<ProtoUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel
            {
            });
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            // TODO: Not logging in, just using last recognized log in, it recognizes the user, and then fowards to correct page
            // but then that USer.Identity.GetUserID is that of the last authenticated user, through registration.
            // Even when clearing browser cache first, which shouldn't even be necessary
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    return View(model);
                }

                await SignInAsync(user, model.RememberMe);
              
                if (user.Roles.Contains(ProtoRoles.Teacher))
                    return RedirectToAction("Index", "TeacherHome", new {area = "Teacher"});

                if (user.Roles.Contains(ProtoRoles.Reviewer))
                    return RedirectToAction("Index", "ReviewerHome", new {area = "Reviewer"});

                if (user.Roles.Contains(ProtoRoles.SystemAdmin))
                    return RedirectToAction("Index", "SystemAdminHome", new {area = "SystemAdmin"});

                if (user.Roles.Contains(ProtoRoles.Student))
                {
                    return RedirectToAction("Index", "StudentHome", new {area = "Student"});
                }

            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterModel
            {
                KeyList = new List<SelectListItem>(){
                    new SelectListItem(){ Text="Student", Value="Student" } ,
                    new SelectListItem(){ Text="Teacher", Value="Teacher" } ,
                    new SelectListItem(){ Text="Reviewer", Value="Reviewer" } ,
                   }
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            //if (model.ConfirmCode != "ImaTeacher")
            //{
            //    ModelState.AddModelError("", "The user name or password provided is incorrect.");
            //}

            if (ModelState.IsValid)
            {
                if (model.AccountType == "Teacher")
                {
                    if (model.ConfirmCode == 0)
                    {
                        ModelState.AddModelError("", "Please enter the code provided by yours System Administrator.");
                        model.KeyList = new List<SelectListItem>()
                        {
                            new SelectListItem() {Text = "Student", Value = "Student"},
                            new SelectListItem() {Text = "Teacher", Value = "Teacher"},
                            new SelectListItem() {Text = "Reviewer", Value = "Reviewer"},
                        };
                        return View(model);
                    }
                    var code = DocumentSession.Load<AddPassView>(model.ConfirmCode);
                    if (code == null)
                    {
                        ModelState.AddModelError("", "The code entered is either incorrect or no longer valid.");
                        model.KeyList = new List<SelectListItem>()
                        {
                            new SelectListItem() {Text = "Student", Value = "Student"},
                            new SelectListItem() {Text = "Teacher", Value = "Teacher"},
                            new SelectListItem() {Text = "Reviewer", Value = "Reviewer"},
                        };
                        return View(model);
                    }
                }
                var user = new ProtoUser { UserName = model.Email, FirstName = model.FirstName, LastName = model.LastName};
                if (model.AccountType == "Teacher")
                {
                    user.Roles.Add(ProtoRoles.Teacher);
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var teacher = new TeacherModel()
                        {
                            Id = "Teacher/" + model.Email,
                            Name = model.FirstName + " " + model.LastName,
                            Classes = new List<Guid>()
                        };
                        DocumentSession.Store(teacher);
                        DocumentSession.Delete<AddPassView>(model.ConfirmCode);
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "TeacherHome", new { area = "Teacher" });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                if (model.AccountType == "Student"){
                    user.Roles.Add(ProtoRoles.Student);
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        // Make the studen't first model
                        var s = new StudentModel()
                        {
                            Id = "StudentModels/" + model.Email,
                            Name = user.FirstName +" "+ user.LastName,
                            ClassIDs = new List<Guid>().ToArray(),
                            
                            //Submissions = new List<SubmissionView>().ToArray()
                        };
                        DocumentSession.Store(s);
                        DocumentSession.SaveChanges();

                        return RedirectToAction("Index", "StudentHome", new { area = "Student" });
                    }
                    else
                    {   
                        AddErrors(result);
                    }
                }
                if (model.AccountType == "Reviewer")
                {
                    user.Roles.Add(ProtoRoles.Reviewer);
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        // Make a reviewer model
                        var r = new ReviewerModel()
                        {
                            Id = user.UserName,
                            Name = user.FirstName+ " " + user.LastName,
                            ClassIDs = new List<Guid>(),
                            Reviews = new List<PastReviewView>()
                        };
                        DocumentSession.Store(r);
                        DocumentSession.SaveChanges();

                        return RedirectToAction("Index", "ReviewerHome", new { area = "Reviewer" });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }                  
               /* else
                {
                    user.Roles.Add(ProtoRoles.Reviewer);
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "ReviewerHome", new { area = "Reviewer" });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }*/
            }

         //   model.KeyList = new SelectList(new[] {"Student", "Teacher", "Reviewer"}, "AccountType");

            // If we got this far, something failed, redisplay form
            var errorModel = new RegisterModel()
            {
                AccountType = model.AccountType,
                ConfirmCode = model.ConfirmCode,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                KeyList = new List<SelectListItem>(){
                    new SelectListItem(){ Text="Student", Value="Student" } ,
                    new SelectListItem(){ Text="Teacher", Value="Teacher" } ,
                    new SelectListItem(){ Text="Reviewer", Value="Reviewer" } ,
                   }
            };

  
            return View(errorModel);
        }

        [AllowAnonymous]
        public ActionResult RegisterTeacher()
        {
            //return View(new RegisterTeacherModel
            //{
            //    GradeKeyList = new SelectList(new[] { "1st", "2nd", "3rd" }, "Grade"),
            //});
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTeacher(RegisterTeacherInput model)
        {
            if (model.ConfirmCode != "01234")
            {
                ModelState.AddModelError("", "The confirmation code is invalid.");
            }

            if (ModelState.IsValid)
            {
                var user = new ProtoUser { UserName = model.Email, FirstName = model.FirstName, LastName = model.LastName };
                user.Roles.Add(ProtoRoles.Teacher);

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "TeacherHome", new { area = "Teacher" });
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterStudent()
        {
            return View(new RegisterStudentModel
            {
                //GradeKeyList = new SelectList(new[] { "1st", "2nd", "3rd" }, "Grade"),
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStudent(RegisterStudentModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ProtoUser() { UserName = model.FirstName };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "StudentHome", new { area = "Student" });
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterReviewer()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterReviewer(RegisterReviewerModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ProtoUser() { UserName = model.FirstName };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "ReviewerHome", new { area = "Reviewer" });
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login", "Account", new {area = "Account"});
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ProtoUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home", new {area = ""});
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ProtoUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }

    public class ProtoRoles
    {
        public const string Teacher = "Proto/Teacher";
        public const string Student = "Proto/Student";
        public const string Reviewer = "Proto/Reviewer";
        public const string SystemAdmin = "Proto/SystemAdmin";
    }
}