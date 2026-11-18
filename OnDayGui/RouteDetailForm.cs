using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OnDayGui
{
    public class RouteDetailForm : Form
    {
        private readonly List<Schedule> schedules;
        private readonly List<RouteItem> routes;
        private readonly ComboBox scheduleCombo = new ComboBox();
        private readonly ComboBox sortCombo = new ComboBox();
        private readonly DataGridView grid = new DataGridView();

        public RouteDetailForm(List<Schedule> travelSchedules, List<RouteItem> routeItems, int defaultScheduleId)
        {
            schedules = travelSchedules;
            routes = routeItems;

            Text = "동선 상세";
            StartPosition = FormStartPosition.CenterParent;
            Width = 860;
            Height = 620;
            BackColor = Color.FromArgb(246, 248, 252);
            Font = new Font("Segoe UI", 10F);

            Label title = new Label();
            title.Text = "동선 상세";
            title.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(30, 42, 62);
            title.Left = 24;
            title.Top = 20;
            title.Width = 360;
            title.Height = 44;
            Controls.Add(title);

            Label scheduleLabel = new Label();
            scheduleLabel.Text = "여행 일정";
            scheduleLabel.Left = 26;
            scheduleLabel.Top = 82;
            scheduleLabel.Width = 78;
            scheduleLabel.Height = 28;
            scheduleLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(scheduleLabel);

            scheduleCombo.Left = 110;
            scheduleCombo.Top = 80;
            scheduleCombo.Width = 310;
            scheduleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (Schedule schedule in schedules)
            {
                scheduleCombo.Items.Add(ScheduleOptionText(schedule));
            }
            int selectedIndex = schedules.FindIndex(s => s.ScheduleId == defaultScheduleId);
            scheduleCombo.SelectedIndex = selectedIndex >= 0 ? selectedIndex : (scheduleCombo.Items.Count > 0 ? 0 : -1);
            scheduleCombo.SelectedIndexChanged += delegate { RefreshGrid(); };
            Controls.Add(scheduleCombo);

            Label sortLabel = new Label();
            sortLabel.Text = "정렬";
            sortLabel.Left = 446;
            sortLabel.Top = 82;
            sortLabel.Width = 48;
            sortLabel.Height = 28;
            sortLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(sortLabel);

            sortCombo.Left = 496;
            sortCombo.Top = 80;
            sortCombo.Width = 130;
            sortCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            sortCombo.Items.AddRange(new object[] { "거리순", "인기순" });
            sortCombo.SelectedIndex = 0;
            sortCombo.SelectedIndexChanged += delegate { RefreshGrid(); };
            Controls.Add(sortCombo);

            grid.Left = 24;
            grid.Top = 130;
            grid.Width = 800;
            grid.Height = 380;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Controls.Add(grid);

            Button close = MakeButton("닫기", Color.FromArgb(38, 103, 255), Color.White, 734, 530, 90, 38);
            close.Click += delegate { Close(); };
            Controls.Add(close);

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            int scheduleId = SelectedScheduleId();
            string sortType = sortCombo.SelectedItem == null ? "거리순" : sortCombo.SelectedItem.ToString();
            IEnumerable<RouteItem> query = routes.Where(r => r.TripScheduleId == scheduleId);
            query = sortType == "인기순"
                ? query.OrderByDescending(r => r.Popularity).ThenBy(r => r.DistanceFromStart)
                : query.OrderBy(r => r.DistanceFromStart).ThenByDescending(r => r.Popularity);

            grid.DataSource = query.Select((r, index) => new
            {
                Order = index + 1,
                Country = r.Country,
                Place = r.PlaceName,
                Category = r.Category,
                Distance = r.DistanceFromStart,
                Popularity = r.Popularity,
                SavedSort = r.SortType
            }).ToList();
        }

        private int SelectedScheduleId()
        {
            if (scheduleCombo.SelectedIndex < 0 || scheduleCombo.SelectedIndex >= schedules.Count)
            {
                return 0;
            }
            return schedules[scheduleCombo.SelectedIndex].ScheduleId;
        }

        private static string ScheduleOptionText(Schedule schedule)
        {
            return schedule.Title + " (" + schedule.Date + ")";
        }

        private static Button MakeButton(string text, Color back, Color fore, int x, int y, int w, int h)
        {
            Button button = new Button();
            button.Text = text;
            button.Left = x;
            button.Top = y;
            button.Width = w;
            button.Height = h;
            button.BackColor = back;
            button.ForeColor = fore;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            return button;
        }
    }
}
