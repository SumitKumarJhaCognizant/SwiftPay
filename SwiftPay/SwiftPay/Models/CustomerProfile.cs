using System;
using Model;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class CustomerProfile
    {
        public int CustomerID { get; set; }

        // foreign key to User
        public int UserID { get; set; }

        public DateTime? DOB { get; set; }

        // stored as JSON string or native JSON type
        public string AddressJSON { get; set; }

        public string Nationality { get; set; }

        // using enum for ratings
        public CustomerRiskRating RiskRating { get; set; }

        public CustomerStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // navigation
        public User User { get; set; }
    }
}