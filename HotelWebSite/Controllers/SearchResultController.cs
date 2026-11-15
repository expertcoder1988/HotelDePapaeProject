using HotelDePapaeMVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using HotelWebSite.Models;

namespace HotelDePapaeMVC.Controllers
{
    public class SearchResultController : Controller
    {
        HotelDBEntities HDE = new HotelDBEntities();
        Booking bi = new Booking();

        // GET: SearchResult
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchResults(string CheckIn, string CheckOut, int Adults, int Children, int NoOfRooms)
        {
            try
            {
                //int PropertyID = 1;
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    string DateString = CheckIn;
                    IFormatProvider culture = new CultureInfo("en-US", true);
                    DateTime datestart = DateTime.ParseExact(DateString, "MM/dd/yyyy", culture);

                    string DateString2 = CheckOut;
                    IFormatProvider culture2 = new CultureInfo("en-US", true);
                    DateTime dateend = DateTime.ParseExact(DateString2, "MM/dd/yyyy", culture2);


                    //string[] sDate = CheckIn.Split('-');
                    //string startDate = sDate[1] + "/" + sDate[0] + "/" + sDate[2];
                    int id = 1;

                    //sDate = CheckOut.Split('-');
                    //string endDate = sDate[1] + "/" + sDate[0] + "/" + sDate[2];

                    IEnumerable<PropertySearch> rowsAffected = db.Database.SqlQuery<PropertySearch>("exec sp_GetSearchResults {0}, {1}, {2}, {3}, {4}",
                         Convert.ToDateTime(datestart), Convert.ToDateTime(dateend), Adults, Children, NoOfRooms).ToList();


                    foreach (var itm in rowsAffected)
                    {
                        int v = VacantRooms(itm.RoomCode, itm.RoomCateCode, CheckIn);
                        int b = BlockedRooms(itm.RoomCode, itm.RoomCateCode, CheckIn, CheckOut);
                        itm.NoOfRooms = itm.NoOfRooms - v - b;

                        if (itm.NoOfRooms < 0)
                        {
                            itm.NoOfRooms = itm.NoOfRooms;
                        }
                        //int b = BlockedRooms(itm.RoomCode, itm.RoomCateCode, CheckOut);
                        //itm.NoOfRooms = itm.NoOfRooms - v;
                        //itm.NoOfRooms = itm.NoOfRooms - b;

                        //if (itm.NoOfRooms < 0)
                        //{
                        //    itm.NoOfRooms = 0;
                        //}
                        itm.Id = id;
                        id++;
                    }

                    Session["results"] = rowsAffected;

                    RoomcatePics();

                    return View();
                    //return Json(rowsAffected, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //ViewBag.Error = ex.Message;
                return Json(ex.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public int VacantRooms(int? roomCode, int? RoomCateCode, string startDate)
        {
            try
            {
                int vacantRooms = 0;
                //int TotalRooms = 0;
                DateTime _startDate = Convert.ToDateTime(startDate);

                // string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    //TotalRooms = db.RoomNumbers.Count(x => x.RoomCode == roomCode && x.Status == false);

                    vacantRooms = db.Bookings.Count(x => x.RoomCode == roomCode && x.CheckIn == _startDate);
                }

                return vacantRooms; // Content(vacantRooms.ToString());
            }
            catch (Exception ex)
            {
                return 0;// Content(ex.Message);
            }
        }

        public int BlockedRooms(int? roomCode, int? RoomCateCode, string startDate, string endDate)
        {
            try
            {
                // Parse the start and end dates
                DateTime _startDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime _endDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                int blockedRooms = 0;

                using (var db = new HotelDBEntities())
                {
                    // Count rooms that have overlapping blocked dates
                    blockedRooms = db.BlockedRooms.Count(x =>
                        x.CateId == RoomCateCode &&
                        x.Status == true &&
                        (
                            (x.StartDate <= _endDate && x.EndDate >= _startDate) // Overlapping check
                        ));
                }

                return blockedRooms;
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return 0;
            }
        }
        //public int BlockedRooms(int? roomCode, int? RoomCateCode, string startDate)
        //{
        //    try
        //    {
        //        DateTime _startDate = Convert.ToDateTime(startDate);
        //        int blockedRooms = 0;
        //        var BlockedRooms = (from obj in HDE.BlockedRooms
        //                            where obj.CateId == RoomCateCode && obj.StartDate <= _startDate && obj.EndDate >= _startDate && obj.Status == true
        //                            select new
        //                            {
        //                                RoomNum = obj.RoomNum
        //                            }).FirstOrDefault();

        //        if (BlockedRooms == null)
        //        {
        //            blockedRooms = 0;
        //        }
        //        else
        //        {
        //            blockedRooms = Convert.ToInt32(BlockedRooms.RoomNum);
        //        }

        //        return blockedRooms; // Content(vacantRooms.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;// Content(ex.Message);
        //    }
        //}

        public ActionResult RoomDetails(int Id)
        {
            try
            {
                //if (Session["params"] != null)
                //{
                //    string Params = (string)Session["params"];
                //    string[] p = Params.Split('&');
                //    DateTime checkin = Convert.ToDateTime(p[0]);
                //}
                RoomDetail(Id);
                // RoomPrice(Id, checkin);
                RoomPictures(Id);
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public JsonResult RoomPics(int Roomcate)
        {
            try
            {
                //int Id = requestRoom.Id; // HttpContext.Request.Form["Id"];
                //string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.RoomPictures
                                where (a.RoomCode == Roomcate)
                                select new
                                {
                                    ImageName = a.Picture
                                }).ToList();

                    ;
                    return Json(room, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string RoomcatePics()
        {
            try
            {
                //int Id = requestRoom.Id; // HttpContext.Request.Form["Id"];
                //string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.RoomPictures
                                where (a.Active == true)
                                select new RoomImages
                                {
                                    Category = a.RoomCode,
                                    ImageName = a.Picture
                                }).ToList();

                    TempData["Images"] = room;
                }

                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string RoomDetail(int Id)
        {
            try
            {
                //int Id = requestRoom.Id; // HttpContext.Request.Form["Id"];
                //string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.Rooms
                                join b in db.RoomCategories
                                on a.RoomCategory equals b.Id
                                where (b.Id == Id)
                                select new RoomDetails
                                {
                                    Id = a.ID,
                                    RoomCode = a.RoomCode,
                                    RoomCategoryId = a.RoomCategory,
                                    RoomCategory = b.Category,
                                    RoomDescription = a.RoomDescription,
                                    PropertyId = a.propertyID,
                                    BedType = a.BedType,
                                    CheckIn = a.CheckIn,
                                    CheckOut = a.CheckOut,
                                    NoOfRooms = a.NoOfRooms,
                                    Capacity = a.Capacity,
                                    Adult = a.Adult,
                                    NoOfChild = a.NoOfChild,
                                    ChildAge = a.ChildAge,
                                    ShortCode = a.ShortCode,
                                    MinimumPrice = a.MinimumPrice,
                                    MinimumStay = a.MinimumStay,
                                    MaximumStay = a.MaximumStay,
                                    Size = a.Size,
                                    Features = a.Features
                                }).FirstOrDefault();

                    //  TempData["room"] = room;
                    ViewBag.room = room;
                }



                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string RoomPrice(int Id, DateTime checkin)
        {
            try
            {
                // int Id = requestRoom.Id;
                // DateTime checkin = requestRoom.Checkin;
                //  string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var roomPrice = (from a in db.RateSlabs
                                     where (a.RoomCategoryCode == Id && (a.StartDate < checkin && a.EndDate > checkin))
                                     select new
                                     {
                                         RoomPrice = a.ExtraAdultPrice
                                     }).FirstOrDefault();
                    TempData["roomPrice"] = roomPrice;
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public JsonResult RoomPolicy(int CancellationPolicy)
        {
            try
            {
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.RatePlans
                                join b in db.CancellationPolicies
                                        on a.CancellationPolicy equals b.Id
                                where (b.Id == CancellationPolicy && a.Status == true)
                                select new
                                {
                                    RatePlanName = a.Name,
                                    CancellationPolicy = b.Name,
                                    FreeCancelBefore = b.PeriodForFreeCharge,
                                    Breakfst = a.Breakfast,
                                    ChargedForNoShow = b.ChargedForNoShow,
                                    GuestChargedForCancellation = b.GuestChargedForCancellation,
                                    HowlongFreeOfCharge = b.HowlongFreeOfCharge
                                }).ToList();

                    ;
                    return Json(room, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string RoomPictures(int Id)
        {
            try
            {
                // int Id = requestRoom.Id;
                // DateTime checkin = requestRoom.Checkin;
                //  string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var roomPictures = (from a in db.RoomPictures
                                        join b in db.Rooms
                                        on a.RoomCode equals b.RoomCategory
                                        where (b.RoomCategory == Id && a.Active == true)
                                        select new Pictures
                                        {
                                            Picture = a.Picture
                                        }).ToList();

                    TempData["roomPictures"] = roomPictures;
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public ActionResult PaymentError(string pp_ResponseCode, string pp_ResponseMessage)
        {
            try
            {
                TempData["pp_ResponseMessage"] = pp_ResponseMessage;
                TempData["pp_ResponseCode"] = pp_ResponseCode;
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public ActionResult Thankyou(string billReference)
        {
            try
            {
                string confirmationId = "";
                string pp_ResponseCode = Request.Form["pp_ResponseCode"];
                string pp_ResponseMessage = Request.Form["pp_ResponseMessage"];

                if (Request.Form["pp_BillReference"] != null)
                {
                    confirmationId = Request.Form["pp_BillReference"];
                }
                else
                {
                    confirmationId = billReference;
                }

                if (pp_ResponseCode == "000" || pp_ResponseCode == null)
                {
                    var _booking = (from obj in HDE.Bookings
                                    join b in HDE.RoomCategories
                                     on obj.RoomCatCode equals b.Id
                                    where (obj.ConfirmationNo == confirmationId)
                                    select new FinalBooking
                                    {
                                        BookingID = obj.BookingID,
                                        PaxTitle = obj.PaxTitle,
                                        PaxFName = obj.PaxFName,
                                        PaxLName = obj.PaxLName,
                                        NoOfRooms = obj.NoOfRooms,
                                        RoomCode = obj.RoomCode,
                                        RoomCatCode = obj.RoomCatCode,
                                        RoomCategory = b.Category,
                                        RoomNum = obj.RoomNum,
                                        PaxEmail = obj.PaxEmail,
                                        PaxPhoneNumber = obj.PaxPhoneNumber,
                                        PaxDescription = obj.PaxDescription,
                                        Adults = obj.Adults,
                                        Children = obj.Children,
                                        CheckIn = obj.CheckIn,
                                        CheckOut = obj.CheckOut,
                                        RoomRate = obj.RoomRate,
                                        HotelCode = obj.HotelCode,
                                        ConfirmationNo = obj.ConfirmationNo,
                                        NoofDay = obj.NoofDay,
                                        TotalRoomRate = obj.TotalRoomRate,
                                        RoomTax = obj.RoomTax
                                    }).ToList();

                    //RoomCates = RoomCates.Remove(0, 1);
                    //TempData["RoomCates"] = RoomCates;
                    //TempData["noOfDay"] = noOfDay;
                    Session["booking"] = _booking;
                    Session["confirmationId"] = confirmationId;

                    using (var db = new HotelDBEntities())
                    {
                        var myData = db.Bookings.Where(x => x.ConfirmationNo == confirmationId).ToList();
                        myData.ForEach(m => m.BookingStatus = "Booked");
                        db.SaveChanges();
                    }

                    // Extract the value before the LINQ query
                    var roomCatCode = _booking[0].RoomCatCode;

                    // Modify the LINQ query to use the extracted value
                    var rm = (from obj in HDE.RoomPictures
                              join r in HDE.Rooms on obj.RoomCode equals r.RoomCategory
                              where obj.RoomCode == roomCatCode
                              select obj.Picture).FirstOrDefault();


                    TimeSpan t = Convert.ToDateTime(_booking[0].CheckOut) - Convert.ToDateTime(_booking[0].CheckIn);
                    int noOfDay = (int)t.TotalDays;

                    try
                    {
                        int? TotalGuests = _booking[0].Adults + _booking[0].Children;

                        StringBuilder htmlString = new StringBuilder();

                        htmlString.Append("<head>");
                        htmlString.Append("<title></title>");
                        htmlString.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
                        htmlString.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                        htmlString.Append("<!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]-->");
                        htmlString.Append("<style>");
                        htmlString.Append("* { box-sizing: border-box; }");
                        htmlString.Append("body { margin: 0; padding: 0; }");
                        htmlString.Append("a[x-apple-data-detectors] { color: inherit !important; text-decoration: inherit !important; }");
                        htmlString.Append("#MessageViewBody a { color: inherit; text-decoration: none; }");
                        htmlString.Append("p { line-height: inherit; }");
                        htmlString.Append(".desktop_hide, .desktop_hide table { mso-hide: all; display: none; max-height: 0px; overflow: hidden; }");
                        htmlString.Append(".image_block img+div { display: none; }");
                        htmlString.Append("sup, sub { line-height: 0; font-size: 75%; }");
                        htmlString.Append("@media (max-width:670px) {");
                        htmlString.Append(".desktop_hide table.icons-inner, .social_block.desktop_hide .social-table { display: inline-block !important; }");
                        htmlString.Append(".icons-inner { text-align: center; }");
                        htmlString.Append(".icons-inner td { margin: 0 auto; }");
                        htmlString.Append(".mobile_hide { display: none; }");
                        htmlString.Append(".row-content { width: 100% !important; }");
                        htmlString.Append(".stack .column { width: 100%; display: block; }");
                        htmlString.Append(".mobile_hide { min-height: 0; max-height: 0; max-width: 0; overflow: hidden; font-size: 0px; }");
                        htmlString.Append(".desktop_hide, .desktop_hide table { display: table !important; max-height: none !important; }");
                        htmlString.Append(".row-3 .column-1 .block-3.spacer_block { height: 20px !important; }");
                        htmlString.Append(".row-3 .column-1 .block-1.paragraph_block td.pad>div { text-align: center !important; font-size: 36px !important; }");
                        htmlString.Append(".row-3 .column-1 .block-1.paragraph_block td.pad, .row-3 .column-2 .block-1.paragraph_block td.pad { padding: 0 !important; }");
                        htmlString.Append(".row-3 .column-1 .block-2.paragraph_block td.pad>div { font-size: 16px !important; }");
                        htmlString.Append(".row-3 .column-2 .block-1.paragraph_block td.pad>div { font-size: 16px !important; }");
                        htmlString.Append(".row-4 .column-1 .block-3.paragraph_block td.pad>div, .row-7 .column-1 .block-3.paragraph_block td.pad>div { text-align: center !important; }");
                        htmlString.Append(".row-4 .column-1 .block-3.paragraph_block td.pad, .row-7 .column-1 .block-3.paragraph_block td.pad { padding: 5px !important; }");
                        htmlString.Append(".row-5 .column-1 .block-3.paragraph_block td.pad>div { font-size: 15px !important; }");
                        htmlString.Append(".row-7 .column-1 .block-2.paragraph_block td.pad>div { text-align: center !important; font-size: 32px !important; }");
                        htmlString.Append(".row-7 .column-1 .block-2.paragraph_block td.pad { padding: 5px 5px 0 !important; }");
                        htmlString.Append(".row-9 .column-1 .block-1.spacer_block { height: 40px !important; }");
                        htmlString.Append(".row-3 .column-1 { padding: 0px 30px !important; }");
                        htmlString.Append(".row-3 .column-2 { padding: 0 30px !important; }");
                        htmlString.Append(".p_mobile_hide { display: none; }");
                        htmlString.Append("}");
                        htmlString.Append("</style>");
                        htmlString.Append("<!--[if mso ]><style>sup, sub { font-size: 100% !important; } sup { mso-text-raise:10% } sub { mso-text-raise:-10% }</style> <![endif]-->");
                        htmlString.Append("</head>");
                        htmlString.Append("<body class='body forceBgColor' style='background-color: transparent; margin: 0; padding: 0; -webkit-text-size-adjust: none; text-size-adjust: none;'>");
                        htmlString.Append("<table class='nl-container' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: transparent; background-repeat: no-repeat; background-image: none; background-position: top left; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad' style='width:100%;'>");
                        htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                        htmlString.Append("<div style='max-width: 650px;'><img src='https://hoteldepapae.com/Content/assets/img/logos/logo.png' style='display: block; height: auto; border: 0; width: auto; padding-top: 20px;padding-bottom: 20px;' width='650' height='auto'></div>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<table class='row row-2' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad' style='width:100%;'>");
                        htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                        htmlString.Append("<div style='max-width: 650px; margin-bottom: 20px;'><img src='https://hoteldepapae.com/uploads/" + rm + "' style='display: block; height: auto; border: 0; width: 100%;' width='650' height='auto'></div>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<!-- main information -->");
                        htmlString.Append("<table class='row row-3' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad' style='width:100%;'>");
                        htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                        htmlString.Append("<div style='max-width: 650px;'>");
                        htmlString.Append("<h1 style='color: #727475;'>Main Informations</h1>");
                        htmlString.Append("</div>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-repeat: no-repeat; border-radius: 0; color: #000000; background-size: auto; background-color: #f9f9f9; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad'>");
                        htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Room : " + _booking[0].RoomCategory + "</p>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Guests : " + TotalGuests + "</p>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad'>");
                        htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>CheckIn Date : " + Convert.ToDateTime(_booking[0].CheckIn).ToShortDateString() + "</p>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>CheckOut Date : " + Convert.ToDateTime(_booking[0].CheckOut).ToShortDateString() + "</p>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<!-- user Informations -->");
                        htmlString.Append("<table class='row row-3' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad' style='width:100%;'>");
                        htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                        htmlString.Append("<div style='max-width: 650px;'>");
                        htmlString.Append("<h1 style='color: #727475;'>User Informations</h1>");
                        htmlString.Append("</div>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-repeat: no-repeat; border-radius: 0; color: #000000; background-size: auto; background-color: #f9f9f9; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad'>");
                        htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Name : " + _booking[0].PaxFName + "</p>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Surname : " + _booking[0].PaxLName + "</p>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("<td class='column column-2' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-right: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='paragraph_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad'>");
                        htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;mso-line-height-alt:21.599999999999998px;'>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Phone : " + _booking[0].PaxPhoneNumber + "</p>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Email : " + _booking[0].PaxEmail + " </p>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<!-- Price -->");
                        htmlString.Append("<table class='row row-3' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad' style='width:100%;'>");
                        htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                        htmlString.Append("<div style='max-width: 650px;'>");
                        htmlString.Append("<h1 style='color: #727475;'>Total Price</h1>");
                        htmlString.Append("</div>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-repeat: no-repeat; border-radius: 0; color: #000000; background-size: auto; background-color: #f9f9f9; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad'>");
                        htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Price : " + (_booking[0].TotalRoomRate - _booking[0].RoomTax) + " PKR</p>");
                        htmlString.Append("<p class='p_mobile_hide' style='margin-bottom: 10px; color: #f9f9f9;'>..</p>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad'>");
                        htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Tax & Fees : " + _booking[0].RoomTax + " PKR</p>");
                        htmlString.Append("<p style='margin-bottom: 10px;'>Total Price : " + _booking[0].TotalRoomRate + " PKR</p>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("<!-- price -->");
                        htmlString.Append("<table class='row row-8 m' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='padding-top: 20px; padding-bottom: 10px; mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td>");
                        htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #f6f5f1; width: 650px; margin: 0 auto;' width='650'>");
                        htmlString.Append("<tbody>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                        htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                        htmlString.Append("<tr>");
                        htmlString.Append("<td class='pad' style='width:100%;'>");
                        htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                        htmlString.Append("<div style='max-width: 650px;'><img src='https://d1oco4z2z1fhwp.cloudfront.net/templates/default/8746/BEE_May20_MarketingAgency_Invoice_v01.jpg' style='display: block; height: auto; border: 0; width: 100%;' width='650' height='auto'></div>");
                        htmlString.Append("</div>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</tbody>");
                        htmlString.Append("</table>");
                        htmlString.Append("</body>");
                        htmlString.Append("</html>");


                        MailMessage mm = new MailMessage();
                        mm.IsBodyHtml = true;
                        mm.To.Add("info@hoteldepapae.com");

                        mm.From = new MailAddress("info@hoteldepapae.com");
                        mm.Subject = "Order Confirmation: " + confirmationId;
                        mm.Body = htmlString.ToString();
                        SmtpClient smc = new SmtpClient();
                        smc.Host = "smtp.gmail.com";
                        smc.Port = 587;  // Use STARTTLS port
                        smc.EnableSsl = true;
                        smc.UseDefaultCredentials = false; // Set explicitly
                        smc.Credentials = new System.Net.NetworkCredential("info@hoteldepapae.com", "xdgk edcd nmfi vshb");
                        smc.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smc.Send(mm);


                    }
                    catch (Exception ex)
                    {
                        TempData["error"] = ex.Message;
                    }

                    if (_booking[0].PaxEmail != "")
                    {
                        try
                        {
                            int? TotalGuests = _booking[0].Adults + _booking[0].Children;

                            StringBuilder htmlString = new StringBuilder();

                            htmlString.Append("<head>");
                            htmlString.Append("<title></title>");
                            htmlString.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
                            htmlString.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                            htmlString.Append("<!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]-->");
                            htmlString.Append("<style>");
                            htmlString.Append("* { box-sizing: border-box; }");
                            htmlString.Append("body { margin: 0; padding: 0; }");
                            htmlString.Append("a[x-apple-data-detectors] { color: inherit !important; text-decoration: inherit !important; }");
                            htmlString.Append("#MessageViewBody a { color: inherit; text-decoration: none; }");
                            htmlString.Append("p { line-height: inherit; }");
                            htmlString.Append(".desktop_hide, .desktop_hide table { mso-hide: all; display: none; max-height: 0px; overflow: hidden; }");
                            htmlString.Append(".image_block img+div { display: none; }");
                            htmlString.Append("sup, sub { line-height: 0; font-size: 75%; }");
                            htmlString.Append("@media (max-width:670px) {");
                            htmlString.Append(".desktop_hide table.icons-inner, .social_block.desktop_hide .social-table { display: inline-block !important; }");
                            htmlString.Append(".icons-inner { text-align: center; }");
                            htmlString.Append(".icons-inner td { margin: 0 auto; }");
                            htmlString.Append(".mobile_hide { display: none; }");
                            htmlString.Append(".row-content { width: 100% !important; }");
                            htmlString.Append(".stack .column { width: 100%; display: block; }");
                            htmlString.Append(".mobile_hide { min-height: 0; max-height: 0; max-width: 0; overflow: hidden; font-size: 0px; }");
                            htmlString.Append(".desktop_hide, .desktop_hide table { display: table !important; max-height: none !important; }");
                            htmlString.Append(".row-3 .column-1 .block-3.spacer_block { height: 20px !important; }");
                            htmlString.Append(".row-3 .column-1 .block-1.paragraph_block td.pad>div { text-align: center !important; font-size: 36px !important; }");
                            htmlString.Append(".row-3 .column-1 .block-1.paragraph_block td.pad, .row-3 .column-2 .block-1.paragraph_block td.pad { padding: 0 !important; }");
                            htmlString.Append(".row-3 .column-1 .block-2.paragraph_block td.pad>div { font-size: 16px !important; }");
                            htmlString.Append(".row-3 .column-2 .block-1.paragraph_block td.pad>div { font-size: 16px !important; }");
                            htmlString.Append(".row-4 .column-1 .block-3.paragraph_block td.pad>div, .row-7 .column-1 .block-3.paragraph_block td.pad>div { text-align: center !important; }");
                            htmlString.Append(".row-4 .column-1 .block-3.paragraph_block td.pad, .row-7 .column-1 .block-3.paragraph_block td.pad { padding: 5px !important; }");
                            htmlString.Append(".row-5 .column-1 .block-3.paragraph_block td.pad>div { font-size: 15px !important; }");
                            htmlString.Append(".row-7 .column-1 .block-2.paragraph_block td.pad>div { text-align: center !important; font-size: 32px !important; }");
                            htmlString.Append(".row-7 .column-1 .block-2.paragraph_block td.pad { padding: 5px 5px 0 !important; }");
                            htmlString.Append(".row-9 .column-1 .block-1.spacer_block { height: 40px !important; }");
                            htmlString.Append(".row-3 .column-1 { padding: 0px 30px !important; }");
                            htmlString.Append(".row-3 .column-2 { padding: 0 30px !important; }");
                            htmlString.Append(".p_mobile_hide { display: none; }");
                            htmlString.Append("}");
                            htmlString.Append("</style>");
                            htmlString.Append("<!--[if mso ]><style>sup, sub { font-size: 100% !important; } sup { mso-text-raise:10% } sub { mso-text-raise:-10% }</style> <![endif]-->");
                            htmlString.Append("</head>");
                            htmlString.Append("<body class='body forceBgColor' style='background-color: transparent; margin: 0; padding: 0; -webkit-text-size-adjust: none; text-size-adjust: none;'>");
                            htmlString.Append("<table class='nl-container' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: transparent; background-repeat: no-repeat; background-image: none; background-position: top left; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad' style='width:100%;'>");
                            htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                            htmlString.Append("<div style='max-width: 650px;'><img src='https://hoteldepapae.com/Content/assets/img/logos/logo.png' style='display: block; height: auto; border: 0; width: auto; padding-top: 20px;padding-bottom: 20px;' width='650' height='auto'></div>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<table class='row row-2' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad' style='width:100%;'>");
                            htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                            htmlString.Append("<div style='max-width: 650px; margin-bottom: 20px;'><img src='https://hoteldepapae.com/uploads/" + rm + "' style='display: block; height: auto; border: 0; width: 100%;' width='650' height='auto'></div>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<!-- main information -->");
                            htmlString.Append("<table class='row row-3' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad' style='width:100%;'>");
                            htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                            htmlString.Append("<div style='max-width: 650px;'>");
                            htmlString.Append("<h1 style='color: #727475;'>Main Informations</h1>");
                            htmlString.Append("</div>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-repeat: no-repeat; border-radius: 0; color: #000000; background-size: auto; background-color: #f9f9f9; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad'>");
                            htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Room : " + _booking[0].RoomCategory + "</p>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Guests : " + TotalGuests + "</p>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad'>");
                            htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>CheckIn Date : "+ Convert.ToDateTime(_booking[0].CheckIn).ToShortDateString() + "</p>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>CheckOut Date : " + Convert.ToDateTime(_booking[0].CheckOut).ToShortDateString() + "</p>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<!-- user Informations -->");
                            htmlString.Append("<table class='row row-3' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad' style='width:100%;'>");
                            htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                            htmlString.Append("<div style='max-width: 650px;'>");
                            htmlString.Append("<h1 style='color: #727475;'>User Informations</h1>");
                            htmlString.Append("</div>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-repeat: no-repeat; border-radius: 0; color: #000000; background-size: auto; background-color: #f9f9f9; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad'>");
                            htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Name : " + _booking[0].PaxFName + "</p>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Surname : " + _booking[0].PaxLName + "</p>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("<td class='column column-2' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-right: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='paragraph_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad'>");
                            htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;mso-line-height-alt:21.599999999999998px;'>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Phone : " + _booking[0].PaxPhoneNumber + "</p>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Email : " + _booking[0].PaxEmail + " </p>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<!-- Price -->");
                            htmlString.Append("<table class='row row-3' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row row-1' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #ffffff; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad' style='width:100%;'>");
                            htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                            htmlString.Append("<div style='max-width: 650px;'>");
                            htmlString.Append("<h1 style='color: #727475;'>Total Price</h1>");
                            htmlString.Append("</div>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-repeat: no-repeat; border-radius: 0; color: #000000; background-size: auto; background-color: #f9f9f9; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad'>");
                            htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Price : " + (_booking[0].TotalRoomRate - _booking[0].RoomTax) + " PKR</p>");
                            htmlString.Append("<p class='p_mobile_hide' style='margin-bottom: 10px; color: #f9f9f9;'>..</p>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("<td class='column column-1' width='50%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 10px; padding-left: 50px; padding-top: 10px; vertical-align: bottom; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='paragraph_block block-2' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad'>");
                            htmlString.Append("<div style='color: #878787; direction:ltr;font-family:\"Helvetica Neue\", Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:left;mso-line-height-alt:21.599999999999998px;'>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Tax & Fees : " + _booking[0].RoomTax + " PKR</p>");
                            htmlString.Append("<p style='margin-bottom: 10px;'>Total Price : " + _booking[0].TotalRoomRate + " PKR</p>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("<!-- price -->");
                            htmlString.Append("<table class='row row-8 m' align='center' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='padding-top: 20px; padding-bottom: 10px; mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td>");
                            htmlString.Append("<table class='row-content stack' align='center' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-radius: 0; color: #000000; background-size: auto; background-color: #f6f5f1; width: 650px; margin: 0 auto;' width='650'>");
                            htmlString.Append("<tbody>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='column column-1' width='100%' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;'>");
                            htmlString.Append("<table class='image_block block-1' width='100%' border='0' cellpadding='0' cellspacing='0' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>");
                            htmlString.Append("<tr>");
                            htmlString.Append("<td class='pad' style='width:100%;'>");
                            htmlString.Append("<div class='alignment' align='center' style='line-height:10px'>");
                            htmlString.Append("<div style='max-width: 650px;'><img src='https://d1oco4z2z1fhwp.cloudfront.net/templates/default/8746/BEE_May20_MarketingAgency_Invoice_v01.jpg' style='display: block; height: auto; border: 0; width: 100%;' width='650' height='auto'></div>");
                            htmlString.Append("</div>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</tbody>");
                            htmlString.Append("</table>");
                            htmlString.Append("</body>");
                            htmlString.Append("</html>");


                            MailMessage mm = new MailMessage();
                            mm.IsBodyHtml = true;
                            mm.To.Add(_booking[0].PaxEmail);

                            mm.From = new MailAddress("info@hoteldepapae.com");
                            mm.Subject = "Order Confirmation: " + confirmationId;
                            mm.Body = htmlString.ToString();
                            SmtpClient smc = new SmtpClient();
                            smc.Host = "smtp.gmail.com";
                            smc.Port = 587;  // Use STARTTLS port
                            smc.EnableSsl = true;
                            smc.UseDefaultCredentials = false; // Set explicitly
                            smc.Credentials = new System.Net.NetworkCredential("info@hoteldepapae.com", "xdgk edcd nmfi vshb");
                            smc.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smc.Send(mm);


                        }
                        catch (Exception ex)
                        {
                            TempData["error"] = ex.Message;
                        }

                    }

                    return View();
                }
                else
                {
                    return RedirectToAction("PaymentError", "SearchResult", new { @pp_ResponseCode = pp_ResponseCode, @pp_ResponseMessage = pp_ResponseMessage });
                }



            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = ex.ToString(), JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult Thankyou(FormCollection frm)
        {
            try
            {

                string PaxTitle = Request.Form["PaxTitle"];
                string PaxFName = Request.Form["PaxFName"];
                string PaxLName = Request.Form["PaxLName"];
                string CheckIn = Convert.ToString(Request.Form["CheckIn"]);
                string CheckOut = Convert.ToString(Request.Form["CheckOut"]);
                int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
                string PaxPhoneNumber = Request.Form["PaxPhoneNumber"];
                string PaxEmail = Request.Form["PaxEmail"];
                string PaxDescription = Request.Form["PaxDescription"];
                int Adults = Convert.ToInt32(Request.Form["Adults"]);
                int Children = Convert.ToInt32(Request.Form["Children"]);
                int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
                int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
                int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
                string RoomCatCodes = Request.Form["RoomCatCode"];
                string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
                string RatePlanIDs = Request.Form["RatePlanIDs"];
                string RoomRates = Request.Form["RoomRates"];

                string[] _CheckIn = CheckIn.Split('-');
                string[] _CheckOut = CheckOut.Split('-');

                CheckIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
                CheckOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

                if (Adults == 0)
                {
                    Adults = 1;
                }

                SaveBooking(PaxTitle, PaxFName, PaxLName, Convert.ToDateTime(CheckIn), Convert.ToDateTime(CheckOut), RoomCatCode, PaxPhoneNumber, PaxEmail, PaxDescription, Adults,
                    Children, NoOfRooms, RoomRate, TotalAmount, RoomCatCodes, RoomCateRoomNums, RatePlanIDs, RoomRates);

                return View();
            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = ex.ToString() });
            }
        }

        public ActionResult Reservation(DateTime checkIn, DateTime checkOut, int Adults, int Children, int NoOfRooms, int RoomRate, int TotalAmount, string RoomCatCodes,
            string Quatntity, string RatePlanIDs, string RoomRates, int TaxAmount)
        {
            var db = new HotelDBEntities();
            var roomcategory = Convert.ToInt32(RoomCatCodes);
            ViewBag.RoomCategory = db.RoomPictures.Where(x => x.RoomCode == roomcategory && x.Active == true).FirstOrDefault();
            ViewBag.Category = db.RoomCategories.Where(x => x.Id == roomcategory).FirstOrDefault();
            ViewBag.checkIn = checkIn;
            ViewBag.checkOut = checkOut;
            ViewBag.Adults = Adults;
            ViewBag.Children = Children;
            ViewBag.NoOfRooms = NoOfRooms;
            ViewBag.RoomRate = RoomRate;
            ViewBag.TotalAmount = TotalAmount;
            ViewBag.RoomCatCodes = RoomCatCodes;
            ViewBag.RoomCateRoomNums = Quatntity;
            ViewBag.RatePlanIDs = RatePlanIDs;
            ViewBag.RoomRates = RoomRates;
            ViewBag.TaxAmount = TaxAmount;

            return View();
        }

        [HttpPost]
        public ActionResult ReservationPost()
        {

            string checkIn = Convert.ToString(Request.Form["CheckIn"]);
            string checkOut = Convert.ToString(Request.Form["CheckOut"]);
            int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
            int Adults = Convert.ToInt32(Request.Form["Adults"]);
            int Children = Convert.ToInt32(Request.Form["Children"]);
            int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
            int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
            int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
            string RoomCatCodes = Request.Form["RoomCatCode"];
            string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
            string RatePlanIDs = Request.Form["RatePlanIDs"];
            string RoomRates = Request.Form["RoomRates"];
            int TaxAmount = Convert.ToInt32(Request.Form["TaxAmount"]);

            string[] _CheckIn = checkIn.Split('-');
            string[] _CheckOut = checkOut.Split('-');

            checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
            checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

            DateTime CheckIn = Convert.ToDateTime(checkIn);
            DateTime CheckOut = Convert.ToDateTime(checkOut);
            return RedirectToAction("Reservation", "SearchResult", new { @checkIn = CheckIn, @checkOut = CheckOut, @Adults = Adults, @Children = Children, @NoOfRooms = NoOfRooms, @RoomRate = RoomRate, @TotalAmount = TotalAmount, @RoomCatCodes = RoomCatCodes, @RoomCateRoomNums = RoomCateRoomNums, @RatePlanIDs = RatePlanIDs, @RoomRates = RoomRates, @TaxAmount = TaxAmount });
        }

        [HttpPost]
        public ActionResult ReservationPost2()
        {

            string checkIn = Convert.ToString(Request.Form["CheckIn"]);
            string checkOut = Convert.ToString(Request.Form["CheckOut"]);
            //  int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
            int Adults = Convert.ToInt32(Request.Form["Adults"]);
            int Children = Convert.ToInt32(Request.Form["Children"]);
            int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
            int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
            int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
            string RoomCatCodes = Request.Form["RoomCate"];
            string txtQuatntity = Request.Form["txtQuatntity"];
            string RatePlanIDs = Request.Form["RatePlanIDs"];
            string RoomRates = Request.Form["RoomRates"];
            int TaxAmount = Convert.ToInt32(Request.Form["TaxAmount"]);

            string DateString = checkIn;
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime datestart = DateTime.ParseExact(DateString, "MM/dd/yyyy", culture);

            string DateString2 = checkOut;
            IFormatProvider culture2 = new CultureInfo("en-US", true);
            DateTime dateend = DateTime.ParseExact(DateString2, "MM/dd/yyyy", culture2);

            //string[] _CheckIn = checkIn.Split('-');
            //string[] _CheckOut = checkOut.Split('-');

            //checkIn = checkIn[1] + "/" + checkIn[0] + "/" + checkIn[2];
            //checkOut = checkIn[1] + "/" + checkIn[0] + "/" + checkIn[2];


            return RedirectToAction("Reservation", "SearchResult", new { @checkIn = datestart, @checkOut = dateend, @Adults = Adults, @Children = Children, @NoOfRooms = NoOfRooms, @RoomRate = RoomRate, @TotalAmount = TotalAmount, @RoomCatCodes = RoomCatCodes, @Quatntity = txtQuatntity, @RatePlanIDs = RatePlanIDs, @RoomRates = RoomRates, @TaxAmount = TaxAmount });
        }


        public string SaveBooking(string PaxTitle, string PaxFName, string PaxLName, DateTime CheckIn, DateTime CheckOut, int RoomCatCode,
            string PaxPhoneNumber, string PaxEmail, string PaxDescription, int Adults, int Children, int NoOfRooms, int RoomRate, int TotalAmount,
            string RoomCatCodes, string RoomCateRoomNums, string RatePlanIDs, string RoomRates)
        {
            try
            {
                using (var db = new HotelDBEntities())
                {
                    var rowsAffected = db.Bookings.Where(x => x.PaxFName == PaxFName && x.PaxLName == PaxLName &&
                                x.CheckIn == CheckIn).FirstOrDefault();

                    if (true == true)
                    {
                        int? bookingID = db.Bookings.DefaultIfEmpty().Max(p => p == null ? 0 : p.BookingID);
                        string confirmationId = "MHH-" + Convert.ToString(bookingID + 1);
                        int? PropertyId = db.Rooms.Where(p => p.RoomCategory == RoomCatCode).Select(p => p.propertyID).FirstOrDefault();
                        int? RoomCode = 0;
                        string RoomCate = "";
                        string RoomCates = "";
                        TimeSpan t = Convert.ToDateTime(CheckOut) - Convert.ToDateTime(CheckIn);
                        int noOfDay = (int)t.TotalDays;
                        string[] _RoomCatCodes;
                        string[] _RoomCateRoomNums;
                        string[] _RatePlanIDs;
                        string[] _RoomRates;

                        RoomRates = RoomRates.Remove(0, 1);
                        _RoomRates = RoomRates.Split(',');

                        RoomCatCodes = RoomCatCodes.Remove(0, 1);
                        _RoomCatCodes = RoomCatCodes.Split(',');

                        RatePlanIDs = RatePlanIDs.Remove(0, 1);
                        _RatePlanIDs = RatePlanIDs.Split(',');

                        RoomCateRoomNums = RoomCateRoomNums.Remove(0, 1);
                        _RoomCateRoomNums = RoomCateRoomNums.Split(',');

                        //foreach (var itm in _RoomCatCodes)
                        for (int itm = 0; itm < _RoomCatCodes.Length; itm++)
                        {
                            int CateCode = Convert.ToInt32(_RoomCatCodes[itm]);
                            RoomCode = db.Rooms.Where(p => p.RoomCategory == CateCode).Select(p => p.RoomCode).FirstOrDefault();
                            RoomCate = db.RoomCategories.Where(p => p.Id == CateCode).Select(p => p.Category).FirstOrDefault();
                            RoomCates = RoomCates + "," + RoomCate;

                            //foreach (var itm1 in _RoomCateRoomNums)
                            {
                                int NoofRooms = Convert.ToInt32(_RoomCateRoomNums[itm]);

                                for (int i = 1; i <= NoofRooms; i++)
                                {
                                    var bookedRooms = db.Bookings.Where(x => x.RoomCode == RoomCode && x.CheckIn == CheckIn).Select(p => p.RoomNum).ToList();
                                    var RoomNum = (db.RoomNumbers.Where(p => p.RoomCode == RoomCode).Select(p => p.RoomNumber1).ToList());
                                    //var roomnum = db.RoomNumbers.Where(p => p.RoomNumber1 == RoomNum).FirstOrDefault();

                                    var roomnum = RoomNum.Except(bookedRooms).FirstOrDefault();

                                    if (roomnum != null)
                                    {
                                        //roomnum.Booked = true;

                                        //db.Entry(roomnum).State = System.Data.EntityState.Modified;
                                        //db.SaveChanges();

                                        Booking booking = new Booking();

                                        booking.PaxTitle = PaxTitle;
                                        booking.PaxFName = PaxFName;
                                        booking.PaxLName = PaxLName;
                                        booking.NoOfRooms = 1;
                                        booking.RoomCode = RoomCode;
                                        booking.RoomCatCode = CateCode;
                                        booking.RoomNum = roomnum;
                                        booking.PaxEmail = PaxEmail;
                                        booking.PaxPhoneNumber = PaxPhoneNumber;
                                        booking.PaxDescription = PaxDescription;
                                        booking.Adults = Adults;
                                        booking.Children = Children;
                                        booking.CheckIn = CheckIn;
                                        booking.CheckOut = CheckOut;
                                        booking.ReservationDate = DateTime.Now;
                                        booking.RoomRate = Convert.ToInt32(_RoomRates[itm]);
                                        booking.TotalRoomRate = Convert.ToInt32(_RoomRates[itm]);
                                        booking.HotelCode = 1;
                                        booking.ConfirmationNo = confirmationId;
                                        booking.DateModified = DateTime.Now;
                                        booking.BookingStatus = "Booked";
                                        booking.NoofDay = noOfDay;
                                        booking.TotalRoomRate = TotalAmount;
                                        booking.ThemeColor = "red";
                                        booking.RatePlanId = Convert.ToInt32(_RatePlanIDs[itm]);

                                        db.Entry(booking).State = System.Data.Entity.EntityState.Added;
                                        db.SaveChanges();




                                    }
                                }
                            }
                        }

                        var _booking = (from obj in HDE.Bookings
                                        join b in HDE.RoomCategories
                                         on obj.RoomCatCode equals b.Id
                                        where (obj.ConfirmationNo == confirmationId)
                                        select new FinalBooking
                                        {
                                            BookingID = obj.BookingID,
                                            PaxTitle = obj.PaxTitle,
                                            PaxFName = obj.PaxFName,
                                            PaxLName = obj.PaxLName,
                                            NoOfRooms = obj.NoOfRooms,
                                            RoomCode = obj.RoomCode,
                                            RoomCatCode = obj.RoomCatCode,
                                            RoomCategory = b.Category,
                                            RoomNum = obj.RoomNum,
                                            PaxEmail = obj.PaxEmail,
                                            PaxPhoneNumber = obj.PaxPhoneNumber,
                                            PaxDescription = obj.PaxDescription,
                                            Adults = obj.Adults,
                                            Children = obj.Children,
                                            CheckIn = obj.CheckIn,
                                            CheckOut = obj.CheckOut,
                                            RoomRate = obj.RoomRate,
                                            HotelCode = obj.HotelCode,
                                            ConfirmationNo = obj.ConfirmationNo,
                                            NoofDay = obj.NoofDay,
                                            TotalRoomRate = obj.TotalRoomRate
                                        }).ToList();

                        RoomCates = RoomCates.Remove(0, 1);
                        TempData["RoomCates"] = RoomCates;
                        TempData["noOfDay"] = noOfDay;
                        Session["booking"] = _booking;
                        Session["confirmationId"] = confirmationId;

                        try
                        {
                            StringBuilder htmlString = new StringBuilder();
                            htmlString.Append(@"<html>");
                            htmlString.Append("<body>");
                            htmlString.Append("<h3>Thanks</h3>");
                            htmlString.Append("<p> New Order Received.</p>");
                            htmlString.Append("Confirmation Num.:  <b>" + confirmationId + "</b><br />");
                            htmlString.Append("<table border='1'><tr>");
                            htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
                            htmlString.Append("<tr><td>" + PaxTitle + " " + PaxFName + " " + PaxLName + " </td><td> " + PaxPhoneNumber + "</td>");
                            htmlString.Append("<td>" + RoomCates + "</td>");
                            htmlString.Append("<td>" + CheckIn + "</td> ");
                            htmlString.Append("<td>" + CheckOut + "</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table><br />");
                            htmlString.Append("<span><b>Adults:</b></span><span>" + Adults + "</span><br />");
                            htmlString.Append("<span><b>Children:</b></span><span>" + Children + "</span><br />");
                            htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + NoOfRooms + "</span><br />");
                            htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
                            htmlString.Append("<span><b>Total Amount:</b></span><span>" + TotalAmount + "</span><br />");
                            htmlString.Append("</body>");
                            htmlString.Append("</html>");

                            MailMessage mm = new MailMessage();
                            mm.IsBodyHtml = true;
                            mm.To.Add("info@hoteldepapae.com");

                            mm.From = new MailAddress("growbiztec@gmail.com");
                            mm.Subject = "Order Confirmation: " + confirmationId;
                            mm.Body = htmlString.ToString();
                            SmtpClient smc = new SmtpClient();
                            smc.Host = "smtp.gmail.com";
                            smc.Port = 587;
                            // smc.UseDefaultCredentials = false;
                            smc.EnableSsl = true;
                            smc.Credentials = new System.Net.NetworkCredential("growbiztec@gmail.com", "growbiz@2022");
                            smc.DeliveryMethod = SmtpDeliveryMethod.Network;

                            smc.Send(mm);


                        }
                        catch (Exception ex)
                        {
                            TempData["error"] = ex.Message;
                        }

                        if (PaxEmail != "")
                        {
                            try
                            {
                                StringBuilder htmlString = new StringBuilder();
                                htmlString.Append(@"<html>");
                                htmlString.Append("<body>");
                                htmlString.Append("<h3>Thanks</h3>");
                                htmlString.Append("<p> We have received your booking order and are working on a confirmation.</p>");
                                htmlString.Append("Your Confirmation Num.:  <b>" + confirmationId + "</b><br />");
                                htmlString.Append("<table border='1'><tr>");
                                htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
                                htmlString.Append("<tr><td>" + PaxTitle + " " + PaxFName + " " + PaxLName + " </td><td>" + PaxPhoneNumber + " </td>");
                                htmlString.Append("<td>" + RoomCates + "</td>");
                                htmlString.Append("<td>" + CheckIn + "</td> ");
                                htmlString.Append("<td>" + CheckOut + "</td>");
                                htmlString.Append("</tr>");
                                htmlString.Append("</table><br />");
                                htmlString.Append("<span><b>Adults:</b></span><span>" + Adults + "</span><br />");
                                htmlString.Append("<span><b>Children:</b></span><span>" + Children + "</span><br />");
                                htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + NoOfRooms + "</span><br />");
                                htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
                                htmlString.Append("<span><b>Total Amount:</b></span><span>" + TotalAmount + "</span><br />");
                                htmlString.Append("</body>");
                                htmlString.Append("</html>");

                                MailMessage mm = new MailMessage();
                                mm.IsBodyHtml = true;
                                mm.To.Add(PaxEmail);

                                mm.From = new MailAddress("info@manilaheights.com");
                                mm.Subject = "Order Confirmation: " + confirmationId;
                                mm.Body = htmlString.ToString();
                                SmtpClient smc = new SmtpClient();
                                smc.Host = "smtp.gmail.com";
                                smc.Port = 587;
                                // smc.UseDefaultCredentials = false;
                                smc.EnableSsl = true;
                                smc.Credentials = new System.Net.NetworkCredential("info@manilaheights.com", "T15sqy1@9");
                                smc.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smc.Send(mm);


                            }
                            catch (Exception ex)
                            {
                                TempData["error"] = ex.Message;
                            }

                        }

                        return ("Booking done succesfully");
                        //jsonString = JsonSerializer.Serialize(confirmationId);

                    }
                    else
                    {
                        // responseBasicProfile.MessageList.Add("Not Record Found.");
                        return ("Something went wrong.");
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return ("Something went wrong.");
            }
        }

        [HttpPost]
        public ActionResult SaveBooking(FormCollection frm)
        {
            try
            {
                string PaymentType = Request.Form["PaymentType"];
                string PaxTitle = Request.Form["PaxTitle"];
                string PaxFName = Request.Form["PaxFName"];
                string PaxLName = Request.Form["PaxLName"];
                string checkIn = Convert.ToString(Request.Form["CheckIn"]);
                string checkOut = Convert.ToString(Request.Form["CheckOut"]);
                int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
                string PaxPhoneNumber = Request.Form["PaxPhoneNumber"];
                string PaxEmail = Request.Form["PaxEmail"];
                string PaxDescription = Request.Form["PaxDescription"];
                int Adults = Convert.ToInt32(Request.Form["Adults"]);
                int Children = Convert.ToInt32(Request.Form["Children"]);
                int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
                int Taxes = Convert.ToInt32(Request.Form["Taxes"]);
                int TotalAmount = Convert.ToInt32(Request.Form["TotalAmount"]);
                string RoomCatCodes = Request.Form["RoomCatCode"];
                string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
                string RatePlanIDs = Request.Form["RatePlanIDs"];
                string RoomRates = Request.Form["RoomRates"];

                int DiscountedAmount = Convert.ToInt32(Request.Form["DiscountedAmount"]);
                string VoucherCode = Request.Form["VoucherCode"];

                // string[] _CheckIn = checkIn.Split('-');
                // string[] _CheckOut = checkOut.Split('-');

                // checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
                // checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

                DateTime CheckIn = Convert.ToDateTime(checkIn);
                DateTime CheckOut = Convert.ToDateTime(checkOut);

                if (Adults == 0)
                {
                    Adults = 1;
                }

                using (var db = new HotelDBEntities())
                {
                    var rowsAffected = db.Bookings.Where(x => x.PaxFName == PaxFName && x.PaxLName == PaxLName &&
                                x.CheckIn == CheckIn).FirstOrDefault();

                    if (true == true)
                    {
                        int? bookingID = db.Bookings.DefaultIfEmpty().Max(p => p == null ? 0 : p.BookingID);
                        string confirmationId = "MHH-" + Convert.ToString(bookingID + 1);
                        int? PropertyId = db.Rooms.Where(p => p.RoomCategory == RoomCatCode).Select(p => p.propertyID).FirstOrDefault();
                        int? RoomCode = 0;
                        string RoomCate = "";
                        string RoomCates = "";
                        TimeSpan t = Convert.ToDateTime(CheckOut) - Convert.ToDateTime(CheckIn);
                        int noOfDay = (int)t.TotalDays;
                        //string[] _RoomCatCodes;
                        //string[] _RoomCateRoomNums;
                        //string[] _RatePlanIDs;
                        //string[] _RoomRates;
                        double GrandTotal = 0;

                        //RoomRates = RoomRates.Remove(0, 1);
                        //_RoomRates = RoomRates.Split(',');

                        //RoomCatCodes = RoomCatCodes.Remove(0, 1);
                        //_RoomCatCodes = RoomCatCodes.Split(',');

                        //RatePlanIDs = RatePlanIDs.Remove(0, 1);
                        //_RatePlanIDs = RatePlanIDs.Split(',');

                        //RoomCateRoomNums = RoomCateRoomNums.Remove(0, 1);
                        //_RoomCateRoomNums = RoomCateRoomNums.Split(',');

                        //foreach (var itm in _RoomCatCodes)
                        //for (int itm = 0; itm < RoomCatCodes.Length; itm++)
                        {
                            int CateCode = Convert.ToInt32(RoomCatCodes);
                            RoomCode = db.Rooms.Where(p => p.RoomCategory == CateCode).Select(p => p.RoomCode).FirstOrDefault();
                            RoomCate = db.RoomCategories.Where(p => p.Id == CateCode).Select(p => p.Category).FirstOrDefault();
                            RoomCates = RoomCates + "," + RoomCate;

                            //foreach (var itm1 in _RoomCateRoomNums)
                            {
                                int NoofRooms = Convert.ToInt32(RoomCateRoomNums);

                                //for (int i = 1; i <= NoofRooms; i++)
                                {
                                    var bookedRooms = db.Bookings.Where(x => x.RoomCode == RoomCode && x.CheckIn == CheckIn).Select(p => p.RoomNum).ToList();
                                    var RoomNum = (db.RoomNumbers.Where(p => p.RoomCode == RoomCode).Select(p => p.RoomNumber1).ToList());
                                    //var roomnum = db.RoomNumbers.Where(p => p.RoomNumber1 == RoomNum).FirstOrDefault();

                                    var roomnum = RoomNum.Except(bookedRooms).FirstOrDefault();

                                    if (roomnum != null)
                                    {
                                        //roomnum.Booked = true;

                                        //db.Entry(roomnum).State = System.Data.EntityState.Modified;
                                        //db.SaveChanges();

                                        GrandTotal = GrandTotal + TotalAmount;

                                        Booking booking = new Booking();

                                        booking.PaxTitle = PaxTitle;
                                        booking.PaxFName = PaxFName;
                                        booking.PaxLName = PaxLName;
                                        booking.NoOfRooms = 1;
                                        booking.RoomCode = RoomCode;
                                        booking.RoomCatCode = CateCode;
                                        booking.RoomNum = roomnum;
                                        booking.PaxEmail = PaxEmail;
                                        booking.PaxPhoneNumber = PaxPhoneNumber;
                                        booking.PaxDescription = PaxDescription;
                                        booking.Adults = Adults;
                                        booking.Children = Children;
                                        booking.CheckIn = CheckIn;
                                        booking.CheckOut = CheckOut;
                                        booking.RoomRate = Convert.ToInt32(RoomRates);
                                        booking.TotalRoomRate = Convert.ToInt32(RoomRates);
                                        booking.HotelCode = 1;
                                        booking.ConfirmationNo = confirmationId;
                                        booking.DateModified = DateTime.Now;
                                        booking.ReservationDate = DateTime.Now;
                                        booking.BookingStatus = "Pending";
                                        booking.NoofDay = noOfDay;
                                     //   booking.TotalRoomRate = TotalAmount;
                                        booking.ThemeColor = "red";
                                        booking.RatePlanId = Convert.ToInt32(RatePlanIDs);
                                        booking.RoomTax = Taxes;

                                        decimal discountAmount = GetDiscountAmountDecimal(VoucherCode);
                                        if (discountAmount > 0)
                                        {
                                            booking.VoucherAmount = Convert.ToInt32(discountAmount);
                                        }
                                        else
                                        {
                                            booking.VoucherAmount = 0;
                                        }

                                        GrandTotal -= (double)discountAmount;
                                        DiscountedAmount -= (int)discountAmount;

                                        // Insert logic to set TotalRoomRate
                                        if (discountAmount > 0)
                                        {
                                            booking.TotalRoomRate = Convert.ToInt32(GrandTotal);
                                        }
                                        else
                                        {
                                            booking.TotalRoomRate = TotalAmount;
                                        }
                                        db.Entry(booking).State = System.Data.Entity.EntityState.Added;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        // Get the discount amount using the voucher code
                        decimal discAmount = GetDiscountAmountDecimal(VoucherCode);
                        DiscountedAmount -= (int)discAmount;

                        if (PaymentType == "1" || PaymentType == null)
                        {
                            return RedirectToAction("Thankyou", "SearchResult", new { @billReference = confirmationId });
                        }
                        else
                        {
                            return RedirectToAction("GetAccessToken", "Payfast", new { @billReference = confirmationId, @GrandTotal = TotalAmount });
                        }

                    }
                    else
                    {
                        // responseBasicProfile.MessageList.Add("Not Record Found.");
                        return RedirectToAction("Index", "Home");

                        //return null;
                    }
                }

            }
            catch (Exception ex)
            {
                //TempData["error"] = ex.Message;
                return RedirectToAction("Index", "Home");
                // return null;
            }
        }

        public JsonResult GetDiscountAmount(string code)
        {
            var voucher = HDE.Vouchers.FirstOrDefault(v => v.Code == code);

            if (voucher != null)
            {
                return Json(new { success = true, discountAmount = voucher.DiscountAmount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, discountAmount = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        private decimal GetDiscountAmountDecimal(string code)
        {
            var voucher = HDE.Vouchers.FirstOrDefault(v => v.Code == code);
            return voucher?.DiscountAmount ?? 0;
        }
    }

    public class BookedRooms
    {
        public string RoomCate { get; set; }
        public int? Quantity { get; set; }
    }

    public class RoomImages
    {
        public int? Category { get; set; }
        public string ImageName { get; set; }
    }

    public class FinalBooking
    {
        public int? BookingID { get; set; }
        public string PaxTitle { get; set; }
        public string PaxFName { get; set; }
        public string PaxLName { get; set; }
        public int? NoOfRooms { get; set; }
        public int? RoomCode { get; set; }
        public int? RoomCatCode { get; set; }
        public string RoomCategory { get; set; }
        public string RoomNum { get; set; }
        public string PaxEmail { get; set; }
        public string PaxPhoneNumber { get; set; }
        public string PaxDescription { get; set; }
        public int? Adults { get; set; }
        public int? Children { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int? RoomRate { get; set; }
        public int? HotelCode { get; set; }
        public string ConfirmationNo { get; set; }
        public int? NoofDay { get; set; }
        public int? TotalRoomRate { get; set; }

        public double? RoomTax { get; set; }
    }
}




//[HttpPost]
//public ActionResult Thankyou(FormCollection frm)
//{
//    try
//    {
//        string confirmationId = Request.Form["pp_BillReference"];
//        string pp_ResponseCode = Request.Form["pp_ResponseCode"];
//        string pp_ResponseMessage = Request.Form["pp_ResponseMessage"];

//        if (pp_ResponseCode == "000")
//        {
//            var _booking = (from obj in HDE.Bookings
//                            join b in HDE.RoomCategories
//                             on obj.RoomCatCode equals b.Id
//                            where (obj.ConfirmationNo == confirmationId)
//                            select new FinalBooking
//                            {
//                                BookingID = obj.BookingID,
//                                PaxTitle = obj.PaxTitle,
//                                PaxFName = obj.PaxFName,
//                                PaxLName = obj.PaxLName,
//                                NoOfRooms = obj.NoOfRooms,
//                                RoomCode = obj.RoomCode,
//                                RoomCatCode = obj.RoomCatCode,
//                                RoomCategory = b.Category,
//                                RoomNum = obj.RoomNum,
//                                PaxEmail = obj.PaxEmail,
//                                PaxPhoneNumber = obj.PaxPhoneNumber,
//                                PaxDescription = obj.PaxDescription,
//                                Adults = obj.Adults,
//                                Children = obj.Children,
//                                CheckIn = obj.CheckIn,
//                                CheckOut = obj.CheckOut,
//                                RoomRate = obj.RoomRate,
//                                HotelCode = obj.HotelCode,
//                                ConfirmationNo = obj.ConfirmationNo,
//                                NoofDay = obj.NoofDay,
//                                TotalRoomRate = obj.TotalRoomRate
//                            }).ToList();

//            //RoomCates = RoomCates.Remove(0, 1);
//            //TempData["RoomCates"] = RoomCates;
//            //TempData["noOfDay"] = noOfDay;
//            Session["booking"] = _booking;
//            Session["confirmationId"] = confirmationId;

//            using (var db = new HotelDBEntities())
//            {
//                var myData = db.Bookings.Where(x => x.ConfirmationNo == confirmationId).ToList();
//                myData.ForEach(m => m.BookingStatus = "Booked");
//                db.SaveChanges();
//            }


//            TimeSpan t = Convert.ToDateTime(_booking[0].CheckOut) - Convert.ToDateTime(_booking[0].CheckIn);
//            int noOfDay = (int)t.TotalDays;

//            try
//            {
//                StringBuilder htmlString = new StringBuilder();
//                htmlString.Append(@"<html>");
//                htmlString.Append("<body>");
//                htmlString.Append("<h3>Thanks</h3>");
//                htmlString.Append("<p> New Order Received.</p>");
//                htmlString.Append("Confirmation Num.:  <b>" + confirmationId + "</b><br />");
//                htmlString.Append("<table border='1'><tr>");
//                htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
//                htmlString.Append("<tr><td>" + _booking[0].PaxTitle + " " + _booking[0].PaxFName + " " + _booking[0].PaxLName + " </td><td> " + _booking[0].PaxPhoneNumber + "</td>");
//                htmlString.Append("<td>" + _booking[0].CheckIn + "</td> ");
//                htmlString.Append("<td>" + _booking[0].CheckOut + "</td>");
//                htmlString.Append("</tr>");
//                htmlString.Append("</table><br />");
//                htmlString.Append("<span><b>Adults:</b></span><span>" + _booking[0].Adults + "</span><br />");
//                htmlString.Append("<span><b>Children:</b></span><span>" + _booking[0].Children + "</span><br />");
//                htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + _booking[0].NoOfRooms + "</span><br />");
//                htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
//                //htmlString.Append("<span><b>Total Amount:</b></span><span>" + _booking[0].TotalAmount + "</span><br />");
//                htmlString.Append("</body>");
//                htmlString.Append("</html>");

//                MailMessage mm = new MailMessage();
//                mm.IsBodyHtml = true;
//                mm.To.Add("info@mountfeasthotel.com");

//                mm.From = new MailAddress("growbiztec@gmail.com");
//                mm.Subject = "Order Confirmation: " + confirmationId;
//                mm.Body = htmlString.ToString();
//                SmtpClient smc = new SmtpClient();
//                smc.Host = "smtp.gmail.com";
//                smc.Port = 587;
//                // smc.UseDefaultCredentials = false;
//                smc.EnableSsl = true;
//                smc.Credentials = new System.Net.NetworkCredential("growbiztec@gmail.com", "growbiz@2022");
//                smc.DeliveryMethod = SmtpDeliveryMethod.Network;

//                smc.Send(mm);


//            }
//            catch (Exception ex)
//            {
//                TempData["error"] = ex.Message;
//            }

//            if (_booking[0].PaxEmail != "")
//            {
//                try
//                {
//                    StringBuilder htmlString = new StringBuilder();
//                    htmlString.Append(@"<html>");
//                    htmlString.Append("<body>");
//                    htmlString.Append("<h3>Thanks</h3>");
//                    htmlString.Append("<p> We have received your booking order and are working on a confirmation.</p>");
//                    htmlString.Append("Your Confirmation Num.:  <b>" + confirmationId + "</b><br />");
//                    htmlString.Append("<table border='1'><tr>");
//                    htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
//                    htmlString.Append("<tr><td>" + _booking[0].PaxTitle + " " + _booking[0].PaxFName + " " + _booking[0].PaxLName + " </td><td>" + _booking[0].PaxPhoneNumber + " </td>");
//                    // htmlString.Append("<td>" + RoomCates + "</td>");
//                    htmlString.Append("<td>" + _booking[0].CheckIn + "</td> ");
//                    htmlString.Append("<td>" + _booking[0].CheckOut + "</td>");
//                    htmlString.Append("</tr>");
//                    htmlString.Append("</table><br />");
//                    htmlString.Append("<span><b>Adults:</b></span><span>" + _booking[0].Adults + "</span><br />");
//                    htmlString.Append("<span><b>Children:</b></span><span>" + _booking[0].Children + "</span><br />");
//                    htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + _booking[0].NoOfRooms + "</span><br />");
//                    htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
//                    //htmlString.Append("<span><b>Total Amount:</b></span><span>" + TotalAmount + "</span><br />");
//                    htmlString.Append("</body>");
//                    htmlString.Append("</html>");

//                    MailMessage mm = new MailMessage();
//                    mm.IsBodyHtml = true;
//                    mm.To.Add(_booking[0].PaxEmail);

//                    mm.From = new MailAddress("info@mountfeasthotel.com");
//                    mm.Subject = "Order Confirmation: " + confirmationId;
//                    mm.Body = htmlString.ToString();
//                    SmtpClient smc = new SmtpClient();
//                    smc.Host = "mail.mountfeasthotel.com";
//                    smc.Port = 587;
//                    // smc.UseDefaultCredentials = false;
//                    smc.EnableSsl = true;
//                    smc.Credentials = new System.Net.NetworkCredential("info@mountfeasthotel.com", "Emirates321");
//                    smc.DeliveryMethod = SmtpDeliveryMethod.Network;
//                    smc.Send(mm);


//                }
//                catch (Exception ex)
//                {
//                    TempData["error"] = ex.Message;
//                }

//            }

//            return View();
//        }
//        else
//        {
//            return RedirectToAction("PaymentError", "SearchResult", new { @pp_ResponseCode = pp_ResponseCode, @pp_ResponseMessage = pp_ResponseMessage });
//        }



//    }
//    catch (Exception ex)
//    {

//        return Json(new { status = "error", message = ex.ToString() });
//    }
//}

//[HttpPost]
//public ActionResult SaveBooking()
//{
//    try
//    {
//        string PaxTitle = Request.Form["PaxTitle"];
//        string PaxFName = Request.Form["PaxFName"];
//        string PaxLName = Request.Form["PaxLName"];
//        string checkIn = Convert.ToString(Request.Form["CheckIn"]);
//        string checkOut = Convert.ToString(Request.Form["CheckOut"]);
//        int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
//        string PaxPhoneNumber = Request.Form["PaxPhoneNumber"];
//        string PaxEmail = Request.Form["PaxEmail"];
//        string PaxDescription = Request.Form["PaxDescription"];
//        int Adults = Convert.ToInt32(Request.Form["Adults"]);
//        int Children = Convert.ToInt32(Request.Form["Children"]);
//        int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
//        int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
//        int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
//        string RoomCatCodes = Request.Form["RoomCatCode"];
//        string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
//        string RatePlanIDs = Request.Form["RatePlanIDs"];
//        string RoomRates = Request.Form["RoomRates"];

//        string[] _CheckIn = checkIn.Split('-');
//        string[] _CheckOut = checkOut.Split('-');

//        checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
//        checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

//        DateTime CheckIn = Convert.ToDateTime(checkIn);
//        DateTime CheckOut = Convert.ToDateTime(checkOut);

//        if (Adults == 0)
//        {
//            Adults = 1;
//        }

//        using (var db = new HotelDBEntities())
//        {
//            var rowsAffected = db.Bookings.Where(x => x.PaxFName == PaxFName && x.PaxLName == PaxLName &&
//                        x.CheckIn == CheckIn).FirstOrDefault();

//            if (true == true)
//            {
//                int? bookingID = db.Bookings.DefaultIfEmpty().Max(p => p == null ? 0 : p.BookingID);
//                string confirmationId = "HMF-" + Convert.ToString(bookingID + 1);
//                int? PropertyId = db.Rooms.Where(p => p.RoomCategory == RoomCatCode).Select(p => p.propertyID).FirstOrDefault();
//                int? RoomCode = 0;
//                string RoomCate = "";
//                string RoomCates = "";
//                TimeSpan t = Convert.ToDateTime(CheckOut) - Convert.ToDateTime(CheckIn);
//                int noOfDay = (int)t.TotalDays;
//                string[] _RoomCatCodes;
//                string[] _RoomCateRoomNums;
//                string[] _RatePlanIDs;
//                string[] _RoomRates;
//                double GrandTotal = 0;

//                RoomRates = RoomRates.Remove(0, 1);
//                _RoomRates = RoomRates.Split(',');

//                RoomCatCodes = RoomCatCodes.Remove(0, 1);
//                _RoomCatCodes = RoomCatCodes.Split(',');

//                RatePlanIDs = RatePlanIDs.Remove(0, 1);
//                _RatePlanIDs = RatePlanIDs.Split(',');

//                RoomCateRoomNums = RoomCateRoomNums.Remove(0, 1);
//                _RoomCateRoomNums = RoomCateRoomNums.Split(',');

//                //foreach (var itm in _RoomCatCodes)
//                for (int itm = 0; itm < _RoomCatCodes.Length; itm++)
//                {
//                    int CateCode = Convert.ToInt32(_RoomCatCodes[itm]);
//                    RoomCode = db.Rooms.Where(p => p.RoomCategory == CateCode).Select(p => p.RoomCode).FirstOrDefault();
//                    RoomCate = db.RoomCategories.Where(p => p.Id == CateCode).Select(p => p.Category).FirstOrDefault();
//                    RoomCates = RoomCates + "," + RoomCate;

//                    //foreach (var itm1 in _RoomCateRoomNums)
//                    {
//                        int NoofRooms = Convert.ToInt32(_RoomCateRoomNums[itm]);

//                        for (int i = 1; i <= NoofRooms; i++)
//                        {
//                            var bookedRooms = db.Bookings.Where(x => x.RoomCode == RoomCode && x.CheckIn == CheckIn).Select(p => p.RoomNum).ToList();
//                            var RoomNum = (db.RoomNumbers.Where(p => p.RoomCode == RoomCode).Select(p => p.RoomNumber1).ToList());
//                            //var roomnum = db.RoomNumbers.Where(p => p.RoomNumber1 == RoomNum).FirstOrDefault();

//                            var roomnum = RoomNum.Except(bookedRooms).FirstOrDefault();

//                            if (roomnum != null)
//                            {
//                                //roomnum.Booked = true;

//                                //db.Entry(roomnum).State = System.Data.EntityState.Modified;
//                                //db.SaveChanges();

//                                GrandTotal = GrandTotal + TotalAmount;

//                                Booking booking = new Booking();

//                                booking.PaxTitle = PaxTitle;
//                                booking.PaxFName = PaxFName;
//                                booking.PaxLName = PaxLName;
//                                booking.NoOfRooms = 1;
//                                booking.RoomCode = RoomCode;
//                                booking.RoomCatCode = CateCode;
//                                booking.RoomNum = roomnum;
//                                booking.PaxEmail = PaxEmail;
//                                booking.PaxPhoneNumber = PaxPhoneNumber;
//                                booking.PaxDescription = PaxDescription;
//                                booking.Adults = Adults;
//                                booking.Children = Children;
//                                booking.CheckIn = CheckIn;
//                                booking.CheckOut = CheckOut;
//                                booking.RoomRate = Convert.ToInt32(_RoomRates[itm]);
//                                booking.TotalRoomRate = Convert.ToInt32(_RoomRates[itm]);
//                                booking.HotelCode = 5;
//                                booking.ConfirmationNo = confirmationId;
//                                booking.DateModified = DateTime.Now;
//                                booking.BookingStatus = "Pending";
//                                booking.NoofDay = noOfDay;
//                                booking.TotalRoomRate = TotalAmount;
//                                booking.ThemeColor = "red";
//                                booking.RatePlanId = Convert.ToInt32(_RatePlanIDs[itm]);

//                                db.Entry(booking).State = EntityState.Added;
//                                db.SaveChanges();
//                            }
//                        }
//                    }
//                }
//                return RedirectToAction("Payment", "Jazzcash", new { @billReference = confirmationId, @GrandTotal = GrandTotal });

//                // return ("Booking done succesfully");
//                //jsonString = JsonSerializer.Serialize(confirmationId);

//            }
//            else
//            {
//                // responseBasicProfile.MessageList.Add("Not Record Found.");
//                return RedirectToAction("Index", "Home");
//            }
//        }

//    }
//    catch (Exception ex)
//    {
//        //TempData["error"] = ex.Message;
//        return RedirectToAction("Index", "Home");
//    }
//}