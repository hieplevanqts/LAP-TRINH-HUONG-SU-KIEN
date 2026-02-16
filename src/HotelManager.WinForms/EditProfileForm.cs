using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class EditProfileForm : Form
{
    private readonly int _employeeId;
    private readonly EmployeeProfileService _profileService = new();

    private readonly Guna2TextBox _txtUsername = new();
    private readonly Guna2TextBox _txtFullName = new();
    private readonly Guna2TextBox _txtPhone = new();
    private readonly Guna2TextBox _txtEmail = new();
    private readonly Guna2TextBox _txtPosition = new();
    private readonly Guna2TextBox _txtBankName = new();
    private readonly Guna2TextBox _txtBankAccountNumber = new();
    private readonly Guna2TextBox _txtBankAccountName = new();
    private readonly Guna2PictureBox _picAvatar = new();
    private readonly Label _lblAvatarPath = new();
    private string? _avatarPath;
    public string? UpdatedAvatarPath { get; private set; }
    public string UpdatedFullName { get; private set; } = string.Empty;
    public Bitmap? UpdatedAvatarImage { get; private set; }

    public EditProfileForm(int employeeId)
    {
        _employeeId = employeeId;

        Text = "Chỉnh sửa thông tin";
        Width = 760;
        Height = 670;
        MinimumSize = new Size(760, 670);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 251);
        Font = new Font("Segoe UI", 9F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var content = BuildContent();
        var actions = BuildActions();

        root.Controls.Add(content, 0, 0);
        root.Controls.Add(actions, 0, 1);
        Controls.Add(root);

        Load += (_, _) => LoadProfile();
    }

    private Control BuildContent()
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        var card = new Guna2Panel
        {
            Width = 640,
            BorderRadius = 12,
            FillColor = Color.White,
            BorderColor = Color.FromArgb(225, 230, 240),
            BorderThickness = 1,
            Padding = new Padding(16)
        };

        ConfigureTextBox(_txtUsername, true);
        ConfigureTextBox(_txtFullName);
        ConfigureTextBox(_txtPhone);
        ConfigureTextBox(_txtEmail);
        ConfigureTextBox(_txtPosition);
        ConfigureTextBox(_txtBankName);
        ConfigureTextBox(_txtBankAccountNumber);
        ConfigureTextBox(_txtBankAccountName);

        _picAvatar.Size = new Size(130, 130);
        _picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
        _picAvatar.BorderRadius = 65;
        _picAvatar.FillColor = Color.FromArgb(238, 243, 252);
        _picAvatar.Image = CreateFallbackAvatar();

        var avatarPanel = new FlowLayoutPanel
        {
            Width = 170,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0)
        };
        var btnChooseAvatar = new Guna2Button
        {
            Text = "Cập nhật avatar",
            Width = 130,
            Height = 34,
            BorderRadius = 8,
            FillColor = Color.FromArgb(35, 102, 220),
            ForeColor = Color.White
        };
        btnChooseAvatar.Click += (_, _) => ChooseAvatar();

        _lblAvatarPath.AutoSize = true;
        _lblAvatarPath.MaximumSize = new Size(160, 0);
        _lblAvatarPath.ForeColor = Color.FromArgb(102, 110, 122);
        _lblAvatarPath.Text = "Chưa chọn ảnh";

        avatarPanel.Controls.Add(_picAvatar);
        avatarPanel.Controls.Add(btnChooseAvatar);
        avatarPanel.Controls.Add(_lblAvatarPath);

        const int labelX = 24;
        const int inputX = 220;
        const int startY = 20;
        const int rowStep = 56;
        var y = startY;

        void AddField(string label, Guna2TextBox textBox)
        {
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(labelX, y + 10),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 69)
            };
            textBox.Location = new Point(inputX, y);
            card.Controls.Add(lbl);
            card.Controls.Add(textBox);
            y += rowStep;
        }

        AddField("Tài khoản", _txtUsername);
        AddField("Họ tên", _txtFullName);
        AddField("Số điện thoại", _txtPhone);
        AddField("Email", _txtEmail);
        AddField("Chức vụ", _txtPosition);
        AddField("Tên ngân hàng", _txtBankName);
        AddField("Số tài khoản", _txtBankAccountNumber);
        AddField("Tên chủ tài khoản", _txtBankAccountName);

        var lblAvatar = new Label
        {
            Text = "Ảnh đại diện",
            AutoSize = true,
            Location = new Point(labelX, y + 10),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 58, 69)
        };
        avatarPanel.Location = new Point(inputX, y);

        card.Controls.Add(lblAvatar);
        card.Controls.Add(avatarPanel);
        card.Height = avatarPanel.Bottom + 24;
        host.Controls.Add(card);

        void CenterCard()
        {
            card.Top = 10;
            card.Left = Math.Max(8, (host.ClientSize.Width - card.Width) / 2);
        }

        host.Resize += (_, _) => CenterCard();
        CenterCard();
        return host;
    }

    private Control BuildActions()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 0)
        };

        var btnSave = new Guna2Button
        {
            Text = "Lưu thay đổi",
            Width = 130,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(35, 102, 220),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnSave.Click += (_, _) => SaveProfile();

        var btnClose = new Guna2Button
        {
            Text = "Đóng",
            Width = 90,
            Height = 36,
            BorderRadius = 8,
            FillColor = Color.FromArgb(231, 236, 245),
            ForeColor = Color.FromArgb(52, 58, 69),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnClose.Click += (_, _) => Close();

        bar.Controls.Add(btnSave);
        bar.Controls.Add(btnClose);
        return bar;
    }

    private static void ConfigureTextBox(Guna2TextBox textBox, bool readOnly = false)
    {
        textBox.Width = 370;
        textBox.Height = 36;
        textBox.BorderRadius = 8;
        textBox.BorderColor = Color.FromArgb(207, 216, 230);
        textBox.FillColor = readOnly ? Color.FromArgb(244, 247, 253) : Color.White;
        textBox.ReadOnly = readOnly;
        textBox.Font = new Font("Segoe UI", 9.5F);
        textBox.ForeColor = Color.FromArgb(33, 37, 41);
    }

    private void LoadProfile()
    {
        var profile = _profileService.GetProfile(_employeeId);
        if (profile is null)
        {
            MessageBox.Show("Không tìm thấy thông tin nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        _txtUsername.Text = profile.Username;
        _txtFullName.Text = profile.FullName;
        _txtPhone.Text = profile.Phone ?? string.Empty;
        _txtEmail.Text = profile.Email ?? string.Empty;
        _txtPosition.Text = profile.Position ?? string.Empty;
        _txtBankName.Text = profile.BankName ?? string.Empty;
        _txtBankAccountNumber.Text = profile.BankAccountNumber ?? string.Empty;
        _txtBankAccountName.Text = profile.BankAccountName ?? string.Empty;
        _avatarPath = profile.AvatarPath;
        RenderAvatar(profile.AvatarPath);
    }

    private void SaveProfile()
    {
        if (string.IsNullOrWhiteSpace(_txtFullName.Text))
        {
            MessageBox.Show("Họ tên không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var dto = new EmployeeProfileDto
        {
            EmployeeId = _employeeId,
            FullName = _txtFullName.Text,
            Phone = _txtPhone.Text,
            Email = _txtEmail.Text,
            Position = _txtPosition.Text,
            AvatarPath = _avatarPath,
            BankName = _txtBankName.Text,
            BankAccountNumber = _txtBankAccountNumber.Text,
            BankAccountName = _txtBankAccountName.Text,
            Username = _txtUsername.Text
        };

        _profileService.UpdateProfile(dto);
        UpdatedAvatarPath = dto.AvatarPath;
        UpdatedFullName = dto.FullName;
        UpdatedAvatarImage = _picAvatar.Image is null ? null : new Bitmap(_picAvatar.Image);
        MessageBox.Show("Cập nhật thông tin thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ChooseAvatar()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh đại diện",
            Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var path = CopyAvatarToLocalStore(dialog.FileName);
        _avatarPath = path;
        RenderAvatar(path);
    }

    private void RenderAvatar(string? path)
    {
        _lblAvatarPath.Text = string.IsNullOrWhiteSpace(path) ? "Chưa chọn ảnh" : Path.GetFileName(path);
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            _picAvatar.Image = Image.FromStream(fs);
        }
        else
        {
            _picAvatar.Image = CreateFallbackAvatar();
        }
    }

    private static string CopyAvatarToLocalStore(string sourcePath)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HotelManager",
            "avatars");

        Directory.CreateDirectory(root);
        var extension = Path.GetExtension(sourcePath);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(root, fileName);
        File.Copy(sourcePath, destination, true);
        return destination;
    }

    private static Bitmap CreateFallbackAvatar()
    {
        var bmp = new Bitmap(130, 130);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(235, 241, 252));
        using var brush = new SolidBrush(Color.FromArgb(95, 111, 138));
        using var font = new Font("Segoe UI", 34F, FontStyle.Bold);
        var text = "NV";
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, (130 - size.Width) / 2, (130 - size.Height) / 2);
        return bmp;
    }
}
