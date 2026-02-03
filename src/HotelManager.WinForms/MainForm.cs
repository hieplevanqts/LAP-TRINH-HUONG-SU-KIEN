using System.Data;
using System.Drawing;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class MainForm : Form
{
    private readonly BookingService _bookingService = new();
    private readonly DataGridView _bookingGrid = new();
    private BookingsForm? _bookingsForm;
    private CustomersForm? _customersForm;
    private PaymentsForm? _paymentsForm;
    private InvoicesForm? _invoicesForm;
    private ReportsForm? _reportsForm;
    private BookingHistoryForm? _bookingHistoryForm;

    public MainForm()
    {
        Text = "Quản lý khách sạn";
        Width = 1200;
        Height = 700;

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

        menu.Items.Add(masterMenu);
        menu.Items.Add(settingsMenu);

        foreach (ToolStripItem item in menu.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                StyleMenuItem(menuItem);
            }
        }

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

        var bookingTab = new TabPage("Đặt phòng") { Padding = new Padding(10) };
        _bookingsForm = new BookingsForm();
        EmbedForm(bookingTab, _bookingsForm);

        var customerTab = new TabPage("Khách hàng");
        _customersForm = new CustomersForm();
        EmbedForm(customerTab, _customersForm);

        var paymentTab = new TabPage("Thanh toán");
        _paymentsForm = new PaymentsForm();
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
        form.ShowDialog(this);
    }
}
