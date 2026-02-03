namespace HotelManager.Models;

public sealed class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class Employee
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public DateTime HireDate { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class Account
{
    public int AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int EmployeeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdNumber { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class RoomType
{
    public int RoomTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int Capacity { get; set; }
    public string? Description { get; set; }
}

public sealed class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public int Floor { get; set; }
    public string Status { get; set; } = "Available";
}

public sealed class Service
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Unit";
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}

public sealed class Booking
{
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public int? CreatedByAccountId { get; set; }
}

public sealed class BookingRoom
{
    public int BookingRoomId { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public decimal PricePerNight { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public string Status { get; set; } = "Reserved";
}

public sealed class ServiceUsage
{
    public int ServiceUsageId { get; set; }
    public int BookingId { get; set; }
    public int ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime UsedAt { get; set; }
    public int? AddedByAccountId { get; set; }
}

public sealed class Invoice
{
    public int InvoiceId { get; set; }
    public int BookingId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public int? CreatedByAccountId { get; set; }
}

public sealed class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string? Note { get; set; }
}

public sealed class AuditLog
{
    public int LogId { get; set; }
    public int? AccountId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Entity { get; set; }
    public string? EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}
