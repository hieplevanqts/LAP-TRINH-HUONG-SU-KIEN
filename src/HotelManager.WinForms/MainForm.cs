using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class MainForm : Form
{
    private readonly BookingService _bookingService = new();
    private readonly EmployeeProfileService _employeeProfileService = new();
    private readonly DataGridView _bookingGrid = new();
    private readonly LoginResult _loginInfo;
    private ToolStripMenuItem? _accountMenu;
    private BookingsForm? _bookingsForm;
    private CustomersForm? _customersForm;
    private PaymentsForm? _paymentsForm;
    private InvoicesForm? _invoicesForm;
    private ReportsForm? _reportsForm;
    private BookingHistoryForm? _bookingHistoryForm;
    private TabControl? _tabs;
    public bool RequestRelogin { get; private set; }

    public MainForm(LoginResult loginInfo)
    {
        _loginInfo = loginInfo;

        Text = "Quản lý khách sạn";
        Width = 1200;
        Height = 860;
        MinimumSize = new Size(1180, 800);

        var menu = BuildMenu();
        var tabs = BuildTabs();

        Controls.Add(tabs);
        Controls.Add(menu);

        Load += (_, _) => LoadBookings();
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip
        {
            BackColor = Color.FromArgb(45, 108, 223),
            ForeColor = Color.White,
            Renderer = new MenuRenderer()
        };

        var masterMenu = new ToolStripMenuItem("Danh mục") { ForeColor = Color.White };
        var roomTypesItem = new ToolStripMenuItem("Loại phòng");
        var roomsItem = new ToolStripMenuItem("Phòng");
        var servicesItem = new ToolStripMenuItem("Dịch vụ");
        servicesItem.Click += (_, _) => new ServicesForm().ShowDialog(this);
        roomTypesItem.Click += (_, _) => new RoomTypesForm().ShowDialog(this);
        roomsItem.Click += (_, _) => new RoomsForm().ShowDialog(this);
        masterMenu.DropDownItems.Add(roomTypesItem);
        masterMenu.DropDownItems.Add(roomsItem);
        masterMenu.DropDownItems.Add(servicesItem);

        var settingsMenu = new ToolStripMenuItem("Cài đặt") { ForeColor = Color.White };
        settingsMenu.Click += (_, _) => OpenSettings();

        _accountMenu = new ToolStripMenuItem
        {
            Alignment = ToolStripItemAlignment.Right,
            Image = CreateAvatarImage((string?)null, _loginInfo.FullName),
            ImageScaling = ToolStripItemImageScaling.None,
            ToolTipText = $"{_loginInfo.FullName} ({_loginInfo.RoleName})",
            ForeColor = Color.White
        };

        var editProfileItem = new ToolStripMenuItem("Chỉnh sửa thông tin");
        editProfileItem.Click += (_, _) =>
        {
            using var form = new EditProfileForm(_loginInfo.EmployeeId);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var displayName = string.IsNullOrWhiteSpace(form.UpdatedFullName) ? _loginInfo.FullName : form.UpdatedFullName;
                if (form.UpdatedAvatarImage is not null)
                {
                    ApplyAccountMenuAvatarFromImage(form.UpdatedAvatarImage, displayName);
                }
                else
                {
                    ApplyAccountMenuAvatar(form.UpdatedAvatarPath, displayName);
                }
            }
        };

        var timeSheetItem = new ToolStripMenuItem("Bảng chấm công");
        timeSheetItem.Click += (_, _) =>
        {
            using var form = new AttendanceForm(_loginInfo.EmployeeId, _loginInfo.FullName);
            form.ShowDialog(this);
        };

        var logoutItem = new ToolStripMenuItem("Đăng xuất");
        logoutItem.Click += (_, _) =>
        {
            RequestRelogin = true;
            Close();
        };

        _accountMenu.DropDownItems.Add(editProfileItem);
        _accountMenu.DropDownItems.Add(timeSheetItem);
        _accountMenu.DropDownItems.Add(new ToolStripSeparator());
        _accountMenu.DropDownItems.Add(logoutItem);

        menu.Items.Add(masterMenu);
        menu.Items.Add(settingsMenu);
        menu.Items.Add(_accountMenu);

        foreach (ToolStripItem item in menu.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                StyleMenuItem(menuItem);
            }
        }

        RefreshAccountMenuAvatar();
        return menu;
    }

    private sealed class MenuRenderer : ToolStripProfessionalRenderer
    {
        private static readonly Color TopBar = Color.FromArgb(45, 108, 223);
        private static readonly Color TopHover = Color.FromArgb(30, 92, 204);
        private static readonly Color DropHover = Color.FromArgb(231, 240, 255);

        public MenuRenderer()
            : base(new ProfessionalColorTable())
        {
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var isTopLevel = e.Item.Owner is MenuStrip;
            var backColor = isTopLevel
                ? (e.Item.Selected ? TopHover : TopBar)
                : (e.Item.Selected ? DropHover : Color.White);

            using var brush = new SolidBrush(backColor);
            e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Bounds.Size));
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            var isTopLevel = e.Item.Owner is MenuStrip;
            e.TextColor = isTopLevel ? Color.White : Color.FromArgb(33, 37, 41);
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(230, 233, 241));
            var y = e.Item.Bounds.Height / 2;
            e.Graphics.DrawLine(pen, 6, y, e.Item.Bounds.Width - 6, y);
        }
    }

    private static void StyleMenuItem(ToolStripMenuItem item)
    {
        item.ForeColor = Color.White;
        item.BackColor = Color.FromArgb(45, 108, 223);
        item.DropDown.BackColor = Color.White;

        foreach (ToolStripItem dropDownItem in item.DropDownItems)
        {
            if (dropDownItem is ToolStripMenuItem child)
            {
                child.ForeColor = Color.FromArgb(33, 37, 41);
                child.BackColor = Color.White;
                StyleMenuItem(child);
            }
        }
    }

    private TabControl BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(16, 8) };
        _tabs = tabs;

        var bookingTab = new TabPage("Đặt phòng") { Padding = new Padding(10) };
        _bookingsForm = new BookingsForm(_loginInfo);
        _bookingsForm.BookingCreated += (_, _) => _paymentsForm?.RefreshData();
        EmbedForm(bookingTab, _bookingsForm);

        var customerTab = new TabPage("Khách hàng");
        _customersForm = new CustomersForm();
        _customersForm.CustomerAdded += (_, customerId) =>
        {
            if (_tabs is not null)
            {
                _tabs.SelectedIndex = 0;
            }

            _bookingsForm?.RefreshCustomersAndSelect(customerId);
        };
        EmbedForm(customerTab, _customersForm);

        var paymentTab = new TabPage("Thanh toán");
        _paymentsForm = new PaymentsForm(_loginInfo);
        _paymentsForm.PaymentCompleted += (_, _) => RefreshAfterPayment();
        EmbedForm(paymentTab, _paymentsForm);

        var historyTab = new TabPage("Lịch sử đặt phòng");
        _bookingHistoryForm = new BookingHistoryForm();
        EmbedForm(historyTab, _bookingHistoryForm);

        var invoiceTab = new TabPage("Hóa đơn");
        _invoicesForm = new InvoicesForm();
        EmbedForm(invoiceTab, _invoicesForm);

        var reportTab = new TabPage("Báo cáo");
        _reportsForm = new ReportsForm();
        EmbedForm(reportTab, _reportsForm);

        tabs.TabPages.Add(bookingTab);
        tabs.TabPages.Add(customerTab);
        tabs.TabPages.Add(paymentTab);
        tabs.TabPages.Add(historyTab);
        tabs.TabPages.Add(invoiceTab);
        tabs.TabPages.Add(reportTab);

        return tabs;
    }

    private static void EmbedForm(TabPage tab, Form form)
    {
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        tab.Controls.Add(form);
        form.Show();
    }

    private void LoadBookings()
    {
        DataTable data = _bookingService.GetAllBookings();
        _bookingGrid.DataSource = data;
    }

    private void RefreshAfterPayment()
    {
        _bookingHistoryForm?.RefreshData();
        _invoicesForm?.RefreshData();
        _reportsForm?.RefreshData();
    }

    private void OpenSettings()
    {
        using var form = new SettingsForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _paymentsForm?.RefreshData();
        }
    }

    private void RefreshAccountMenuAvatar()
    {
        if (_accountMenu is null)
        {
            return;
        }

        var profile = _employeeProfileService.GetProfile(_loginInfo.EmployeeId);
        var displayName = string.IsNullOrWhiteSpace(profile?.FullName) ? _loginInfo.FullName : profile!.FullName;
        var avatarPath = profile?.AvatarPath;

        ApplyAccountMenuAvatar(avatarPath, displayName);
    }

    private void ApplyAccountMenuAvatar(string? avatarPath, string displayName)
    {
        if (_accountMenu is null)
        {
            return;
        }

        if (_accountMenu.Image is Image oldImage)
        {
            _accountMenu.Image = null;
            oldImage.Dispose();
        }

        _accountMenu.Image = CreateAvatarImage(avatarPath, displayName);
        _accountMenu.ToolTipText = $"{displayName} ({_loginInfo.RoleName})";
        _accountMenu.Owner?.Invalidate();
    }

    private void ApplyAccountMenuAvatarFromImage(Image sourceImage, string displayName)
    {
        if (_accountMenu is null)
        {
            return;
        }

        if (_accountMenu.Image is Image oldImage)
        {
            _accountMenu.Image = null;
            oldImage.Dispose();
        }

        _accountMenu.Image = CreateAvatarImage(sourceImage, displayName);
        _accountMenu.ToolTipText = $"{displayName} ({_loginInfo.RoleName})";
        _accountMenu.Owner?.Invalidate();
    }

    private static Bitmap CreateAvatarImage(string? avatarPath, string fullName)
    {
        if (!string.IsNullOrWhiteSpace(avatarPath) && File.Exists(avatarPath))
        {
            try
            {
                using var source = Image.FromFile(avatarPath);
                return CreateAvatarImage(source, fullName);
            }
            catch
            {
                // Fallback to initials avatar
            }
        }

        return CreateAvatarImage((Image?)null, fullName);
    }

    private static Bitmap CreateAvatarImage(Image? sourceImage, string fullName)
    {
        var bmp = new Bitmap(24, 24);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var path = new GraphicsPath();
        path.AddEllipse(0, 0, 23, 23);
        g.SetClip(path);

        if (sourceImage is not null)
        {
            g.DrawImage(sourceImage, new Rectangle(0, 0, 24, 24));
            return bmp;
        }

        var initials = GetInitials(fullName);
        using var bgBrush = new SolidBrush(Color.FromArgb(16, 54, 133));
        g.FillEllipse(bgBrush, 0, 0, 23, 23);

        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
        var rect = new RectangleF(0, 0, 24, 24);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(initials, font, textBrush, rect, format);

        return bmp;
    }

    private static string GetInitials(string fullName)
    {
        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
        }

        if (parts.Length == 1 && parts[0].Length > 0)
        {
            return char.ToUpperInvariant(parts[0][0]).ToString();
        }

        return "U";
    }
}
