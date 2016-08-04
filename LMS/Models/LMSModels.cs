using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LMS.Models
{
    public partial class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        [Required(ErrorMessage = "Please Provide Username", AllowEmptyStrings = false)]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please provide password", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; }
    }


    public class FromToModel
    {
        public List<FromModel> sFromModel { get; set; }
        public List<ToModel> sToModel { get; set; }
        public List<Car> sCar { get; set; }
        public List<SubAgent> sSubAgent { get; set; }

    }
    public class SubAgent
    {
        public int AId { get; set; }
        public string AName { get; set; }
        public int UserID { get; set; }

        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public int CreditTerm { get; set; }
        public string Tax { get; set; }
        public string Remark { get; set; }


    }
    public class FromModel
    {
        public int FId { get; set; }
        public string FDetail { get; set; }
    
    }

    public class ToModel
    {
       
        public int TId { get; set; }
        public string TDetail { get; set; }

    }
    public class Car
    {

        public int CId { get; set; }
        public string CModel { get; set; }

    }

    public class CarList
    {
        public int Id { get; set; }
        public string VehicleRegis { get; set; }
        public int VehicleID { get; set; }
        public string Model { get; set; }
        public string PhotoPath { get; set; }
        public int SitNumber { get; set; }
        public int Status { get; set; }
      
    }
    public class Driver
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Mobile { get; set; }
        public string IDCard { get; set; }
        public int Status { get; set; }
        public string Nationality { get; set; }
        public string Experience { get; set; }
        public int ENG_Listen { get; set; }
        public int ENG_Speak { get; set; }
        public int ENG_Read { get; set; }
        public int ENG_Write { get; set; }
        public int TH_Listen { get; set; }
        public int TH_Speak { get; set; }
        public int TH_Read { get; set; }
        public int TH_Write { get; set; }
        public int YOS { get; set; }
    }

    public class dBookingDetail
    {
       
         public string BookingID { get; set; }
         public DateTime BookingDate { get; set; }
         public string Title { get; set; }
         public string FirstName { get; set; }
         public string LastName { get; set; }
         public string Address { get; set; }
         public string Telephone { get; set; }
         public string Mobile { get; set; }
         public string Email { get; set; }
         public string OrderBy { get; set; }
         public int CustomerType { get; set; }
         public int ServiceType { get; set; }
         public DateTime Date { get; set; }
         public string Time { get; set; }
         public int Passenger { get; set; }
         public int Luggage { get; set; }
         public int Vechile { get; set; }
         public string FlightNo { get; set; }
         public string FlightTime { get; set; }
         public string FromDetail { get; set; }
         public string ToDetail { get; set; }
         public int FromID { get; set; }
         public int ToID { get; set; }
         public string Remark { get; set; }
         public int ProductID { get; set; }
         public int VechileID { get; set; }
         public string CarModel { get; set; }
         public Decimal Price { get; set; }
         public Decimal DiscountP { get; set; }
         public Decimal DiscountB { get; set; }
         public Decimal Discount { get; set; }
         public Decimal TotalPrice { get; set; }
         public int Status { get; set; }
         public int UserID { get; set; }
         public int AgentID { get; set; }
         public string AgentEmail { get; set; }
         public string AgentName { get; set; }

         public int DID { get; set; }
         public int CarID { get; set; }
         public int DriverID { get; set; }
         public int TID { get; set; }
         public string VehicleRegis { get; set; }
         public string DTitle { get; set; }
         public string DFirstName { get; set; }
         public string DLastName { get; set; }
         public string DNickName { get; set; }
         public string DLastNameE { get; set; }
         public string DFirstNameE { get; set; }
         public string DNickNameE { get; set; }
         public string DMobile { get; set; }

         public string Currency { get; set; }
         public string RouteDetail { get; set; }

         public int PaymentType { get; set; }
         public int PaymentStatus { get; set; }

    }
    public class SaleVistDetail
    {
        public string BookingID { get; set; }
        public int ID { get; set; }
        public string RouteDetail { get; set; }
    }
    public class BookingList
    {
        public string BookingID { get; set; }
        public int ServiceType { get; set; }
        public string FromDetail { get; set; }
        public string ToDetail { get; set; }
        public int FromID { get; set; }
        public int ToID { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int CarID { get; set; }
        public string CarModel { get; set; }
        public int Status { get; set; }
        public int AgentID { get; set; }
        public int UserID { get; set; }
        public string RouteDetail { get; set; }

        public string Currency { get; set; }

        public string sDate { get; set; }
        public string eDate { get; set; }

        public int DriverID { get; set; }
        public string DTitle { get; set; }
        public string DName { get; set; }
        public string DLastName { get; set; }
    }

    public class WorkTime
    {
        public int TID { get; set; }
        public string TStart { get; set; }
        public string TEnd { get; set; }
    }
    public class QueueList
    {
       
        public string VehicleRegis { get; set; }
        public string Model { get; set; }
        public int DriverID { get; set; }
        public string DTitle { get; set; }
        public string DFirstName { get; set; }
        public string DLastName { get; set; }
        public string DMobile { get; set; }
        public string TRemark { get; set; }
        public DateTime LastJobDate { get; set; }
        public string LastJobTime { get; set; }
        public string LastBooking { get; set; }
    }

    public class BookingInfo
    {

        public string BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string OrderBy { get; set; }
        public int CustomerType { get; set; }
        public int ServiceType { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int Passenger { get; set; }
        public int Luggage { get; set; }
        public int Vechile { get; set; }
        public string FlightNo { get; set; }
        public string FlightTime { get; set; }
        public string FromDetail { get; set; }
        public string ToDetail { get; set; }
        public int FromID { get; set; }
        public int ToID { get; set; }
        public string Remark { get; set; }
        public int ProductID { get; set; }
        public int VechileID { get; set; }
        public string CarModel { get; set; }
        public Decimal Price { get; set; }
        public Decimal DiscountP { get; set; }
        public Decimal DiscountB { get; set; }
        public Decimal Discount { get; set; }
        public Decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public int AgentID { get; set; }
        public string AgentEmail { get; set; }
        public string AgentName { get; set; }
        public string AgentMobile { get; set; }

        public int DID { get; set; }
        public int CarID { get; set; }
        public int DriverID { get; set; }
        public int TID { get; set; }
        public string VehicleRegis { get; set; }
        public string DTitle { get; set; }
        public string DFirstName { get; set; }
        public string DLastName { get; set; }
        public string DNickName { get; set; }
        public string DLastNameE { get; set; }
        public string DFirstNameE { get; set; }
        public string DNickNameE { get; set; }
        public string DMobile { get; set; }

        public string UName { get; set; }
        public string UTelephone { get; set; }
        public string UEmail { get; set; }

        public string Currency { get; set; }

        public string RouteDetail { get; set; }
        public int PaymentType { get; set;}
        public int PaymentStatus { get; set; }

    }


    public class Invoice
    {
        public int InvoiceID { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int SudID { get; set; }
        public string SubName { get; set; }
        public string SubAddress { get; set; }
        public string SubTel { get; set; }
        public string SubTax { get; set; }
        public int NumOfBooking { get; set; }
        public int CreditTerm { get; set; }
        public int IStatus { get; set; }
        public decimal GrandTotal { get; set; }

    }

    public class InvoiceDetail
    {
        public int InvoideDID { get; set; }
        public string InvoiceNo { get; set; }
        public string Booking { get; set; }
    }

    public class InvoiceList
    {
        public List<Invoice> LInvoice { get; set; }
        public List<InvoiceDetail> LInvoiceDetail { get; set; }
        public List<BookingInfo> LBooking { get; set; }
        public List<SubAgent> LSubAgent { get; set; }

    }

    public class Receipt
    {
        public int RID { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentType { get; set; }
        public decimal GrandTotal { get; set; }
        public int Status { get; set; }

        public string SubName { get; set; }
        public string SubAddress { get; set; }
        public string SubTel { get; set; }
        public string SubTax { get; set; }

    }

    public class ReceiptList
    {
        public List<Receipt> LReceipt { get; set; }
        public List<Invoice> LInvoice { get; set; }
        public List<InvoiceDetail> LInvoiceDetail { get; set; }
        public List<BookingInfo> LBooking { get; set; }
        public List<SubAgent> LSubAgent { get; set; }

    }

    public class DBookingList
    {
        public dBookingDetail dBookingDetail { get; set; }
        public List<SaleVistDetail> dSaleVisit { get; set; }
      
    }
}
