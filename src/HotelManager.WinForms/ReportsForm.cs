using System.Data;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class ReportsForm : Form
{
    private readonly ReportService _reportService = new();
    private readonly Guna2DataGridView _dailyGrid = new();
    private readonly Guna2DataGridView _monthlyGrid = new();
    private readonly Guna2DateTimePicker _dtFrom = new();
    private readonly Guna2DateTimePicker _dtTo = new();

    public ReportsForm()
    {
        Text = "Báo cáo doanh thu";
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
        var tabs = BuildTabs();

        layout.Controls.Add(filterPanel, 0, 0);
        layout.Controls.Add(tabs, 0, 1);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            _dtFrom.Value = DateTime.Today.AddDays(-30);
            _dtTo.Value = DateTime.Today;
            LoadReports();
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

        var btnFilter = CreatePrimaryButton("Lọc", 100);
        btnFilter.Click += (_, _) => LoadReports();

        panel.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(_dtFrom);
        panel.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        panel.Controls.Add(_dtTo);
        panel.Controls.Add(btnFilter);

        return panel;
    }

    private Control BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };

        var dailyTab = new TabPage("Theo ngày") { Padding = new Padding(10) };
        var monthlyTab = new TabPage("Theo tháng") { Padding = new Padding(10) };

        ConfigureGrid(_dailyGrid);
        ConfigureGrid(_monthlyGrid);

        dailyTab.Controls.Add(_dailyGrid);
        monthlyTab.Controls.Add(_monthlyGrid);

        tabs.TabPages.Add(dailyTab);
        tabs.TabPages.Add(monthlyTab);

        return tabs;
    }

    private void LoadReports()
    {
        if (_dtTo.Value.Date < _dtFrom.Value.Date)
        {
            MessageBox.Show("Ngày đến phải >= ngày từ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _dailyGrid.DataSource = _reportService.GetDailyRevenue(_dtFrom.Value, _dtTo.Value);
        _monthlyGrid.DataSource = _reportService.GetMonthlyRevenue(_dtFrom.Value, _dtTo.Value);

        if (_dailyGrid.Columns["RevenueDate"] is { } dailyDateColumn)
        {
            dailyDateColumn.HeaderText = "Ngày";
        }
        if (_dailyGrid.Columns["TotalRevenue"] is { } dailyTotalColumn)
        {
            dailyTotalColumn.HeaderText = "Tổng doanh thu";
        }

        if (_monthlyGrid.Columns["RevenueMonth"] is { } monthColumn)
        {
            monthColumn.HeaderText = "Tháng";
        }
        if (_monthlyGrid.Columns["TotalRevenue"] is { } monthTotalColumn)
        {
            monthTotalColumn.HeaderText = "Tổng doanh thu";
        }
    }

    public void RefreshData()
    {
        LoadReports();
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
}
