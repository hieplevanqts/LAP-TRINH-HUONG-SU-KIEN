using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class InvoiceDetailsForm : Form
{
    private readonly InvoiceService _invoiceService = new();
    private readonly int _invoiceId;

    private readonly Label _lblInvoiceNo = new();
    private readonly Label _lblCustomer = new();
    private readonly Label _lblContact = new();
    private readonly Label _lblDates = new();
    private readonly Label _lblBookedBy = new();
    private readonly Label _lblPaidBy = new();
    private readonly Label _lblMethod = new();
    private readonly Label _lblStatus = new();
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

    public InvoiceDetailsForm(int invoiceId)
    {
        _invoiceId = invoiceId;

        Text = "Chi tiết hóa đơn";
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

        ConfigureHeaderValueLabel(_lblInvoiceNo);
        ConfigureHeaderValueLabel(_lblCustomer);
        ConfigureHeaderValueLabel(_lblContact);
        ConfigureHeaderValueLabel(_lblDates);
        ConfigureHeaderValueLabel(_lblBookedBy);
        ConfigureHeaderValueLabel(_lblPaidBy);
        ConfigureHeaderValueLabel(_lblMethod);
        ConfigureHeaderValueLabel(_lblStatus);

        panel.Controls.Add(new Label { Text = "Mã hóa đơn", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        panel.Controls.Add(_lblInvoiceNo, 1, 0);
        panel.Controls.Add(new Label { Text = "Khách hàng", AutoSize = true }, 0, 1);
        panel.Controls.Add(_lblCustomer, 1, 1);
        panel.Controls.Add(new Label { Text = "Liên hệ", AutoSize = true }, 0, 2);
        panel.Controls.Add(_lblContact, 1, 2);
        panel.Controls.Add(new Label { Text = "Thời gian thuê", AutoSize = true }, 0, 3);
        panel.Controls.Add(_lblDates, 1, 3);
        panel.Controls.Add(new Label { Text = "Nhân viên đặt", AutoSize = true }, 0, 4);
        panel.Controls.Add(_lblBookedBy, 1, 4);
        panel.Controls.Add(new Label { Text = "Nhân viên thu", AutoSize = true }, 0, 5);
        panel.Controls.Add(_lblPaidBy, 1, 5);
        panel.Controls.Add(new Label { Text = "Phương thức thanh toán", AutoSize = true }, 0, 6);
        panel.Controls.Add(_lblMethod, 1, 6);
        panel.Controls.Add(new Label { Text = "Trạng thái", AutoSize = true }, 0, 7);
        panel.Controls.Add(_lblStatus, 1, 7);

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
        panel.Controls.Add(new Label { Text = "Tổng thanh toán", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 5);
        panel.Controls.Add(_lblTotal, 1, 5);

        var btnPrint = CreatePrimaryButton("In bill");
        btnPrint.Click += (_, _) => PrintInvoice();
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
        var header = _invoiceService.GetInvoiceHeader(_invoiceId);
        if (header is null)
        {
            MessageBox.Show("Không tìm thấy hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
            return;
        }

        _lblInvoiceNo.Text = $"# {_invoiceId} - Đặt phòng {header["BookingId"]}";
        _lblCustomer.Text = header["FullName"].ToString();
        _lblContact.Text = $"{header["Phone"]} | {header["Email"]}";
        _lblDates.Text = $"{Convert.ToDateTime(header["CheckInDate"]):dd/MM/yyyy} - {Convert.ToDateTime(header["CheckOutDate"]):dd/MM/yyyy}";
        _lblBookedBy.Text = FormatEmployee(header["BookedByEmployeeId"], header["BookedByEmployeeName"], "Không rõ");
        _lblPaidBy.Text = FormatEmployee(header["PaidByEmployeeId"], header["PaidByEmployeeName"], "Không rõ");
        _lblMethod.Text = MapPaymentMethod(header["PaymentMethod"]?.ToString());
        _lblStatus.Text = MapInvoiceStatus(header["Status"]?.ToString());

        _roomLines = _invoiceService.GetInvoiceRoomLines(_invoiceId);
        _serviceLines = _invoiceService.GetInvoiceServiceLines(_invoiceId);
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
        if (_roomsGrid.Columns["Nights"] is { } nights)
        {
            nights.HeaderText = "Số đêm";
            nights.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            nights.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        if (_roomsGrid.Columns["LineTotal"] is { } roomAmount)
        {
            roomAmount.HeaderText = "Thành tiền";
            roomAmount.DefaultCellStyle.Format = "N0";
            roomAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            roomAmount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

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
        if (_servicesGrid.Columns["Amount"] is { } serviceAmount)
        {
            serviceAmount.HeaderText = "Thành tiền";
            serviceAmount.DefaultCellStyle.Format = "N0";
            serviceAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            serviceAmount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        var roomTotal = SumColumn(_roomLines, "LineTotal");
        var serviceTotal = SumColumn(_serviceLines, "Amount");
        var subtotal = Convert.ToDecimal(header["Subtotal"]);
        var discount = Convert.ToDecimal(header["Discount"]);
        var tax = Convert.ToDecimal(header["Tax"]);
        var total = Convert.ToDecimal(header["Total"]);

        _lblRoomTotal.Text = FormatMoney(roomTotal);
        _lblServiceTotal.Text = FormatMoney(serviceTotal);
        _lblSubtotal.Text = FormatMoney(subtotal);
        _lblDiscount.Text = FormatMoney(discount);
        _lblTax.Text = FormatMoney(tax);
        _lblTotal.Text = FormatMoney(total);
    }

    private void PrintInvoice()
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

        g.DrawString("HÓA ĐƠN THANH TOÁN", titleFont, Brushes.Black, left, top);
        top += lineHeight * 2;

        g.DrawString($"Mã hóa đơn: {_lblInvoiceNo.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Khách hàng: {_lblCustomer.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Liên hệ: {_lblContact.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Thời gian thuê: {_lblDates.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Nhân viên đặt: {_lblBookedBy.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Nhân viên thu: {_lblPaidBy.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Phương thức: {_lblMethod.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Trạng thái: {_lblStatus.Text}", regularFont, Brushes.Black, left, top); top += lineHeight * 2;

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
        g.DrawString($"Tiền phòng: {_lblRoomTotal.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Tiền dịch vụ: {_lblServiceTotal.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Tạm tính: {_lblSubtotal.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Giảm giá: {_lblDiscount.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Thuế: {_lblTax.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Tổng thanh toán: {_lblTotal.Text}", boldFont, Brushes.Black, left, top);
    }

    private static string MapInvoiceStatus(string? status)
    {
        return status switch
        {
            "Paid" => "Đã thanh toán",
            "Unpaid" => "Chưa thanh toán",
            _ => status ?? string.Empty
        };
    }

    private static string MapPaymentMethod(string? method)
    {
        return method switch
        {
            "Cash" => "Tiền mặt",
            "Card" => "Thẻ",
            "Transfer" => "Chuyển khoản",
            _ => method ?? string.Empty
        };
    }

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

    private static decimal SumColumn(DataTable? table, string columnName)
    {
        if (table is null || !table.Columns.Contains(columnName))
        {
            return 0;
        }

        decimal sum = 0;
        foreach (DataRow row in table.Rows)
        {
            if (row[columnName] == DBNull.Value)
            {
                continue;
            }

            sum += Convert.ToDecimal(row[columnName]);
        }
        return sum;
    }

    private static string FormatMoney(decimal amount) => $"{amount:N0} đ";
}
