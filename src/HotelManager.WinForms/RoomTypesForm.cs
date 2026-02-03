using System.Data;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class RoomTypesForm : Form
{
    private readonly RoomTypeService _roomTypeService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2TextBox _txtTypeName = new();
    private readonly Guna2NumericUpDown _numBasePrice = new();
    private readonly Guna2NumericUpDown _numCapacity = new();
    private readonly Guna2TextBox _txtDescription = new();
    private int? _selectedRoomTypeId;

    public RoomTypesForm()
    {
        Text = "Quản lý loại phòng";
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
        _grid.CellClick += (_, _) => LoadSelectedRoomType();

        layout.Controls.Add(inputPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(layout);

        Load += (_, _) => LoadRoomTypes();
    }

    private Control BuildInputPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            AutoSize = true,
            Padding = new Padding(10)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        panel.Controls.Add(new Label { Text = "Tên loại", AutoSize = true }, 0, 0);
        StyleTextBox(_txtTypeName, "Deluxe");
        _txtTypeName.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtTypeName, 1, 0);
        panel.Controls.Add(new Label { Text = "Giá cơ bản", AutoSize = true }, 2, 0);
        StyleNumeric(_numBasePrice);
        _numBasePrice.Dock = DockStyle.Fill;
        panel.Controls.Add(_numBasePrice, 3, 0);

        panel.Controls.Add(new Label { Text = "Sức chứa", AutoSize = true }, 0, 1);
        StyleNumeric(_numCapacity);
        _numCapacity.Dock = DockStyle.Fill;
        panel.Controls.Add(_numCapacity, 1, 1);
        panel.Controls.Add(new Label { Text = "Mô tả", AutoSize = true }, 2, 1);
        StyleTextBox(_txtDescription, "Mô tả");
        _txtDescription.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtDescription, 3, 1);
        panel.SetColumnSpan(_txtDescription, 3);

        _txtTypeName.Width = 200;
        _numCapacity.Minimum = 1;
        _numCapacity.Maximum = 100;
        _numCapacity.Value = 2;

        _numBasePrice.Minimum = 0;
        _numBasePrice.Maximum = 1000000000;
        _numBasePrice.DecimalPlaces = 2;
        _numBasePrice.ThousandsSeparator = true;
        _numBasePrice.Increment = 10000;

        _txtDescription.Width = 300;

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

        btnAdd.Click += (_, _) => AddRoomType();
        btnUpdate.Click += (_, _) => UpdateRoomType();
        btnDelete.Click += (_, _) => DeleteRoomType();
        btnClear.Click += (_, _) => ClearForm();

        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnUpdate);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(btnClear);

        return panel;
    }

    private void LoadRoomTypes()
    {
        _grid.DataSource = _roomTypeService.GetRoomTypes();
        if (_grid.Columns["RoomTypeId"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_grid.Columns["TypeName"] is { } typeNameColumn)
        {
            typeNameColumn.HeaderText = "Tên loại";
        }
        if (_grid.Columns["BasePrice"] is { } basePriceColumn)
        {
            basePriceColumn.HeaderText = "Giá cơ bản";
        }
        if (_grid.Columns["Capacity"] is { } capacityColumn)
        {
            capacityColumn.HeaderText = "Sức chứa";
        }
        if (_grid.Columns["Description"] is { } descriptionColumn)
        {
            descriptionColumn.HeaderText = "Mô tả";
        }
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

    private void LoadSelectedRoomType()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView rowView)
        {
            var row = rowView.Row;
            _selectedRoomTypeId = Convert.ToInt32(row["RoomTypeId"]);
            _txtTypeName.Text = row["TypeName"].ToString();
            _numBasePrice.Value = Convert.ToDecimal(row["BasePrice"]);
            _numCapacity.Value = Convert.ToInt32(row["Capacity"]);
            _txtDescription.Text = row["Description"]?.ToString() ?? string.Empty;
        }
    }

    private void AddRoomType()
    {
        if (!TryGetFormValues(out var typeName, out var basePrice, out var capacity, out var description))
        {
            return;
        }

        try
        {
            _roomTypeService.AddRoomType(typeName, basePrice, capacity, description);
            LoadRoomTypes();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Thêm loại phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateRoomType()
    {
        if (_selectedRoomTypeId is null)
        {
            MessageBox.Show("Chọn loại phòng cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryGetFormValues(out var typeName, out var basePrice, out var capacity, out var description))
        {
            return;
        }

        try
        {
            _roomTypeService.UpdateRoomType(_selectedRoomTypeId.Value, typeName, basePrice, capacity, description);
            LoadRoomTypes();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sửa loại phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteRoomType()
    {
        if (_selectedRoomTypeId is null)
        {
            MessageBox.Show("Chọn loại phòng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show("Bạn chắc chắn muốn xóa loại phòng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _roomTypeService.DeleteRoomType(_selectedRoomTypeId.Value);
            LoadRoomTypes();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xóa loại phòng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryGetFormValues(out string typeName, out decimal basePrice, out int capacity, out string? description)
    {
        typeName = _txtTypeName.Text.Trim();
        basePrice = _numBasePrice.Value;
        capacity = (int)_numCapacity.Value;
        description = _txtDescription.Text.Trim();

        if (string.IsNullOrWhiteSpace(typeName))
        {
            MessageBox.Show("Nhập tên loại phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        if (capacity <= 0)
        {
            MessageBox.Show("Sức chứa phải lớn hơn 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        _selectedRoomTypeId = null;
        _txtTypeName.Clear();
        _numBasePrice.Value = 0;
        _numCapacity.Value = 2;
        _txtDescription.Clear();
    }
}
