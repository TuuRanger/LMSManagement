//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LMS
{
    using System;
    using System.Collections.Generic;
    
    public partial class LMS_Booking
    {
        public int ID { get; set; }
        public string BookingID { get; set; }
        public Nullable<System.DateTime> BookingDate { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> CustomerType { get; set; }
        public Nullable<int> ServiceType { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Time { get; set; }
        public Nullable<int> Passenger { get; set; }
        public Nullable<int> Luggage { get; set; }
        public string FlightNo { get; set; }
        public string FlightTime { get; set; }
        public string FromDetail { get; set; }
        public string ToDetail { get; set; }
        public string Remark { get; set; }
        public Nullable<int> ProductID { get; set; }
        public Nullable<int> CarID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> AgentID { get; set; }
        public Nullable<int> DID { get; set; }
        public Nullable<int> DriverID { get; set; }
        public string Currency { get; set; }
        public string RouteDetail { get; set; }
        public Nullable<int> PaymentType { get; set; }
        public Nullable<int> PaymentStatus { get; set; }
    }
}
