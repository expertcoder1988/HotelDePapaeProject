using HotelWebSite.Models;
using HotelDePapaeMVC.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HotelDePapaeMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly HotelDBEntities HDE = new HotelDBEntities();
        private readonly EncryptionManager enc = new EncryptionManager();

        private const string UserSessionKey = "UserInfo";
        private const string AccessSessionKey = "Access";

        // GET: Login
        public ActionResult Index()
        {
            // If user is already authenticated, redirect to the dashboard
            if (Session[UserSessionKey] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        public ActionResult Logout()
        {
            // Clear session and authentication
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string ReturnUrl)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                password = Accessories.Encrypt(password);
                var user = (from u in HDE.Users
                            where u.vUserName == username && u.vPassword == password
                            select new
                            {
                                UserName = u.vUserName,
                                Password = password,
                                FullName = u.vFirstName + " " + u.vLastName,
                                Status = u.tiStatus,
                                UserID = u.intCityID,
                                ProfilePic = u.Image,
                                AccessType = u.tiAccessType
                            }).FirstOrDefault();

                if (user != null && !string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.Password))
                {
                    if (user.Status == 1)
                    {
                        FormsAuthentication.SetAuthCookie(user.UserName, false);

                        var access = (from u in HDE.Users
                                      where u.vUserName == username
                                      select new { Access = u.AccessCodes }).FirstOrDefault();

                        Session[AccessSessionKey] = access?.Access ?? "0,";
                        Session[UserSessionKey] = user;

                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        TempData["Error"] = GetStatusMessage(user.Status);
                        return RedirectToAction("Index", "Login");
                    }
                }
                else
                {
                    TempData["Error"] = "Invalid username or password.";
                }
            }
            else
            {
                TempData["Error"] = "Please provide username and password.";
            }

            return RedirectToAction("Index", "Login");
        }

        private string GetStatusMessage(byte? status)
        {
            if (status == null) return "User status is unavailable.";

            switch (status.Value)
            {
                case 0: return "Your request is pending. Please wait.";
                case 1: return "Your account is active.";
                case 2: return "Your credentials have been disabled.";
                case 3: return "An activation link has been sent to your email address.";
                default: return "Your account has been disabled.";
            }
        }

    }
}
