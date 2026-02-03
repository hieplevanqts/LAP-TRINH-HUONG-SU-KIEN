namespace HotelManager.WinForms;

public sealed class PaymentSettings
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string QrImagePath { get; set; } = string.Empty;
}
