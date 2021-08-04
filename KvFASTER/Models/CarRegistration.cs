using System;

namespace KvFASTER.Models
{
    public class CarRegistration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Model { get; set; }
        public double Price { get; set; }
        public string Color { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
        public CarStatus CarStatus { get; set; } = CarStatus.Available;
        public override string ToString()
        {
            return $"[{Model}], going for ${Price:N2}, Status: {CarStatus}, Color: {Color}, Date Registered: {RegisteredDate}";
        }
    }
    public enum CarStatus
    {
        Available,
        SoldOut,
        LoanedOut
    }
}
