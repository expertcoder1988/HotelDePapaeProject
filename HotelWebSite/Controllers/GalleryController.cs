using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelDePapaeMVC.Controllers
{
    public class GalleryController : Controller
    {
       [Route("~/depapae-hotel-islamabad-pictures", Name = "imageshoteldepapae")]

        // GET: Gallery
        public ActionResult Index()
        {
            return View();
        }
    }
}