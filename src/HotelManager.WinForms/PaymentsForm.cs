using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class PaymentsForm : Form
{
    public event EventHandler? PaymentCompleted;

    private readonly PaymentService _paymentService = new();
    private readonly ServiceService _serviceService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2ComboBox _cbService = new();
    private readonly Guna2NumericUpDown _numServiceQty = new();
    private readonly ListBox _selectedServicesList = new();
    private readonly Guna2NumericUpDown _numDiscount = new();
    private readonly Guna2NumericUpDown _numTax = new();
    private readonly Guna2ComboBox _cbMethod = new();
    private readonly Guna2TextBox _txtNote = new();
    private readonly Label _lblSubtotal = new();
    private readonly Label _lblTotal = new();
    private int? _selectedBookingId;
    private decimal _currentSubtotal;

    public PaymentsForm()
    {
        Text = "Thanh toán";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        ConfigureGrid(_grid);
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns["Status"] is { } statusColumn && e.ColumnIndex == statusColumn.Index)
            {
                e.Value = MapBookingStatus(e.Value?.ToString());
                e.FormattingApplied = true;
            }
        };
        _grid.CellClick += (_, _) => LoadSelectedBooking();

        var servicesPanel = BuildServicesPanel();
        var inputPanel = BuildInputPanel();
        var buttonPanel = BuildButtonPanel();

        layout.Controls.Add(_grid, 0, 0);
        layout.Controls.Add(servicesPanel, 0, 1);
        layout.Controls.Add(inputPanel, 0, 2);
        layout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            LoadServiceOptions();
            LoadBookings();
        };
    }

    public void RefreshData()
    {
        LoadBookings(_selectedBookingId);
    }

    private Control BuildServicesPanel()
    {
        var panel = new Guna2GroupBox
        {
            Text = "Dịch vụ phát sinh",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var topRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 6)
        };

        StyleComboBox(_cbService);
        _cbService.Width = 280;

        StyleNumeric(_numServiceQty);
        _numServiceQty.Width = 120;
        _numServiceQty.Minimum = 1;
        _numServiceQty.Maximum = 1000;
        _numServiceQty.Value = 1;

        var btnAddService = CreateSecondaryButton("Thêm dịch vụ");
        btnAddService.Width = 140;
        btnAddService.Click += (_, _) => AddServiceToSelectedBooking();

        topRow.Controls.Add(new Label { Text = "Dịch vụ", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        topRow.Controls.Add(_cbService);
        topRow.Controls.Add(new Label { Text = "Số lượng", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
        topRow.Controls.Add(_numServiceQty);
        topRow.Controls.Add(btnAddService);

        _selectedServicesList.Dock = DockStyle.Fill;
        _selectedServicesList.BorderStyle = BorderStyle.None;

        container.Controls.Add(topRow, 0, 0);
        container.Controls.Add(_selectedServicesList, 0, 1);
        panel.Controls.Add(container);

        return panel;
    }

    private void LoadServiceOptions()
    {
        var data = _serviceService.GetServices(true);
        var options = new List<ServiceOption>();
        foreach (DataRow row in data.Rows)
        {
            options.Add(new ServiceOption(
                Convert.ToInt32(row["ServiceId"]),
                row["ServiceName"].ToString() ?? string.Empty,
                Convert.ToDecimal(row["UnitPrice"])
            ));
        }

        _cbService.DataSource = options;
        _cbService.DisplayMember = nameof(ServiceOption.Display);
        _cbService.ValueMember = nameof(ServiceOption.ServiceId);

        if (_cbService.Items.Count > 0)
        {
            _cbService.SelectedIndex = 0;
        }
    }

    private Control BuildInputPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            AutoSize = true,
            Padding = new Padding(10)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        panel.Controls.Add(new Label { Text = "Giảm giá", AutoSize = true }, 0, 0);
        StyleNumeric(_numDiscount);
        _numDiscount.Dock = DockStyle.Fill;
        panel.Controls.Add(_numDiscount, 1, 0);
        panel.Controls.Add(new Label { Text = "Thuế", AutoSize = true }, 2, 0);
        StyleNumeric(_numTax);
        _numTax.Dock = DockStyle.Fill;
        panel.Controls.Add(_numTax, 3, 0);

        panel.Controls.Add(new Label { Text = "Phương thức", AutoSize = true }, 0, 1);
        StyleComboBox(_cbMethod);
        _cbMethod.Dock = DockStyle.Fill;
        panel.Controls.Add(_cbMethod, 1, 1);
        panel.Controls.Add(new Label { Text = "Ghi chú", AutoSize = true }, 2, 1);
        StyleTextBox(_txtNote, "Ghi chú");
        _txtNote.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtNote, 3, 1);

        panel.Controls.Add(new Label { Text = "Tạm tính", AutoSize = true }, 0, 2);
        panel.Controls.Add(_lblSubtotal, 1, 2);
        panel.Controls.Add(new Label { Text = "Tổng cộng", AutoSize = true }, 2, 2);
        panel.Controls.Add(_lblTotal, 3, 2);

        _numDiscount.Minimum = 0;
        _numDiscount.Maximum = 1000000000;
        _numDiscount.DecimalPlaces = 2;
        _numDiscount.ThousandsSeparator = true;
        _numDiscount.ValueChanged += (_, _) => UpdateTotals();

        _numTax.Minimum = 0;
        _numTax.Maximum = 1000000000;
        _numTax.DecimalPlaces = 2;
        _numTax.ThousandsSeparator = true;
        _numTax.ValueChanged += (_, _) => UpdateTotals();

        _cbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbMethod.Items.AddRange(
        [
            new PaymentMethodOption("Cash", "Tiền mặt"),
            new PaymentMethodOption("Card", "Thẻ"),
            new PaymentMethodOption("Transfer", "Chuyển khoản")
        ]);
        _cbMethod.SelectedIndex = 0;

        _lblSubtotal.AutoSize = true;
        _lblTotal.AutoSize = true;

        return panel;
    }

    private Control BuildButtonPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(10),
            AutoSize = true
        };

        var btnPay = CreatePrimaryButton("Thanh toán");
        var btnRefresh = CreateSecondaryButton("Tải lại");
        var btnInfo = CreateSecondaryButton("Xem thông tin thanh toán");
        btnInfo.Width = 200;

        btnPay.Click += (_, _) => Pay();
        btnRefresh.Click += (_, _) => LoadBookings(_selectedBookingId);
        btnInfo.Click += (_, _) => new PaymentInfoForm().ShowDialog(this);

        panel.Controls.Add(btnPay);
        panel.Controls.Add(btnRefresh);
        panel.Controls.Add(btnInfo);

        return panel;
    }

    private void LoadBookings(int? bookingIdToSelect = null)
    {
        _grid.DataSource = _paymentService.GetBookingsForPayment();
        if (_grid.Columns["BookingId"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_grid.Columns["FullName"] is { } fullNameColumn)
        {
            fullNameColumn.HeaderText = "Khách hàng";
        }
        if (_grid.Columns["CheckInDate"] is { } checkInColumn)
        {
            checkInColumn.HeaderText = "Nhận phòng";
        }
        if (_grid.Columns["CheckOutDate"] is { } checkOutColumn)
        {
            checkOutColumn.HeaderText = "Trả phòng";
        }
        if (_grid.Columns["Status"] is { } statusColumn)
        {
            statusColumn.HeaderText = "Trạng thái";
        }
        if (_grid.Columns["Subtotal"] is { } subtotalColumn)
        {
            subtotalColumn.HeaderText = "Tạm tính";
        }

        if (bookingIdToSelect.HasValue && TrySelectBooking(bookingIdToSelect.Value))
        {
            return;
        }

        _selectedBookingId = null;
        _currentSubtotal = 0;
        _numDiscount.Value = 0;
        _numTax.Value = 0;
        _selectedServicesList.Items.Clear();
        UpdateTotals();
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

    private static void StyleComboBox(Guna2ComboBox comboBox)
    {
        comboBox.BorderRadius = 8;
        comboBox.BorderThickness = 1;
        comboBox.BorderColor = Color.FromArgb(217, 221, 230);
        comboBox.FillColor = Color.White;
        comboBox.Font = new Font("Segoe UI", 9F);
        comboBox.Height = 34;
    }

    private static void StyleTextBox(Guna2TextBox textBox, string placeholder)
    {
        textBox.PlaceholderText = placeholder;
        textBox.BorderRadius = 8;
        textBox.BorderThickness = 1;
        textBox.BorderColor = Color.FromArgb(217, 221, 230);
        textBox.FillColor = Color.White;
        textBox.Font = new Font("Segoe UI", 9F);
        textBox.Height = 34;
    }

    private static void StyleNumeric(Guna2NumericUpDown numeric)
    {
        numeric.BorderRadius = 8;
        numeric.BorderThickness = 1;
        numeric.BorderColor = Color.FromArgb(217, 221, 230);
        numeric.FillColor = Color.White;
        numeric.Font = new Font("Segoe UI", 9F);
        numeric.Height = 34;
        numeric.UpDownButtonFillColor = Color.FromArgb(45, 108, 223);
        numeric.UpDownButtonForeColor = Color.White;
    }

    private static Guna2Button CreatePrimaryButton(string text)
    {
        var button = new Guna2Button
        {
            Text = text,
            Width = 120,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(45, 108, 223),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
    }

    private static Guna2Button CreateSecondaryButton(string text)
    {
        var button = new Guna2Button
        {
            Text = text,
            Width = 120,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(233, 236, 239),
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
    }

    private void LoadSelectedBooking()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView rowView)
        {
            var row = rowView.Row;
            _selectedBookingId = Convert.ToInt32(row["BookingId"]);
            _currentSubtotal = Convert.ToDecimal(row["Subtotal"]);
            LoadServiceUsages(_selectedBookingId.Value);
            UpdateTotals();
        }
    }

    private bool TrySelectBooking(int bookingId)
    {
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not DataRowView rowView)
            {
                continue;
            }

            if (Convert.ToInt32(rowView.Row["BookingId"]) != bookingId)
            {
                continue;
            }

            row.Selected = true;
            _grid.CurrentCell = row.Cells
                .Cast<DataGridViewCell>()
                .FirstOrDefault(cell => cell.Visible && (cell.OwningColumn?.Visible ?? false));
            _selectedBookingId = bookingId;
            _currentSubtotal = Convert.ToDecimal(rowView.Row["Subtotal"]);
            LoadServiceUsages(bookingId);
            UpdateTotals();
            return true;
        }

        return false;
    }

    private void LoadServiceUsages(int bookingId)
    {
        _selectedServicesList.Items.Clear();
        var data = _paymentService.GetBookingServiceUsages(bookingId);
        foreach (DataRow row in data.Rows)
        {
            var serviceName = row["ServiceName"].ToString() ?? string.Empty;
            var quantity = Convert.ToInt32(row["Quantity"]);
            var amount = Convert.ToDecimal(row["Amount"]);
            _selectedServicesList.Items.Add($"{serviceName} x {quantity} - {amount:N0}");
        }
    }

    private void AddServiceToSelectedBooking()
    {
        if (_selectedBookingId is null)
        {
            MessageBox.Show("Chọn đặt phòng trước khi thêm dịch vụ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_cbService.SelectedItem is not ServiceOption serviceOption)
        {
            MessageBox.Show("Không có dịch vụ khả dụng để thêm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var quantity = (int)_numServiceQty.Value;
        if (quantity <= 0)
        {
            MessageBox.Show("Số lượng phải lớn hơn 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            _paymentService.AddServiceUsage(_selectedBookingId.Value, serviceOption.ServiceId, quantity);
            LoadBookings(_selectedBookingId);
            MessageBox.Show("Đã thêm dịch vụ vào danh sách của khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể thêm dịch vụ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateTotals()
    {
        var total = _currentSubtotal - _numDiscount.Value + _numTax.Value;
        if (total < 0)
        {
            total = 0;
        }

        _lblSubtotal.Text = _currentSubtotal.ToString("N2");
        _lblTotal.Text = total.ToString("N2");
    }

    private void Pay()
    {
        if (_selectedBookingId is null)
        {
            MessageBox.Show("Chọn đặt phòng cần thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            _paymentService.PayBooking(
                _selectedBookingId.Value,
                _numDiscount.Value,
                _numTax.Value,
                _cbMethod.SelectedItem is PaymentMethodOption option ? option.Value : "Cash",
                _txtNote.Text.Trim()
            );

            MessageBox.Show("Thanh toán thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBookings();
            PaymentCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Thanh toán thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string MapBookingStatus(string? status)
    {
        return status switch
        {
            "Pending" => "Chờ",
            "Paid" => "Đã thanh toán",
            _ => status ?? string.Empty
        };
    }

    private sealed class ServiceOption
    {
        public ServiceOption(int serviceId, string name, decimal price)
        {
            ServiceId = serviceId;
            Name = name;
            Price = price;
        }

        public int ServiceId { get; }
        public string Name { get; }
        public decimal Price { get; }

        public string Display => $"{Name} ({Price:N0})";
    }

    private sealed class PaymentMethodOption
    {
        public PaymentMethodOption(string value, string display)
        {
            Value = value;
            Display = display;
        }

        public string Value { get; }
        public string Display { get; }

        public override string ToString() => Display;
    }
}
