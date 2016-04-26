using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LMS.Controllers
{
    public class LMSController : Controller
    {
        //
        // GET: /LMS/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Booking ()
        {
            return View();
        }

        public ActionResult BookingDetail()
        {
            return View();
        }

        public ActionResult Queue()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();

        }

        public ActionResult QueueDetail()
        {
            return View();
        }
    }
}
