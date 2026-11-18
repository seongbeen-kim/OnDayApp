using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OnDayGui
{
    public class RouteSelectForm : Form
    {
        private readonly List<Place> sourcePlaces;
        private readonly DataGridView grid = new DataGridView();
        private readonly ComboBox sortCombo = new ComboBox();
        private readonly NumericUpDown randomCount = new NumericUpDown();

        public List<Place> SelectedPlaces { get; private set; }
        public string SortType { get; private set; }
        public bool UseRandom { get; private set; }
        public int RandomCount { get; private set; }

        public RouteSelectForm(string country, List<Place> places)
        {
            sourcePlaces = places;
            SelectedPlaces = new List<Place>();
            SortType = "거리순";
            RandomCount = Math.Min(5, Math.Max(1, places.Count));

            Text = "동선 장소 선택";
            StartPosition = FormStartPosition.CenterParent;
            Width = 820;
            Height = 560;
            BackColor = Color.FromArgb(246, 248, 252);
            Font = new Font("Segoe UI", 10F);

            Label title = new Label();
            title.Text = country + " 장소 선택";
            title.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(30, 42, 62);
            title.Left = 24;
            title.Top = 20;
            title.Width = 420;
            title.Height = 44;
            Controls.Add(title);

            Label hint = new Label();
            hint.Text = "표에서 장소를 선택해 저장하거나, 원하는 개수만큼 랜덤으로 저장할 수 있습니다.";
            hint.ForeColor = Color.FromArgb(106, 119, 143);
            hint.Left = 26;
            hint.Top = 66;
            hint.Width = 600;
            hint.Height = 26;
            Controls.Add(hint);

            Label sortLabel = new Label();
            sortLabel.Text = "정렬";
            sortLabel.Left = 26;
            sortLabel.Top = 104;
            sortLabel.Width = 48;
            sortLabel.Height = 28;
            sortLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(sortLabel);

            sortCombo.Left = 78;
            sortCombo.Top = 102;
            sortCombo.Width = 130;
            sortCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            sortCombo.Items.AddRange(new object[] { "거리순", "인기순" });
            sortCombo.SelectedIndex = 0;
            sortCombo.SelectedIndexChanged += delegate { RefreshGrid(); };
            Controls.Add(sortCombo);

            Label countLabel = new Label();
            countLabel.Text = "랜덤 개수";
            countLabel.Left = 232;
            countLabel.Top = 104;
            countLabel.Width = 84;
            countLabel.Height = 28;
            countLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(countLabel);

            randomCount.Left = 320;
            randomCount.Top = 102;
            randomCount.Width = 72;
            randomCount.Minimum = 1;
            randomCount.Maximum = Math.Max(1, places.Count);
            randomCount.Value = RandomCount;
            Controls.Add(randomCount);

            grid.Left = 24;
            grid.Top = 146;
            grid.Width = 750;
            grid.Height = 300;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = true;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Controls.Add(grid);

            Button selected = MakeButton("선택 저장", Color.FromArgb(38, 103, 255), Color.White, 424, 468, 110, 38);
            selected.Click += delegate
            {
                CaptureSelection();
                if (SelectedPlaces.Count == 0)
                {
                    MessageBox.Show(this, "저장할 장소를 하나 이상 선택하세요.", "동선 저장");
                    return;
                }
                UseRandom = false;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(selected);

            Button random = MakeButton("랜덤 저장", Color.FromArgb(229, 237, 255), Color.FromArgb(38, 103, 255), 548, 468, 110, 38);
            random.Click += delegate
            {
                UseRandom = true;
                SortType = sortCombo.SelectedItem.ToString();
                RandomCount = (int)randomCount.Value;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(random);

            Button cancel = MakeButton("취소", Color.FromArgb(232, 236, 244), Color.FromArgb(30, 42, 62), 672, 468, 90, 38);
            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(cancel);

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            SortType = sortCombo.SelectedItem == null ? "거리순" : sortCombo.SelectedItem.ToString();
            IEnumerable<Place> rows = sourcePlaces;
            rows = SortType == "인기순"
                ? rows.OrderByDescending(p => p.RecommendScore).ThenBy(p => p.DistanceFromStart)
                : rows.OrderBy(p => p.DistanceFromStart).ThenByDescending(p => p.RecommendScore);

            grid.DataSource = rows.Select(p => new
            {
                ID = p.PlaceId,
                Place = p.PlaceName,
                Category = p.Category,
                Distance = p.DistanceFromStart,
                Popularity = p.RecommendScore,
                Address = p.Address
            }).ToList();
        }

        private void CaptureSelection()
        {
            SortType = sortCombo.SelectedItem == null ? "거리순" : sortCombo.SelectedItem.ToString();
            SelectedPlaces.Clear();
            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                int id;
                object value = row.Cells["ID"].Value;
                if (value != null && Int32.TryParse(value.ToString(), out id))
                {
                    Place place = sourcePlaces.FirstOrDefault(p => p.PlaceId == id);
                    if (place != null && !SelectedPlaces.Any(p => p.PlaceId == id))
                    {
                        SelectedPlaces.Add(place);
                    }
                }
            }
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
