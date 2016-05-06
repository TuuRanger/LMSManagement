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
                                    UserID = b.OrderBy
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
                                    UserID = b.OrderBy
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

        public ActionResult Booking ()
        {
           
            var dForm = (from p in db.LMS_From
                         select new
                    {
                        cDetail = p.Detail,
                        cID = p.ID
                    }
                    ).ToList();
            var dTo = (from t in db.LMS_TO
                         select new
                         {
                             tDetail = t.Detail,
                             tID = t.ID
                         }
                  ).ToList();

            var dCar = (from c in db.LMS_Vehicle
                       select new
                       {
                           tModel = c.Name,
                           tID = c.ID
                       }
                ).ToList();

            var dSubAgent = (from c in db.LMS_SubAgent
                        select new
                        {
                            tName = c.Name,
                            tID = c.ID
                        }
               ).ToList();

            List<FromToModel> model = new List<FromToModel>();
            List<FromModel> FModel = new List<FromModel>();
            List<ToModel> TModel = new List<ToModel>();
            List<Car> Car = new List<Car>();
            List<SubAgent> SubAgent = new List<SubAgent>();

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

            foreach (var bs in dSubAgent)
            {
                SubAgent s = new SubAgent();
                s.AName = bs.tName.ToString();
                s.AId = Convert.ToInt32(bs.tID.ToString());
                SubAgent.Add(s);
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

            bkd.ServiceType = Convert.ToInt32(Request["ServiceType"]);

            if (bkd.ServiceType == 1)
            {
                bkd.FromID = Convert.ToInt32(Request["From"]);
            
           }else if (bkd.ServiceType == 3)
            {
                bkd.FromID = Convert.ToInt32(Request["From3"]);
            }
          
            if (bkd.FromID != 0)
            {
                bkd.ServiceType = Convert.ToInt32(Request["ServiceType"]);
                bkd.AgentID = Convert.ToInt32(Request["SubAgent"]);
                bkd.CustomerType = Convert.ToInt32(Request["CustomerType"]);
                bkd.OrderBy = Request["UserType"];
                bkd.Status = 1;
                if (bkd.ServiceType == 1)
                {
                    bkd.CarID = Convert.ToInt32(Request["CarModel"]);

                    bkd.Date = Convert.ToDateTime(Request["Date"]);


                    bkd.FlightNo = Request["Flight"];
                    bkd.FlightTime = Request["FlightTime"];

                    bkd.ToID = Convert.ToInt32(Request["To"]);
                    bkd.Luggage = Convert.ToInt32(Request["Luggage"]);
                  
                  
                    bkd.Passenger = Convert.ToInt32(Request["Passenger"]);

                   
                    bkd.Time = Request["Time"];
               
                    bkd.Vechile = Convert.ToInt32(Request["Vechile"]);

                }else if (bkd.ServiceType == 3)
                {
                    bkd.CarID = Convert.ToInt32(Request["CarModel3"]);

                    bkd.Date = Convert.ToDateTime(Request["Date3"]);


                    bkd.FlightNo = Request["Flight3"];
                    bkd.FlightTime = Request["FlightTime3"];

                    bkd.ToID = Convert.ToInt32(Request["To3"]);
                    bkd.Luggage = Convert.ToInt32(Request["Luggage3"]);
     
                    bkd.Passenger = Convert.ToInt32(Request["Passenger3"]);

 
                 
                    bkd.Time = Request["Time3"];
        
                    bkd.Vechile = Convert.ToInt32(Request["Vechile3"]);
                }

               

                var CarModel = (from c in db.LMS_Car
                                where c.ID == bkd.CarID
                                select c.Model
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
                               where r.FromID == bkd.FromID && r.ToID == bkd.ToID && p.TpyeOfService == bkd.ServiceType && p.TpyeOfVihicle == bkd.CarID
                               select new
                               {
                                   ProductID = p.ID,
                                   Price = p.Price
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
                                      aDiscount = ap.Discount,
                                      aMarkup = ap.Markup
                                  }
                  ).FirstOrDefault();

              
                bkd.ProductID = Convert.ToInt32(Product.ProductID.ToString());
                bkd.Price = Convert.ToDecimal(Product.Price.ToString()) * bkd.Vechile;
                if (bkd.AgentID == 0)
                {
                    bkd.AgentEmail = "";
                    bkd.Discount = 0;
                    bkd.AgentName = "";
                }else
                {
                    bkd.AgentEmail = AgentEmail.aEmail.ToString();
                    bkd.Discount = bkd.Price * (Convert.ToDecimal(AgentPrice.aDiscount)/100);
                    bkd.AgentName = AgentEmail.aName.ToString();
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
            AddBooking.OrderBy = bkd.OrderBy;
            AddBooking.Passenger = bkd.Passenger;
            AddBooking.Price = bkd.Price;
            AddBooking.ProductID = bkd.ProductID;
            AddBooking.Remark = bkd.Remark;
            AddBooking.ServiceType = bkd.ServiceType;
            AddBooking.Status = bkd.Status;
            AddBooking.Telephone = bkd.Telephone;
            AddBooking.Time = bkd.Time;
            AddBooking.Title = bkd.Title;
            AddBooking.ToDetail = bkd.ToDetail;
            AddBooking.TotalPrice = bkd.TotalPrice;
            AddBooking.AgentID = bkd.AgentID;

            db.LMS_Booking.Add(AddBooking);
            db.SaveChanges();
           
            //EMail
            StringBuilder sbE = new StringBuilder();

                    sbE.Append("<table>");
                    sbE.Append(" <tr>");
                    sbE.Append("  <td><font size=\"2\">");
                    sbE.Append("  <b>LMS Booking : "+ BID +"</b>");
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
                        msg.Subject = "LMS Booking" + BID;
                        msg.Body = "CONFIRMATION Your booking has been successful.(Booking Reference :" + BID +")";
                        msg.Attachments.Add(new Attachment(new MemoryStream(bytes), "LMSBooking.pdf"));
                        msg.Priority = MailPriority.High;
                        msg.BodyEncoding = UTF8Encoding.UTF8;
                        msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        client.Send(msg);

                     
                        //EMail

                    }
                    return RedirectToAction("BookingInfo", "LMS");
            //return View();
        }

        public ActionResult BookingInfo()
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
                                        UserID = b.OrderBy
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
                                        UserID = b.OrderBy
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
        public ActionResult ReportBooking()
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
                                        UserID = b.OrderBy
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
                                        UserID = b.OrderBy
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
    }
}
