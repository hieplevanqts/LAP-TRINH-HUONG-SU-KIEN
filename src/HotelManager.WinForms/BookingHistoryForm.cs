using System.Data;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class BookingHistoryForm : Form
{
    private readonly BookingService _bookingService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2DateTimePicker _dtFrom = new();
    private readonly Guna2DateTimePicker _dtTo = new();
    private readonly Guna2ComboBox _cbStatus = new();

    public BookingHistoryForm()
    {
        Text = "Lịch sử đặt phòng";
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
                e.Value = MapBookingStatus(e.Value?.ToString());
                e.FormattingApplied = true;
            }
        };

        layout.Controls.Add(filterPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            _dtFrom.Value = DateTime.Today.AddDays(-30);
            _dtTo.Value = DateTime.Today;
            _cbStatus.SelectedIndex = 0;
            LoadHistory();
        };
    }

    private Control BuildFilterPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(10)
        };

        StyleDatePicker(_dtFrom);
        StyleDatePicker(_dtTo);

        StyleComboBox(_cbStatus);
        _cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbStatus.Items.AddRange(
        [
            new StatusOption("All", "Tất cả"),
            new StatusOption("Pending", "Chờ"),
            new StatusOption("Paid", "Đã thanh toán")
        ]);

        var btnFilter = CreatePrimaryButton("Lọc", 100);
        btnFilter.Click += (_, _) => LoadHistory();

        panel.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(_dtFrom);
        panel.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        panel.Controls.Add(_dtTo);
        panel.Controls.Add(new Label { Text = "Trạng thái", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        panel.Controls.Add(_cbStatus);
        panel.Controls.Add(btnFilter);

        return panel;
    }

    private void LoadHistory()
    {
        if (_dtTo.Value.Date < _dtFrom.Value.Date)
        {
            MessageBox.Show("Ngày đến phải >= ngày từ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var status = _cbStatus.SelectedItem is StatusOption option ? option.Value : "All";
        _grid.DataSource = _bookingService.GetBookingHistory(_dtFrom.Value, _dtTo.Value, status);
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
        if (_grid.Columns["Adults"] is { } adultsColumn)
        {
            adultsColumn.HeaderText = "Người lớn";
        }
        if (_grid.Columns["Children"] is { } childrenColumn)
        {
            childrenColumn.HeaderText = "Trẻ em";
        }
        if (_grid.Columns["Status"] is { } statusColumn)
        {
            statusColumn.HeaderText = "Trạng thái";
        }
        if (_grid.Columns["CreatedAt"] is { } createdAtColumn)
        {
            createdAtColumn.HeaderText = "Ngày tạo";
        }
    }

    public void RefreshData()
    {
        LoadHistory();
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

    private sealed class StatusOption
    {
        public StatusOption(string value, string display)
        {
            Value = value;
            Display = display;
        }

        public string Value { get; }
        public string Display { get; }

        public override string ToString() => Display;
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
}
