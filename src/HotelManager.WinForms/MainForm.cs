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

    private ToolStripMenuItem? _masterMenu;
    private ToolStripMenuItem? _settingsMenu;
    private ToolStripMenuItem? _aboutMenu;
    private ToolStripMenuItem? _accountMenu;
    private ToolStripMenuItem? _activeTopMenu;

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

        _masterMenu = new ToolStripMenuItem("Danh mục (F7)") { ForeColor = Color.White };
        var roomTypesItem = new ToolStripMenuItem("Loại phòng");
        var roomsItem = new ToolStripMenuItem("Phòng");
        var servicesItem = new ToolStripMenuItem("Dịch vụ");
        servicesItem.Click += (_, _) => new ServicesForm().ShowDialog(this);
        roomTypesItem.Click += (_, _) => new RoomTypesForm().ShowDialog(this);
        roomsItem.Click += (_, _) => new RoomsForm().ShowDialog(this);
        _masterMenu.DropDownItems.Add(roomTypesItem);
        _masterMenu.DropDownItems.Add(roomsItem);
        _masterMenu.DropDownItems.Add(servicesItem);
        _masterMenu.DropDownOpened += (_, _) => SetActiveTopMenu(_masterMenu);
        _masterMenu.DropDownClosed += (_, _) => ClearActiveTopMenu(_masterMenu);

        _settingsMenu = new ToolStripMenuItem("Cài đặt (F8)") { ForeColor = Color.White };
        _settingsMenu.Click += (_, _) =>
        {
            SetActiveTopMenu(_settingsMenu);
            OpenSettings();
        };

        _aboutMenu = new ToolStripMenuItem("About (F9)") { ForeColor = Color.White };
        _aboutMenu.Click += (_, _) =>
        {
            SetActiveTopMenu(_aboutMenu);
            ShowAbout();
        };

        _accountMenu = new ToolStripMenuItem
        {
            Text = $"{_loginInfo.FullName} (F10)",
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
        _accountMenu.DropDownOpened += (_, _) => SetActiveTopMenu(_accountMenu);
        _accountMenu.DropDownClosed += (_, _) => ClearActiveTopMenu(_accountMenu);

        _accountMenu.DropDownItems.Add(editProfileItem);
        _accountMenu.DropDownItems.Add(timeSheetItem);
        _accountMenu.DropDownItems.Add(new ToolStripSeparator());
        _accountMenu.DropDownItems.Add(logoutItem);

        menu.Items.Add(_masterMenu);
        menu.Items.Add(_settingsMenu);
        menu.Items.Add(_aboutMenu);
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
        private static readonly Color TopActive = Color.FromArgb(21, 74, 170);
        private static readonly Color DropHover = Color.FromArgb(231, 240, 255);

        public MenuRenderer()
            : base(new ProfessionalColorTable())
        {
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var isTopLevel = e.Item.Owner is MenuStrip;
            var backColor = isTopLevel
                ? (e.Item is ToolStripMenuItem menuItem && menuItem.Checked
                    ? TopActive
                    : (e.Item.Selected ? TopHover : TopBar))
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

    private void SetActiveTopMenu(ToolStripMenuItem? menuItem)
    {
        if (menuItem is null)
        {
            return;
        }

        if (_activeTopMenu is not null && !ReferenceEquals(_activeTopMenu, menuItem))
        {
            _activeTopMenu.Checked = false;
        }

        menuItem.Checked = true;
        _activeTopMenu = menuItem;
        menuItem.Owner?.Invalidate();
    }

    private void ClearActiveTopMenu(ToolStripMenuItem? menuItem)
    {
        if (menuItem is null || !ReferenceEquals(_activeTopMenu, menuItem))
        {
            return;
        }

        menuItem.Checked = false;
        _activeTopMenu = null;
        menuItem.Owner?.Invalidate();
    }

    private TabControl BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(16, 8) };
        _tabs = tabs;

        var bookingTab = new TabPage("Đặt phòng (F1)") { Padding = new Padding(10) };
        _bookingsForm = new BookingsForm(_loginInfo);
        _bookingsForm.BookingCreated += (_, _) => _paymentsForm?.RefreshData();
        EmbedForm(bookingTab, _bookingsForm);

        var customerTab = new TabPage("Khách hàng (F2)");
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

        var paymentTab = new TabPage("Thanh toán (F3)");
        _paymentsForm = new PaymentsForm(_loginInfo);
        _paymentsForm.PaymentCompleted += (_, _) => RefreshAfterPayment();
        EmbedForm(paymentTab, _paymentsForm);

        var historyTab = new TabPage("Lịch sử đặt phòng (F4)");
        _bookingHistoryForm = new BookingHistoryForm();
        EmbedForm(historyTab, _bookingHistoryForm);

        var invoiceTab = new TabPage("Hóa đơn (F5)");
        _invoicesForm = new InvoicesForm();
        EmbedForm(invoiceTab, _invoicesForm);

        var reportTab = new TabPage("Báo cáo (F6)");
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

        ClearActiveTopMenu(_settingsMenu);
    }

    private void ShowAbout()
    {
        const string message =
            "Hotel Manager là phần mềm quản lý khách sạn, hỗ trợ vận hành hiệu quả các nghiệp vụ " +
            "đặt phòng, thanh toán, hóa đơn và báo cáo.\n\n" +
            "Phần mềm được phát triển bởi nhóm sinh viên lớp CNT422 - Trường Đại học Mở Hà Nội:\n" +
            "• Lê Văn Hiệp\n" +
            "• Lưu Quang Huy\n" +
            "• Đặng Hoàng Nhật\n" +
            "• Đỗ Ngọc Phúc\n" +
            "• Nguyễn Danh Thành";

        MessageBox.Show(
            message,
            "About - Hotel Manager",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        ClearActiveTopMenu(_aboutMenu);
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
        _accountMenu.Text = $"{displayName} (F10)";
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
        _accountMenu.Text = $"{displayName} (F10)";
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

    private bool OpenMenu(ToolStripMenuItem? menuItem)
    {
        if (menuItem is null)
        {
            return false;
        }

        if (menuItem.HasDropDownItems)
        {
            menuItem.ShowDropDown();
        }
        else
        {
            menuItem.PerformClick();
        }

        return true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var keyCode = keyData & Keys.KeyCode;

        switch (keyCode)
        {
            case Keys.F7:
                SetActiveTopMenu(_masterMenu);
                return OpenMenu(_masterMenu);
            case Keys.F8:
                SetActiveTopMenu(_settingsMenu);
                return OpenMenu(_settingsMenu);
            case Keys.F9:
                SetActiveTopMenu(_aboutMenu);
                return OpenMenu(_aboutMenu);
            case Keys.F10:
                SetActiveTopMenu(_accountMenu);
                return OpenMenu(_accountMenu);
        }

        var tabIndex = keyCode switch
        {
            Keys.F1 => 0,
            Keys.F2 => 1,
            Keys.F3 => 2,
            Keys.F4 => 3,
            Keys.F5 => 4,
            Keys.F6 => 5,
            _ => -1
        };

        if (tabIndex < 0 || _tabs is null || tabIndex >= _tabs.TabPages.Count)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }

        _tabs.SelectedIndex = tabIndex;
        return true;
    }
}
