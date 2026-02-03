using System.Data;
using HotelManager.BLL;

namespace HotelManager.WinForms;

public sealed class MainForm : Form
{
    private readonly BookingService _bookingService = new();
    private readonly DataGridView _bookingGrid = new();

    public MainForm()
    {
        Text = "Hotel Management";
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
        var menu = new MenuStrip();

        var masterMenu = new ToolStripMenuItem("Danh muc");
        masterMenu.DropDownItems.Add("Loai phong");
        masterMenu.DropDownItems.Add("Phong");
        masterMenu.DropDownItems.Add("Dich vu");

        var bookingMenu = new ToolStripMenuItem("Dat phong");
        bookingMenu.DropDownItems.Add("Dat phong");
        bookingMenu.DropDownItems.Add("Check-in");
        bookingMenu.DropDownItems.Add("Check-out");

        var reportMenu = new ToolStripMenuItem("Bao cao");
        reportMenu.DropDownItems.Add("Doanh thu");
        reportMenu.DropDownItems.Add("Cong suat phong");

        menu.Items.Add(masterMenu);
        menu.Items.Add(bookingMenu);
        menu.Items.Add(reportMenu);

        return menu;
    }

    private TabControl BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(16, 8) };

        var bookingTab = new TabPage("Dat phong") { Padding = new Padding(10) };
        _bookingGrid.Dock = DockStyle.Fill;
        bookingTab.Controls.Add(_bookingGrid);

        var customerTab = new TabPage("Khach hang");
        var invoiceTab = new TabPage("Hoa don");
        var reportTab = new TabPage("Bao cao");

        tabs.TabPages.Add(bookingTab);
        tabs.TabPages.Add(customerTab);
        tabs.TabPages.Add(invoiceTab);
        tabs.TabPages.Add(reportTab);

        return tabs;
    }

    private void LoadBookings()
    {
        DataTable data = _bookingService.GetAllBookings();
        _bookingGrid.DataSource = data;
    }
}
