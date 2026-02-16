using System.Drawing;
using Guna.UI2.WinForms;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class LoginForm : Form
{
    private readonly AuthService _authService = new();
    private readonly Label _lblStatus = new();

    private readonly Guna2Button _btnTabLogin = new();
    private readonly Guna2Button _btnTabRegister = new();
    private readonly Guna2Panel _contentHost = new();
    private readonly Guna2Panel _loginCard;
    private readonly Guna2Panel _registerCard;

    public LoginResult? LoginInfo { get; private set; }

    public LoginForm()
    {
        Text = "Đăng nhập hệ thống";
        Width = 920;
        Height = 680;
        MinimumSize = new Size(860, 620);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(243, 246, 252);
        Font = new Font("Segoe UI", 9F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new Guna2Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BorderRadius = 14,
            FillColor = Color.FromArgb(232, 240, 255),
            BorderColor = Color.FromArgb(196, 212, 242),
            BorderThickness = 1
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Quản lý khách sạn - Đăng nhập / Đăng ký nhân viên",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(23, 43, 77)
        });

        var outerBox = new Guna2Panel
        {
            Dock = DockStyle.Fill,
            BorderRadius = 14,
            FillColor = Color.White,
            BorderColor = Color.FromArgb(220, 228, 243),
            BorderThickness = 1,
            Padding = new Padding(14)
        };

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var tabBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 8)
        };
        tabBar.Controls.Add(_btnTabLogin);
        tabBar.Controls.Add(_btnTabRegister);
        ConfigureTabButton(_btnTabLogin, "Đăng nhập", true, (_, _) => ShowLogin());
        ConfigureTabButton(_btnTabRegister, "Đăng ký", false, (_, _) => ShowRegister());

        var tabBarHost = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            AutoSize = true
        };
        tabBarHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tabBarHost.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabBarHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tabBarHost.Controls.Add(tabBar, 1, 0);

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.FillColor = Color.FromArgb(250, 252, 255);
        _contentHost.BorderRadius = 12;
        _contentHost.Padding = new Padding(12);
        _contentHost.AutoScroll = true;
        _contentHost.Resize += (_, _) => CenterCurrentCard();

        _loginCard = BuildLoginCard();
        _registerCard = BuildRegisterCard();

        main.Controls.Add(tabBarHost, 0, 0);
        main.Controls.Add(_contentHost, 0, 1);
        outerBox.Controls.Add(main);

        _lblStatus.Dock = DockStyle.Fill;
        _lblStatus.Height = 28;
        _lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        _lblStatus.ForeColor = Color.FromArgb(49, 61, 80);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(outerBox, 0, 1);
        root.Controls.Add(_lblStatus, 0, 2);
        Controls.Add(root);

        ShowLogin();
    }

    private void ConfigureTabButton(Guna2Button button, string text, bool selected, EventHandler onClick)
    {
        button.Text = text;
        button.Width = 170;
        button.Height = 38;
        button.BorderRadius = 8;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.Click += onClick;
        ApplyTabButtonState(button, selected);
    }

    private void ApplyTabButtonState(Guna2Button button, bool selected)
    {
        if (selected)
        {
            button.FillColor = Color.FromArgb(19, 74, 185);
            button.ForeColor = Color.White;
            button.BorderThickness = 0;
        }
        else
        {
            button.FillColor = Color.White;
            button.ForeColor = Color.FromArgb(25, 35, 50);
            button.BorderColor = Color.FromArgb(203, 214, 234);
            button.BorderThickness = 1;
        }
    }

    private void ShowLogin()
    {
        ApplyTabButtonState(_btnTabLogin, true);
        ApplyTabButtonState(_btnTabRegister, false);
        ShowCard(_loginCard);
    }

    private void ShowRegister()
    {
        ApplyTabButtonState(_btnTabLogin, false);
        ApplyTabButtonState(_btnTabRegister, true);
        ShowCard(_registerCard);
    }

    private void ShowCard(Control card)
    {
        _contentHost.Controls.Clear();
        _contentHost.Controls.Add(card);
        CenterCurrentCard();
    }

    private void CenterCurrentCard()
    {
        if (_contentHost.Controls.Count == 0)
        {
            return;
        }

        var card = _contentHost.Controls[0];
        card.Top = 12;
        card.Left = Math.Max(12, (_contentHost.ClientSize.Width - card.Width) / 2);
    }

    private Guna2Panel BuildLoginCard()
    {
        var card = CreateCard(680, 220);

        var txtUsername = CreateTextBox("Nhập tên đăng nhập");
        txtUsername.Location = new Point(220, 24);

        var txtPassword = CreateTextBox("Nhập mật khẩu");
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.Location = new Point(220, 82);

        var btnLogin = CreatePrimaryButton("Đăng nhập");
        btnLogin.Location = new Point(500, 144);

        void DoLogin()
        {
            var result = _authService.Authenticate(txtUsername.Text, txtPassword.Text);
            SetStatus(result.Message, result.IsSuccess);
            if (!result.IsSuccess)
            {
                return;
            }

            LoginInfo = result;
            DialogResult = DialogResult.OK;
            Close();
        }

        btnLogin.Click += (_, _) => DoLogin();

        void HandleEnterToLogin(object? _, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            e.Handled = true;
            DoLogin();
        }

        txtUsername.KeyDown += HandleEnterToLogin;
        txtPassword.KeyDown += HandleEnterToLogin;

        card.Controls.Add(CreateLabel("Tên đăng nhập", 24, 34));
        card.Controls.Add(txtUsername);
        card.Controls.Add(CreateLabel("Mật khẩu", 24, 92));
        card.Controls.Add(txtPassword);
        card.Controls.Add(btnLogin);
        return card;
    }

    private Guna2Panel BuildRegisterCard()
    {
        var card = CreateCard(680, 520);

        var txtFullName = CreateTextBox("Ví dụ: Nguyễn Văn A");
        var txtPhone = CreateTextBox("Ví dụ: 0901234567");
        var txtEmail = CreateTextBox("Ví dụ: abc@gmail.com");
        var txtPosition = CreateTextBox("Ví dụ: Lễ tân");
        var txtUsername = CreateTextBox("Tên đăng nhập");
        var txtPassword = CreateTextBox("Mật khẩu");
        var txtConfirm = CreateTextBox("Nhập lại mật khẩu");
        txtPassword.UseSystemPasswordChar = true;
        txtConfirm.UseSystemPasswordChar = true;

        txtFullName.Location = new Point(220, 20);
        txtPhone.Location = new Point(220, 76);
        txtEmail.Location = new Point(220, 132);
        txtPosition.Location = new Point(220, 188);
        txtUsername.Location = new Point(220, 244);
        txtPassword.Location = new Point(220, 300);
        txtConfirm.Location = new Point(220, 356);

        var btnRegister = CreatePrimaryButton("Đăng ký");
        btnRegister.Location = new Point(500, 418);
        btnRegister.Click += (_, _) =>
        {
            var request = new RegisterEmployeeRequest(
                txtFullName.Text,
                txtPhone.Text,
                txtEmail.Text,
                txtPosition.Text,
                txtUsername.Text,
                txtPassword.Text,
                txtConfirm.Text);

            var result = _authService.RegisterEmployee(request);
            SetStatus(result.Message, result.IsSuccess);
            if (!result.IsSuccess)
            {
                return;
            }

            MessageBox.Show(
                $"Đăng ký thành công. Mã nhân viên: {result.EmployeeId}",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            txtFullName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtPosition.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirm.Clear();
        };

        card.Controls.Add(CreateLabel("Họ tên *", 24, 30));
        card.Controls.Add(txtFullName);
        card.Controls.Add(CreateLabel("Số điện thoại", 24, 86));
        card.Controls.Add(txtPhone);
        card.Controls.Add(CreateLabel("Email", 24, 142));
        card.Controls.Add(txtEmail);
        card.Controls.Add(CreateLabel("Chức vụ", 24, 198));
        card.Controls.Add(txtPosition);
        card.Controls.Add(CreateLabel("Tên đăng nhập *", 24, 254));
        card.Controls.Add(txtUsername);
        card.Controls.Add(CreateLabel("Mật khẩu *", 24, 310));
        card.Controls.Add(txtPassword);
        card.Controls.Add(CreateLabel("Xác nhận mật khẩu *", 24, 366));
        card.Controls.Add(txtConfirm);
        card.Controls.Add(btnRegister);

        return card;
    }

    private void SetStatus(string message, bool isSuccess)
    {
        _lblStatus.Text = message;
        _lblStatus.ForeColor = isSuccess ? Color.FromArgb(18, 133, 62) : Color.FromArgb(201, 62, 52);
    }

    private static Label CreateLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Location = new Point(x, y),
            ForeColor = Color.FromArgb(41, 51, 68),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
        };
    }

    private static Guna2Panel CreateCard(int width, int height)
    {
        return new Guna2Panel
        {
            Width = width,
            Height = height,
            BorderRadius = 14,
            FillColor = Color.White,
            Padding = new Padding(20),
            ShadowDecoration = { Enabled = true, Depth = 8, Shadow = new Padding(0, 0, 0, 6) }
        };
    }

    private static Guna2TextBox CreateTextBox(string placeholder)
    {
        return new Guna2TextBox
        {
            Width = 430,
            Height = 38,
            BorderRadius = 8,
            BorderThickness = 1,
            BorderColor = Color.FromArgb(205, 214, 230),
            FillColor = Color.White,
            ForeColor = Color.FromArgb(33, 37, 41),
            PlaceholderText = placeholder,
            PlaceholderForeColor = Color.FromArgb(96, 109, 130),
            Font = new Font("Segoe UI", 9.5F)
        };
    }

    private static Guna2Button CreatePrimaryButton(string text)
    {
        return new Guna2Button
        {
            Text = text,
            Width = 150,
            Height = 38,
            BorderRadius = 8,
            FillColor = Color.FromArgb(19, 74, 185),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
    }
}
