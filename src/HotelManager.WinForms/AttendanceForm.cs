using System.Data;
using System.Globalization;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class AttendanceForm : Form
{
    private readonly AttendanceService _attendanceService = new();
    private readonly int _employeeId;
    private readonly string _employeeName;
    private readonly DataGridView _grid = new();
    private readonly Guna2ComboBox _cbMonth = new();
    private readonly Guna2ComboBox _cbYear = new();
    private readonly Label _lblSummary = new();
    private readonly CultureInfo _viCulture = new("vi-VN");

    public AttendanceForm(int employeeId, string employeeName)
    {
        _employeeId = employeeId;
        _employeeName = employeeName;

        Text = "Bảng chấm công";
        Width = 980;
        Height = 640;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildFilterBar(), 0, 0);
        root.Controls.Add(BuildGridCard(), 0, 1);
        root.Controls.Add(_lblSummary, 0, 2);
        Controls.Add(root);

        InitializeFilters();
        LoadAttendance();
    }

    private Control BuildFilterBar()
    {
        var panel = new Guna2Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BorderRadius = 12,
            FillColor = Color.White,
            BorderColor = Color.FromArgb(224, 230, 241),
            BorderThickness = 1,
            Padding = new Padding(12)
        };

        var title = new Label
        {
            Text = $"Nhân viên: {_employeeName} (ID: {_employeeId})",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(12, 10)
        };

        ConfigureCombo(_cbMonth, new Point(12, 38), 130);
        ConfigureCombo(_cbYear, new Point(152, 38), 130);

        var btnLoad = new Guna2Button
        {
            Text = "Xem bảng công",
            Location = new Point(292, 38),
            Width = 140,
            Height = 30,
            BorderRadius = 8,
            FillColor = Color.FromArgb(35, 102, 220),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnLoad.Click += (_, _) => LoadAttendance();

        panel.Controls.Add(title);
        panel.Controls.Add(_cbMonth);
        panel.Controls.Add(_cbYear);
        panel.Controls.Add(btnLoad);
        return panel;
    }

    private Control BuildGridCard()
    {
        var card = new Guna2Panel
        {
            Dock = DockStyle.Fill,
            BorderRadius = 12,
            FillColor = Color.White,
            BorderColor = Color.FromArgb(224, 230, 241),
            BorderThickness = 1,
            Padding = new Padding(10)
        };

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;

        card.Controls.Add(_grid);
        return card;
    }

    private static void ConfigureCombo(Guna2ComboBox combo, Point location, int width)
    {
        combo.Location = location;
        combo.Width = width;
        combo.Height = 30;
        combo.BorderRadius = 8;
        combo.BorderColor = Color.FromArgb(207, 216, 230);
        combo.DrawMode = DrawMode.OwnerDrawFixed;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.ItemHeight = 24;
        combo.Font = new Font("Segoe UI", 9F);
    }

    private void InitializeFilters()
    {
        for (var month = 1; month <= 12; month++)
        {
            _cbMonth.Items.Add($"Tháng {month}");
        }

        var currentYear = DateTime.Today.Year;
        for (var year = currentYear - 2; year <= currentYear + 1; year++)
        {
            _cbYear.Items.Add(year.ToString());
        }

        _cbMonth.SelectedIndex = DateTime.Today.Month - 1;
        _cbYear.SelectedItem = currentYear.ToString();
    }

    private void LoadAttendance()
    {
        if (_cbMonth.SelectedIndex < 0 || _cbYear.SelectedItem is null)
        {
            return;
        }

        var month = _cbMonth.SelectedIndex + 1;
        var year = int.Parse(_cbYear.SelectedItem.ToString()!);
        var raw = _attendanceService.GetMonthlyAttendance(_employeeId, year, month);

        var view = BuildViewTable(raw);
        _grid.DataSource = view;

        var presentDays = view.AsEnumerable().Count(r => !string.IsNullOrWhiteSpace(r.Field<string>("Check-in")));
        var totalHours = view.AsEnumerable()
            .Where(r => decimal.TryParse(r.Field<string>("Tổng giờ"), out _))
            .Sum(r =>
            {
                _ = decimal.TryParse(r.Field<string>("Tổng giờ"), out var hours);
                return hours;
            });

        _lblSummary.Text = $"Tháng {month}/{year}: Đi làm {presentDays} ngày | Tổng giờ làm: {totalHours:N2} giờ";
        _lblSummary.AutoSize = true;
        _lblSummary.ForeColor = Color.FromArgb(62, 70, 84);
        _lblSummary.Padding = new Padding(4, 8, 0, 0);
    }

    private DataTable BuildViewTable(DataTable raw)
    {
        var table = new DataTable();
        table.Columns.Add("Ngày");
        table.Columns.Add("Thứ");
        table.Columns.Add("Check-in");
        table.Columns.Add("Check-out");
        table.Columns.Add("Tổng giờ");
        table.Columns.Add("Chi tiết");

        foreach (DataRow row in raw.Rows)
        {
            var workDate = row.Field<DateTime>("WorkDate");
            var checkIn = row["CheckInTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CheckInTime"]);
            var checkOut = row["CheckOutTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CheckOutTime"]);
            var hours = row["WorkHours"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["WorkHours"]);
            var note = row["Note"] == DBNull.Value ? string.Empty : row["Note"].ToString() ?? string.Empty;

            var detail = string.IsNullOrWhiteSpace(note) ? "Không có ghi chú" : note;
            if (checkIn is null && checkOut is null)
            {
                detail = "Chưa có dữ liệu chấm công";
            }

            table.Rows.Add(
                workDate.ToString("dd/MM/yyyy"),
                _viCulture.TextInfo.ToTitleCase(workDate.ToString("dddd", _viCulture)),
                checkIn?.ToString("HH:mm") ?? string.Empty,
                checkOut?.ToString("HH:mm") ?? string.Empty,
                hours?.ToString("N2") ?? string.Empty,
                detail);
        }

        return table;
    }
}
