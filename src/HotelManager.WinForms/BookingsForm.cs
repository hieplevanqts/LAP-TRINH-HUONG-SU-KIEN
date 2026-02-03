using System.Data;
using System.Drawing;
using System.Collections.Generic;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class BookingsForm : Form
{
    private readonly BookingService _bookingService = new();
    private readonly ServiceService _serviceService = new();
    private readonly Guna2ComboBox _cbCustomer = new();
    private readonly Guna2DateTimePicker _dtCheckIn = new();
    private readonly Guna2DateTimePicker _dtCheckOut = new();
    private readonly Guna2NumericUpDown _numAdults = new();
    private readonly Guna2NumericUpDown _numChildren = new();
    private readonly CheckedListBox _roomsList = new();
    private readonly Guna2CheckBox _chkOnlyAvailable = new() { Text = "Chỉ hiện phòng trống", Checked = true, AutoSize = true };
    private readonly Guna2ComboBox _cbService = new();
    private readonly Guna2NumericUpDown _numServiceQty = new();
    private readonly ListBox _selectedServicesList = new();
    private readonly Dictionary<int, ServiceSelection> _selectedServices = new();

    public BookingsForm()
    {
        Text = "Đặt phòng";
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
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var inputPanel = BuildInputPanel();
        var roomsPanel = BuildRoomsPanel();
        var servicesPanel = BuildServicesPanel();
        var buttonPanel = BuildButtonPanel();

        layout.Controls.Add(inputPanel, 0, 0);
        layout.Controls.Add(roomsPanel, 0, 1);
        layout.Controls.Add(servicesPanel, 0, 2);
        layout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            LoadCustomers();
            LoadRooms();
            LoadServices();
        };
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

        panel.Controls.Add(new Label { Text = "Khách hàng", AutoSize = true }, 0, 0);
        StyleComboBox(_cbCustomer);
        _cbCustomer.Dock = DockStyle.Fill;
        panel.Controls.Add(_cbCustomer, 1, 0);
        panel.Controls.Add(new Label { Text = "Nhận phòng", AutoSize = true }, 2, 0);
        StyleDatePicker(_dtCheckIn);
        _dtCheckIn.Dock = DockStyle.Fill;
        panel.Controls.Add(_dtCheckIn, 3, 0);

        panel.Controls.Add(new Label { Text = "Trả phòng", AutoSize = true }, 0, 1);
        StyleDatePicker(_dtCheckOut);
        _dtCheckOut.Dock = DockStyle.Fill;
        panel.Controls.Add(_dtCheckOut, 1, 1);
        panel.Controls.Add(new Label { Text = "Người lớn", AutoSize = true }, 2, 1);
        StyleNumeric(_numAdults);
        _numAdults.Dock = DockStyle.Fill;
        panel.Controls.Add(_numAdults, 3, 1);

        panel.Controls.Add(new Label { Text = "Trẻ em", AutoSize = true }, 0, 2);
        StyleNumeric(_numChildren);
        _numChildren.Dock = DockStyle.Fill;
        panel.Controls.Add(_numChildren, 1, 2);

        _cbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbCustomer.Width = 250;

        _dtCheckIn.Format = DateTimePickerFormat.Short;
        _dtCheckOut.Format = DateTimePickerFormat.Short;
        _dtCheckIn.Value = DateTime.Today;
        _dtCheckOut.Value = DateTime.Today.AddDays(1);

        _numAdults.Minimum = 1;
        _numAdults.Maximum = 20;
        _numAdults.Value = 1;

        _numChildren.Minimum = 0;
        _numChildren.Maximum = 20;

        return panel;
    }

    private Control BuildRoomsPanel()
    {
        var panel = new Guna2GroupBox
        {
            Text = "Chọn phòng",
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

        _chkOnlyAvailable.CheckedChanged += (_, _) => LoadRooms();

        _roomsList.Dock = DockStyle.Fill;
        _roomsList.CheckOnClick = true;
        _roomsList.BorderStyle = BorderStyle.None;
        _roomsList.BackColor = Color.White;

        container.Controls.Add(_chkOnlyAvailable, 0, 0);
        container.Controls.Add(_roomsList, 0, 1);
        panel.Controls.Add(container);

        return panel;
    }

    private Control BuildServicesPanel()
    {
        var panel = new Guna2GroupBox
        {
            Text = "Chọn dịch vụ",
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

        var btnAdd = CreatePrimaryButton("Thêm dịch vụ");
        btnAdd.Width = 140;
        btnAdd.Click += (_, _) => AddServiceSelection();

        var btnRemove = CreateSecondaryButton("Xóa");
        btnRemove.Width = 80;
        btnRemove.Click += (_, _) => RemoveServiceSelection();

        topRow.Controls.Add(new Label { Text = "Dịch vụ", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        topRow.Controls.Add(_cbService);
        topRow.Controls.Add(new Label { Text = "Số lượng", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
        topRow.Controls.Add(_numServiceQty);
        topRow.Controls.Add(btnAdd);
        topRow.Controls.Add(btnRemove);

        _selectedServicesList.Dock = DockStyle.Fill;

        container.Controls.Add(topRow, 0, 0);
        container.Controls.Add(_selectedServicesList, 0, 1);
        panel.Controls.Add(container);
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

        var btnCreate = CreatePrimaryButton("Đặt phòng");
        var btnRefresh = CreateSecondaryButton("Tải lại");

        btnCreate.Click += (_, _) => CreateBooking();
        btnRefresh.Click += (_, _) =>
        {
            LoadCustomers();
            LoadRooms();
        };

        panel.Controls.Add(btnCreate);
        panel.Controls.Add(btnRefresh);

        return panel;
    }

    private void LoadCustomers()
    {
        DataTable customers = _bookingService.GetCustomers();
        _cbCustomer.DataSource = customers;
        _cbCustomer.DisplayMember = "FullName";
        _cbCustomer.ValueMember = "CustomerId";
        if (_cbCustomer.Items.Count > 0)
        {
            _cbCustomer.SelectedIndex = 0;
        }
    }

    private void LoadRooms()
    {
        _roomsList.Items.Clear();
        DataTable rooms = _bookingService.GetRooms(_chkOnlyAvailable.Checked);
        foreach (DataRow row in rooms.Rows)
        {
            var roomId = Convert.ToInt32(row["RoomId"]);
            var roomNumber = row["RoomNumber"].ToString();
            var roomType = row["RoomType"].ToString();
            var floor = row["Floor"];
            var status = row["Status"].ToString();
            _roomsList.Items.Add(new RoomItem(roomId, $"{roomNumber} - {roomType} (Tầng {floor}) - {MapRoomStatus(status)}"));
        }
    }

    private void LoadServices()
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

    private void AddServiceSelection()
    {
        if (_cbService.SelectedItem is not ServiceOption option)
        {
            return;
        }

        var qty = (int)_numServiceQty.Value;
        if (_selectedServices.TryGetValue(option.ServiceId, out var existing))
        {
            _selectedServices[option.ServiceId] = existing with { Quantity = existing.Quantity + qty };
        }
        else
        {
            _selectedServices[option.ServiceId] = new ServiceSelection(option.ServiceId, qty);
        }

        RefreshSelectedServicesList();
    }

    private void RemoveServiceSelection()
    {
        if (_selectedServicesList.SelectedItem is not ServiceDisplay display)
        {
            return;
        }

        _selectedServices.Remove(display.ServiceId);
        RefreshSelectedServicesList();
    }

    private void RefreshSelectedServicesList()
    {
        _selectedServicesList.Items.Clear();
        if (_cbService.DataSource is not List<ServiceOption> options)
        {
            return;
        }

        foreach (var entry in _selectedServices)
        {
            var option = options.Find(o => o.ServiceId == entry.Key);
            var name = option?.Name ?? $"#{entry.Key}";
            _selectedServicesList.Items.Add(new ServiceDisplay(entry.Key, $"{name} x {entry.Value.Quantity}"));
        }
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

    private void CreateBooking()
    {
        if (_cbCustomer.SelectedValue is not int customerId)
        {
            MessageBox.Show("Chọn khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_dtCheckOut.Value.Date <= _dtCheckIn.Value.Date)
        {
            MessageBox.Show("Ngày trả phòng phải sau ngày nhận phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedRoomIds = _roomsList.CheckedItems
            .OfType<RoomItem>()
            .Select(item => item.RoomId)
            .ToList();

        if (selectedRoomIds.Count == 0)
        {
            MessageBox.Show("Chọn ít nhất 1 phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var services = new List<ServiceSelection>(_selectedServices.Values);
            _bookingService.CreateBookingWithRoomsAndServices(
                customerId,
                _dtCheckIn.Value,
                _dtCheckOut.Value,
                (int)_numAdults.Value,
                (int)_numChildren.Value,
                null,
                selectedRoomIds,
                services
            );

            MessageBox.Show("Đặt phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadRooms();
            _selectedServices.Clear();
            RefreshSelectedServicesList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đặt phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class RoomItem
    {
        public RoomItem(int roomId, string display)
        {
            RoomId = roomId;
            Display = display;
        }

        public int RoomId { get; }
        public string Display { get; }

        public override string ToString() => Display;
    }

    private static string MapRoomStatus(string? status)
    {
        return status switch
        {
            "Available" => "Trống",
            "Occupied" => "Đang ở",
            "Maintenance" => "Bảo trì",
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

    private sealed class ServiceDisplay
    {
        public ServiceDisplay(int serviceId, string display)
        {
            ServiceId = serviceId;
            Display = display;
        }

        public int ServiceId { get; }
        public string Display { get; }

        public override string ToString() => Display;
    }
}
