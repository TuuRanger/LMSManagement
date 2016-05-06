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

    public class BookingDetail
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
         public int CarID { get; set; }
         public string CarModel { get; set; }
         public Decimal Price { get; set; }
         public Decimal Discount { get; set; }
         public Decimal TotalPrice { get; set; }
         public int Status { get; set; }
         public int UserID { get; set; }
         public int AgentID { get; set; }
         public string AgentEmail { get; set; }
         public string AgentName { get; set; }
       
    
    }

    public class BookingList
    {
        public string BookingID { get; set; }
        public int ServiceType { get; set; }
        public string FromDetail { get; set; }
        public string ToDetail { get; set; }
        public int FromID { get; set; }
        public int ToID { get; set; }
        public DateTime Date { get; set; }
        public int CarID { get; set; }
        public string CarModel { get; set; }
        public int Status { get; set; }
        public int AgentID { get; set; }
        public int UserID { get; set; }
    }
    
}
