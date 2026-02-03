using System.Data;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class InvoicesForm : Form
{
    private readonly InvoiceService _invoiceService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2DateTimePicker _dtFrom = new();
    private readonly Guna2DateTimePicker _dtTo = new();
    private readonly Guna2Button _btnDetails = new() { Text = "Xem chi tiết", Width = 120 };

    public InvoicesForm()
    {
        Text = "Hóa đơn";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filterPanel = BuildFilterPanel();

        ConfigureGrid(_grid);
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns["Status"] is { } statusColumn && e.ColumnIndex == statusColumn.Index)
            {
                e.Value = MapInvoiceStatus(e.Value?.ToString());
                e.FormattingApplied = true;
                return;
            }

            if (_grid.Columns["PaymentMethod"] is { } methodColumn && e.ColumnIndex == methodColumn.Index)
            {
                e.Value = MapPaymentMethod(e.Value?.ToString());
                e.FormattingApplied = true;
            }
        };
        _grid.CellDoubleClick += (_, _) => ShowDetails();

        layout.Controls.Add(filterPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            _dtFrom.Value = DateTime.Today.AddDays(-30);
            _dtTo.Value = DateTime.Today;
            LoadInvoices();
        };
    }

    private Control BuildFilterPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(10)
        };

        StyleDatePicker(_dtFrom);
        StyleDatePicker(_dtTo);

        var btnFilter = CreatePrimaryButton("Lọc", 100);
        btnFilter.Click += (_, _) => LoadInvoices();
        _btnDetails.Click += (_, _) => ShowDetails();
        StyleSecondaryButton(_btnDetails);

        panel.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(_dtFrom);
        panel.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        panel.Controls.Add(_dtTo);
        panel.Controls.Add(btnFilter);
        panel.Controls.Add(_btnDetails);

        return panel;
    }

    private void LoadInvoices()
    {
        if (_dtTo.Value.Date < _dtFrom.Value.Date)
        {
            MessageBox.Show("Ngày đến phải >= ngày từ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _grid.DataSource = _invoiceService.GetInvoices(_dtFrom.Value, _dtTo.Value);
        if (_grid.Columns["InvoiceId"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_grid.Columns["BookingId"] is { } bookingIdColumn)
        {
            bookingIdColumn.Visible = false;
        }
        if (_grid.Columns["FullName"] is { } fullNameColumn)
        {
            fullNameColumn.HeaderText = "Khách hàng";
        }
        if (_grid.Columns["InvoiceDate"] is { } invoiceDateColumn)
        {
            invoiceDateColumn.HeaderText = "Ngày hóa đơn";
        }
        if (_grid.Columns["Subtotal"] is { } subtotalColumn)
        {
            subtotalColumn.HeaderText = "Tạm tính";
        }
        if (_grid.Columns["Discount"] is { } discountColumn)
        {
            discountColumn.HeaderText = "Giảm giá";
        }
        if (_grid.Columns["Tax"] is { } taxColumn)
        {
            taxColumn.HeaderText = "Thuế";
        }
        if (_grid.Columns["Total"] is { } totalColumn)
        {
            totalColumn.HeaderText = "Tổng cộng";
        }
        if (_grid.Columns["Status"] is { } statusColumn)
        {
            statusColumn.HeaderText = "Trạng thái";
        }
        if (_grid.Columns["PaymentMethod"] is { } paymentMethodColumn)
        {
            paymentMethodColumn.HeaderText = "Phương thức";
        }
        if (_grid.Columns["PaidAt"] is { } paidAtColumn)
        {
            paidAtColumn.HeaderText = "Thời điểm thanh toán";
        }
    }

    public void RefreshData()
    {
        LoadInvoices();
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

    private static void StyleDatePicker(Guna2DateTimePicker picker)
    {
        picker.BorderRadius = 10;
        picker.BorderThickness = 1;
        picker.BorderColor = Color.FromArgb(217, 221, 230);
        picker.FillColor = Color.White;
        picker.ForeColor = Color.FromArgb(33, 37, 41);
        picker.Font = new Font("Segoe UI", 9F);
        picker.Format = DateTimePickerFormat.Custom;
        picker.CustomFormat = "dd/MM/yyyy";
        picker.Height = 36;
    }

    private static Guna2Button CreatePrimaryButton(string text, int width)
    {
        var button = new Guna2Button
        {
            Text = text,
            Width = width,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(45, 108, 223),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
    }

    private static void StyleSecondaryButton(Guna2Button button)
    {
        button.Height = 36;
        button.BorderRadius = 8;
        button.FillColor = Color.FromArgb(233, 236, 239);
        button.ForeColor = Color.FromArgb(33, 37, 41);
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }

    private void ShowDetails()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView rowView)
        {
            MessageBox.Show("Chọn hóa đơn để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var invoiceId = Convert.ToInt32(rowView.Row["InvoiceId"]);
        using var form = new InvoiceDetailsForm(invoiceId);
        form.ShowDialog(this);
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
