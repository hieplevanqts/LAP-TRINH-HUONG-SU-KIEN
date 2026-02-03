using System.Drawing;
using System.IO;
using Guna.UI2.WinForms;

namespace HotelManager.WinForms;

public sealed class PaymentInfoForm : Form
{
    private readonly Label _lblBankName = new();
    private readonly Label _lblAccountNumber = new();
    private readonly Label _lblAccountName = new();
    private readonly Guna2PictureBox _qrImage = new();
    private readonly Label _lblQrHint = new();
    private readonly Label _lblQrPath = new();

    public PaymentInfoForm()
    {
        Text = "Thông tin thanh toán";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var infoPanel = BuildInfoPanel();
        var buttonPanel = BuildButtonPanel();

        layout.Controls.Add(infoPanel, 0, 0);
        layout.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(layout);

        Load += (_, _) => RefreshPaymentInfo();
    }

    private Control BuildInfoPanel()
    {
        var panel = new Guna2GroupBox
        {
            Text = "Thông tin chuyển khoản",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var info = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            AutoSize = true
        };
        info.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        info.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        info.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        info.Controls.Add(new Label { Text = "Ngân hàng:", AutoSize = true }, 0, 0);
        info.Controls.Add(_lblBankName, 1, 0);
        info.Controls.Add(new Label { Text = "Số tài khoản:", AutoSize = true }, 0, 1);
        info.Controls.Add(_lblAccountNumber, 1, 1);
        info.Controls.Add(new Label { Text = "Chủ tài khoản:", AutoSize = true }, 0, 2);
        info.Controls.Add(_lblAccountName, 1, 2);

        _lblBankName.AutoSize = true;
        _lblAccountNumber.AutoSize = true;
        _lblAccountName.AutoSize = true;

        var qrPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3
        };
        qrPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        qrPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        qrPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _qrImage.Dock = DockStyle.Fill;
        _qrImage.SizeMode = PictureBoxSizeMode.Zoom;
        _qrImage.BorderRadius = 8;
        _qrImage.FillColor = Color.FromArgb(245, 247, 251);
        _qrImage.MaximumSize = new Size(240, 240);

        _lblQrHint.AutoSize = true;
        _lblQrHint.ForeColor = Color.FromArgb(108, 117, 125);
        _lblQrHint.TextAlign = ContentAlignment.MiddleCenter;

        _lblQrPath.AutoSize = true;
        _lblQrPath.ForeColor = Color.FromArgb(108, 117, 125);
        _lblQrPath.TextAlign = ContentAlignment.MiddleCenter;

        qrPanel.Controls.Add(_qrImage, 0, 0);
        qrPanel.Controls.Add(_lblQrHint, 0, 1);
        qrPanel.Controls.Add(_lblQrPath, 0, 2);

        container.Controls.Add(info, 0, 0);
        container.Controls.Add(qrPanel, 1, 0);
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

        var btnClose = CreateSecondaryButton("Đóng");
        btnClose.Click += (_, _) => Close();

        panel.Controls.Add(btnClose);
        return panel;
    }

    private void RefreshPaymentInfo()
    {
        var settings = SettingsService.Load();
        _lblBankName.Text = string.IsNullOrWhiteSpace(settings.BankName) ? "Chưa thiết lập" : settings.BankName;
        _lblAccountNumber.Text = string.IsNullOrWhiteSpace(settings.AccountNumber) ? "Chưa thiết lập" : settings.AccountNumber;
        _lblAccountName.Text = string.IsNullOrWhiteSpace(settings.AccountName) ? "Chưa thiết lập" : settings.AccountName;

        _qrImage.Image?.Dispose();
        _qrImage.Image = null;
        _lblQrHint.Text = string.Empty;
        _lblQrPath.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(settings.QrImagePath))
        {
            _lblQrHint.Text = "Chưa có ảnh QR";
            return;
        }

        if (!File.Exists(settings.QrImagePath))
        {
            _lblQrHint.Text = "Không tìm thấy ảnh QR";
            _lblQrPath.Text = settings.QrImagePath;
            return;
        }

        try
        {
            var bytes = File.ReadAllBytes(settings.QrImagePath);
            using var ms = new MemoryStream(bytes);
            using var image = Image.FromStream(ms);
            _qrImage.Image = new Bitmap(image);
            _lblQrPath.Text = settings.QrImagePath;
        }
        catch
        {
            _lblQrHint.Text = "Không đọc được ảnh QR";
            _lblQrPath.Text = settings.QrImagePath;
        }
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
