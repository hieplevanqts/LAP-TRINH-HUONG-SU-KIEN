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
    private readonly Label _lblMethod = new();
    private readonly Label _lblStatus = new();
    private readonly Label _lblSubtotal = new();
    private readonly Label _lblDiscount = new();
    private readonly Label _lblTax = new();
    private readonly Label _lblTotal = new();

    private readonly Guna2DataGridView _grid = new();
    private readonly PrintDocument _printDocument = new();
    private DataTable? _linesTable;

    public InvoiceDetailsForm(int invoiceId)
    {
        _invoiceId = invoiceId;

        Text = "Chi tiết hóa đơn";
        Width = 800;
        Height = 600;
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

        var headerPanel = BuildHeaderPanel();
        var footerPanel = BuildFooterPanel();

        ConfigureGrid(_grid);

        layout.Controls.Add(headerPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(footerPanel, 0, 2);

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

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        panel.Controls.Add(new Label { Text = "Hóa đơn", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        panel.Controls.Add(_lblInvoiceNo, 1, 0);

        panel.Controls.Add(new Label { Text = "Khách hàng", AutoSize = true }, 0, 1);
        panel.Controls.Add(_lblCustomer, 1, 1);

        panel.Controls.Add(new Label { Text = "Liên hệ", AutoSize = true }, 0, 2);
        panel.Controls.Add(_lblContact, 1, 2);

        panel.Controls.Add(new Label { Text = "Ngày ở", AutoSize = true }, 0, 3);
        panel.Controls.Add(_lblDates, 1, 3);

        panel.Controls.Add(new Label { Text = "Thanh toán", AutoSize = true }, 0, 4);
        panel.Controls.Add(_lblMethod, 1, 4);

        panel.Controls.Add(new Label { Text = "Trạng thái", AutoSize = true }, 0, 5);
        panel.Controls.Add(_lblStatus, 1, 5);

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

        panel.Controls.Add(new Label { Text = "Tạm tính", AutoSize = true }, 0, 0);
        panel.Controls.Add(_lblSubtotal, 1, 0);

        panel.Controls.Add(new Label { Text = "Giảm giá", AutoSize = true }, 0, 1);
        panel.Controls.Add(_lblDiscount, 1, 1);

        panel.Controls.Add(new Label { Text = "Thuế", AutoSize = true }, 0, 2);
        panel.Controls.Add(_lblTax, 1, 2);

        panel.Controls.Add(new Label { Text = "Tổng cộng", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 3);
        panel.Controls.Add(_lblTotal, 1, 3);

        var btnPrint = CreatePrimaryButton("In");
        btnPrint.Click += (_, _) => PrintInvoice();
        var btnClose = CreateSecondaryButton("Đóng");
        btnClose.Click += (_, _) => Close();
        panel.Controls.Add(btnPrint, 0, 4);
        panel.Controls.Add(btnClose, 1, 4);

        return panel;
    }

    private void ConfigureGrid(Guna2DataGridView grid)
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
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(231, 240, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(33, 37, 41);
    }

    private static Guna2Button CreateSecondaryButton(string text)
    {
        var button = new Guna2Button
        {
            Text = text,
            Width = 110,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(233, 236, 239),
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
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

        _lblInvoiceNo.Text = $"# {header["InvoiceId"]} - Đặt phòng {header["BookingId"]}";
        _lblCustomer.Text = header["FullName"].ToString();
        _lblContact.Text = $"{header["Phone"]} | {header["Email"]}";
        _lblDates.Text = $"{Convert.ToDateTime(header["CheckInDate"]):d} - {Convert.ToDateTime(header["CheckOutDate"]):d}";
        _lblMethod.Text = $"{MapPaymentMethod(header["PaymentMethod"]?.ToString())} | {Convert.ToDateTime(header["InvoiceDate"]):g}";
        _lblStatus.Text = MapInvoiceStatus(header["Status"]?.ToString());

        _lblSubtotal.Text = Convert.ToDecimal(header["Subtotal"]).ToString("N2");
        _lblDiscount.Text = Convert.ToDecimal(header["Discount"]).ToString("N2");
        _lblTax.Text = Convert.ToDecimal(header["Tax"]).ToString("N2");
        _lblTotal.Text = Convert.ToDecimal(header["Total"]).ToString("N2");

        _linesTable = _invoiceService.GetInvoiceRoomLines(_invoiceId);
        _grid.DataSource = _linesTable;
        if (_grid.Columns["RoomNumber"] is { } roomNumberColumn)
        {
            roomNumberColumn.HeaderText = "Số phòng";
        }
        if (_grid.Columns["TypeName"] is { } typeNameColumn)
        {
            typeNameColumn.HeaderText = "Loại phòng";
        }
        if (_grid.Columns["PricePerNight"] is { } priceColumn)
        {
            priceColumn.HeaderText = "Giá/đêm";
        }
        if (_grid.Columns["Nights"] is { } nightsColumn)
        {
            nightsColumn.HeaderText = "Số đêm";
        }
        if (_grid.Columns["LineTotal"] is { } lineTotalColumn)
        {
            lineTotalColumn.HeaderText = "Thành tiền";
        }
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

        g.DrawString("HÓA ĐƠN", titleFont, Brushes.Black, left, top);
        top += lineHeight * 2;

        g.DrawString($"Mã: {_lblInvoiceNo.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Khách hàng: {_lblCustomer.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Liên hệ: {_lblContact.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Ngày ở: {_lblDates.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Thanh toán: {_lblMethod.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Trạng thái: {_lblStatus.Text}", regularFont, Brushes.Black, left, top); top += lineHeight * 2;

        g.DrawString("CHI TIẾT PHÒNG", boldFont, Brushes.Black, left, top);
        top += lineHeight;

        g.DrawString("Số phòng", boldFont, Brushes.Black, left, top);
        g.DrawString("Loại", boldFont, Brushes.Black, left + 120, top);
        g.DrawString("Giá/đêm", boldFont, Brushes.Black, left + 320, top);
        g.DrawString("Số đêm", boldFont, Brushes.Black, left + 420, top);
        g.DrawString("Thành tiền", boldFont, Brushes.Black, left + 520, top);
        top += lineHeight;

        if (_linesTable is not null)
        {
            foreach (DataRow row in _linesTable.Rows)
            {
                g.DrawString(row["RoomNumber"].ToString(), regularFont, Brushes.Black, left, top);
                g.DrawString(row["TypeName"].ToString(), regularFont, Brushes.Black, left + 120, top);
                g.DrawString(Convert.ToDecimal(row["PricePerNight"]).ToString("N0"), regularFont, Brushes.Black, left + 320, top);
                g.DrawString(row["Nights"].ToString(), regularFont, Brushes.Black, left + 420, top);
                g.DrawString(Convert.ToDecimal(row["LineTotal"]).ToString("N0"), regularFont, Brushes.Black, left + 520, top);
                top += lineHeight;
            }
        }

        top += lineHeight;
        g.DrawString($"Tạm tính: {_lblSubtotal.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Giảm giá: {_lblDiscount.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Thuế: {_lblTax.Text}", regularFont, Brushes.Black, left, top); top += lineHeight;
        g.DrawString($"Tổng cộng: {_lblTotal.Text}", boldFont, Brushes.Black, left, top);
    }

    private static Guna2Button CreatePrimaryButton(string text)
    {
        var button = new Guna2Button
        {
            Text = text,
            Width = 110,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(45, 108, 223),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
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
}
