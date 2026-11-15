using HotelWebSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelDePapaeMVC.Controllers.Dashboard
{
    public class DashboardController : Controller
    {
        HotelDBEntities HDE = new HotelDBEntities();

        [Authorize]
        // GET: Dashboard
        public ActionResult Index()
        {
            Counts();
            ReservationCounts("All");
            return View();
        }

        public string Counts()
        {
            try
            {
                int TodaysBooking = HDE.Bookings.Where(x => x.ReservationDate == DateTime.Today).Count();
                int TodaysCheckIn = HDE.Bookings.Where(x => x.CheckIn == DateTime.Today).Count();
                int TodaysCheckOut = HDE.Bookings.Where(x => x.CheckOut == DateTime.Today).Count();
                int TodaysCancelled = HDE.Bookings.Where(x => x.CheckIn == DateTime.Today && x.BookingStatus == "Cancelled").Count();
                int TotalCategories = HDE.RoomCategories.Where(x => x.Status == true).Count();
                int TotalRooms = HDE.RoomNumbers.Where(x => x.Status == true).Count();
                int TotalBookings = HDE.Bookings.Count();

                TempData["TodaysBooking"] = TodaysBooking;
                TempData["TodaysCheckIn"] = TodaysCheckIn;
                TempData["TodaysCheckOut"] = TodaysCheckOut;
                TempData["TodaysCancelled"] = TodaysCancelled;
                TempData["TotalCategories"] = TotalCategories;
                TempData["TotalRooms"] = TotalRooms;
                TempData["TotalBookings"] = TotalBookings;

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }


        // Code for Arrival and Departure Report by Snr Software Engineer Zubair
        public string ReservationCounts(string dateRange = "Today")
        {
            try
            {
                // Define the filter date range based on the input (Today, Yesterday, Tomorrow, or All)
                DateTime? startDate, endDate;
                GetFilterDateRange(dateRange, out startDate, out endDate);

                // Fetch arrivals based on the CheckIn date range
                var arrivalsQuery = HDE.Bookings.AsQueryable();
                if (startDate.HasValue && endDate.HasValue)
                {
                    arrivalsQuery = arrivalsQuery.Where(obj => obj.CheckIn.HasValue &&
                                                               obj.CheckIn.Value.Date >= startDate.Value &&
                                                               obj.CheckIn.Value.Date < endDate.Value);
                }
                else if (dateRange == "All")
                {
                    arrivalsQuery = arrivalsQuery.Where(obj => obj.CheckIn.HasValue);
                }

                // Count the arrivals for the specified date range
                int arrivalCount = arrivalsQuery.Count();

                // Fetch departures based on the CheckOut date range
                var departuresQuery = HDE.Bookings.AsQueryable();
                if (startDate.HasValue && endDate.HasValue)
                {
                    departuresQuery = departuresQuery.Where(obj => obj.CheckOut.HasValue &&
                                                                   obj.CheckOut.Value.Date >= startDate.Value &&
                                                                   obj.CheckOut.Value.Date < endDate.Value);
                }
                else if (dateRange == "All")
                {
                    departuresQuery = departuresQuery.Where(obj => obj.CheckOut.HasValue);
                }

                // Count the departures for the specified date range
                int departureCount = departuresQuery.Count();

                // Set counts in ViewBag to use in the view
                ViewBag.ArrivalCount = arrivalCount;
                ViewBag.DepartureCount = departureCount;

                return "";
            }
            catch (Exception ex)
            {
                // Handle any errors and set counts to 0
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                ViewBag.ArrivalCount = 0;
                ViewBag.DepartureCount = 0;
                return "";
            }
        }
        [HttpPost]
        public JsonResult GetDepartureReport(string dateRange)
        {
            try
            {
                // Determine the filter date range based on the input (Today, Yesterday, Tomorrow)
                DateTime? startDate, endDate;
                GetFilterDateRange(dateRange, out startDate, out endDate);

                // Query for departures based on the CheckOut date range
                var query = HDE.Bookings
                    .Join(HDE.RoomCategories,
                          obj => obj.RoomCatCode,
                          room => room.Id,
                          (obj, room) => new { obj, room })
                    .Where(x => startDate == null ||
                                (x.obj.CheckOut >= startDate && x.obj.CheckOut < endDate))
                    .OrderByDescending(x => x.obj.CheckOut)
                    .Select(x => new
                    {
                        x.obj.ConfirmationNo,
                        x.obj.BookingStatus,
                        x.obj.ReservationDate,
                        x.obj.HotelCode,
                        x.obj.PaxTitle,
                        x.obj.PaxFName,
                        x.obj.PaxLName,
                        x.obj.NoOfRooms,
                        x.obj.RoomCode,
                        x.obj.RoomCatCode,
                        x.obj.RoomNum,
                        x.obj.PaxEmail,
                        x.obj.PaxPhoneNumber,
                        x.obj.PaxDescription,
                        x.obj.Adults,
                        x.obj.Children,
                        x.obj.CheckIn,
                        x.obj.CheckOut,
                        x.obj.NoofDay,
                        x.obj.CountryCode,
                        x.obj.TotalRoomRate,
                        Category = x.room.Category
                    })
                    .ToList();

                return Json(query, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("An error occurred: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetArrivalReport(string dateRange)
        {
            try
            {
                // Determine the filter date range based on the input (Today, Yesterday, Tomorrow)
                DateTime? startDate, endDate;
                GetFilterDateRange(dateRange, out startDate, out endDate);

                // Query for arrivals based on the CheckIn date range
                var res = HDE.Bookings
                    .Join(HDE.RoomCategories,
                          obj => obj.RoomCatCode,
                          room => room.Id,
                          (obj, room) => new { obj, room })
                    .Where(x => startDate == null ||
                                (x.obj.CheckIn >= startDate && x.obj.CheckIn < endDate))
                    .OrderByDescending(x => x.obj.CheckIn)
                    .Select(x => new
                    {
                        x.obj.ConfirmationNo,
                        x.obj.BookingStatus,
                        x.obj.ReservationDate,
                        x.obj.HotelCode,
                        x.obj.PaxTitle,
                        x.obj.PaxFName,
                        x.obj.PaxLName,
                        x.obj.NoOfRooms,
                        x.obj.RoomCode,
                        x.obj.RoomCatCode,
                        x.obj.RoomNum,
                        x.obj.PaxEmail,
                        x.obj.PaxPhoneNumber,
                        x.obj.PaxDescription,
                        x.obj.Adults,
                        x.obj.Children,
                        x.obj.CheckIn,
                        x.obj.CheckOut,
                        x.obj.NoofDay,
                        x.obj.CountryCode,
                        x.obj.TotalRoomRate,
                        Category = x.room.Category
                    })
                    .ToList();

                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("An error occurred: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to determine the filter date range based on date range
        private void GetFilterDateRange(string dateRange, out DateTime? startDate, out DateTime? endDate)
        {
            DateTime today = DateTime.Now.Date;

            switch (dateRange)
            {
                case "Today":
                    startDate = today;
                    endDate = today.AddDays(1);
                    break;
                case "Yesterday":
                    startDate = today.AddDays(-1);
                    endDate = today;
                    break;
                case "Tomorrow":
                    startDate = today.AddDays(1);
                    endDate = today.AddDays(2);
                    break;
                default:
                    startDate = null;
                    endDate = null;
                    break;
            }
        }


    }
}