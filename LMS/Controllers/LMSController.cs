using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LMS.Models;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace LMS.Controllers
{
    public class LMSController : Controller
    {
        //
        // GET: /LMS/
        public synergilimoEntities db = new synergilimoEntities();

       
     
        public ActionResult Login(User u)
        {
            // this action is for handle post (login)
            if (ModelState.IsValid) // this is check validity
            {
               
                {
                    var v = db.LMS_User.Where(a => a.UserName.Equals(u.UserName) && a.Password.Equals(u.Password)).FirstOrDefault();
                    if (v != null)
                    {
                        Session["LogedUserID"] = v.UserID.ToString();
                        Session["LogedUserFullname"] = v.FullName.ToString();
                        Session["LogedUserType"] = v.UesrType.ToString();
                        Session["LogedAgentID"] = v.AgentID.ToString();

                        return RedirectToAction("Booking");
                    }
                }
            }
            return View(u);
        }

        public ActionResult LogOut()
        {
            Session["LogedUserID"] = "";
            Session["LogedUserFullname"] = "";
            Session["LogedUserType"] = "";
            Session["LogedAgentID"] = "";
            return RedirectToAction("Booking");
        }
        public ActionResult AfterLogin()
        {
            if (Session["LogedUserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Index()
        {
             string sUserType = "0";
             if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

             int UserType = 0;
             if (sUserType == "")
             {
                 UserType = 0;
             }
             else
             {
                 UserType = Convert.ToInt32(sUserType);
             }
         

         
            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                              select new
                              {
                                  uAgentID = u.AgentID
                              }
                   ).FirstOrDefault();
            List<BookingList> model = new List<BookingList>();
         if (UserID.uAgentID == 0)
         {
              var sBookingList = (from b in db.LMS_Booking
                                join c in db.LMS_Car on b.CarID equals c.ID               
                                  where b.BookingDate.Value.Month == DateTime.Now.Month && b.BookingDate.Value.Year == DateTime.Now.Year
                            //    where Convert.ToDateTime(b.BookingDate).Month == DateTime.Now.Month  
                                orderby b.BookingID descending
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    BDate = b.Date,
                                    BTime = b.Time,
                                    BService = b.ServiceType,
                                    BForm = b.FromDetail,
                                    BTo = b.ToDetail,
                                    RouteDetai = b.RouteDetail,
                                    CarID = b.CarID,
                                    CarModel = c.Model,
                                    BStatus = b.Status,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID
                                }
               ).ToList();
              foreach (var bl in sBookingList)
              {
                  BookingList a = new BookingList();
                  a.BookingID = bl.BookingID.ToString();
                  a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                  a.Date = Convert.ToDateTime(bl.BDate);
                  a.Time = bl.BTime.ToString();
                  a.ServiceType = Convert.ToInt32(bl.BService.ToString());
                  a.FromDetail = bl.BForm.ToString();
                  a.ToDetail = bl.BTo.ToString();
                  a.CarModel = bl.CarModel.ToString();
                  a.RouteDetail = bl.RouteDetai;
                  a.Status = Convert.ToInt32(bl.BStatus);
                  a.AgentID = Convert.ToInt32(bl.AgentID);
                  a.UserID = Convert.ToInt32(bl.UserID);
                  model.Add(a);

              }
         }
         else
         {
             var sBookingList = (from b in db.LMS_Booking
                                join v in db.LMS_Vehicle on b.CarID equals v.ID
                                where b.AgentID == UserID.uAgentID
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BDate = b.Date,
                                    BService = b.ServiceType,
                                    BForm = b.FromDetail,
                                    BTo = b.ToDetail,
                                    CarID = b.CarID,
                                    CarModel = v.Name,
                                    BStatus = b.Status,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID
                                }
               ).ToList();
             foreach (var bl in sBookingList)
             {
                 BookingList a = new BookingList();
                 a.BookingID = bl.BookingID.ToString();
                 a.Date = Convert.ToDateTime(bl.BDate);
                 a.FromDetail = bl.BForm.ToString();
                 a.ToDetail = bl.BTo.ToString();
                 a.CarModel = bl.CarModel.ToString();
                 a.Status = Convert.ToInt32(bl.BStatus);
                 a.AgentID = Convert.ToInt32(bl.AgentID);
                 a.UserID = Convert.ToInt32(bl.UserID);
                 model.Add(a);

             }
         }
           
           
            return View(model);
        }


        public JsonResult searchRoute(string dFrom)
         {
             int dS = 1;
             dS = Convert.ToInt32(dFrom);
             var dTo = (from r in db.LMS_Route
                     join  t in db.LMS_TO  on r.ToID equals t.ID
                        where r.FromID == dS
                     orderby t.Detail ascending
                         select new
                         {
                             tDetail = t.Detail,
                             tID = t.ID
                         }
                  ).ToList();
          List<ToModel> data = new List<ToModel>();
          foreach (var bt in dTo)
          {
              ToModel b = new ToModel();
              b.TDetail = bt.tDetail.ToString();
              b.TId = Convert.ToInt32(bt.tID.ToString());
              data.Add(b);
          }

          return Json(data, JsonRequestBehavior.AllowGet);
         }

        public ActionResult Booking ()
        {
         //  var path = Path.Combine(Server.MapPath("~/DriverProfile/"));
            string sUserID = "0";
            if (Session["LogedUserID"] != null)
            {
                sUserID = Session["LogedUserID"].ToString();
            }
            else
            {
                sUserID = "0";
            }

            int UserID = 0;
            if (sUserID == "")
            {
                UserID = 0;
            }
            else
            {
               UserID = Convert.ToInt32(sUserID);
            }
         
           
            var dForm = (from p in db.LMS_From
                       //  orderby p.Detail ascending
                         select new
                    {
                        cDetail = p.Detail,
                        cID = p.ID
                    }
                    ).ToList();
            var dTo = (from t in db.LMS_TO
                       where t.FromID == 1 
                       orderby t.Detail ascending
                         select new
                         {
                             tDetail = t.Detail,
                             tID = t.ID
                         }
                  ).ToList();

            var dCar = (from c in db.LMS_Vehicle
                        orderby c.Name ascending
                       select new
                       {
                           tModel = c.Name,
                           tID = c.ID
                       }
                ).ToList();

          

            List<FromToModel> model = new List<FromToModel>();
            List<FromModel> FModel = new List<FromModel>();
            List<ToModel> TModel = new List<ToModel>();
            List<Car> Car = new List<Car>();
            List<SubAgent> SubAgent = new List<SubAgent>();

            if (UserID == 1)
            {
                var dSubAgent = (from c in db.LMS_SubAgent
                                 select new
                                 {
                                     tName = c.Name,
                                     tID = c.ID,
                                     tUserID = c.UserID
                                 }
              ).ToList();
                foreach (var bs in dSubAgent)
                {
                    SubAgent s = new SubAgent();
                    s.AName = bs.tName.ToString();
                    s.AId = Convert.ToInt32(bs.tID.ToString());
                    s.UserID = Convert.ToInt32(bs.tUserID.ToString());
                    SubAgent.Add(s);
                }
            }
            else
            {
                var dSubAgent = (from c in db.LMS_SubAgent
                                 join u in db.LMS_UserSub on c.ID equals u.SubID
                                 where u.UserID == UserID
                                 select new
                                 {
                                     tName = c.Name,
                                     tID = c.ID,
                                     tUserID = c.UserID
                                 }
              ).ToList();
                foreach (var bs in dSubAgent)
                {
                    SubAgent s = new SubAgent();
                    s.AName = bs.tName.ToString();
                    s.AId = Convert.ToInt32(bs.tID.ToString());
                    s.UserID = Convert.ToInt32(bs.tUserID.ToString());
                    SubAgent.Add(s);
                }
            }
           
            foreach (var bf in dForm)
            {
                FromModel a = new FromModel();
                a.FDetail = bf.cDetail.ToString();
                a.FId = Convert.ToInt32(bf.cID.ToString());
                FModel.Add(a);
            }

            foreach (var bt in dTo)
            {
                ToModel b = new ToModel();
                b.TDetail = bt.tDetail.ToString();
                b.TId = Convert.ToInt32(bt.tID.ToString());
                TModel.Add(b);
            }

          

            foreach (var cc in dCar)
            {
                Car c = new Car();
                c.CModel = cc.tModel.ToString();
                c.CId = Convert.ToInt32(cc.tID.ToString());
                Car.Add(c);
            }

           
            FromToModel FT = new FromToModel();
            FT.sFromModel = FModel.ToList();
            FT.sToModel = TModel.ToList();
            FT.sCar = Car.ToList();
            FT.sSubAgent = SubAgent.ToList();

            model.Add(FT);
          
          //  a.Id = 22;
         
            return View(model);
        }

      
        public ActionResult BookingDetail(FormCollection form)
        {

        
            BookingDetail bkd = new BookingDetail();

            bkd.ServiceType = Convert.ToInt32(Request.QueryString["ServiceType"]);

            if (bkd.ServiceType == 1)
            {
                bkd.FromID = Convert.ToInt32(Request.QueryString["From"]);
            
           }else if (bkd.ServiceType == 3)
            {
                bkd.FromID = 1;
            }
          
            if (bkd.FromID != 0)
            {
                bkd.ServiceType = Convert.ToInt32(Request.QueryString["ServiceType"]);
                bkd.AgentID = Convert.ToInt32(Request.QueryString["SubAgent"]);
                bkd.CustomerType = Convert.ToInt32(Request.QueryString["CustomerType"]);
                bkd.UserID = Convert.ToInt32(Request.QueryString["UserType"]);
                bkd.Status = 1;

                string sDate = "";

                if (bkd.ServiceType == 1)
                {
                    bkd.VechileID = Convert.ToInt32(Request.QueryString["CarModel"]);

                    sDate = Request.QueryString["Date"];

                    if (sDate == null || sDate == "")
                    {
                        sDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }

                    bkd.Date = Convert.ToDateTime(sDate);
                   

                    bkd.FlightNo = Request.QueryString["Flight"];
                    bkd.FlightTime = Request.QueryString["FlightTime"];

                    bkd.ToID = Convert.ToInt32(Request.QueryString["To"]);
                    bkd.Luggage = Convert.ToInt32(Request.QueryString["Luggage"]);
                  
                  
                    bkd.Passenger = Convert.ToInt32(Request.QueryString["Passenger"]);

                   
                    bkd.Time = Request["Time"];
               
                    bkd.Vechile = Convert.ToInt32(Request.QueryString["Vechile"]);
                    bkd.Currency = Request["Currency"];

                }
                else if (bkd.ServiceType == 3)
                {
                    bkd.VechileID = Convert.ToInt32(Request.QueryString["CarModel3"]);

                    sDate = Request.QueryString["Date3"];

                    if (sDate == null || sDate == "")
                    {
                        sDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }


                    bkd.Date = Convert.ToDateTime(sDate);


                    bkd.FlightNo = Request.QueryString["Flight3"];
                    bkd.FlightTime = Request.QueryString["FlightTime3"];

                    bkd.ToID = 1;
                    bkd.Luggage = Convert.ToInt32(Request.QueryString["Luggage3"]);
     
                    bkd.Passenger = Convert.ToInt32(Request.QueryString["Passenger3"]);
                    bkd.RouteDetail = Request.QueryString["RouteDetail"];
 
                 
                    bkd.Time = Request.QueryString["Time3"];
        
                    bkd.Vechile = Convert.ToInt32(Request.QueryString["Vechile3"]);
                    bkd.Currency = Request.QueryString["Currency3"];
                }



                var CarModel = (from c in db.LMS_Vehicle
                                where c.ID == bkd.VechileID
                                select c.Name
                   ).First();

                var FromDetail = (from f in db.LMS_From
                                  where f.ID == bkd.FromID
                                  select f.Detail
                    ).First();

                var ToDetail = (from t in db.LMS_TO
                                where t.ID == bkd.ToID
                                select t.Detail
                    ).First();

                bkd.CarModel = CarModel.ToString();
                bkd.FromDetail = FromDetail.ToString();
                bkd.ToDetail = ToDetail.ToString();

                var Product = (from p in db.LMS_Product
                               join r in db.LMS_Route on p.RouteID equals r.ID
                               where r.FromID == bkd.FromID && r.ToID == bkd.ToID && p.TpyeOfService == bkd.ServiceType && p.TpyeOfVihicle == bkd.VechileID
                               select new
                               {
                                   ProductID = p.ID,
                                   PriceTHB = p.PriceTHB,
                                   PriceUSD = p.PriceUSD,
                                   PriceEUR = p.PriceEUR
                               }
                      ).First();

                var SaleVisitPrice = (from p in db.LMS_Vehicle
                                      where p.ID == bkd.VechileID
                               select new
                               {
                                   ProductID = p.ID,
                                   PriceTHB = p.PriceTHB,
                                   PriceUSD = p.PriceUSD,
                                   PriceEUR = p.PriceEUR
                               }
                    ).First();

                string dTime = "";
                dTime = bkd.Time.Substring(0, 2) + bkd.Time.Substring(3, 2);
                int iTime = Convert.ToInt32(dTime);
                var WorkTime = (from t in db.LMS_WorkTime
                                where t.TStart <= iTime && t.TEnd >= iTime
                                select t.TID
                   ).FirstOrDefault();

                int iTID = 0; 
                if (WorkTime == 0)
                {
                    iTID = 2;
                }
                else
                {
                    iTID = WorkTime;
                }
                
               
                var Car = (from c in db.LMS_Car
                               join dc in db.LMS_DriverOfCar on c.ID equals dc.CarID
                                join d in db.LMS_Driver on dc.DriverID equals d.ID
                           where c.VehicleID == bkd.VechileID && dc.TID == iTID
                           orderby dc.LastBooking,dc.LastJobDate ascending,dc.LastJobTime ascending,c.ID
                               select new
                               {
                                   DID = dc.DID,
                                   CarID = dc.CarID,
                                   DriverID = dc.DriverID,
                                   TID = dc.TID,
                                   VehicleRegis = c.VehicleRegis,
                                   DTitle = d.Title,
                                   DFirstName = d.Name,
                                   DLastName = d.LastName,
                                   DMobile = d.Mobile
                               }
                     ).First();

                var AgentEmail = (from ag in db.LMS_SubAgent
                               where ag.ID == bkd.AgentID 
                               select new
                               {
                                   aID = ag.ID,
                                   aName = ag.Name,
                                   aEmail = ag.Email
                               }
                    ).FirstOrDefault();

                var AgentPrice = (from ap in db.LMS_AgentPrice
                                  where ap.AgentID == bkd.AgentID
                                  select new
                                  {
                                      aID = ap.ID,
                                      aDiscountP = ap.DiscountP,
                                      aDiscountB = ap.DiscountB,
                                      aMarkup = ap.Markup,
                                      aPriceTHB = ap.PriceTHB,
                                      aPriceUSD = ap.PriceUSD,
                                      aPriceEUR = ap.PriceEUR
                                  }
                  ).FirstOrDefault();

              
                bkd.ProductID = Convert.ToInt32(Product.ProductID.ToString());

                if (bkd.ServiceType == 3)
                {
                    if (bkd.Currency == "THB")
                    {
                        bkd.Price = Convert.ToDecimal(SaleVisitPrice.PriceTHB.ToString()) * bkd.Vechile;
                    }
                    else if (bkd.Currency == "USD")
                    {
                        bkd.Price = Convert.ToDecimal(SaleVisitPrice.PriceUSD.ToString()) * bkd.Vechile;
                    }
                    else if (bkd.Currency == "EUR")
                    {
                        bkd.Price = Convert.ToDecimal(SaleVisitPrice.PriceEUR.ToString()) * bkd.Vechile;
                    }
                }
                else
                {
                    if (bkd.Currency == "THB")
                    {
                        bkd.Price = Convert.ToDecimal(Product.PriceTHB.ToString()) * bkd.Vechile;
                    }
                    else if (bkd.Currency == "USD")
                    {
                        bkd.Price = Convert.ToDecimal(Product.PriceUSD.ToString()) * bkd.Vechile;
                    }
                    else if (bkd.Currency == "EUR")
                    {
                        bkd.Price = Convert.ToDecimal(Product.PriceEUR.ToString()) * bkd.Vechile;
                    }
                }
                
               

                bkd.DID = Convert.ToInt32(Car.DID.ToString());
                bkd.CarID = Convert.ToInt32(Car.CarID.ToString());
                bkd.DriverID = Convert.ToInt32(Car.DriverID.ToString());
                bkd.TID = Convert.ToInt32(Car.TID.ToString());
                bkd.VehicleRegis = Car.VehicleRegis.ToString();
                bkd.DTitle = Car.DTitle.ToString();
                bkd.DFirstName = Car.DFirstName.ToString();
                bkd.DLastName = Car.DLastName.ToString();
                bkd.DMobile = Car.DMobile.ToString();

                if (bkd.CustomerType == 2)
                {
                    if (bkd.AgentID == 0)
                    {
                        bkd.AgentEmail = "";
                        bkd.DiscountP = 0;
                        bkd.DiscountB = 0;
                        bkd.Discount = 0;
                        bkd.AgentName = "";
                    }else
                    {
                   
                            if (bkd.Currency == "THB")
                            {
                                bkd.Price = Convert.ToDecimal(AgentPrice.aPriceTHB.ToString()) * bkd.Vechile;
                            }
                            else if (bkd.Currency == "USD")
                            {
                                bkd.Price = Convert.ToDecimal(AgentPrice.aPriceUSD.ToString()) * bkd.Vechile;
                            }
                            else if (bkd.Currency == "EUR")
                            {
                                bkd.Price = Convert.ToDecimal(AgentPrice.aPriceEUR.ToString()) * bkd.Vechile;
                            }
                        }

                    bkd.AgentEmail = AgentEmail.aEmail.ToString();
                    bkd.DiscountP = Convert.ToDecimal(AgentPrice.aDiscountP);
                    bkd.DiscountB = Convert.ToDecimal(AgentPrice.aDiscountB);
                    bkd.Discount = (bkd.Price * (Convert.ToDecimal(AgentPrice.aDiscountP) / 100)) + Convert.ToDecimal(AgentPrice.aDiscountB);
                    bkd.AgentName = AgentEmail.aName.ToString();
                  
                    }
                else
                {
                    bkd.AgentID = 0;
  
                }
              
                Session["BookingDetail"] = bkd;
            } else
            {
                bkd = (BookingDetail)Session["BookingDetail"];
            }
            
         

            return View(bkd);
        }

        public ActionResult BookingCommit()
        {
            BookingDetail bkd = new BookingDetail();
            bkd = (BookingDetail)Session["BookingDetail"];
            bkd.BookingDate = DateTime.Today.Date;
            bkd.Title = Request["Title"];
            bkd.FirstName = Request["FirstName"];
            bkd.LastName = Request["LastName"];
            bkd.Address = Request["Address"];
            bkd.Telephone = Request["Telephone"];
            bkd.Mobile = Request["Mobile"];
            bkd.Email = Request["Email"];
            bkd.Remark = Request["Remark"];
            bkd.Price = Convert.ToDecimal(Request["Price"]);
            bkd.Discount = Convert.ToDecimal(Request["Discount"]);
            bkd.TotalPrice = Convert.ToDecimal(Request["TotalPrice"]);
            bkd.PaymentType = Convert.ToInt32(Request["optPayment"]);
            bkd.PaymentStatus = 1;

            string bYM = DateTime.Today.Year.ToString().Substring(2,2) +  DateTime.Today.Month.ToString("00");

      
            var BookingID = (from b in db.LMS_Booking
                             where b.BookingID.Substring(0, 4) == bYM
                            orderby b.BookingID descending
                            select b.BookingID
                  ).FirstOrDefault();

            string BID = "";
            string RBID = "";
            if (BookingID == null)
            {
                BID = bYM + "0001";
            }
            else
            {
                RBID = BookingID.Substring(4,4);
                BID = bYM + (Convert.ToInt32(RBID) + 1).ToString("0000");
            }

            if (bkd.ServiceType == 3)
            {
                bkd.ProductID = 0;
                bkd.FromID = 0;
                bkd.FromDetail = "";
                bkd.ToID = 0;
                bkd.ToDetail = "";

            }

            LMS_Booking AddBooking = new LMS_Booking();
         //   LMS_From ff = new LMS_From();

            //ff.Detail = "aaaaaa";
            //db.LMS_From.Add(ff);
            //db.SaveChanges();
       
            AddBooking.Address = bkd.Address;
            AddBooking.BookingDate = bkd.BookingDate;
            AddBooking.BookingID = BID;
            AddBooking.CarID = bkd.CarID;
            AddBooking.CustomerType = bkd.CustomerType;
            AddBooking.Date = bkd.Date;
            AddBooking.Discount = bkd.Discount;
            AddBooking.Email = bkd.Email;
            AddBooking.FirstName = bkd.FirstName;
            AddBooking.FlightNo = bkd.FlightNo;
            AddBooking.FlightTime = bkd.FlightTime;
            AddBooking.FromDetail = bkd.FromDetail;
            AddBooking.LastName = bkd.LastName;
            AddBooking.Luggage = bkd.Luggage;
            AddBooking.Mobile = bkd.Mobile;
            AddBooking.UserID = bkd.UserID;
            AddBooking.Passenger = bkd.Passenger;
            AddBooking.Price = bkd.Price;
            AddBooking.ProductID = bkd.ProductID;
            AddBooking.Remark = bkd.Remark;
            AddBooking.RouteDetail = bkd.RouteDetail;
            AddBooking.ServiceType = bkd.ServiceType;
            AddBooking.Status = bkd.Status;
            AddBooking.Telephone = bkd.Telephone;
            AddBooking.Time = bkd.Time;
            AddBooking.Title = bkd.Title;
            AddBooking.ToDetail = bkd.ToDetail;
            AddBooking.TotalPrice = bkd.TotalPrice;
            AddBooking.AgentID = bkd.AgentID;
            AddBooking.DID = bkd.DID;
            AddBooking.DriverID = bkd.DriverID;
            AddBooking.Currency = bkd.Currency;
            AddBooking.PaymentType = bkd.PaymentType;
            AddBooking.PaymentStatus = bkd.PaymentStatus;

            db.LMS_Booking.Add(AddBooking);
            db.SaveChanges();

            LMS_DriverOfCar UpdateQ = new LMS_DriverOfCar();

            var DriverOfCar = (from dc in db.LMS_DriverOfCar
                           where dc.DID == bkd.DID
                           select dc).Single();

            DriverOfCar.LastJobDate = bkd.Date;
            DriverOfCar.LastJobTime = bkd.Time;
            DriverOfCar.LastBooking = BID;
        
            db.SaveChanges();

               StringBuilder sb = new StringBuilder();
            //BookingInfo

              var sBookingInfo = (from b in db.LMS_Booking
                                   join c in db.LMS_Car on b.CarID equals c.ID
                                   //join u in db.LMS_User on b.UserID equals u.UserID
                                   //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                   join d in db.LMS_Driver on b.DriverID equals d.ID
                                where b.BookingID == BID
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName =b.FirstName,
                                    LastName = b.LastName,
                                    Address  = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    sCurrency = b.Currency
                                    //AgentEmail = a.Email,
                                }
                 ).ToList();
              foreach (var bl in sBookingInfo)
              {
                  BookingInfo a = new BookingInfo();
                  a.Address = bl.Address;
                  //a.AgentEmail = bl.AgentEmail.ToString();
                  //a.AgentName = bl.AgentName.ToString();
                  a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                  a.BookingID = bl.BookingID;
                  a.CarID = Convert.ToInt32(bl.CarID);
                  a.CarModel = bl.CModel;
                  a.CustomerType = Convert.ToInt32(bl.CustomerType);
                  a.Date = Convert.ToDateTime(bl.bDate);
                  a.DFirstName = bl.DName;
                  a.DLastName = bl.DLastName;
                  a.DMobile = bl.DMobile;
                  a.DriverID = Convert.ToInt32(bl.DriverID);
                  a.DTitle = bl.DTitle;
                  a.Email = bl.Email;
                  a.FirstName = bl.FirstName;
                  a.FlightNo = bl.FlightNo;
                  a.FlightTime = bl.FlightTime;
                  a.FromDetail = bl.FromDetail;
                  a.LastName = bl.LastName;
                  a.Luggage = Convert.ToInt32(bl.Luggage);
                  a.Mobile = bl.Mobile;
                  //a.OrderBy = bl.OrderBy.ToString();
                  a.Passenger = Convert.ToInt32(bl.Passenger);
                  a.Price = Convert.ToDecimal(bl.Price);
                  a.Remark = bl.Remark;
                  a.RouteDetail = bl.RouteDetail;
                  a.ServiceType = Convert.ToInt32(bl.ServiceType);
                  a.Status = Convert.ToInt32(bl.Status);
                  a.Telephone = bl.Telephone;
                  a.Time = bl.bTime;
                  a.Title = bl.Title;
                  a.ToDetail = bl.ToDetail;
                  a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                  a.VehicleRegis = bl.VehicleRegis;
                  a.Currency = bl.sCurrency;

                  if (bl.AgentID != 0)
                  {
                      var sAgent = (from sg in db.LMS_SubAgent
                                    where sg.ID == bl.AgentID
                                    select new
                                    {
                                        AgentName = sg.Name,
                                        AgentMobile = sg.Telephone,
                                        AgentEMail = sg.Email
                                    }
                   ).FirstOrDefault();

                      a.AgentEmail = sAgent.AgentEMail;
                      a.AgentMobile = sAgent.AgentMobile;
                      a.AgentName = sAgent.AgentName;

                  }
                  else
                  {

                      a.AgentEmail = "";
                      a.AgentMobile = "";
                      a.AgentName = "";

                  }
                  if (bl.UserID != 0)
                  {
                      var sUser = (from u in db.LMS_User
                                   where u.UserID == bl.UserID
                                   select new
                                   {
                                       UName = u.FullName,
                                       UEmail = u.Email,
                                       UTelephone = u.Telephone
                                   }
                   ).FirstOrDefault();

                      a.UName = sUser.UName;
                      a.UEmail = sUser.UEmail;
                      a.UTelephone = sUser.UTelephone;

                  }
                  else
                  {

                      a.UName = "";
                      a.UEmail = "";
                      a.UTelephone = "";

                  }

                  string SType = "";
                  if (a.ServiceType == 1)
                  {
                      SType = "One Way";
                  }
                  else if (a.ServiceType == 2)
                  {
                      SType = "Return";
                  }
                  else if (a.ServiceType == 3)
                  {
                      SType = "Sale Visit";
                  }
                  sb.Append(" <font size=\"2\">");
            //      sb.Append("                <table>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td>");
            //sb.Append("                        Thank you for your reservation with Synergi.Your transfer service has been booked as follow<br>");
            //sb.Append("                            Please check to ensure all details are correct.");
            //sb.Append("                        </td>");
            //sb.Append("                </table>");
            //sb.Append("                <br>");
            //sb.Append("                <table border=\"1\" width=\"100%\">");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Lead Passenger : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Title +" &nbsp;" + a.FirstName +" &nbsp;"+ a.LastName +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Address : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Address +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>EMail : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Email +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Contact Call : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Mobile +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                </table>");
            //sb.Append("                <br>");
            //sb.Append("                <table border=\"1\" width=\"100%\">");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Type of Service  : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ SType +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Passenger : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Passenger +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Luggage : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Luggage +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Flight Detail : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.FlightNo +" &nbsp; "+ a.FlightTime +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Pick up date : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Date +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Pick up time : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Time +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>From : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.FromDetail +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>To : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.ToDetail +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Remark : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.Remark +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                </table>");
            //sb.Append("                <br>");
            //sb.Append("                <table border=\"1\" width=\"100%\">");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Agent : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.AgentName +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>EMail : &nbsp;</b></td>");
            //sb.Append("                        <td>"+ a.AgentEmail +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Contact Call : &nbsp;</b></td>");
            //sb.Append("                        <td>"+ a.AgentMobile +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Booked by : &nbsp;</b></td>");
            //sb.Append("                        <td>"+ a.UName +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Booked ID : &nbsp;</b></td>");
            //sb.Append("                        <td>IV &nbsp;"+ a.BookingID +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Booked Date : &nbsp;</b></td>");
            //sb.Append("                        <td>"+ a.BookingDate +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>EMail : &nbsp;</b></td>");
            //sb.Append("                        <td>"+ a.UEmail +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Contact Call : &nbsp;</b></td>");
            //sb.Append("                        <td>"+ a.UTelephone +"<br> </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                </table>");
            //sb.Append("                <br>");
            //sb.Append("                <table border=\"1\" width=\"100%\">");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Meeting point : &nbsp;</b> </td>");
            //sb.Append("                        <td>The chauffeur will send SMS or call to re -confirmed 24 hours before your travel date. <br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                </table>");
            //sb.Append("                <br>");
            //sb.Append("                <table border=\"1\" width=\"100%\">");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Type of car : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.CarModel +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>Chauffeur : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.DTitle +" &nbsp;"+ a.DFirstName +" &nbsp;"+ a.DLastName +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                       <td width=\"20%\"><b>License Plate : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.VehicleRegis +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td width=\"20%\"><b>Contact Call : &nbsp;</b> </td>");
            //sb.Append("                        <td>"+ a.DMobile +"<br></td>");
            //sb.Append("                    </tr>");
            //sb.Append("                </table>");
            //sb.Append("                <br>");
            //sb.Append("                <table>");
            //sb.Append("                    <tr>");
            //sb.Append("                        <td>");
            //sb.Append("                        Term and Condition<br>");
            //sb.Append("                            Cancellation/ No show<br>");
            //sb.Append("                            1.For any cancellation made more than 48 hours in advance, there is no charge.<br>");
            //sb.Append("                            2. For any cancellation made within 48 hours, there is a 50% charge.<br>");
            //sb.Append("                            3.For any cancellation made within 24 hours, or no show there is a 100% charge.<br><br>");
            //sb.Append("                            Airport Transfer<br>");
            //sb.Append("                            1.The company is not responsible for any damage arising from the use or not use any.<br>");
            //sb.Append("                            2. If the passengers do not appear or late show up more than 2 hours from appointed time<br>");
            //sb.Append("                            we reserve the right to assume the inability as NO SHOW the service will charge in full.<br>");
            //sb.Append("                            3. Due to the size of the vehicle<br>");
            //sb.Append("                            Big luggage size 28x20x12 โ€ including wheels and handle.<br>");
            //sb.Append("                            Small luggage size 22x14x9 โ€ including wheels and handle.<br>");
            //sb.Append("                            4. Maximum of passenger to be seated / Camry sedan 3 passengers / Van 6 passengers<br>");
            //sb.Append("                        </td>");
            //sb.Append("                    </tr>");
            //sb.Append("                </table>");
                  sb.Append("    <table>");
                  sb.Append("                    <tr>");
                  sb.Append("                      ");
                  sb.Append("                        <td>");
                  sb.Append("                            <b>Dear</b>"+ a.Title +" &nbsp;"+ a.FirstName +" &nbsp;"+ a.LastName +"");
                  sb.Append("                            <br />");
                  sb.Append("                        Thank you for your reservation with Synergi.Your transfer service has been booked as follow<br>");
                  sb.Append("                            Please check to ensure all details are correct.");
                  sb.Append("                            <br />");
                  sb.Append("                  ");
                  sb.Append("                        </td>");
                  sb.Append("                    </tr>");
                  sb.Append("              ");
                  sb.Append("                </table>");
                  sb.Append("                <br>");
                  sb.Append("                <table>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td>");
                  sb.Append("                            <b>Reservation Code :</b> IV "+ a.BookingID +"");
                  sb.Append("                            <br />");
                  sb.Append("                        </td>");
                  sb.Append("                    </tr>");
                  sb.Append("                </table>");
                  sb.Append("                <br />");
                  sb.Append("                <table border=\"1\" width=\"100%\">");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Passenger Name : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Title +"&nbsp;"+ a.FirstName +" &nbsp;"+ a.LastName +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Address : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Address +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>EMail : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Email +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Contact Call : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Mobile +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Type of Service  : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ @SType +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Passenger/Pax  : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Passenger +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Luggage : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Luggage +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Flight Detail : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.FlightNo +" &nbsp;"+ a.FlightTime +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Pick up date : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Date +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Pick up time : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.Time +"<br></td>");
                  sb.Append("                    </tr>");
                  if (a.ServiceType != 3)
                  {
                      sb.Append("                    <tr>");
                      sb.Append("                        <td width=\"20%\"><b>From : &nbsp;</b> </td>");
                      sb.Append("                        <td>" + a.FromDetail + "<br></td>");
                      sb.Append("                    </tr>");
                      sb.Append("                    <tr>");
                      sb.Append("                        <td width=\"20%\"><b>To : &nbsp;</b> </td>");
                      sb.Append("                        <td>" + a.ToDetail + "<br></td>");
                      sb.Append("                    </tr>");
                  }
                  else
                  {
                      sb.Append("                    <tr>");
                      sb.Append("                        <td width=\"20%\"><b>Route Detail : &nbsp;</b> </td>");
                      sb.Append("                        <td>" + a.RouteDetail + "<br></td>");
                      sb.Append("                    </tr>");
                  }
                 
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Meeting point : &nbsp;</b> </td>");
                  sb.Append("                        <td>The chauffeur will send SMS or call to re -confirmed 24 hours before your travel date. <br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                </table>");
                  sb.Append("                <br>");
                  sb.Append("                <table>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td>");
                  sb.Append("                            <b>Chauffeur</b> ");
                  sb.Append("                            <br />");
                  sb.Append("                        </td>");
                  sb.Append("                    </tr>");
                  sb.Append("                </table>");
                  sb.Append("                <br />");
                  sb.Append("                <table border=\"1\" width=\"100%\">");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Chauffeur Name : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.DTitle +" &nbsp;"+ a.DFirstName +" &nbsp;"+ a.DLastName +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Plate Number : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.VehicleRegis +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Type of vehicle : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.CarModel +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td width=\"20%\"><b>Contact Call : &nbsp;</b> </td>");
                  sb.Append("                        <td>"+ a.DMobile +"<br></td>");
                  sb.Append("                    </tr>");
                  sb.Append("                </table>");
                  sb.Append("                <br>");
                  if (a.CustomerType == 2)
                  {
                      sb.Append("                <table>");
                      sb.Append("                    <tr>");
                      sb.Append("                        <td>");
                      sb.Append("                            <b>Agent</b>");
                      sb.Append("                            <br />");
                      sb.Append("                        </td>");
                      sb.Append("                    </tr>");
                      sb.Append("                </table>");
                      sb.Append("                <br />");
                      sb.Append("                <table border=\"1\" width=\"100%\">");
                      sb.Append("                    <tr>");
                      sb.Append("                        <td width=\"20%\"><b>Agent Information : &nbsp;</b> </td>");
                      sb.Append("                        <td>" + a.AgentName + "<br></td>");
                      sb.Append("                    </tr>");
                      sb.Append("                    <tr>");
                      sb.Append("                        <td width=\"20%\"><b>EMail : &nbsp;</b></td>");
                      sb.Append("                        <td>" + a.AgentEmail + "<br> </td>");
                      sb.Append("                    </tr>");
                      sb.Append("                    <tr>");
                      sb.Append("                        <td width=\"20%\"><b>Contact Call : &nbsp;</b></td>");
                      sb.Append("                        <td>" + a.AgentMobile + "<br> </td>");
                      sb.Append("                    </tr>     ");
                      sb.Append("                </table>");
                      sb.Append("                <br>");
                  }
                
                  sb.Append("                <table>");
                  sb.Append("                    <tr>");
                  sb.Append("                        <td>");
                  sb.Append("                            <b>Term and Condition</b><br>");
                  sb.Append("                            <b>Cancellation/ No show</b><br>");
                  sb.Append("                            1.For any cancellation made more than 48 hours in advance, there is no charge.<br>");
                  sb.Append("                            2. For any cancellation made within 48 hours, there is a 50% charge.<br>");
                  sb.Append("                            3.For any cancellation made within 24 hours, or no show there is a 100% charge.<br><br>");
                  sb.Append("                            <b>Airport Transfer</b><br>");
                  sb.Append("                            1.The company is not responsible for any damage arising from the use or not use any.<br>");
                  sb.Append("                            2. If the passengers do not appear or late show up more than 2 hours from appointed time<br>");
                  sb.Append("                            we reserve the right to assume the inability as NO SHOW the service will charge in full.<br>");
                  sb.Append("                            3. Due to the size of the vehicle<br>");
                  sb.Append("                            Big luggage size 28x20x12 โ€ including wheels and handle.<br>");
                  sb.Append("                            Small luggage size 22x14x9 โ€ including wheels and handle.<br>");
                  sb.Append("                            4. Maximum of passenger to be seated / Camry sedan 3 passengers / Van 6 passengers<br>");
                  sb.Append("                        </td>");
                  sb.Append("                    </tr>");
                  sb.Append("                </table>");
            sb.Append("                <br>");
            sb.Append("  </font>");

              }

            //BookingInfo

            //EMail
         
                    StringReader sr = new StringReader(sb.ToString());
               
                    //C:\Miramar\Hotel\img\imgEmail
                    //string imagepath = "E:\\Test\\MirmarHotel\\Miramar.Hotel\\img\\imgEmail\\";
                        //string imagepath = "C:\\Miramar\\Hotel\\img\\imgEmail\\";
                      //  string imagepath = "C:\\Miramar\\Conxxe\\img\\imgEmail\\";
                
                 //   string pdfpath = "C:\\inetpub\\wwwroot\\LMS\\DriverProfile\\";
                    BaseFont EnCodefont = BaseFont.CreateFont(Server.MapPath("/font/ANGSA.TTF"), BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font Nfont = new Font(EnCodefont, 18, Font.NORMAL);
                    string pdfpath = Path.Combine(Server.MapPath("~/DriverProfile/"));
                    Document pdfDoc = new Document(PageSize.A4, 36f, 36f, 0f, 0f);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfDoc.Open();
                        // PdfWriter.GetInstance(pdfDoc, new FileStream(pdfpath +"test"+ rcb.BookingReference+".pdf", FileMode.Create));
                        // pdfDoc.Open();

                        pdfDoc.Add(new Paragraph("", Nfont));  
                        htmlparser.Parse(sr);
                      
                        pdfDoc.Close();
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();

                        SmtpClient client = new SmtpClient();
                        client.Port = 587;
                        client.Host = "mail.synergilimo.com";
                        client.EnableSsl = false;
                        client.Timeout = 10000;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential("booking@synergilimo.com", "Booking@2015");

                        MailMessage msg = new MailMessage();
                        msg.From = new MailAddress("booking@synergilimo.com");
                        msg.To.Add(bkd.Email);
                        msg.Subject = "CONFIRMATION Your booking has been successful.(Booking Reference : IV" + BID + ")";
                        msg.Body = sb.ToString();
                        msg.IsBodyHtml = true;
                     //   msg.IsBodyHtml = sb.ToString();
                   
                   //    msg.Body = "Thank you for your reservation with Synergi.Your transfer service has been booked as follow  Please check to ensure all details are correct.";
                        msg.Attachments.Add(new Attachment(new MemoryStream(bytes), "LMSBooking.pdf"));
                        msg.Attachments.Add(new Attachment(pdfpath + "DriverProfile" + bkd.DriverID + ".pdf"));
                    //    msg.Attachments.Add(new FileStream(pdfpath +"test"+ rcb.BookingReference+".pdf");
                        msg.Priority = MailPriority.High;
                        msg.BodyEncoding = UTF8Encoding.UTF8;
                        msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        client.Send(msg);

                     
                        //EMail

                    }
                  //  Session["BookingID"] = BID;
                  //  return RedirectToAction("BookingInfo", "LMS");
                    return RedirectToAction("BookingInfo", "LMS", new { BID = BID });
            //return View();
        }

        public ActionResult BookingInfo()
        {
            string BID = "16050049";
            //if (Session["BookingID"] != null)
            //{
            //    BID = Session["BookingID"].ToString();
            //}
            //else
            //{
            //    if (Request.QueryString["BID"] != null)
            //    {
                   BID = Request.QueryString["BID"];
            //    }
            //    else{
            //          BID = "";
            //    return RedirectToAction("Booking", "LMS");
            //    }
              
            //}
            List<BookingInfo> model = new List<BookingInfo>();
            var sBookingInfo = (from b in db.LMS_Booking
                                   join c in db.LMS_Car on b.CarID equals c.ID
                                   //join u in db.LMS_User on b.UserID equals u.UserID
                                   //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                   join d in db.LMS_Driver on b.DriverID equals d.ID
                                where b.BookingID == BID
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName =b.FirstName,
                                    LastName = b.LastName,
                                    Address  = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                  where u.UserID == bl.UserID
                                  select new
                                  {
                                      UName = u.FullName,
                                      UEmail = u.Email,
                                      UTelephone = u.Telephone
                                  }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);
               

                model.Add(a);

            }

            return View(model);
        }
        public ActionResult PaymentSelect()
        {
            return View();
        }
        public ActionResult Queue()
        {
            int UserType = 0;
            if (Session["LogedUserType"] != null)
            {
                UserType = Convert.ToInt32(Session["LogedUserType"]);
            }
            else
            {
                UserType = 0;
            }


            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                          select new
                          {
                              uAgentID = u.AgentID
                          }
                   ).FirstOrDefault();
            List<BookingList> model = new List<BookingList>();
            if (UserID.uAgentID == 0)
            {
                var sBookingList = (from b in db.LMS_Booking
                                    join v in db.LMS_Vehicle on b.CarID equals v.ID
                                    select new
                                    {
                                        BookingID = b.BookingID,
                                        BDate = b.Date,
                                        BService = b.ServiceType,
                                        BForm = b.FromDetail,
                                        BTo = b.ToDetail,
                                        CarID = b.CarID,
                                        CarModel = v.Name,
                                        BStatus = b.Status,
                                        AgentID = b.AgentID,
                                        UserID = b.UserID
                                    }
                 ).ToList();
                foreach (var bl in sBookingList)
                {
                    BookingList a = new BookingList();
                    a.BookingID = bl.BookingID.ToString();
                    a.Date = Convert.ToDateTime(bl.BDate);
                    a.FromDetail = bl.BForm.ToString();
                    a.ToDetail = bl.BTo.ToString();
                    a.CarModel = bl.CarModel.ToString();
                    a.Status = Convert.ToInt32(bl.BStatus);
                    a.AgentID = Convert.ToInt32(bl.AgentID);
                    a.UserID = Convert.ToInt32(bl.UserID);
                    model.Add(a);

                }
            }
            else
            {
                var sBookingList = (from b in db.LMS_Booking
                                    join v in db.LMS_Vehicle on b.CarID equals v.ID
                                    where b.AgentID == UserID.uAgentID
                                    select new
                                    {
                                        BookingID = b.BookingID,
                                        BDate = b.Date,
                                        BService = b.ServiceType,
                                        BForm = b.FromDetail,
                                        BTo = b.ToDetail,
                                        CarID = b.CarID,
                                        CarModel = v.Name,
                                        BStatus = b.Status,
                                        AgentID = b.AgentID,
                                        UserID = b.UserID
                                    }
                  ).ToList();
                foreach (var bl in sBookingList)
                {
                    BookingList a = new BookingList();
                    a.BookingID = bl.BookingID.ToString();
                    a.Date = Convert.ToDateTime(bl.BDate);
                    a.FromDetail = bl.BForm.ToString();
                    a.ToDetail = bl.BTo.ToString();
                    a.CarModel = bl.CarModel.ToString();
                    a.Status = Convert.ToInt32(bl.BStatus);
                    a.AgentID = Convert.ToInt32(bl.AgentID);
                    a.UserID = Convert.ToInt32(bl.UserID);
                    model.Add(a);

                }
            }
           
            return View(model);
        }

        public ActionResult DriverList()
        {
            int UserType = 0;
            if (Session["LogedUserType"] != null)
            {
                UserType = Convert.ToInt32(Session["LogedUserType"]);
            }
            else
            {
                UserType = 0;
            }


            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                          select new
                          {
                              uAgentID = u.AgentID
                          }
                   ).FirstOrDefault();
            List<Driver> model = new List<Driver>();
            if (UserID.uAgentID == 0)
            {
                var sDriver = (from d in db.LMS_Driver
                                  
                                    select new
                                    {
                                        ID = d.ID,
                                        Title = d.Title,
                                        Name = d.Name,
                                        LastName = d.LastName,
                                        Mobile = d.Mobile
                                     
                                    }
                 ).ToList();
                foreach (var dl in sDriver)
                {
                    Driver a = new Driver();
                    a.Id = Convert.ToInt32(dl.ID);
                    a.Title = dl.Title.ToString();
                    a.Name = dl.Name.ToString();
                    a.LastName = dl.LastName.ToString();
                    a.Mobile = dl.Mobile.ToString();
                    model.Add(a);

                }
            }
            else
            {
              
            }

            return View(model);
        }
        public ActionResult CarList()
        {
            int UserType = 0;
            if (Session["LogedUserType"] != null)
            {
                UserType = Convert.ToInt32(Session["LogedUserType"]);
            }
            else
            {
                UserType = 0;
            }


            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                          select new
                          {
                              uAgentID = u.AgentID
                          }
                   ).FirstOrDefault();
            List<CarList> model = new List<CarList>();
            if (UserID.uAgentID == 0)
            {
                var sCar = (from d in db.LMS_Car

                               select new
                               {
                                   ID = d.ID,
                                   VehicleRegis = d.VehicleRegis,
                                   VehicleID = d.VehicleID,
                                   Model = d.Model,
                                   PhotoPath = d.PhotoPath,
                                   SitNumber = d.SitNumber,
                                   Status = d.Status

                               }
                 ).ToList();
                foreach (var dl in sCar)
                {
                    CarList a = new CarList();
                    a.Id = Convert.ToInt32(dl.ID);
                    a.VehicleRegis = dl.VehicleRegis.ToString();
                    a.VehicleID = Convert.ToInt32(dl.VehicleID);
                    a.Model = dl.Model.ToString();
                    a.PhotoPath = dl.PhotoPath.ToString();
                    a.SitNumber = Convert.ToInt32(dl.SitNumber);
                    a.Status = Convert.ToInt32(dl.Status);
                    model.Add(a);

                }
            }
            else
            {

            }

            return View(model);
        }
        public ActionResult QueueList()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }

            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                          select new
                          {
                              uAgentID = u.AgentID
                          }
                   ).FirstOrDefault();
            List<QueueList> model = new List<QueueList>();
            if (UserID.uAgentID == 0)
            {
                var sQueue = (from c in db.LMS_Car
                           join dc in db.LMS_DriverOfCar on c.ID equals dc.CarID
                           join d in db.LMS_Driver on dc.DriverID equals d.ID
                        join w in db.LMS_WorkTime on dc.TID equals w.TID
                              orderby dc.TID,c.VehicleID,dc.LastBooking ascending,dc.LastJobDate ascending, dc.LastJobTime ascending, c.ID
                           select new
                           {
                               DID = dc.DID,
                               CarID = dc.CarID,
                               CModel = c.Model,
                               DriverID = dc.DriverID,
                               TID = dc.TID,
                               VehicleRegis = c.VehicleRegis,
                               DTitle = d.Title,
                               DFirstName = d.Name,
                               DLastName = d.LastName,
                               DMobile = d.Mobile,
                               TRemark = w.TRemark,
                               LastJobDate = dc.LastJobDate,
                               LastJobTime = dc.LastJobTime,
                               LastBooking = dc.LastBooking
                           }
                  ).ToList();

                foreach (var dl in sQueue)
                {
                    QueueList a = new QueueList();
                  
                    a.VehicleRegis = dl.VehicleRegis.ToString();
                    a.Model = dl.CModel.ToString();
                    a.DFirstName = dl.DFirstName.ToString();
                    a.DLastName = dl.DLastName.ToString();
                    a.DMobile = dl.DMobile.ToString();
                    a.DriverID = Convert.ToInt32(dl.DriverID);
                    a.DTitle = dl.DTitle.ToString();
                    a.LastJobDate = Convert.ToDateTime(dl.LastJobDate);
                    a.LastJobTime = dl.LastJobTime.ToString();
                    a.TRemark = dl.TRemark.ToString();
                    a.LastBooking = dl.LastBooking.ToString();
                    model.Add(a);

                }
            }
            else
            {

            }

            return View(model);
        }
        public ActionResult Car()
        {
            //int UserType = 0;
            //if (Session["LogedUserType"] != null)
            //{
            //    UserType = Convert.ToInt32(Session["LogedUserType"]);
            //}
            //else
            //{
            //    UserType = 0;
            //}


            //var UserID = (from u in db.LMS_User
            //              where u.UesrType == UserType
            //              select new
            //              {
            //                  uAgentID = u.AgentID
            //              }
            //       ).FirstOrDefault();
            //List<CarList> model = new List<CarList>();
            //if (UserID.uAgentID == 0)
            //{
            //    var sCar = (from d in db.LMS_Car

            //                select new
            //                {
            //                    ID = d.ID,
            //                    VehicleRegis = d.VehicleRegis,
            //                    VehicleID = d.VehicleID,
            //                    Model = d.Model,
            //                    PhotoPath = d.PhotoPath,
            //                    SitNumber = d.SitNumber,
            //                    Status = d.Status

            //                }
            //     ).ToList();
            //    foreach (var dl in sCar)
            //    {
            //        CarList a = new CarList();
            //        a.Id = Convert.ToInt32(dl.ID);
            //        a.VehicleRegis = dl.VehicleRegis.ToString();
            //        a.VehicleID = Convert.ToInt32(dl.VehicleID);
            //        a.Model = dl.Model.ToString();
            //        a.PhotoPath = dl.PhotoPath.ToString();
            //        a.SitNumber = Convert.ToInt32(dl.SitNumber);
            //        a.Status = Convert.ToInt32(dl.Status);
            //        model.Add(a);

            //    }
            //}
            //else
            //{

            //}

            //return View(model);
            return View();
        }
        public ActionResult NewCar()
        {
            return View();
        }
        public ActionResult NewDriver()
        {
            return View();
        }
        public ActionResult ReportBooking()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }


            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                          select new
                          {
                              uAgentID = u.AgentID
                          }
                   ).FirstOrDefault();

            string aDate = Request.QueryString["sDate"];
            string bDate = Request.QueryString["eDate"];

            if (aDate == null || aDate == "")
            {
                aDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (bDate == null || bDate == "")
            {
                bDate = DateTime.Now.ToString("yyyy-MM-dd");
            }


            DateTime sDate = Convert.ToDateTime(aDate);
            DateTime eDate = Convert.ToDateTime(bDate);

            List<BookingList> model = new List<BookingList>();
            if (UserID.uAgentID == 0)
            {
                
                var sBookingList = (from b in db.LMS_Booking
                                    join c in db.LMS_Car on b.CarID equals c.ID
                                    where b.BookingDate >= sDate && b.BookingDate <= eDate 
                                    orderby b.BookingID descending
                                    select new
                                    {
                                        BookingID = b.BookingID,
                                        BookingDate = b.BookingDate,
                                        BDate = b.Date,
                                        BTime = b.Time,
                                        BService = b.ServiceType,
                                        BForm = b.FromDetail,
                                        BTo = b.ToDetail,
                                        CarID = b.CarID,
                                        CarModel = c.Model,
                                        BStatus = b.Status,
                                        AgentID = b.AgentID,
                                        RouteDetail = b.RouteDetail,
                                        UserID = b.UserID
                                    }
                 ).ToList();
                foreach (var bl in sBookingList)
                {
                    BookingList a = new BookingList();
                    a.BookingID = bl.BookingID.ToString();
                    a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                    a.Date = Convert.ToDateTime(bl.BDate);
                    a.Time = bl.BTime.ToString();
                    a.ServiceType = Convert.ToInt32(bl.BService);
                    a.FromDetail = bl.BForm.ToString();
                    a.ToDetail = bl.BTo.ToString();
                    a.CarModel = bl.CarModel.ToString();
                    a.Status = Convert.ToInt32(bl.BStatus);
                    a.AgentID = Convert.ToInt32(bl.AgentID);
                    a.UserID = Convert.ToInt32(bl.UserID);
                    a.RouteDetail = bl.RouteDetail;
                    a.sDate = sDate.ToString("dd/MM/yyyy");
                    a.eDate = eDate.ToString("dd/MM/yyyy");
                    model.Add(a);

                }
            }
            else
            {
                var sBookingList = (from b in db.LMS_Booking
                                    join v in db.LMS_Vehicle on b.CarID equals v.ID
                                    where b.AgentID == UserID.uAgentID
                                    select new
                                    {
                                        BookingID = b.BookingID,
                                        BDate = b.Date,
                                        BService = b.ServiceType,
                                        BForm = b.FromDetail,
                                        BTo = b.ToDetail,
                                        CarID = b.CarID,
                                        CarModel = v.Name,
                                        BStatus = b.Status,
                                        AgentID = b.AgentID,
                                        UserID = b.UserID
                                    }
                  ).ToList();
                foreach (var bl in sBookingList)
                {
                    BookingList a = new BookingList();
                    a.BookingID = bl.BookingID.ToString();
                    a.Date = Convert.ToDateTime(bl.BDate);
                    a.FromDetail = bl.BForm.ToString();
                    a.ToDetail = bl.BTo.ToString();
                    a.CarModel = bl.CarModel.ToString();
                    a.Status = Convert.ToInt32(bl.BStatus);
                    a.AgentID = Convert.ToInt32(bl.AgentID);
                    a.UserID = Convert.ToInt32(bl.UserID);
                    model.Add(a);

                }
            }

            return View(model);
        }

        public ActionResult JobOrder()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }


            var UserID = (from u in db.LMS_User
                          where u.UesrType == UserType
                          select new
                          {
                              uAgentID = u.AgentID
                          }
                   ).FirstOrDefault();

            string aDate = Request.QueryString["sDate"];
            string bDate = Request.QueryString["eDate"];

            if (aDate == null || aDate == "")
            {
                aDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (bDate == null || bDate == "")
            {
                bDate = DateTime.Now.ToString("yyyy-MM-dd");
            }


            DateTime sDate = Convert.ToDateTime(aDate);
            DateTime eDate = Convert.ToDateTime(bDate);

            List<BookingList> model = new List<BookingList>();
            if (UserID.uAgentID == 0)
            {

                var sBookingList = (from b in db.LMS_Booking
                                    join c in db.LMS_Car on b.CarID equals c.ID
                                    join d in db.LMS_Driver on b.DriverID equals d.ID 
                                    where b.Date >= sDate && b.Date <= eDate
                                    orderby b.Date,b.DriverID
                                    select new
                                    {
                                        BookingID = b.BookingID,
                                        BookingDate = b.BookingDate,
                                        BDate = b.Date,
                                        BTime = b.Time,
                                        BService = b.ServiceType,
                                        BForm = b.FromDetail,
                                        BTo = b.ToDetail,
                                        CarID = b.CarID,
                                        CarModel = c.Model,
                                        BStatus = b.Status,
                                        AgentID = b.AgentID,
                                        RouteDetail = b.RouteDetail,
                                        UserID = b.UserID,
                                        DriverID = b.DriverID,
                                        DTitle = d.Title,
                                        DName = d.Name,
                                        DLastName = d.LastName
                                    }
                 ).ToList();
                foreach (var bl in sBookingList)
                {
                    BookingList a = new BookingList();
                    a.BookingID = bl.BookingID.ToString();
                    a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                    a.Date = Convert.ToDateTime(bl.BDate);
                    a.Time = bl.BTime.ToString();
                    a.ServiceType = Convert.ToInt32(bl.BService);
                    a.FromDetail = bl.BForm.ToString();
                    a.ToDetail = bl.BTo.ToString();
                    a.CarModel = bl.CarModel.ToString();
                    a.Status = Convert.ToInt32(bl.BStatus);
                    a.AgentID = Convert.ToInt32(bl.AgentID);
                    a.UserID = Convert.ToInt32(bl.UserID);
                    a.sDate = sDate.ToString("dd/MM/yyyy");
                    a.eDate = eDate.ToString("dd/MM/yyyy");
                    a.RouteDetail = bl.RouteDetail;
                    a.DriverID = Convert.ToInt32(bl.DriverID);
                    a.DTitle = bl.DTitle;
                    a.DName = bl.DName;
                    a.DLastName = bl.DLastName;
                    model.Add(a);

                }
            }
            else
            {
                var sBookingList = (from b in db.LMS_Booking
                                    join v in db.LMS_Vehicle on b.CarID equals v.ID
                                    where b.AgentID == UserID.uAgentID
                                    select new
                                    {
                                        BookingID = b.BookingID,
                                        BDate = b.Date,
                                        BService = b.ServiceType,
                                        BForm = b.FromDetail,
                                        BTo = b.ToDetail,
                                        CarID = b.CarID,
                                        CarModel = v.Name,
                                        BStatus = b.Status,
                                        AgentID = b.AgentID,
                                        UserID = b.UserID
                                    }
                  ).ToList();
                foreach (var bl in sBookingList)
                {
                    BookingList a = new BookingList();
                    a.BookingID = bl.BookingID.ToString();
                    a.Date = Convert.ToDateTime(bl.BDate);
                    a.FromDetail = bl.BForm.ToString();
                    a.ToDetail = bl.BTo.ToString();
                    a.CarModel = bl.CarModel.ToString();
                    a.Status = Convert.ToInt32(bl.BStatus);
                    a.AgentID = Convert.ToInt32(bl.AgentID);
                    a.UserID = Convert.ToInt32(bl.UserID);
                    model.Add(a);

                }
            }

            return View(model);
        }
        public ActionResult QueueDetail()
        {
            return View();
        }

        public ActionResult TestMail()
        {
              StringBuilder sbE = new StringBuilder();

                    sbE.Append("<table>");
                    sbE.Append(" <tr>");
                    sbE.Append("  <td><font size=\"2\">");
                    sbE.Append("  <b>SUPPLIER</b>");
                    sbE.Append("  </font></td>");
                    sbE.Append(" </tr>");
                    sbE.Append("</table> ");

                    StringReader srE = new StringReader(sbE.ToString());
               
                    //C:\Miramar\Hotel\img\imgEmail
                    //string imagepath = "E:\\Test\\MirmarHotel\\Miramar.Hotel\\img\\imgEmail\\";
                        //string imagepath = "C:\\Miramar\\Hotel\\img\\imgEmail\\";
                      //  string imagepath = "C:\\Miramar\\Conxxe\\img\\imgEmail\\";
                   // string pdfpath = "E:\\Test\\";

                    Document pdfDoc = new Document(PageSize.A4, 36f, 36f, 0f, 0f);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfDoc.Open();
                        // PdfWriter.GetInstance(pdfDoc, new FileStream(pdfpath +"test"+ rcb.BookingReference+".pdf", FileMode.Create));
                        // pdfDoc.Open();
                        pdfDoc.Add(new Paragraph(""));
                        //Image imgHeadYnotFly = Image.GetInstance(imagepath + "HeadYnotFly.png");
                        //imgHeadYnotFly.ScalePercent(23f);
                        //Image imgGeneral = Image.GetInstance(imagepath + "General.png");
                        //imgGeneral.ScalePercent(23f);
                        //Image imgArrival = Image.GetInstance(imagepath + "Arrival.png");
                        //imgArrival.ScalePercent(23f);
                        //Image imgDeparture = Image.GetInstance(imagepath + "Departure.png");
                        //imgDeparture.ScalePercent(23f);
                        //Image imgEmergency = Image.GetInstance(imagepath + "Emergency.png");
                        //imgEmergency.ScalePercent(23f);
                        //Image imgPaymentMethod = Image.GetInstance(imagepath + "PaymentMethod.png");
                        //imgPaymentMethod.ScalePercent(23f);
                        //Image imgPayment = Image.GetInstance(imagepath + "Payment.png");
                        //imgPayment.ScalePercent(23f);
                        //Image imgCredit = Image.GetInstance(imagepath + "Credit.png");
                        //imgCredit.ScalePercent(23f);
                        //Image imgTicketingMethod = Image.GetInstance(imagepath + "TicketingMethod.png");
                        //imgTicketingMethod.ScalePercent(23f);
                        //Image imgFooterBooking = Image.GetInstance(imagepath + "FooterBooking.png");
                        //imgFooterBooking.ScalePercent(23f);
                        //pdfDoc.Add(imgHeadYnotFly);
                        //htmlparser.Parse(srH);
                        //pdfDoc.Add(imgGeneral);
                        //htmlparser.Parse(srG);

                        //if (rcb.BookingTypeId == "1")
                        //{
                        //    pdfDoc.Add(imgArrival);
                        //    htmlparser.Parse(srA);
                        //    pdfDoc.Add(imgDeparture);
                        //    htmlparser.Parse(srD);
                        //}
                        //else if (rcb.BookingTypeId == "2")
                        //{
                        //    pdfDoc.Add(imgArrival);
                        //    htmlparser.Parse(srA);
                        //                        }
                        //else if (rcb.BookingTypeId == "3")
                        //{
                        //    pdfDoc.Add(imgDeparture);
                        //    htmlparser.Parse(srD);
                        //}

                        // pdfDoc.Add(imgEmergency);
                        htmlparser.Parse(srE);
                        //pdfDoc.Add(imgPaymentMethod);
                        //pdfDoc.Add(imgCredit);
                        //pdfDoc.Add(imgPayment);
                        //pdfDoc.Add(imgTicketingMethod);
                        //htmlparser.Parse(srT);
                        //pdfDoc.Add(imgFooterBooking);


                        pdfDoc.Close();
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();

                    

                     

                        //SmtpClient client = new SmtpClient();
                        //client.Host = "smtp.synergilimo.com";
                        //client.Port = 587;
                        //client.UseDefaultCredentials = false;
                        //client.Credentials = new System.Net.NetworkCredential("booking@synergilimo.com", "Booking@2015");
                        //client.EnableSsl = true;
                        //client.Send(msg);

                        SmtpClient client = new SmtpClient();
                        client.Port = 587;
                        client.Host = "mail.synergilimo.com";
                        client.EnableSsl = false;
                        client.Timeout = 10000;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential("booking@synergilimo.com", "Booking@2015");
                      
                        MailMessage msg = new MailMessage();
                        msg.From = new MailAddress("booking@synergilimo.com");
                        msg.To.Add("tuuranger@hotmail.com");
                        msg.Subject = "LMS Booking ";
                        msg.Body = "CONFIRMATION Your booking has been successful.(Booking Reference :)";
                        msg.Attachments.Add(new Attachment(new MemoryStream(bytes), "LMSBooking.pdf"));
                        msg.Priority = MailPriority.High;
                        msg.BodyEncoding = UTF8Encoding.UTF8;
                        msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        client.Send(msg);
                    }
                   
            return View();
        }
        public ActionResult JobOrderInfo()
        {
            string BID = "16050049";
            if (Session["BookingID"] != null)
            {
                BID = Session["BookingID"].ToString();
            }
            else
            {
                if (Request.QueryString["BID"] != null)
                {
                    BID = Request.QueryString["BID"];
                }
                else
                {
                    BID = "";
                    return RedirectToAction("Booking", "LMS");
                }

            }
            List<BookingInfo> model = new List<BookingInfo>();
            var sBookingInfo = (from b in db.LMS_Booking
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where b.BookingID == BID
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                model.Add(a);

            }

            return View(model);
        }
        public ActionResult JobOrderPrint()
        {
            string BID = "16050049";
            if (Session["BookingID"] != null)
            {
                BID = Session["BookingID"].ToString();
            }
            else
            {
                if (Request.QueryString["BID"] != null)
                {
                    BID = Request.QueryString["BID"];
                }
                else
                {
                    BID = "";
                    return RedirectToAction("Booking", "LMS");
                }

            }
            List<BookingInfo> model = new List<BookingInfo>();
            var sBookingInfo = (from b in db.LMS_Booking
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where b.BookingID == BID
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                model.Add(a);

            }

            return View(model);
        }

        public ActionResult InvoiceList()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }



            List<Invoice> model = new List<Invoice>();
           
                var sInvoice = (from i in db.LMS_Invoice
                                join s in db.LMS_SubAgent on i.SubID equals s.ID
                                    where i.InvoiceDate.Value.Month == DateTime.Now.Month && i.InvoiceDate.Value.Year == DateTime.Now.Year
                                    //    where Convert.ToDateTime(b.BookingDate).Month == DateTime.Now.Month  
                                    orderby i.InvoiceNo descending
                                    select new
                                    {
                                        InvoiceID = i.ID,
                                        InvoiceNo = i.InvoiceNo,
                                        InvoiceDate = i.InvoiceDate,
                                        DueDate = i.DueDate,
                                        GrandTotal = i.GrandTotal,
                                        CreditTerm = i.CreditTerm,
                                        Status = i.Status,
                                        SubID = i.SubID,
                                        SubName = s.Name
                                      
                                    }
                 ).ToList();
                foreach (var bl in sInvoice)
                {
                    Invoice a = new Invoice();
                    a.CreditTerm = Convert.ToInt32(bl.CreditTerm);
                    a.DueDate = Convert.ToDateTime(bl.DueDate);
                    a.InvoiceDate = Convert.ToDateTime(bl.InvoiceDate);
                    a.InvoiceID = Convert.ToInt32(bl.InvoiceID);
                    a.InvoiceNo = bl.InvoiceNo;
                    a.IStatus = Convert.ToInt32(bl.Status);
                    a.SubName = bl.SubName;

                    model.Add(a);

                }
           
       
            return View(model);
        }
        public ActionResult InvoiceSub()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }



            List<SubAgent> model = new List<SubAgent>();

            var sSubAgent = (from s in db.LMS_SubAgent
                            select new
                            {
                                SubID = s.ID,
                                SubName = s.Name,
                                SubContact = s.Contact,
                                SubTel = s.Telephone

                            }
             ).ToList();
            foreach (var bl in sSubAgent)
            {
                SubAgent a = new SubAgent();
                a.AId = bl.SubID;
                a.AName = bl.SubName;
                a.Contact = bl.SubContact;
                a.Telephone = bl.SubTel;
                model.Add(a);

            }

            return View(model);
        }
        
        public ActionResult InvoiceNew()
        {
            string sUserID = "0";
            if (Session["LogedUserID"] != null)
            {
                sUserID = Session["LogedUserID"].ToString();
            }
            else
            {
                sUserID = "0";
            }

            int UserID = 0;
            if (sUserID == "")
            {
                UserID = 0;
            }
            else
            {
                UserID = Convert.ToInt32(sUserID);
            }

            int SubID = 0;
            if (Request.QueryString["SubID"] != null)
            {
                SubID = Convert.ToInt32(Request.QueryString["SubID"]);
            }

            var sSubAgent = (from s in db.LMS_SubAgent
                             where s.ID == SubID
                             select new
                             {
                                 SubID = s.ID,
                                 SubName = s.Name,
                                 SubContact = s.Contact,
                                  SubTel = s.Telephone,
                                 SubAddress = s.Address,
                                 SubTax = s.Tax,
                                 Remark = s.Remark,
                                 CreditTerm = s.CreditTerm

                             }
                    ).ToList();


            List<InvoiceList> model = new List<InvoiceList>();
            List<BookingInfo> BModel = new List<BookingInfo>();
            List<SubAgent> SModel = new List<SubAgent>();

            foreach (var bs in sSubAgent)
            {
                SubAgent s = new SubAgent();
                s.AName = bs.SubName;
                s.AId = Convert.ToInt32(bs.SubID);
                s.Contact = bs.SubContact;
                s.Telephone = bs.SubTel;
                s.Tax = bs.SubTax;
                s.Address = bs.SubAddress;
                s.CreditTerm = Convert.ToInt32(bs.CreditTerm);
                s.Remark = bs.Remark;
                SModel.Add(s);
            }

            var sBookingInfo = (from b in db.LMS_Booking
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where b.AgentID == SubID && b.PaymentStatus == 1
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus,
                                    Currcney = b.Currency
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);
                a.Currency = bl.Currcney;

                BModel.Add(a);

            }

            InvoiceList IL = new InvoiceList();
            IL.LBooking = BModel.ToList();
            IL.LSubAgent = SModel.ToList();
            model.Add(IL);

      
            return View(model);
        }
        public ActionResult InvoiceCommit(FormCollection form)
        {
           

            string bYM = DateTime.Today.Year.ToString().Substring(2, 2) + DateTime.Today.Month.ToString("00");


            var InvoiceID = (from b in db.LMS_Invoice
                             where b.InvoiceNo.Substring(0, 4) == bYM
                             orderby b.InvoiceNo descending
                             select b.InvoiceNo
                  ).FirstOrDefault();

            string InvoiceNo = "";
            string RBID = "";
            if (InvoiceID == null)
            {
                InvoiceNo = bYM + "0001";
            }
            else
            {
                RBID = InvoiceID.Substring(4, 4);
                InvoiceNo = bYM + (Convert.ToInt32(RBID) + 1).ToString("0000");
            }


            int iRow = 0;
            string sRow = "";
            string dRow = "";
            decimal GrandTotal = 0;

            if (form["Row"] != "")
            {
                iRow = Convert.ToInt32(form["Row"]) - 1;

            }

            for (var i = 1; i <= iRow; i++)
             {
                 sRow = "Booking" + i;
                 dRow = Request.Form[sRow];

                if (dRow != null)
                {
                    var BPrice = (from b in db.LMS_Booking
                                  where b.BookingID == dRow
                                  select b.TotalPrice
             ).FirstOrDefault();

                    GrandTotal += Convert.ToDecimal(BPrice);

                    LMS_InvoiceDetail AddInvoiceD = new LMS_InvoiceDetail();
                    AddInvoiceD.BookingID = dRow;
                    AddInvoiceD.InvoiceNo = InvoiceNo;
                    db.LMS_InvoiceDetail.Add(AddInvoiceD);
                    db.SaveChanges();

                    LMS_Booking UpdateB = new LMS_Booking();

                    var BookingU = (from b in db.LMS_Booking
                                    where b.BookingID == dRow
                                    select b).Single();

                    BookingU.PaymentStatus = 2;

                    db.SaveChanges();
                }
               
             }

            LMS_Invoice AddInvoice = new LMS_Invoice();

            AddInvoice.InvoiceNo = InvoiceNo;
            AddInvoice.InvoiceDate = Convert.ToDateTime(form["InvoiceDate"]);
            AddInvoice.DueDate = Convert.ToDateTime(form["DueDate"]);
            AddInvoice.SubID = Convert.ToInt32(form["SubID"]);
            AddInvoice.GrandTotal = GrandTotal;
            AddInvoice.CreditTerm = Convert.ToInt32(form["CreditTerm"]);
            AddInvoice.Status = 1;

            db.LMS_Invoice.Add(AddInvoice);
            db.SaveChanges();


         //   Session["InvoiceNo"] = InvoiceNo;
        //    return RedirectToAction("InvoiceInfo", "LMS");
            return RedirectToAction("InvoiceInfo", "LMS", new { InvoiceNo = InvoiceNo });
            //return View();
        }
        public ActionResult InvoiceInfo()
        {
            string InvoiceNo = "16070001";
            //if (Session["InvoiceNo"] != null)
            //{
            //    InvoiceNo = Session["InvoiceNo"].ToString();
            //}
            //else
            //{
            //    if (Request.QueryString["InvoiceNo"] != null)
            //    {
                   InvoiceNo = Request.QueryString["InvoiceNo"];
            //    }
            //    else
            //    {
            //        InvoiceNo = "";
            //        return RedirectToAction("InvoiceList", "LMS");
            //    }

            //}

            List<InvoiceList> model = new List<InvoiceList>();
            List<BookingInfo> BModel = new List<BookingInfo>();
            List<Invoice> IModel = new List<Invoice>();

            var sInvoiceInfo = (from i in db.LMS_Invoice
                                join s in db.LMS_SubAgent on i.SubID equals s.ID
                                where i.InvoiceNo == InvoiceNo
                                select new
                                {
                                    InvoiceNo = i.InvoiceNo,
                                    InvoiceDate = i.InvoiceDate,
                                    DueDate = i.DueDate,
                                    SubId = i.SubID,
                                    CreditTerm = i.CreditTerm,
                                    Status = i.Status,
                                    GrandTotal = i.GrandTotal,
                                    SubName = s.Name,
                                    SubAddress = s.Address,
                                    SubTel = s.Telephone,
                                    SubTax = s.Tax
                                }
                                    ).ToList();


            var sBookingInfo = (from id in db.LMS_InvoiceDetail
                                join b in db.LMS_Booking on id.BookingID equals b.BookingID
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where id.InvoiceNo == InvoiceNo
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                BModel.Add(a);

            }

            foreach (var il in sInvoiceInfo)
            {
                Invoice a = new Invoice();
                a.CreditTerm = Convert.ToInt32(il.CreditTerm);
                a.DueDate = Convert.ToDateTime(il.DueDate);
                a.InvoiceDate = Convert.ToDateTime(il.InvoiceDate);
                a.InvoiceNo = il.InvoiceNo;
                    a.IStatus = Convert.ToInt32(il.Status);
                    a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                    a.SubName = il.SubName;
                    a.SubAddress = il.SubAddress;
                    a.SubTel = il.SubTel;
                    a.SubTax = il.SubTax;
                    IModel.Add(a);
               
            }

            InvoiceList IL = new InvoiceList();
            IL.LBooking = BModel.ToList();
            IL.LInvoice = IModel.ToList();
            model.Add(IL);

            return View(model);
        }
        public ActionResult InvoicePrint()
        {
            string InvoiceNo = "16070001";
            if (Session["InvoiceNo"] != null)
            {
                InvoiceNo = Session["InvoiceNo"].ToString();
            }
            else
            {
                if (Request.QueryString["InvoiceNo"] != null)
                {
                    InvoiceNo = Request.QueryString["InvoiceNo"];
                }
                else
                {
                    InvoiceNo = "";
                    return RedirectToAction("InvoiceList", "LMS");
                }

            }

            List<InvoiceList> model = new List<InvoiceList>();
            List<BookingInfo> BModel = new List<BookingInfo>();
            List<Invoice> IModel = new List<Invoice>();

            var sInvoiceInfo = (from i in db.LMS_Invoice
                                join s in db.LMS_SubAgent on i.SubID equals s.ID
                                where i.InvoiceNo == InvoiceNo
                                select new
                                {
                                    InvoiceNo = i.InvoiceNo,
                                    InvoiceDate = i.InvoiceDate,
                                    DueDate = i.DueDate,
                                    SubId = i.SubID,
                                    CreditTerm = i.CreditTerm,
                                    Status = i.Status,
                                    GrandTotal = i.GrandTotal,
                                    SubName = s.Name,
                                    SubAddress = s.Address,
                                    SubTel = s.Telephone,
                                    SubTax = s.Tax
                                }
                                    ).ToList();


            var sBookingInfo = (from id in db.LMS_InvoiceDetail
                                join b in db.LMS_Booking on id.BookingID equals b.BookingID
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where id.InvoiceNo == InvoiceNo
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                BModel.Add(a);

            }

            foreach (var il in sInvoiceInfo)
            {
                Invoice a = new Invoice();
                a.CreditTerm = Convert.ToInt32(il.CreditTerm);
                a.DueDate = Convert.ToDateTime(il.DueDate);
                a.InvoiceDate = Convert.ToDateTime(il.InvoiceDate);
                a.InvoiceNo = il.InvoiceNo;
                a.IStatus = Convert.ToInt32(il.Status);
                a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                a.SubName = il.SubName;
                a.SubAddress = il.SubAddress;
                a.SubTel = il.SubTel;
                a.SubTax = il.SubTax;
                IModel.Add(a);

            }

            InvoiceList IL = new InvoiceList();
            IL.LBooking = BModel.ToList();
            IL.LInvoice = IModel.ToList();
            model.Add(IL);

            return View(model);
        }

        public ActionResult ReceiptList()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }



            List<Receipt> model = new List<Receipt>();

            var sReceipt = (from r in db.LMS_Receipt
                            join i in db.LMS_Invoice on r.InvoiceNo equals i.InvoiceNo
                            join s in db.LMS_SubAgent on i.SubID equals s.ID
                            where i.InvoiceDate.Value.Month == DateTime.Now.Month && i.InvoiceDate.Value.Year == DateTime.Now.Year
                            //    where Convert.ToDateTime(b.BookingDate).Month == DateTime.Now.Month  
                            orderby i.InvoiceNo descending
                            select new
                            {
                                ReceiptNo = r.ReceiptNo,
                                ReceiptDate = r.ReceiptDate,
                                PaymentDate = r.PaymentDate,
                                PaymentType = r.PaymentType,
                                GrandTotal = r.GrandTotal,
                                InvoiceID = i.ID,
                                InvoiceNo = i.InvoiceNo,
                                InvoiceDate = i.InvoiceDate,
                                DueDate = i.DueDate,
                                CreditTerm = i.CreditTerm,
                                Status = i.Status,
                                SubID = i.SubID,
                                SubName = s.Name

                            }
             ).ToList();
            foreach (var bl in sReceipt)
            {
                Receipt a = new Receipt();
                a.GrandTotal = Convert.ToDecimal(bl.GrandTotal);
                a.InvoiceNo = bl.InvoiceNo;
                a.PaymentDate = Convert.ToDateTime(bl.PaymentDate);
                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.ReceiptDate = Convert.ToDateTime(bl.PaymentDate);
                a.ReceiptNo = bl.ReceiptNo;
                a.Status = Convert.ToInt32(bl.Status);
                a.SubName = bl.SubName;
              

                model.Add(a);

            }


            return View(model);
        }
        public ActionResult ReceiptInvoice()
        {
            string sUserType = "0";
            if (Session["LogedUserType"] != null)
            {
                sUserType = Session["LogedUserType"].ToString();
            }
            else
            {
                sUserType = "0";
                return RedirectToAction("Booking", "LMS");
            }

            int UserType = 0;
            if (sUserType == "")
            {
                UserType = 0;
            }
            else
            {
                UserType = Convert.ToInt32(sUserType);
            }



            List<Invoice> model = new List<Invoice>();

            var sInvoice = (from i in db.LMS_Invoice
                            join s in db.LMS_SubAgent on i.SubID equals s.ID
                            where i.Status == 1
                           // where i.InvoiceDate.Value.Month == DateTime.Now.Month && i.InvoiceDate.Value.Year == DateTime.Now.Year
                            //    where Convert.ToDateTime(b.BookingDate).Month == DateTime.Now.Month  
                            orderby i.InvoiceNo descending
                            select new
                            {
                                InvoiceID = i.ID,
                                InvoiceNo = i.InvoiceNo,
                                InvoiceDate = i.InvoiceDate,
                                DueDate = i.DueDate,
                                GrandTotal = i.GrandTotal,
                                CreditTerm = i.CreditTerm,
                                Status = i.Status,
                                SubID = i.SubID,
                                SubName = s.Name

                            }
             ).ToList();
            foreach (var bl in sInvoice)
            {
                Invoice a = new Invoice();
                a.CreditTerm = Convert.ToInt32(bl.CreditTerm);
                a.DueDate = Convert.ToDateTime(bl.DueDate);
                a.InvoiceDate = Convert.ToDateTime(bl.InvoiceDate);
                a.InvoiceID = Convert.ToInt32(bl.InvoiceID);
                a.InvoiceNo = bl.InvoiceNo;
                a.IStatus = Convert.ToInt32(bl.Status);
                a.SubName = bl.SubName;

                model.Add(a);

            }


            return View(model);
        }
        public ActionResult ReceiptNew()
        {
            string InvoiceNo = "16070001";
            if (Session["InvoiceNo"] != null)
            {
                InvoiceNo = Session["InvoiceNo"].ToString();
            }
            else
            {
                if (Request.QueryString["InvoiceNo"] != null)
                {
                    InvoiceNo = Request.QueryString["InvoiceNo"];
                }
                else
                {
                    InvoiceNo = "";
                    return RedirectToAction("InvoiceList", "LMS");
                }

            }

            List<ReceiptList> model = new List<ReceiptList>();
            List<BookingInfo> BModel = new List<BookingInfo>();
            List<Invoice> IModel = new List<Invoice>();

            var sInvoiceInfo = (from i in db.LMS_Invoice
                                join s in db.LMS_SubAgent on i.SubID equals s.ID
                                where i.InvoiceNo == InvoiceNo
                                select new
                                {
                                    InvoiceNo = i.InvoiceNo,
                                    InvoiceDate = i.InvoiceDate,
                                    DueDate = i.DueDate,
                                    SubId = i.SubID,
                                    CreditTerm = i.CreditTerm,
                                    Status = i.Status,
                                    GrandTotal = i.GrandTotal,
                                    SubName = s.Name,
                                    SubAddress = s.Address,
                                    SubTel = s.Telephone,
                                    SubTax = s.Tax
                                }
                                    ).ToList();


            var sBookingInfo = (from id in db.LMS_InvoiceDetail
                                join b in db.LMS_Booking on id.BookingID equals b.BookingID
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where id.InvoiceNo == InvoiceNo
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                BModel.Add(a);

            }

            foreach (var il in sInvoiceInfo)
            {
                Invoice a = new Invoice();
                a.CreditTerm = Convert.ToInt32(il.CreditTerm);
                a.DueDate = Convert.ToDateTime(il.DueDate);
                a.InvoiceDate = Convert.ToDateTime(il.InvoiceDate);
                a.InvoiceNo = il.InvoiceNo;
                a.IStatus = Convert.ToInt32(il.Status);
                a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                a.SubName = il.SubName;
                a.SubAddress = il.SubAddress;
                a.SubTel = il.SubTel;
                a.SubTax = il.SubTax;
                IModel.Add(a);

            }

            ReceiptList RL = new ReceiptList();
            RL.LBooking = BModel.ToList();
            RL.LInvoice = IModel.ToList();
            model.Add(RL);

            return View(model);
        }

        public ActionResult ReceiptCommit(FormCollection form)
        {


            string bYM = DateTime.Today.Year.ToString().Substring(2, 2) + DateTime.Today.Month.ToString("00");


            var ReceiptID = (from b in db.LMS_Receipt
                             where b.ReceiptNo.Substring(0, 4) == bYM
                             orderby b.ReceiptNo descending
                             select b.ReceiptNo
                  ).FirstOrDefault();

            string ReceiptNo = "";
            string RBID = "";
            if (ReceiptID == null)
            {
                ReceiptNo = bYM + "0001";
            }
            else
            {
                RBID = ReceiptID.Substring(4, 4);
                ReceiptNo = bYM + (Convert.ToInt32(RBID) + 1).ToString("0000");
            }


          
            decimal GrandTotal = 0;
            string InvoiceNo = form["InvoiceNo"];

            if (form["GrandTotal"] != "")
            {
                GrandTotal = Convert.ToDecimal(form["GrandTotal"]);
            }



            LMS_Receipt Addreceipt = new LMS_Receipt();
            Addreceipt.ReceiptNo = ReceiptNo;
            Addreceipt.ReceiptDate = Convert.ToDateTime(form["ReceiptDate"]);
            Addreceipt.InvoiceNo = InvoiceNo;
            Addreceipt.PaymentDate = Convert.ToDateTime(form["PaymentDate"]);
            Addreceipt.GrandTotal = GrandTotal;
            Addreceipt.PaymentType = Convert.ToInt32(form["PaymentType"]);
            Addreceipt.Status = 1;

            db.LMS_Receipt.Add(Addreceipt);
            db.SaveChanges();

            LMS_Invoice UpdateI = new LMS_Invoice();

            var InvoiceU = (from b in db.LMS_Invoice
                            where b.InvoiceNo == InvoiceNo
                            select b).Single();

            InvoiceU.Status = 2;

            db.SaveChanges();

          
            var sBookingInfo = (from b in db.LMS_Booking
                            join i in db.LMS_InvoiceDetail on b.BookingID equals i.BookingID
                                where i.InvoiceNo == InvoiceNo
                            select new
                                {
                                    BookingID = b.BookingID
                                }
                                ).ToList();

            foreach (var bl in sBookingInfo)
            {
                LMS_Booking UpdateB = new LMS_Booking();

                var BookingU = (from b in db.LMS_Booking
                                where b.BookingID == bl.BookingID
                                select b).Single();

                BookingU.PaymentStatus = 3;
                BookingU.PaymentType = Convert.ToInt32(form["PaymentType"]);
                db.SaveChanges();
            }

          //  Session["ReceiptNo"] = ReceiptNo;
          //  return RedirectToAction("ReceiptInfo", "LMS");
            return RedirectToAction("ReceiptInfo", "LMS", new { ReceiptNo = ReceiptNo });
            //return View();
        }

        public ActionResult ReceiptInfo()
        {
            string ReceiptNo = "16070001";
            //if (Session["ReceiptNo"] != null)
            //{
            //    ReceiptNo = Session["ReceiptNo"].ToString();
            //}
            //else
            //{
            //    if (Request.QueryString["ReceiptNo"] != null)
            //    {
                   ReceiptNo = Request.QueryString["ReceiptNo"];
            //    }
            //    else
            //    {
            //        ReceiptNo = "";
            //        return RedirectToAction("ReceiptList", "LMS");
            //    }

            //}

            List<ReceiptList> model = new List<ReceiptList>();
            List<BookingInfo> BModel = new List<BookingInfo>();
            List<Invoice> IModel = new List<Invoice>();
            List<Receipt> RModel = new List<Receipt>();

            var sReceiptInfo = (from r in db.LMS_Receipt 
                                where r.ReceiptNo == ReceiptNo
                                select new
                                {
                                    ReceiptNo = r.ReceiptNo,
                                    ReceiptDate = r.ReceiptDate,
                                    InvoiceNo = r.InvoiceNo,
                                    PaymentDate = r.PaymentDate,
                                    GrandTotal = r.GrandTotal,
                                    PaymentType = r.PaymentType,
                                    Status = r.Status
                                  
                                }
                                ).ToList();

            var sInvoiceInfo = (from i in db.LMS_Invoice
                                join r in db.LMS_Receipt on i.InvoiceNo equals r.InvoiceNo
                                join s in db.LMS_SubAgent on i.SubID equals s.ID
                                where r.ReceiptNo == ReceiptNo
                                select new
                                {
                                    InvoiceNo = i.InvoiceNo,
                                    InvoiceDate = i.InvoiceDate,
                                    DueDate = i.DueDate,
                                    SubId = i.SubID,
                                    CreditTerm = i.CreditTerm,
                                    Status = i.Status,
                                    GrandTotal = i.GrandTotal,
                                    SubName = s.Name,
                                    SubAddress = s.Address,
                                    SubTel = s.Telephone,
                                    SubTax = s.Tax
                                }
                                    ).ToList();


            var sBookingInfo = (from id in db.LMS_InvoiceDetail
                                join r in db.LMS_Receipt on id.InvoiceNo equals r.InvoiceNo
                                join b in db.LMS_Booking on id.BookingID equals b.BookingID
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where r.ReceiptNo == ReceiptNo
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                BModel.Add(a);

            }

            foreach (var il in sInvoiceInfo)
            {
                Invoice a = new Invoice();
                a.CreditTerm = Convert.ToInt32(il.CreditTerm);
                a.DueDate = Convert.ToDateTime(il.DueDate);
                a.InvoiceDate = Convert.ToDateTime(il.InvoiceDate);
                a.InvoiceNo = il.InvoiceNo;
                a.IStatus = Convert.ToInt32(il.Status);
                a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                a.SubName = il.SubName;
                a.SubAddress = il.SubAddress;
                a.SubTel = il.SubTel;
                a.SubTax = il.SubTax;
                IModel.Add(a);

            }

            foreach (var il in sReceiptInfo)
            {
                Receipt a = new Receipt();
                a.InvoiceNo = il.InvoiceNo;
                a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                a.InvoiceNo = il.InvoiceNo;
                a.ReceiptNo = il.ReceiptNo;
                a.ReceiptDate = Convert.ToDateTime(il.ReceiptDate);
                a.PaymentDate = Convert.ToDateTime(il.PaymentDate);
                a.PaymentType = Convert.ToInt32(il.PaymentType);
                a.Status = Convert.ToInt32(il.Status);

                RModel.Add(a);

            }

            ReceiptList IL = new ReceiptList();
            IL.LBooking = BModel.ToList();
            IL.LInvoice = IModel.ToList();
            IL.LReceipt = RModel.ToList();
            model.Add(IL);

            return View(model);
        }
        public ActionResult ReceiptPrint()
        {
            string ReceiptNo = "16070001";
            if (Session["ReceiptNo"] != null)
            {
                ReceiptNo = Session["ReceiptNo"].ToString();
            }
            else
            {
                if (Request.QueryString["ReceiptNo"] != null)
                {
                    ReceiptNo = Request.QueryString["ReceiptNo"];
                }
                else
                {
                    ReceiptNo = "";
                    return RedirectToAction("ReceiptList", "LMS");
                }

            }

            List<ReceiptList> model = new List<ReceiptList>();
            List<BookingInfo> BModel = new List<BookingInfo>();
            List<Invoice> IModel = new List<Invoice>();
            List<Receipt> RModel = new List<Receipt>();

            var sReceiptInfo = (from r in db.LMS_Receipt
                                where r.ReceiptNo == ReceiptNo
                                select new
                                {
                                    ReceiptNo = r.ReceiptNo,
                                    ReceiptDate = r.ReceiptDate,
                                    InvoiceNo = r.InvoiceNo,
                                    PaymentDate = r.PaymentDate,
                                    GrandTotal = r.GrandTotal,
                                    PaymentType = r.PaymentType,
                                    Status = r.Status

                                }
                                ).ToList();

            var sInvoiceInfo = (from i in db.LMS_Invoice
                                join r in db.LMS_Receipt on i.InvoiceNo equals r.InvoiceNo
                                join s in db.LMS_SubAgent on i.SubID equals s.ID
                                where r.ReceiptNo == ReceiptNo
                                select new
                                {
                                    InvoiceNo = i.InvoiceNo,
                                    InvoiceDate = i.InvoiceDate,
                                    DueDate = i.DueDate,
                                    SubId = i.SubID,
                                    CreditTerm = i.CreditTerm,
                                    Status = i.Status,
                                    GrandTotal = i.GrandTotal,
                                    SubName = s.Name,
                                    SubAddress = s.Address,
                                    SubTel = s.Telephone,
                                    SubTax = s.Tax
                                }
                                    ).ToList();


            var sBookingInfo = (from id in db.LMS_InvoiceDetail
                                join r in db.LMS_Receipt on id.InvoiceNo equals r.InvoiceNo
                                join b in db.LMS_Booking on id.BookingID equals b.BookingID
                                join c in db.LMS_Car on b.CarID equals c.ID
                                //join u in db.LMS_User on b.UserID equals u.UserID
                                //join a in db.LMS_SubAgent on b.AgentID equals a.ID
                                join d in db.LMS_Driver on b.DriverID equals d.ID
                                where r.ReceiptNo == ReceiptNo
                                select new
                                {
                                    BookingID = b.BookingID,
                                    BookingDate = b.BookingDate,
                                    Title = b.Title,
                                    FirstName = b.FirstName,
                                    LastName = b.LastName,
                                    Address = b.Address,
                                    Telephone = b.Telephone,
                                    Mobile = b.Mobile,
                                    Email = b.Email,
                                    //OrderBy = u.UserName,
                                    CustomerType = b.CustomerType,
                                    ServiceType = b.ServiceType,
                                    bDate = b.Date,
                                    bTime = b.Time,
                                    Passenger = b.Passenger,
                                    Luggage = b.Luggage,
                                    FlightNo = b.FlightNo,
                                    FlightTime = b.FlightTime,
                                    FromDetail = b.FromDetail,
                                    ToDetail = b.ToDetail,
                                    Remark = b.Remark,
                                    RouteDetail = b.RouteDetail,
                                    ProductID = b.ProductID,
                                    CarID = b.CarID,
                                    CModel = c.Model,
                                    Price = b.Price,
                                    Discount = b.Discount,
                                    TotalPrice = b.TotalPrice,
                                    Status = b.Status,
                                    //AgentName = a.Name,
                                    DriverID = b.DriverID,
                                    DTitle = d.Title,
                                    DName = d.Name,
                                    DLastName = d.LastName,
                                    DMobile = d.Mobile,
                                    VehicleRegis = c.VehicleRegis,
                                    AgentID = b.AgentID,
                                    UserID = b.UserID,
                                    //AgentEmail = a.Email,
                                    PaymentType = b.PaymentType,
                                    PaymentStatus = b.PaymentStatus
                                }
                 ).ToList();
            foreach (var bl in sBookingInfo)
            {
                BookingInfo a = new BookingInfo();
                a.Address = bl.Address.ToString();
                //a.AgentEmail = bl.AgentEmail.ToString();
                //a.AgentName = bl.AgentName.ToString();
                a.BookingDate = Convert.ToDateTime(bl.BookingDate);
                a.BookingID = bl.BookingID.ToString();
                a.CarID = Convert.ToInt32(bl.CarID);
                a.CarModel = bl.CModel.ToString();
                a.CustomerType = Convert.ToInt32(bl.CustomerType);
                a.Date = Convert.ToDateTime(bl.bDate);
                a.DFirstName = bl.DName.ToString();
                a.DLastName = bl.DLastName.ToString();
                a.DMobile = bl.DMobile.ToString();
                a.DriverID = Convert.ToInt32(bl.DriverID);
                a.DTitle = bl.DTitle.ToString();
                a.Email = bl.Email.ToString();
                a.FirstName = bl.FirstName.ToString();
                a.FlightNo = bl.FlightNo.ToString();
                a.FlightTime = bl.FlightTime.ToString();
                a.FromDetail = bl.FromDetail.ToString();
                a.LastName = bl.LastName.ToString();
                a.Luggage = Convert.ToInt32(bl.Luggage);
                a.Mobile = bl.Mobile.ToString();
                //a.OrderBy = bl.OrderBy.ToString();
                a.Passenger = Convert.ToInt32(bl.Passenger);
                a.Price = Convert.ToDecimal(bl.Price);
                a.Remark = bl.Remark.ToString();
                a.RouteDetail = bl.RouteDetail;
                a.ServiceType = Convert.ToInt32(bl.ServiceType);
                a.Status = Convert.ToInt32(bl.Status);
                a.Telephone = bl.Telephone.ToString();
                a.Time = bl.bTime.ToString();
                a.Title = bl.Title.ToString();
                a.ToDetail = bl.ToDetail.ToString();
                a.TotalPrice = Convert.ToDecimal(bl.TotalPrice);
                a.VehicleRegis = bl.VehicleRegis.ToString();

                if (bl.AgentID != 0)
                {
                    var sAgent = (from sg in db.LMS_SubAgent
                                  where sg.ID == bl.AgentID
                                  select new
                                  {
                                      AgentName = sg.Name,
                                      AgentMobile = sg.Telephone,
                                      AgentEMail = sg.Email
                                  }
                 ).FirstOrDefault();

                    a.AgentEmail = sAgent.AgentEMail.ToString();
                    a.AgentMobile = sAgent.AgentMobile.ToString();
                    a.AgentName = sAgent.AgentName.ToString();

                }
                else
                {

                    a.AgentEmail = "";
                    a.AgentMobile = "";
                    a.AgentName = "";

                }
                if (bl.UserID != 0)
                {
                    var sUser = (from u in db.LMS_User
                                 where u.UserID == bl.UserID
                                 select new
                                 {
                                     UName = u.FullName,
                                     UEmail = u.Email,
                                     UTelephone = u.Telephone
                                 }
                 ).FirstOrDefault();

                    a.UName = sUser.UName.ToString();
                    a.UEmail = sUser.UEmail.ToString();
                    a.UTelephone = sUser.UTelephone.ToString();

                }
                else
                {

                    a.UName = "";
                    a.UEmail = "";
                    a.UTelephone = "";

                }

                a.PaymentType = Convert.ToInt32(bl.PaymentType);
                a.PaymentStatus = Convert.ToInt32(bl.PaymentStatus);


                BModel.Add(a);

            }

            foreach (var il in sInvoiceInfo)
            {
                Invoice a = new Invoice();
                a.CreditTerm = Convert.ToInt32(il.CreditTerm);
                a.DueDate = Convert.ToDateTime(il.DueDate);
                a.InvoiceDate = Convert.ToDateTime(il.InvoiceDate);
                a.InvoiceNo = il.InvoiceNo;
                a.IStatus = Convert.ToInt32(il.Status);
                a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                a.SubName = il.SubName;
                a.SubAddress = il.SubAddress;
                a.SubTel = il.SubTel;
                a.SubTax = il.SubTax;
                IModel.Add(a);

            }

            foreach (var il in sReceiptInfo)
            {
                Receipt a = new Receipt();
                a.InvoiceNo = il.InvoiceNo;
                a.GrandTotal = Convert.ToDecimal(il.GrandTotal);
                a.InvoiceNo = il.InvoiceNo;
                a.ReceiptNo = il.ReceiptNo;
                a.ReceiptDate = Convert.ToDateTime(il.ReceiptDate);
                a.PaymentDate = Convert.ToDateTime(il.PaymentDate);
                a.PaymentType = Convert.ToInt32(il.PaymentType);
                a.Status = Convert.ToInt32(il.Status);

                RModel.Add(a);

            }

            ReceiptList IL = new ReceiptList();
            IL.LBooking = BModel.ToList();
            IL.LInvoice = IModel.ToList();
            IL.LReceipt = RModel.ToList();
            model.Add(IL);

            return View(model);
        }
    }
}
