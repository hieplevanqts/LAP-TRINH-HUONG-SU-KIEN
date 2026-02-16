using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class PendingBillDetailsForm : Form
{
    private readonly PaymentService _paymentService = new();
    private readonly int _bookingId;
    private readonly decimal _discount;
    private readonly decimal _tax;
    private readonly bool _useActualCheckoutPricing;

    private readonly Label _lblBooking = new();
    private readonly Label _lblCustomer = new();
    private readonly Label _lblContact = new();
    private readonly Label _lblDates = new();
    private readonly Label _lblBookedBy = new();
    private readonly Label _lblCheckoutMode = new();
    private readonly Label _lblRoomTotal = new();
    private readonly Label _lblServiceTotal = new();
    private readonly Label _lblSubtotal = new();
    private readonly Label _lblDiscount = new();
    private readonly Label _lblTax = new();
    private readonly Label _lblTotal = new();

    private readonly Guna2DataGridView _roomsGrid = new();
    private readonly Guna2DataGridView _servicesGrid = new();
    private readonly PrintDocument _printDocument = new();

    private DataTable? _roomLines;
    private DataTable? _serviceLines;

    public PendingBillDetailsForm(int bookingId, decimal discount, decimal tax, bool useActualCheckoutPricing)
    {
        _bookingId = bookingId;
        _discount = discount;
        _tax = tax;
        _useActualCheckoutPricing = useActualCheckoutPricing;

        Text = "Chi tiết bill cần thanh toán";
        Width = 980;
        Height = 720;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(BuildHeaderPanel(), 0, 0);
        layout.Controls.Add(BuildContentPanel(), 0, 1);
        layout.Controls.Add(BuildFooterPanel(), 0, 2);

        Controls.Add(layout);

        _printDocument.PrintPage += OnPrintPage;
        Load += (_, _) => LoadDetails();
    }

    private Control BuildHeaderPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(12)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        ConfigureHeaderValueLabel(_lblBooking);
        ConfigureHeaderValueLabel(_lblCustomer);
        ConfigureHeaderValueLabel(_lblContact);
        ConfigureHeaderValueLabel(_lblDates);
        ConfigureHeaderValueLabel(_lblBookedBy);
        ConfigureHeaderValueLabel(_lblCheckoutMode);

        panel.Controls.Add(new Label { Text = "Mã đặt phòng", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        panel.Controls.Add(_lblBooking, 1, 0);
        panel.Controls.Add(new Label { Text = "Khách hàng", AutoSize = true }, 0, 1);
        panel.Controls.Add(_lblCustomer, 1, 1);
        panel.Controls.Add(new Label { Text = "Liên hệ", AutoSize = true }, 0, 2);
        panel.Controls.Add(_lblContact, 1, 2);
        panel.Controls.Add(new Label { Text = "Thời gian thuê", AutoSize = true }, 0, 3);
        panel.Controls.Add(_lblDates, 1, 3);
        panel.Controls.Add(new Label { Text = "Nhân viên đặt", AutoSize = true }, 0, 4);
        panel.Controls.Add(_lblBookedBy, 1, 4);
        panel.Controls.Add(new Label { Text = "Cách tính tiền phòng", AutoSize = true }, 0, 5);
        panel.Controls.Add(_lblCheckoutMode, 1, 5);

        return panel;
    }

    private Control BuildContentPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        var roomsGroup = new Guna2GroupBox
        {
            Text = "Tiền phòng",
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };
        ConfigureGrid(_roomsGrid);
        roomsGroup.Controls.Add(_roomsGrid);

        var servicesGroup = new Guna2GroupBox
        {
            Text = "Dịch vụ đã sử dụng",
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };
        ConfigureGrid(_servicesGrid);
        servicesGroup.Controls.Add(_servicesGrid);

        panel.Controls.Add(roomsGroup, 0, 0);
        panel.Controls.Add(servicesGroup, 0, 1);

        return panel;
    }

    private Control BuildFooterPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(12)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        panel.Controls.Add(new Label { Text = "Tiền phòng", AutoSize = true }, 0, 0);
        panel.Controls.Add(_lblRoomTotal, 1, 0);
        panel.Controls.Add(new Label { Text = "Tiền dịch vụ", AutoSize = true }, 0, 1);
        panel.Controls.Add(_lblServiceTotal, 1, 1);
        panel.Controls.Add(new Label { Text = "Tạm tính", AutoSize = true }, 0, 2);
        panel.Controls.Add(_lblSubtotal, 1, 2);
        panel.Controls.Add(new Label { Text = "Giảm giá", AutoSize = true }, 0, 3);
        panel.Controls.Add(_lblDiscount, 1, 3);
        panel.Controls.Add(new Label { Text = "Thuế", AutoSize = true }, 0, 4);
        panel.Controls.Add(_lblTax, 1, 4);
        panel.Controls.Add(new Label { Text = "Tổng cần thanh toán", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 5);
        panel.Controls.Add(_lblTotal, 1, 5);

        var btnPrint = CreatePrimaryButton("In bill");
        btnPrint.Click += (_, _) => PrintBill();
        var btnClose = CreateSecondaryButton("Đóng");
        btnClose.Click += (_, _) => Close();

        panel.Controls.Add(btnPrint, 0, 6);
        panel.Controls.Add(btnClose, 1, 6);

        return panel;
    }

    private static void ConfigureGrid(Guna2DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = Color.White;
        grid.GridColor = Color.FromArgb(231, 234, 243);
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 108, 223);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(231, 240, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(33, 37, 41);
    }

    private static void ConfigureHeaderValueLabel(Label label)
    {
        label.AutoSize = true;
        label.MaximumSize = new Size(720, 0);
        label.Margin = new Padding(3, 4, 3, 4);
    }

    private static Guna2Button CreatePrimaryButton(string text)
    {
        return new Guna2Button
        {
            Text = text,
            Width = 120,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(45, 108, 223),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
    }

    private static Guna2Button CreateSecondaryButton(string text)
    {
        return new Guna2Button
        {
            Text = text,
            Width = 120,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(233, 236, 239),
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
    }

    private void LoadDetails()
    {
        var header = _paymentService.GetPendingBillHeader(_bookingId, _useActualCheckoutPricing);
        if (header is null)
        {
            MessageBox.Show("Không tìm thấy thông tin bill.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
            return;
        }

        _lblBooking.Text = $"# {_bookingId}";
        _lblCustomer.Text = header["FullName"].ToString();
        _lblContact.Text = $"{header["Phone"]} | {header["Email"]}";
        _lblDates.Text = $"{Convert.ToDateTime(header["CheckInDate"]):dd/MM/yyyy} - {Convert.ToDateTime(header["CheckOutDate"]):dd/MM/yyyy}";
        _lblBookedBy.Text = FormatEmployee(header["CreatedByEmployeeId"], header["CreatedByEmployeeName"], "Không rõ");
        _lblCheckoutMode.Text = _useActualCheckoutPricing
            ? "Theo thời điểm trả thực tế"
            : "Theo thời gian đặt (trừ khi đã quá checkout)";

        _roomLines = _paymentService.GetPendingBillRoomLines(_bookingId, _useActualCheckoutPricing);
        _serviceLines = _paymentService.GetBookingServiceUsages(_bookingId);

        _roomsGrid.DataSource = _roomLines;
        _servicesGrid.DataSource = _serviceLines;

        if (_roomsGrid.Columns["RoomNumber"] is { } roomNumber)
        {
            roomNumber.HeaderText = "Phòng";
            roomNumber.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            roomNumber.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        if (_roomsGrid.Columns["TypeName"] is { } typeName)
        {
            typeName.HeaderText = "Loại phòng";
        }
        if (_roomsGrid.Columns["PricePerNight"] is { } price)
        {
            price.HeaderText = "Giá/đêm";
            price.DefaultCellStyle.Format = "N0";
            price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            price.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (_roomsGrid.Columns["ChargeDays"] is { } days)
        {
            days.HeaderText = "Số ngày tính";
            days.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            days.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        if (_roomsGrid.Columns["LineTotal"] is { } total)
        {
            total.HeaderText = "Thành tiền";
            total.DefaultCellStyle.Format = "N0";
            total.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            total.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        if (_servicesGrid.Columns["ServiceUsageId"] is { } usageId) usageId.Visible = false;
        if (_servicesGrid.Columns["ServiceId"] is { } serviceId) serviceId.Visible = false;
        if (_servicesGrid.Columns["ServiceName"] is { } serviceName)
        {
            serviceName.HeaderText = "Dịch vụ";
        }
        if (_servicesGrid.Columns["Quantity"] is { } qty)
        {
            qty.HeaderText = "Số lượng";
            qty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            qty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        if (_servicesGrid.Columns["UnitPrice"] is { } unitPrice)
        {
            unitPrice.HeaderText = "Đơn giá";
            unitPrice.DefaultCellStyle.Format = "N0";
            unitPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            unitPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (_servicesGrid.Columns["Amount"] is { } amount)
        {
            amount.HeaderText = "Thành tiền";
            amount.DefaultCellStyle.Format = "N0";
            amount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            amount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        var roomTotal = Convert.ToDecimal(header["RoomTotal"]);
        var serviceTotal = Convert.ToDecimal(header["ServiceTotal"]);
        var subtotal = roomTotal + serviceTotal;
        var totalPay = subtotal - _discount + _tax;
        if (totalPay < 0)
        {
            totalPay = 0;
        }

        _lblRoomTotal.Text = FormatMoney(roomTotal);
        _lblServiceTotal.Text = FormatMoney(serviceTotal);
        _lblSubtotal.Text = FormatMoney(subtotal);
        _lblDiscount.Text = FormatMoney(_discount);
        _lblTax.Text = FormatMoney(_tax);
        _lblTotal.Text = FormatMoney(totalPay);
    }

    private void PrintBill()
    {
        using var preview = new PrintPreviewDialog
        {
            Document = _printDocument,
            Width = 900,
            Height = 700
        };
        preview.ShowDialog(this);
    }

    private void OnPrintPage(object? sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        if (g is null)
        {
            return;
        }

        var left = e.MarginBounds.Left;
        var top = e.MarginBounds.Top;
        var lineHeight = (int)Font.GetHeight(g) + 4;

        using var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        using var boldFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var regularFont = new Font("Segoe UI", 9F);

        g.DrawString("BILL CẦN THANH TOÁN", titleFont, Brushes.Black, left, top);
        top += lineHeight * 2;

        g.DrawString($"Đặt phòng: {_lblBooking.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Khách hàng: {_lblCustomer.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Liên hệ: {_lblContact.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Thời gian thuê: {_lblDates.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Nhân viên đặt: {_lblBookedBy.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Cách tính tiền phòng: {_lblCheckoutMode.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight * 2;

        g.DrawString("TIỀN PHÒNG", boldFont, Brushes.Black, left, top);
        top += lineHeight;
        if (_roomLines is not null)
        {
            foreach (DataRow row in _roomLines.Rows)
            {
                var text = $"{row["RoomNumber"]} - {row["TypeName"]}: {FormatMoney(Convert.ToDecimal(row["LineTotal"]))}";
                g.DrawString(text, regularFont, Brushes.Black, left, top);
                top += lineHeight;
            }
        }

        top += lineHeight;
        g.DrawString("DỊCH VỤ", boldFont, Brushes.Black, left, top);
        top += lineHeight;
        if (_serviceLines is not null)
        {
            foreach (DataRow row in _serviceLines.Rows)
            {
                var text = $"{row["ServiceName"]} x {row["Quantity"]}: {FormatMoney(Convert.ToDecimal(row["Amount"]))}";
                g.DrawString(text, regularFont, Brushes.Black, left, top);
                top += lineHeight;
            }
        }

        top += lineHeight;
        g.DrawString($"Tiền phòng: {_lblRoomTotal.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Tiền dịch vụ: {_lblServiceTotal.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Tạm tính: {_lblSubtotal.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Giảm giá: {_lblDiscount.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Thuế: {_lblTax.Text}", regularFont, Brushes.Black, left, top);
        top += lineHeight;
        g.DrawString($"Tổng cần thanh toán: {_lblTotal.Text}", boldFont, Brushes.Black, left, top);
    }

    private static string FormatMoney(decimal amount) => $"{amount:N0} đ";

    private static string FormatEmployee(object employeeId, object employeeName, string fallback)
    {
        if (employeeId == DBNull.Value || employeeName == DBNull.Value)
        {
            return fallback;
        }

        var id = Convert.ToInt32(employeeId);
        var name = employeeName.ToString();
        return string.IsNullOrWhiteSpace(name) ? fallback : $"NV{id:D4} - {name}";
    }
}
