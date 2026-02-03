using System.Data;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class CustomersForm : Form
{
    private readonly CustomerService _customerService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2TextBox _txtFullName = new();
    private readonly Guna2TextBox _txtPhone = new();
    private readonly Guna2TextBox _txtEmail = new();
    private readonly Guna2TextBox _txtIdNumber = new();
    private readonly Guna2TextBox _txtAddress = new();
    private int? _selectedCustomerId;

    public CustomersForm()
    {
        Text = "Quản lý khách hàng";
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
        _grid.CellClick += (_, _) => LoadSelectedCustomer();

        layout.Controls.Add(inputPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(layout);

        Load += (_, _) => LoadCustomers();
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

        panel.Controls.Add(new Label { Text = "Họ tên", AutoSize = true }, 0, 0);
        StyleTextBox(_txtFullName, "Nguyễn Văn A");
        _txtFullName.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtFullName, 1, 0);
        panel.Controls.Add(new Label { Text = "Điện thoại", AutoSize = true }, 2, 0);
        StyleTextBox(_txtPhone, "098xxxxxxx");
        _txtPhone.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtPhone, 3, 0);

        panel.Controls.Add(new Label { Text = "Email", AutoSize = true }, 0, 1);
        StyleTextBox(_txtEmail, "email@example.com");
        _txtEmail.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtEmail, 1, 1);
        panel.Controls.Add(new Label { Text = "CCCD/CMND", AutoSize = true }, 2, 1);
        StyleTextBox(_txtIdNumber, "CCCD/CMND");
        _txtIdNumber.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtIdNumber, 3, 1);

        panel.Controls.Add(new Label { Text = "Địa chỉ", AutoSize = true }, 0, 2);
        StyleTextBox(_txtAddress, "Địa chỉ");
        _txtAddress.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtAddress, 1, 2);
        panel.SetColumnSpan(_txtAddress, 3);

        _txtFullName.Width = 250;
        _txtPhone.Width = 200;
        _txtEmail.Width = 250;
        _txtIdNumber.Width = 200;

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

        btnAdd.Click += (_, _) => AddCustomer();
        btnUpdate.Click += (_, _) => UpdateCustomer();
        btnDelete.Click += (_, _) => DeleteCustomer();
        btnClear.Click += (_, _) => ClearForm();

        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnUpdate);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(btnClear);

        return panel;
    }

    private void LoadCustomers()
    {
        _grid.DataSource = _customerService.GetCustomers();
        if (_grid.Columns["CustomerId"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_grid.Columns["FullName"] is { } fullNameColumn)
        {
            fullNameColumn.HeaderText = "Họ tên";
        }
        if (_grid.Columns["Phone"] is { } phoneColumn)
        {
            phoneColumn.HeaderText = "Điện thoại";
        }
        if (_grid.Columns["Email"] is { } emailColumn)
        {
            emailColumn.HeaderText = "Email";
        }
        if (_grid.Columns["IdNumber"] is { } idNumberColumn)
        {
            idNumberColumn.HeaderText = "CCCD/CMND";
        }
        if (_grid.Columns["Address"] is { } addressColumn)
        {
            addressColumn.HeaderText = "Địa chỉ";
        }
        if (_grid.Columns["CreatedAt"] is { } createdAtColumn)
        {
            createdAtColumn.HeaderText = "Ngày tạo";
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

    private void LoadSelectedCustomer()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView rowView)
        {
            var row = rowView.Row;
            _selectedCustomerId = Convert.ToInt32(row["CustomerId"]);
            _txtFullName.Text = row["FullName"].ToString();
            _txtPhone.Text = row["Phone"]?.ToString() ?? string.Empty;
            _txtEmail.Text = row["Email"]?.ToString() ?? string.Empty;
            _txtIdNumber.Text = row["IdNumber"]?.ToString() ?? string.Empty;
            _txtAddress.Text = row["Address"]?.ToString() ?? string.Empty;
        }
    }

    private void AddCustomer()
    {
        if (!TryGetFormValues(out var fullName, out var phone, out var email, out var idNumber, out var address))
        {
            return;
        }

        try
        {
            _customerService.AddCustomer(fullName, phone, email, idNumber, address);
            LoadCustomers();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Thêm khách hàng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateCustomer()
    {
        if (_selectedCustomerId is null)
        {
            MessageBox.Show("Chọn khách hàng cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryGetFormValues(out var fullName, out var phone, out var email, out var idNumber, out var address))
        {
            return;
        }

        try
        {
            _customerService.UpdateCustomer(_selectedCustomerId.Value, fullName, phone, email, idNumber, address);
            LoadCustomers();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sửa khách hàng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteCustomer()
    {
        if (_selectedCustomerId is null)
        {
            MessageBox.Show("Chọn khách hàng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show("Bạn chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _customerService.DeleteCustomer(_selectedCustomerId.Value);
            LoadCustomers();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xóa khách hàng thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryGetFormValues(out string fullName, out string? phone, out string? email, out string? idNumber, out string? address)
    {
        fullName = _txtFullName.Text.Trim();
        phone = _txtPhone.Text.Trim();
        email = _txtEmail.Text.Trim();
        idNumber = _txtIdNumber.Text.Trim();
        address = _txtAddress.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Nhập họ tên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
        email = string.IsNullOrWhiteSpace(email) ? null : email;
        idNumber = string.IsNullOrWhiteSpace(idNumber) ? null : idNumber;
        address = string.IsNullOrWhiteSpace(address) ? null : address;

        return true;
    }

    private void ClearForm()
    {
        _selectedCustomerId = null;
        _txtFullName.Clear();
        _txtPhone.Clear();
        _txtEmail.Clear();
        _txtIdNumber.Clear();
        _txtAddress.Clear();
    }
}
