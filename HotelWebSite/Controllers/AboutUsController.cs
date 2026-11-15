using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelDePapaeMVC.Controllers
{
    public class AboutUsController : Controller
    {
       [Route("~/about-hotel-islamabad", Name = "abouthoteldepapae")]
        // GET: AboutUs
        public ActionResult Index()
        {
            return View();
        }
    }
}