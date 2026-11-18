using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OnDayGui
{
    public class TripDetailForm : Form
    {
        public TripDetailForm(
            Schedule schedule,
            IEnumerable<Reservation> reservations,
            IEnumerable<RouteItem> routes,
            IEnumerable<ChecklistItem> checklist,
            IEnumerable<TripShare> shares)
        {
            Text = "Trip Detail";
            StartPosition = FormStartPosition.CenterParent;
            Width = 900;
            Height = 700;
            BackColor = Color.FromArgb(246, 248, 252);
            Font = new Font("Segoe UI", 10F);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.ColumnCount = 1;
            layout.Padding = new Padding(18);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            Label title = new Label();
            title.Text = schedule.Title + " 상세";
            title.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(30, 42, 62);
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleLeft;
            layout.Controls.Add(title, 0, 0);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(16, 8);
            layout.Controls.Add(tabs, 0, 1);

            var reservationRows = reservations.Select(r => new
            {
                ID = r.ReservationId,
                Type = r.ReservationType,
                Name = r.ReservationName,
                Date = r.StartDate,
                Time = r.StartTime + "-" + r.EndTime,
                Place = r.Location,
                Price = r.Price
            }).ToList();

            var routeRows = routes.OrderBy(r => r.DisplayOrder).Select(r => new
            {
                Order = r.DisplayOrder,
                Country = r.Country,
                Place = r.PlaceName,
                Category = r.Category,
                Distance = r.DistanceFromStart,
                Popularity = r.Popularity,
                Sort = r.SortType
            }).ToList();

            var checklistRows = checklist.Select(c => new
            {
                ID = c.ItemId,
                Item = c.ItemName,
                Category = c.Category,
                Checked = c.IsChecked == StatusType.Done ? "완료" : "미완료",
                Shared = c.IsShared == 1 ? "공유" : "-"
            }).ToList();

            var shareRows = shares.Select(s => new
            {
                ID = s.ShareId,
                Friend = s.FriendUserId,
                Rating = s.Rating,
                Description = s.Description,
                Date = s.CreatedDate
            }).ToList();

            tabs.TabPages.Add(BuildGridPage("예약 (" + reservationRows.Count + ")", reservationRows));
            tabs.TabPages.Add(BuildGridPage("최적 동선 (" + routeRows.Count + ")", routeRows));
            tabs.TabPages.Add(BuildGridPage("짐 체크리스트 (" + checklistRows.Count + ")", checklistRows));
            tabs.TabPages.Add(BuildGridPage("공유 기록 (" + shareRows.Count + ")", shareRows));
        }

        private TabPage BuildGridPage(string title, object data)
        {
            TabPage page = new TabPage(title);
            page.BackColor = Color.White;

            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.DataSource = data;
            page.Controls.Add(grid);

            return page;
        }
    }
}
