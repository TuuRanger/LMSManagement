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
        public ActionResult test()
        {
          

            return View();
        }

        public ActionResult Index()
        {
          
            return View();
        }

        public ActionResult Booking ()
        {
            LMSEntities db = new LMSEntities();

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

            var dCar = (from c in db.LMS_Car
                       select new
                       {
                           tModel = c.Model,
                           tID = c.ID
                       }
                ).ToList();

            List<FromToModel> model = new List<FromToModel>();
            List<FromModel> FModel = new List<FromModel>();
            List<ToModel> TModel = new List<ToModel>();
            List<Car> Car = new List<Car>();
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
            model.Add(FT);
          
          //  a.Id = 22;
         
            return View(model);
        }

      
        public ActionResult BookingDetail(FormCollection form)
        {

            LMSEntities db = new LMSEntities();

            BookingDetail bkd = new BookingDetail();
            bkd.FromID = Convert.ToInt32(Request["From"]);
            if (bkd.FromID != 0)
            {
                bkd.CarID = Convert.ToInt32(form["CarModel"]);
                bkd.CarID = Convert.ToInt32(Request["CarModel"]);

                bkd.Date = Convert.ToDateTime(Request["Date"]);
                bkd.Discount = 0;

                bkd.FlightNo = Request["Flight"];
                bkd.FlightTime = Request["FlightTime"];

                bkd.ToID = Convert.ToInt32(Request["To"]);
                bkd.Luggage = Convert.ToInt32(Request["Luggage"]);
                bkd.OrderBy = "Test";
                bkd.Passenger = Convert.ToInt32(Request["Passenger"]);

                bkd.ServiceType = Convert.ToInt32(Request["ServiceType"]);
                bkd.Status = 1;
                bkd.Time = Request["Time"];
                bkd.CustomerType = Convert.ToInt32(Request["CustomerType"]);

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

                bkd.ProductID = Convert.ToInt32(Product.ProductID.ToString());
                bkd.Price = Convert.ToDecimal(Product.Price.ToString());
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
        
            LMSEntities db = new LMSEntities();

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

            db.LMS_Booking.Add(AddBooking);
            db.SaveChanges();
           
            //EMail
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

                      //  MailMessage msg = new MailMessage();
                      //  msg.From = new MailAddress("tuuranger@hotmail.com");
                      //  msg.To.Add(bkd.Email);
                      //  msg.Subject = "LMS Booking " + bkd.BookingID ;

                      //  msg.Priority = MailPriority.High;
                      //  msg.IsBodyHtml = true;
                      //  msg.Body = "CONFIRMATION Your booking has been successful.(Booking Reference :" + bkd.BookingID + ")";
                      //  msg.Attachments.Add(new Attachment(new MemoryStream(bytes), "LMSBooking.pdf"));
                      //  SmtpClient client = new SmtpClient();
                      ////  client.Credentials = new NetworkCredential("ticket@ynotfly.com", "ynotfly123");
                      //  client.Host = "203.151.59.34";
                      //  client.Port = 25;
                      //  client.DeliveryMethod = SmtpDeliveryMethod.Network;
                      //  client.EnableSsl = false;
                      //  client.UseDefaultCredentials = true;
                      //  client.Send(msg);

                    //    MailMessage msg = new MailMessage();

                    //    msg.From = new MailAddress("noppornchaothai@gmail.com");
                    //    msg.To.Add(bkd.Email);
                    //    msg.Subject = "LMS Booking " + bkd.BookingID;
                    //    msg.Body = "CONFIRMATION Your booking has been successful.(Booking Reference :" + bkd.BookingID + ")";
                    //    msg.Attachments.Add(new Attachment(new MemoryStream(bytes), "LMSBooking.pdf"));
                    //    msg.Priority = MailPriority.High;

                    //    SmtpClient client = new SmtpClient();

                    //   // client.Credentials = new NetworkCredential("noppornchaothai@gmail.com", "nopporn", "smtp.gmail.com");
                    ////    client.Host = "smtp.gmail.com";
                    //    client.Port = 587;
                    //    client.UseDefaultCredentials = false;
                    //    client.Credentials = new System.Net.NetworkCredential("noppornchaothai@gmail.com", "orrismileiceberg");
                    //    client.EnableSsl = true;
                    //    client.Send(msg);

                     
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
