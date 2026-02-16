using System.Data;
using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class ServicesForm : Form
{
    private readonly ServiceService _serviceService = new();
    private readonly Guna2DataGridView _grid = new();
    private readonly Guna2TextBox _txtServiceName = new();
    private readonly Guna2NumericUpDown _numUnitPrice = new();
    private readonly Guna2CheckBox _chkActive = new() { Text = "KÃ­ch hoáº¡t", Checked = true, AutoSize = true };
    private int? _selectedServiceId;

    public ServicesForm()
    {
        Text = "Quáº£n lÃ½ dá»‹ch vá»¥";
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
        _grid.CellClick += (_, _) => LoadSelectedService();

        layout.Controls.Add(inputPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(layout);

        Load += (_, _) => LoadServices();
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

        panel.Controls.Add(new Label { Text = "TÃªn dá»‹ch vá»¥", AutoSize = true }, 0, 0);
        StyleTextBox(_txtServiceName, "Giáº·t á»§i");
        _txtServiceName.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtServiceName, 1, 0);

        panel.Controls.Add(new Label { Text = "GiÃ¡ tiá»n", AutoSize = true }, 2, 0);
        StyleNumeric(_numUnitPrice);
        _numUnitPrice.Dock = DockStyle.Fill;
        _numUnitPrice.Minimum = 0;
        _numUnitPrice.Maximum = 1000000000;
        _numUnitPrice.DecimalPlaces = 0;
        _numUnitPrice.ThousandsSeparator = true;
        panel.Controls.Add(_numUnitPrice, 3, 0);

        panel.Controls.Add(_chkActive, 1, 1);

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

        var btnAdd = CreatePrimaryButton("ThÃªm");
        var btnUpdate = CreatePrimaryButton("Sá»­a");
        var btnDelete = CreateDangerButton("XÃ³a");
        var btnClear = CreateSecondaryButton("LÃ m má»›i");

        btnAdd.Click += (_, _) => AddService();
        btnUpdate.Click += (_, _) => UpdateService();
        btnDelete.Click += (_, _) => DeleteService();
        btnClear.Click += (_, _) => ClearForm();

        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnUpdate);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(btnClear);

        return panel;
    }

    private void LoadServices()
    {
        _grid.DataSource = _serviceService.GetServices();
        if (_grid.Columns["ServiceId"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_grid.Columns["ServiceName"] is { } nameColumn)
        {
            nameColumn.HeaderText = "TÃªn dá»‹ch vá»¥";
        }
        if (_grid.Columns["UnitPrice"] is { } priceColumn)
        {
            priceColumn.HeaderText = "GiÃ¡ tiá»n";
            priceColumn.DefaultCellStyle.Format = "N0";
            priceColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (_grid.Columns["IsActive"] is { } activeColumn)
        {
            activeColumn.HeaderText = "KÃ­ch hoáº¡t";
        }
    }

    private void LoadSelectedService()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView rowView)
        {
            var row = rowView.Row;
            _selectedServiceId = Convert.ToInt32(row["ServiceId"]);
            _txtServiceName.Text = row["ServiceName"].ToString();
            _numUnitPrice.Value = Convert.ToDecimal(row["UnitPrice"]);
            _chkActive.Checked = Convert.ToBoolean(row["IsActive"]);
        }
    }

    private void AddService()
    {
        if (!TryGetFormValues(out var name, out var price, out var isActive))
        {
            return;
        }

        try
        {
            _serviceService.AddService(name, price, isActive);
            LoadServices();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ThÃªm dá»‹ch vá»¥ tháº¥t báº¡i: {ex.Message}", "Lá»—i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateService()
    {
        if (_selectedServiceId is null)
        {
            MessageBox.Show("Chá»n dá»‹ch vá»¥ cáº§n sá»­a.", "ThÃ´ng bÃ¡o", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryGetFormValues(out var name, out var price, out var isActive))
        {
            return;
        }

        try
        {
            _serviceService.UpdateService(_selectedServiceId.Value, name, price, isActive);
            LoadServices();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sá»­a dá»‹ch vá»¥ tháº¥t báº¡i: {ex.Message}", "Lá»—i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteService()
    {
        if (_selectedServiceId is null)
        {
            MessageBox.Show("Chá»n dá»‹ch vá»¥ cáº§n xÃ³a.", "ThÃ´ng bÃ¡o", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show("Báº¡n cháº¯c cháº¯n muá»‘n xÃ³a dá»‹ch vá»¥ nÃ y?", "XÃ¡c nháº­n", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _serviceService.DeleteService(_selectedServiceId.Value);
            LoadServices();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"XÃ³a dá»‹ch vá»¥ tháº¥t báº¡i: {ex.Message}", "Lá»—i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryGetFormValues(out string name, out decimal price, out bool isActive)
    {
        name = _txtServiceName.Text.Trim();
        price = _numUnitPrice.Value;
        isActive = _chkActive.Checked;

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Nháº­p tÃªn dá»‹ch vá»¥.", "ThÃ´ng bÃ¡o", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        _selectedServiceId = null;
        _txtServiceName.Clear();
        _numUnitPrice.Value = 0;
        _chkActive.Checked = true;
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
}

