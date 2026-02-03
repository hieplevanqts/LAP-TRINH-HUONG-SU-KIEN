using System.Data;
using System.Drawing;
using System.Linq;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class RoomsForm : Form
{
    private readonly RoomService _roomService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2TextBox _txtRoomNumber = new();
    private readonly Guna2ComboBox _cbRoomType = new();
    private readonly Guna2NumericUpDown _numFloor = new();
    private readonly Guna2ComboBox _cbStatus = new();
    private int? _selectedRoomId;

    public RoomsForm()
    {
        Text = "Quản lý phòng";
        Width = 900;
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

        var inputPanel = BuildInputPanel();
        var buttonPanel = BuildButtonPanel();

        ConfigureGrid(_grid);
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns["Status"] is { } statusColumn && e.ColumnIndex == statusColumn.Index)
            {
                e.Value = MapRoomStatus(e.Value?.ToString());
                e.FormattingApplied = true;
            }
        };
        _grid.CellClick += (_, _) => LoadSelectedRoom();

        layout.Controls.Add(inputPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            LoadRoomTypes();
            LoadRooms();
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

        panel.Controls.Add(new Label { Text = "Số phòng", AutoSize = true }, 0, 0);
        StyleTextBox(_txtRoomNumber, "101");
        _txtRoomNumber.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtRoomNumber, 1, 0);
        panel.Controls.Add(new Label { Text = "Loại phòng", AutoSize = true }, 2, 0);
        StyleComboBox(_cbRoomType);
        _cbRoomType.Dock = DockStyle.Fill;
        panel.Controls.Add(_cbRoomType, 3, 0);

        panel.Controls.Add(new Label { Text = "Tầng", AutoSize = true }, 0, 1);
        StyleNumeric(_numFloor);
        _numFloor.Dock = DockStyle.Fill;
        panel.Controls.Add(_numFloor, 1, 1);
        panel.Controls.Add(new Label { Text = "Trạng thái", AutoSize = true }, 2, 1);
        StyleComboBox(_cbStatus);
        _cbStatus.Dock = DockStyle.Fill;
        panel.Controls.Add(_cbStatus, 3, 1);

        _txtRoomNumber.Width = 200;
        _numFloor.Minimum = 0;
        _numFloor.Maximum = 200;
        _numFloor.Value = 1;

        _cbRoomType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbStatus.Items.AddRange(
        [
            new StatusOption("Available", "Trống"),
            new StatusOption("Occupied", "Đang ở"),
            new StatusOption("Maintenance", "Bảo trì")
        ]);
        _cbStatus.SelectedIndex = 0;

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

        var btnAdd = CreatePrimaryButton("Thêm");
        var btnUpdate = CreatePrimaryButton("Sửa");
        var btnDelete = CreateDangerButton("Xóa");
        var btnClear = CreateSecondaryButton("Làm mới");

        btnAdd.Click += (_, _) => AddRoom();
        btnUpdate.Click += (_, _) => UpdateRoom();
        btnDelete.Click += (_, _) => DeleteRoom();
        btnClear.Click += (_, _) => ClearForm();

        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnUpdate);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(btnClear);

        return panel;
    }

    private void LoadRoomTypes()
    {
        DataTable types = _roomService.GetRoomTypes();
        _cbRoomType.DataSource = types;
        _cbRoomType.DisplayMember = "TypeName";
        _cbRoomType.ValueMember = "RoomTypeId";
        if (_cbRoomType.Items.Count > 0)
        {
            _cbRoomType.SelectedIndex = 0;
        }
    }

    private void LoadRooms()
    {
        _grid.DataSource = _roomService.GetRooms();
        if (_grid.Columns["RoomId"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_grid.Columns["RoomTypeId"] is { } typeIdColumn)
        {
            typeIdColumn.Visible = false;
        }
        if (_grid.Columns["RoomNumber"] is { } roomNumberColumn)
        {
            roomNumberColumn.HeaderText = "Số phòng";
        }
        if (_grid.Columns["RoomType"] is { } roomTypeColumn)
        {
            roomTypeColumn.HeaderText = "Loại phòng";
        }
        if (_grid.Columns["Floor"] is { } floorColumn)
        {
            floorColumn.HeaderText = "Tầng";
        }
        if (_grid.Columns["Status"] is { } statusColumn)
        {
            statusColumn.HeaderText = "Trạng thái";
        }
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

    private static void StyleComboBox(Guna2ComboBox comboBox)
    {
        comboBox.BorderRadius = 8;
        comboBox.BorderThickness = 1;
        comboBox.BorderColor = Color.FromArgb(217, 221, 230);
        comboBox.FillColor = Color.White;
        comboBox.Font = new Font("Segoe UI", 9F);
        comboBox.Height = 34;
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
            Width = 110,
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
            Width = 110,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(233, 236, 239),
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
    }

    private static Guna2Button CreateDangerButton(string text)
    {
        var button = new Guna2Button
        {
            Text = text,
            Width = 110,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return button;
    }

    private void LoadSelectedRoom()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView rowView)
        {
            var row = rowView.Row;
            _selectedRoomId = Convert.ToInt32(row["RoomId"]);
            _txtRoomNumber.Text = row["RoomNumber"].ToString();
            _cbRoomType.SelectedValue = Convert.ToInt32(row["RoomTypeId"]);
            _numFloor.Value = Convert.ToInt32(row["Floor"]);
            var statusValue = row["Status"].ToString() ?? "Available";
            _cbStatus.SelectedItem = _cbStatus.Items
                .OfType<StatusOption>()
                .FirstOrDefault(option => option.Value == statusValue) ?? _cbStatus.Items[0];
        }
    }

    private void AddRoom()
    {
        if (!TryGetFormValues(out var roomNumber, out var roomTypeId, out var floor, out var status))
        {
            return;
        }

        try
        {
            _roomService.AddRoom(roomNumber, roomTypeId, floor, status);
            LoadRooms();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Thêm phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateRoom()
    {
        if (_selectedRoomId is null)
        {
            MessageBox.Show("Chọn phòng cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryGetFormValues(out var roomNumber, out var roomTypeId, out var floor, out var status))
        {
            return;
        }

        try
        {
            _roomService.UpdateRoom(_selectedRoomId.Value, roomNumber, roomTypeId, floor, status);
            LoadRooms();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sửa phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteRoom()
    {
        if (_selectedRoomId is null)
        {
            MessageBox.Show("Chọn phòng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show("Bạn chắc chắn muốn xóa phòng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _roomService.DeleteRoom(_selectedRoomId.Value);
            LoadRooms();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xóa phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryGetFormValues(out string roomNumber, out int roomTypeId, out int floor, out string status)
    {
        roomNumber = _txtRoomNumber.Text.Trim();
        status = _cbStatus.SelectedItem is StatusOption option ? option.Value : "Available";
        floor = (int)_numFloor.Value;
        roomTypeId = _cbRoomType.SelectedValue is int id ? id : 0;

        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            MessageBox.Show("Nhập số phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        if (roomTypeId <= 0)
        {
            MessageBox.Show("Chọn loại phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        _selectedRoomId = null;
        _txtRoomNumber.Clear();
        _numFloor.Value = 1;
        if (_cbRoomType.Items.Count > 0)
        {
            _cbRoomType.SelectedIndex = 0;
        }
        _cbStatus.SelectedIndex = 0;
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
}
