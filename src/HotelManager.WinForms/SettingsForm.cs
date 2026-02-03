using System.Drawing;
using Guna.UI2.WinForms;

namespace HotelManager.WinForms;

public sealed class SettingsForm : Form
{
    private readonly Guna2TextBox _txtBankName = new();
    private readonly Guna2TextBox _txtAccountNumber = new();
    private readonly Guna2TextBox _txtAccountName = new();
    private readonly Guna2TextBox _txtQrPath = new();

    public SettingsForm()
    {
        Text = "Cài đặt thanh toán";
        Width = 600;
        Height = 400;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var formPanel = BuildFormPanel();
        var buttonPanel = BuildButtonPanel();

        layout.Controls.Add(formPanel, 0, 0);
        layout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(layout);

        Load += (_, _) => LoadSettings();
    }

    private Control BuildFormPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(12)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

        panel.Controls.Add(new Label { Text = "Ngân hàng", AutoSize = true }, 0, 0);
        StyleTextBox(_txtBankName, "Ví dụ: Vietcombank");
        _txtBankName.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtBankName, 1, 0);

        panel.Controls.Add(new Label { Text = "Số tài khoản", AutoSize = true }, 0, 1);
        StyleTextBox(_txtAccountNumber, "0123456789");
        _txtAccountNumber.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtAccountNumber, 1, 1);

        panel.Controls.Add(new Label { Text = "Chủ tài khoản", AutoSize = true }, 0, 2);
        StyleTextBox(_txtAccountName, "Nguyễn Văn A");
        _txtAccountName.Dock = DockStyle.Fill;
        panel.Controls.Add(_txtAccountName, 1, 2);

        panel.Controls.Add(new Label { Text = "Ảnh QR", AutoSize = true }, 0, 3);
        var qrPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        StyleTextBox(_txtQrPath, "Đường dẫn ảnh QR");
        _txtQrPath.Width = 320;
        var btnBrowse = CreateSecondaryButton("Chọn ảnh");
        btnBrowse.Click += (_, _) => BrowseQrImage();
        qrPanel.Controls.Add(_txtQrPath);
        qrPanel.Controls.Add(btnBrowse);
        panel.Controls.Add(qrPanel, 1, 3);

        return panel;
    }

    private Control BuildButtonPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12),
            AutoSize = true
        };

        var btnSave = CreatePrimaryButton("Lưu");
        var btnCancel = CreateSecondaryButton("Hủy");

        btnSave.Click += (_, _) => SaveSettings();
        btnCancel.Click += (_, _) => Close();

        panel.Controls.Add(btnSave);
        panel.Controls.Add(btnCancel);

        return panel;
    }

    private void LoadSettings()
    {
        var settings = SettingsService.Load();
        _txtBankName.Text = settings.BankName;
        _txtAccountNumber.Text = settings.AccountNumber;
        _txtAccountName.Text = settings.AccountName;
        _txtQrPath.Text = settings.QrImagePath;
    }

    private void SaveSettings()
    {
        var settings = new PaymentSettings
        {
            BankName = _txtBankName.Text.Trim(),
            AccountNumber = _txtAccountNumber.Text.Trim(),
            AccountName = _txtAccountName.Text.Trim(),
            QrImagePath = _txtQrPath.Text.Trim()
        };

        try
        {
            SettingsService.Save(settings);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lưu cài đặt thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BrowseQrImage()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh QR",
            Filter = "PNG (*.png)|*.png|JPG (*.jpg;*.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtQrPath.Text = dialog.FileName;
        }
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
}
