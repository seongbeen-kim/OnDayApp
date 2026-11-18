using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OnDayGui
{
    public class MainForm : Form
    {
        private readonly DataStore store = new DataStore();
        private readonly Panel root = new Panel();
        private readonly Timer alarmTimer = new Timer();
        private readonly HashSet<int> shownAlarms = new HashSet<int>();
        private readonly Random random = new Random();
        private const string PublicShareTarget = "PUBLIC";
        private User currentUser;
        private string reviewKeyword = "";

        private readonly Color navy = Color.FromArgb(8, 25, 58);
        private readonly Color blue = Color.FromArgb(38, 103, 255);
        private readonly Color paleBlue = Color.FromArgb(237, 244, 255);
        private readonly Color pageBack = Color.FromArgb(246, 248, 252);
        private readonly Color textDark = Color.FromArgb(30, 42, 62);
        private readonly Color textMuted = Color.FromArgb(106, 119, 143);

        public MainForm()
        {
            Text = "OnDay GUI";
            Width = 1180;
            Height = 760;
            MinimumSize = new Size(1040, 680);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);
            BackColor = pageBack;

            root.Dock = DockStyle.Fill;
            Controls.Add(root);

            store.LoadAll();
            alarmTimer.Interval = 5000;
            alarmTimer.Tick += delegate { CheckDueAlarms(); };
            alarmTimer.Start();
            ShowAuthView();
        }

        private void ShowAuthView()
        {
            root.Controls.Clear();
            root.BackColor = pageBack;

            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill;
            page.ColumnCount = 3;
            page.RowCount = 3;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 520));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 520));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.Controls.Add(page);

            Panel card = new Panel();
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(28);
            page.Controls.Add(card, 1, 1);

            Label brand = new Label();
            brand.Text = "OnDay";
            brand.ForeColor = blue;
            brand.Font = new Font("Segoe UI", 34F, FontStyle.Bold);
            brand.Left = 34;
            brand.Top = 28;
            brand.Width = 240;
            brand.Height = 58;
            card.Controls.Add(brand);

            Label message = new Label();
            message.Text = "여행과 일상을 하나의 앱으로";
            message.ForeColor = textDark;
            message.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            message.Left = 38;
            message.Top = 92;
            message.Width = 360;
            message.Height = 34;
            card.Controls.Add(message);

            Label detail = new Label();
            detail.Text = "일정, 경비, 준비물, 공유, 리뷰를 클릭으로 관리합니다.";
            detail.ForeColor = textMuted;
            detail.Left = 40;
            detail.Top = 126;
            detail.Width = 420;
            detail.Height = 28;
            card.Controls.Add(detail);

            TextBox id = MakeInput(card, 40, 190, 420, "아이디");
            TextBox password = MakeInput(card, 40, 248, 420, "비밀번호");
            password.UseSystemPasswordChar = true;
            TextBox nickname = MakeInput(card, 40, 306, 420, "닉네임(회원가입 때 사용)");
            card.Controls.Add(id);
            card.Controls.Add(password);
            card.Controls.Add(nickname);

            Button login = MakeButton("로그인", blue, Color.White, 40, 374, 200, 44);
            login.Click += delegate { Login(id.Text, password.Text); };
            card.Controls.Add(login);

            Button signup = MakeButton("회원가입", Color.FromArgb(229, 237, 255), blue, 260, 374, 200, 44);
            signup.Click += delegate { SignUp(id.Text, password.Text, nickname.Text); };
            card.Controls.Add(signup);

            Label hint = new Label();
            hint.Text = "기존 콘솔 버전의 data 폴더와 같은 저장 파일을 사용합니다.";
            hint.ForeColor = textMuted;
            hint.Left = 40;
            hint.Top = 444;
            hint.Width = 420;
            hint.Height = 34;
            card.Controls.Add(hint);
        }

        private TextBox MakeInput(Control parent, int x, int y, int w, string label)
        {
            TextBox box = new TextBox();
            box.Left = x;
            box.Top = y;
            box.Width = w;
            box.Height = 32;
            box.Font = new Font("Segoe UI", 11F);

            Label caption = new Label();
            caption.Text = label;
            caption.Left = x;
            caption.Top = y - 24;
            caption.Width = w;
            caption.Height = 20;
            caption.ForeColor = textMuted;
            parent.Controls.Add(caption);

            return box;
        }

        private void Login(string userId, string password)
        {
            User user = store.Users.FirstOrDefault(u => u.UserId == userId.Trim() && u.Password == password.Trim());
            if (user == null)
            {
                MessageBox.Show(this, "아이디와 비밀번호를 확인하세요.", "로그인 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentUser = user;
            ShowHomeView();
        }

        private void SignUp(string userId, string password, string nickname)
        {
            userId = CleanInput(userId);
            password = CleanInput(password);
            nickname = CleanInput(nickname);

            if (userId.Length == 0 || password.Length == 0 || nickname.Length == 0)
            {
                MessageBox.Show(this, "아이디, 비밀번호, 닉네임을 모두 입력하세요.", "회원가입", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (store.FindUser(userId) != null)
            {
                MessageBox.Show(this, "이미 존재하는 아이디입니다.", "회원가입", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            User user = new User { UserId = userId, Password = password, Nickname = nickname };
            store.Users.Add(user);
            store.SaveAll();
            currentUser = user;
            ShowHomeView();
        }

        private void ShowShell(string title, string active, Action<FlowLayoutPanel> buildContent)
        {
            root.Controls.Clear();
            root.BackColor = pageBack;

            TableLayoutPanel shell = new TableLayoutPanel();
            shell.Dock = DockStyle.Fill;
            shell.ColumnCount = 2;
            shell.RowCount = 1;
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 224));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(shell);

            Panel side = new Panel();
            side.Dock = DockStyle.Fill;
            side.BackColor = navy;
            shell.Controls.Add(side, 0, 0);

            Label logo = new Label();
            logo.Text = "OnDay";
            logo.ForeColor = Color.White;
            logo.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            logo.Left = 24;
            logo.Top = 26;
            logo.Width = 170;
            logo.Height = 50;
            side.Controls.Add(logo);

            Label tagline = new Label();
            tagline.Text = "Switch On";
            tagline.ForeColor = Color.FromArgb(162, 185, 236);
            tagline.Left = 28;
            tagline.Top = 76;
            tagline.Width = 160;
            tagline.Height = 24;
            side.Controls.Add(tagline);

            int navTop = 126;
            AddNavButton(side, "홈", "Home", active, navTop, delegate { ShowHomeView(); });
            AddNavButton(side, "여행 모드", "Travel", active, navTop + 54, delegate { ShowTravelView(); });
            AddNavButton(side, "데일리 모드", "Daily", active, navTop + 108, delegate { ShowDailyView(); });
            AddNavButton(side, "친구/공유", "Friends", active, navTop + 162, delegate { ShowFriendsView(); });
            AddNavButton(side, "리뷰", "Reviews", active, navTop + 216, delegate { ShowReviewsView(); });
            AddNavButton(side, "설정", "Settings", active, navTop + 270, delegate { ShowSettingsView(); });

            Label user = new Label();
            user.Text = currentUser == null ? "" : currentUser.Nickname + "\n" + currentUser.UserId;
            user.ForeColor = Color.White;
            user.Left = 24;
            user.Top = Height - 150;
            user.Width = 170;
            user.Height = 58;
            user.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            side.Controls.Add(user);

            Panel contentWrap = new Panel();
            contentWrap.Dock = DockStyle.Fill;
            contentWrap.BackColor = pageBack;
            shell.Controls.Add(contentWrap, 1, 0);

            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.ColumnCount = 1;
            body.RowCount = 2;
            body.BackColor = pageBack;
            body.Padding = new Padding(28, 20, 28, 20);
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            contentWrap.Controls.Add(body);

            Label header = new Label();
            header.Text = title;
            header.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            header.ForeColor = textDark;
            header.Dock = DockStyle.Fill;
            header.TextAlign = ContentAlignment.MiddleLeft;
            body.Controls.Add(header, 0, 0);

            FlowLayoutPanel content = new FlowLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.AutoScroll = true;
            content.FlowDirection = FlowDirection.TopDown;
            content.WrapContents = false;
            content.BackColor = pageBack;
            body.Controls.Add(content, 0, 1);

            buildContent(content);
        }

        private void AddNavButton(Panel side, string text, string key, string active, int top, EventHandler click)
        {
            bool selected = key == active;
            Button button = new Button();
            button.Text = text;
            button.Left = 20;
            button.Top = top;
            button.Width = 184;
            button.Height = 42;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = selected ? blue : navy;
            button.ForeColor = Color.White;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(16, 0, 0, 0);
            button.Font = new Font("Segoe UI", 10F, selected ? FontStyle.Bold : FontStyle.Regular);
            button.Click += click;
            side.Controls.Add(button);
        }

        private Panel AddCard(FlowLayoutPanel parent, string title, int height)
        {
            Panel card = new Panel();
            card.Width = 900;
            card.Height = height;
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Margin = new Padding(0, 0, 18, 18);

            Label label = new Label();
            label.Text = title;
            label.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label.ForeColor = textDark;
            label.Left = 18;
            label.Top = 14;
            label.Width = 500;
            label.Height = 32;
            card.Controls.Add(label);

            parent.Controls.Add(card);
            return card;
        }

        private Button MakeButton(string text, Color back, Color fore, int x, int y, int width, int height)
        {
            Button button = new Button();
            button.Text = text;
            button.Left = x;
            button.Top = y;
            button.Width = width;
            button.Height = height;
            button.BackColor = back;
            button.ForeColor = fore;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            return button;
        }

        private DataGridView MakeGrid(object data, int x, int y, int width, int height)
        {
            DataGridView grid = new DataGridView();
            grid.Left = x;
            grid.Top = y;
            grid.Width = width;
            grid.Height = height;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.DataSource = data;
            return grid;
        }

        private int SelectedId(DataGridView grid)
        {
            if (grid.SelectedRows.Count == 0)
            {
                return 0;
            }

            object value = grid.SelectedRows[0].Cells[0].Value;
            int id;
            return value != null && Int32.TryParse(value.ToString(), out id) ? id : 0;
        }

        private Schedule SelectedTravelSchedule(DataGridView grid)
        {
            int id = SelectedId(grid);
            return MySchedules().FirstOrDefault(s => s.ScheduleId == id && s.Mode == ModeType.Travel);
        }

        private void OpenSelectedTripDetail(DataGridView grid)
        {
            Schedule schedule = SelectedTravelSchedule(grid);
            if (schedule == null)
            {
                ShowSelectMessage();
                return;
            }
            ShowTripDetail(schedule);
        }

        private void ShowTripDetail(Schedule schedule)
        {
            using (TripDetailForm detail = new TripDetailForm(
                schedule,
                MyReservations().Where(r => r.TripScheduleId == schedule.ScheduleId),
                MyRouteItems().Where(r => r.TripScheduleId == schedule.ScheduleId),
                MyChecklist(ModeType.Travel).Where(c => c.TripScheduleId == schedule.ScheduleId || c.TripScheduleId == 0),
                MyTripShares().Where(s => s.TripScheduleId == schedule.ScheduleId)))
            {
                detail.ShowDialog(this);
            }
        }

        private void OpenSelectedRouteDetail(DataGridView grid)
        {
            int routeId = SelectedId(grid);
            RouteItem route = MyRouteItems().FirstOrDefault(r => r.RouteId == routeId);
            OpenRouteDetail(route == null ? 0 : route.TripScheduleId);
        }

        private void OpenRouteDetail(int defaultScheduleId)
        {
            List<Schedule> travelSchedules = MySchedules()
                .Where(s => s.Mode == ModeType.Travel)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.ScheduleId)
                .ToList();
            if (travelSchedules.Count == 0)
            {
                MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "동선 상세");
                return;
            }

            using (RouteDetailForm detail = new RouteDetailForm(
                travelSchedules,
                MyRouteItems().ToList(),
                defaultScheduleId))
            {
                detail.ShowDialog(this);
            }
        }

        private void OpenSharedTripDetail(int shareId)
        {
            TripShare share = AccessibleTripShares().FirstOrDefault(s => s.ShareId == shareId);
            if (share == null)
            {
                ShowSelectMessage();
                return;
            }

            Schedule schedule = store.Schedules.FirstOrDefault(s =>
                s.UserId == share.OwnerUserId &&
                s.ScheduleId == share.TripScheduleId &&
                s.Mode == ModeType.Travel);
            if (schedule == null)
            {
                MessageBox.Show(this, "공유된 여행 일정을 찾을 수 없습니다.", "공유 상세");
                return;
            }

            using (TripDetailForm detail = new TripDetailForm(
                schedule,
                store.Reservations.Where(r => r.UserId == share.OwnerUserId && r.TripScheduleId == schedule.ScheduleId),
                store.RouteItems.Where(r => r.UserId == share.OwnerUserId && r.TripScheduleId == schedule.ScheduleId),
                store.Checklist.Where(c => c.UserId == share.OwnerUserId && c.Mode == ModeType.Travel && (c.TripScheduleId == schedule.ScheduleId || c.TripScheduleId == 0)),
                store.TripShares.Where(s => s.OwnerUserId == share.OwnerUserId && s.TripScheduleId == schedule.ScheduleId)))
            {
                detail.ShowDialog(this);
            }
        }

        private void ImportSharedTrip(int shareId)
        {
            TripShare share = AccessibleTripShares().FirstOrDefault(s => s.ShareId == shareId);
            if (share == null)
            {
                ShowSelectMessage();
                return;
            }

            if (share.OwnerUserId == currentUser.UserId)
            {
                MessageBox.Show(this, "내가 공유한 여행은 이미 내 여행 일정에 있습니다.", "여행 불러오기");
                return;
            }

            Schedule source = store.Schedules.FirstOrDefault(s =>
                s.UserId == share.OwnerUserId &&
                s.ScheduleId == share.TripScheduleId &&
                s.Mode == ModeType.Travel);
            if (source == null)
            {
                MessageBox.Show(this, "불러올 여행 일정을 찾을 수 없습니다.", "여행 불러오기");
                return;
            }

            int newScheduleId = store.NextScheduleId();
            store.Schedules.Add(new Schedule
            {
                ScheduleId = newScheduleId,
                UserId = currentUser.UserId,
                Mode = ModeType.Travel,
                Title = source.Title,
                Date = source.Date,
                StartTime = source.StartTime,
                EndTime = source.EndTime,
                Location = source.Location,
                Category = source.Category,
                Importance = source.Importance,
                Memo = source.Memo,
                IsCompleted = StatusType.NotDone
            });

            foreach (Reservation reservation in store.Reservations.Where(r => r.UserId == share.OwnerUserId && r.TripScheduleId == source.ScheduleId).ToList())
            {
                store.Reservations.Add(new Reservation
                {
                    ReservationId = store.NextReservationId(),
                    TripScheduleId = newScheduleId,
                    UserId = currentUser.UserId,
                    ReservationType = reservation.ReservationType,
                    ReservationName = reservation.ReservationName,
                    StartDate = reservation.StartDate,
                    StartTime = reservation.StartTime,
                    EndDate = reservation.EndDate,
                    EndTime = reservation.EndTime,
                    Location = reservation.Location,
                    Price = reservation.Price,
                    Memo = reservation.Memo
                });
            }

            foreach (RouteItem route in store.RouteItems.Where(r => r.UserId == share.OwnerUserId && r.TripScheduleId == source.ScheduleId).OrderBy(r => r.DisplayOrder).ToList())
            {
                store.RouteItems.Add(new RouteItem
                {
                    RouteId = store.NextRouteId(),
                    UserId = currentUser.UserId,
                    TripScheduleId = newScheduleId,
                    Country = route.Country,
                    PlaceName = route.PlaceName,
                    Category = route.Category,
                    DistanceFromStart = route.DistanceFromStart,
                    Popularity = route.Popularity,
                    SortType = route.SortType,
                    DisplayOrder = route.DisplayOrder
                });
            }

            foreach (ChecklistItem item in store.Checklist.Where(c => c.UserId == share.OwnerUserId && c.Mode == ModeType.Travel && (c.TripScheduleId == source.ScheduleId || c.TripScheduleId == 0)).ToList())
            {
                store.Checklist.Add(new ChecklistItem
                {
                    ItemId = store.NextChecklistId(),
                    TripScheduleId = newScheduleId,
                    UserId = currentUser.UserId,
                    Mode = ModeType.Travel,
                    ItemName = item.ItemName,
                    Category = item.Category,
                    IsChecked = item.IsChecked,
                    IsShared = 0
                });
            }

            foreach (Expense expense in store.Expenses.Where(e => e.UserId == share.OwnerUserId && e.Mode == ModeType.Travel && e.TripScheduleId == source.ScheduleId).ToList())
            {
                store.Expenses.Add(new Expense
                {
                    ExpenseId = store.NextExpenseId(),
                    TripScheduleId = newScheduleId,
                    UserId = currentUser.UserId,
                    Mode = ModeType.Travel,
                    Date = expense.Date,
                    Category = expense.Category,
                    Payer = expense.Payer,
                    ParticipantCount = expense.ParticipantCount,
                    Amount = expense.Amount,
                    Memo = expense.Memo
                });
            }

            store.SaveAll();
            MessageBox.Show(this, source.Title + " 일정과 상세 내용을 내 여행 일정으로 불러왔습니다.", "여행 불러오기");
            ShowTravelView();
        }

        private string FirstTravelScheduleIdText()
        {
            Schedule schedule = MySchedules()
                .Where(s => s.Mode == ModeType.Travel)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.ScheduleId)
                .FirstOrDefault();
            return schedule == null ? "0" : schedule.ScheduleId.ToString();
        }

        private string[] TravelScheduleOptions()
        {
            return MySchedules()
                .Where(s => s.Mode == ModeType.Travel)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.ScheduleId)
                .Select(s => ScheduleOptionText(s))
                .ToArray();
        }

        private string TravelScheduleOptionFor(int scheduleId)
        {
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == scheduleId && s.Mode == ModeType.Travel);
            string[] options = TravelScheduleOptions();
            if (schedule != null)
            {
                return ScheduleOptionText(schedule);
            }
            return options.Length == 0 ? "" : options[0];
        }

        private string FirstTravelScheduleOption()
        {
            string[] options = TravelScheduleOptions();
            return options.Length == 0 ? "" : options[0];
        }

        private string ScheduleOptionText(Schedule schedule)
        {
            return schedule.Title + " (" + schedule.Date + ")";
        }

        private int ParseScheduleOption(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return 0;
            }

            Schedule selected = MySchedules()
                .Where(s => s.Mode == ModeType.Travel)
                .FirstOrDefault(s => ScheduleOptionText(s) == value);
            if (selected != null)
            {
                return selected.ScheduleId;
            }

            string first = value.Split('|')[0].Trim();
            return ParseInt(first, 0);
        }

        private string[] CountryOptions()
        {
            return new string[] { "japan", "jeju", "seoul", "busan", "china", "australia" };
        }

        private void ShowHomeView()
        {
            ShowShell("홈 대시보드", "Home", delegate (FlowLayoutPanel content)
            {
                Panel summary = AddCard(content, "오늘 요약", 210);
                string today = DateTime.Today.ToString("yyyy-MM-dd");
                int todaySchedules = MySchedules().Count(s => s.Date == today);
                int todoTotal = MyTodos().Count();
                int todoDone = MyTodos().Count(t => t.IsCompleted == StatusType.Done);
                int checkTotal = MyChecklist(null).Count();
                int checkDone = MyChecklist(null).Count(c => c.IsChecked == StatusType.Done);
                int travelExpense = MyExpenses(ModeType.Travel).Sum(e => e.Amount);

                AddMetric(summary, "오늘 일정", todaySchedules.ToString(), 22, 62);
                AddMetric(summary, "Todo 완료", todoDone + " / " + todoTotal, 232, 62);
                AddMetric(summary, "준비물 체크", checkDone + " / " + checkTotal, 442, 62);
                AddMetric(summary, "여행 경비", travelExpense.ToString("N0") + "원", 652, 62);

                Panel preview = AddCard(content, "다가오는 일정", 300);
                var rows = MySchedules()
                    .OrderBy(s => s.Date)
                    .ThenBy(s => s.StartTime)
                    .Take(8)
                    .Select(s => new
                    {
                        ID = s.ScheduleId,
                        Mode = ModeText(s.Mode),
                        Date = s.Date,
                        Time = s.StartTime + "-" + s.EndTime,
                        Title = s.Title,
                        Place = s.Location,
                        Status = StatusText(s.IsCompleted)
                    })
                    .ToList();
                preview.Controls.Add(MakeGrid(rows, 18, 60, 840, 210));

                Panel quick = AddCard(content, "빠른 실행", 140);
                Button travel = MakeButton("여행 일정 추가", blue, Color.White, 20, 62, 170, 42);
                travel.Click += delegate { AddSchedule(ModeType.Travel, ShowHomeView); };
                quick.Controls.Add(travel);

                Button todo = MakeButton("Todo 추가", Color.FromArgb(229, 237, 255), blue, 210, 62, 150, 42);
                todo.Click += delegate { AddTodo(ShowHomeView); };
                quick.Controls.Add(todo);

                Button review = MakeButton("리뷰 작성", Color.FromArgb(229, 237, 255), blue, 380, 62, 150, 42);
                review.Click += delegate { AddReview(ShowHomeView); };
                quick.Controls.Add(review);
            });
        }

        private void AddMetric(Panel parent, string label, string value, int x, int y)
        {
            Panel tile = new Panel();
            tile.Left = x;
            tile.Top = y;
            tile.Width = 180;
            tile.Height = 104;
            tile.BackColor = paleBlue;
            parent.Controls.Add(tile);

            Label title = new Label();
            title.Text = label;
            title.ForeColor = textMuted;
            title.Left = 16;
            title.Top = 14;
            title.Width = 145;
            title.Height = 24;
            tile.Controls.Add(title);

            Label number = new Label();
            number.Text = value;
            number.ForeColor = blue;
            number.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            number.Left = 14;
            number.Top = 42;
            number.Width = 150;
            number.Height = 40;
            tile.Controls.Add(number);
        }

        private void ShowTravelView()
        {
            ShowShell("여행 모드", "Travel", delegate (FlowLayoutPanel content)
            {
                AddScheduleCard(content, ModeType.Travel, ShowTravelView);
                AddReservationCard(content);
                AddRouteCard(content);
                AddExpenseCard(content, ModeType.Travel, ShowTravelView);
                AddChecklistCard(content, ModeType.Travel, ShowTravelView);
                AddWeatherCard(content, ModeType.Travel, ShowTravelView);
            });
        }

        private void ShowDailyView()
        {
            ShowShell("데일리 모드", "Daily", delegate (FlowLayoutPanel content)
            {
                AddScheduleCard(content, ModeType.Daily, ShowDailyView);
                AddTodoCard(content);
                AddJobCard(content);
                AddSleepCard(content);
                AddChecklistCard(content, ModeType.Daily, ShowDailyView);
                AddWeatherCard(content, ModeType.Daily, ShowDailyView);
                AddExpenseCard(content, ModeType.Daily, ShowDailyView);
            });
        }

        private void AddScheduleCard(FlowLayoutPanel content, ModeType mode, Action refresh)
        {
            Panel card = AddCard(content, mode == ModeType.Travel ? "여행 일정 관리" : "시간표 일정 관리", 330);
            Button add = MakeButton("일정 추가", blue, Color.White, 18, 58, 120, 38);
            add.Click += delegate { AddSchedule(mode, refresh); };
            card.Controls.Add(add);

            Button toggle = MakeButton("완료 처리", Color.FromArgb(229, 237, 255), blue, 150, 58, 120, 38);
            card.Controls.Add(toggle);

            Button detail = null;
            Button reservation = null;
            if (mode == ModeType.Travel)
            {
                detail = MakeButton("상세 보기", Color.FromArgb(229, 237, 255), blue, 282, 58, 120, 38);
                card.Controls.Add(detail);
                reservation = MakeButton("예약 등록", Color.FromArgb(229, 237, 255), blue, 414, 58, 120, 38);
                card.Controls.Add(reservation);
            }

            var rows = MySchedules()
                .Where(s => s.Mode == mode)
                .Where(s => mode != ModeType.Travel || !s.Title.StartsWith("예약:"))
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    ID = s.ScheduleId,
                    Date = s.Date,
                    Time = s.StartTime + "-" + s.EndTime,
                    Title = s.Title,
                    Category = s.Category,
                    Place = s.Location,
                    Status = StatusText(s.IsCompleted)
                })
                .ToList();
            DataGridView grid = MakeGrid(rows, 18, 108, 840, 190);
            card.Controls.Add(grid);

            if (mode == ModeType.Travel)
            {
                grid.CellDoubleClick += delegate { OpenSelectedTripDetail(grid); };
                detail.Click += delegate { OpenSelectedTripDetail(grid); };
                reservation.Click += delegate
                {
                    Schedule selected = SelectedTravelSchedule(grid);
                    if (selected == null) { ShowSelectMessage(); return; }
                    AddReservation(selected.ScheduleId);
                };
            }

            toggle.Click += delegate
            {
                int id = SelectedId(grid);
                Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == id && s.Mode == mode);
                if (schedule == null)
                {
                    ShowSelectMessage();
                    return;
                }
                schedule.IsCompleted = schedule.IsCompleted == StatusType.Done ? StatusType.NotDone : StatusType.Done;
                store.SaveAll();
                refresh();
            };
        }

        private void AddReservationCard(FlowLayoutPanel content)
        {
            Panel card = AddCard(content, "예약 자동 등록", 300);
            Button add = MakeButton("예약 추가", blue, Color.White, 18, 58, 120, 38);
            add.Click += delegate { AddReservation(0); };
            card.Controls.Add(add);

            var rows = MyReservations()
                .OrderBy(r => r.StartDate)
                .Select(r => new
                {
                    ID = r.ReservationId,
                    TripID = r.TripScheduleId,
                    Type = r.ReservationType,
                    Name = r.ReservationName,
                    Date = r.StartDate,
                    Time = r.StartTime + "-" + r.EndTime,
                    Place = r.Location,
                    Price = r.Price
                })
                .ToList();
            card.Controls.Add(MakeGrid(rows, 18, 108, 840, 160));
        }

        private void AddRouteCard(FlowLayoutPanel content)
        {
            Panel card = AddCard(content, "동선 추가", 320);
            Button add = MakeButton("동선 추가", blue, Color.White, 18, 58, 110, 38);
            add.Click += delegate { AddRouteForTrip(0); };
            card.Controls.Add(add);

            Button placeAdd = MakeButton("장소 직접 추가", Color.FromArgb(229, 237, 255), blue, 142, 58, 130, 38);
            placeAdd.Click += delegate { AddPlaceAndAttachToTrip(); };
            card.Controls.Add(placeAdd);

            Button detail = MakeButton("상세 보기", Color.FromArgb(229, 237, 255), blue, 286, 58, 110, 38);
            card.Controls.Add(detail);

            var rows = MyRouteItems()
                .OrderBy(p => p.TripScheduleId)
                .ThenBy(p => p.DisplayOrder)
                .Select(p => new
                {
                    ID = p.RouteId,
                    TripID = p.TripScheduleId,
                    Country = p.Country,
                    Place = p.PlaceName,
                    Category = p.Category,
                    Distance = p.DistanceFromStart,
                    Popularity = p.Popularity,
                    Sort = p.SortType,
                    Order = p.DisplayOrder
                })
                .ToList();
            DataGridView grid = MakeGrid(rows, 18, 108, 840, 180);
            card.Controls.Add(grid);
            detail.Click += delegate { OpenSelectedRouteDetail(grid); };
        }

        private void AddExpenseCard(FlowLayoutPanel content, ModeType mode, Action refresh)
        {
            Panel card = AddCard(content, mode == ModeType.Travel ? "여행 경비 계산" : "일상 지출 기록", 320);
            Button add = MakeButton("지출 추가", blue, Color.White, 18, 58, 120, 38);
            add.Click += delegate { AddExpense(mode, refresh); };
            card.Controls.Add(add);

            List<Expense> expenses = MyExpenses(mode).ToList();
            int total = expenses.Sum(e => e.Amount);
            double share = expenses.Sum(e => e.ParticipantCount <= 0 ? 0 : (double)e.Amount / e.ParticipantCount);

            Label summary = new Label();
            summary.Text = "총 지출 " + total.ToString("N0") + "원 / 항목별 1인 부담 합계 " + share.ToString("N0") + "원";
            summary.Left = 154;
            summary.Top = 64;
            summary.Width = 650;
            summary.Height = 26;
            summary.ForeColor = textDark;
            card.Controls.Add(summary);

            var rows = expenses
                .OrderBy(e => e.Date)
                .Select(e => new
                {
                    ID = e.ExpenseId,
                    Date = e.Date,
                    Category = e.Category,
                    Payer = e.Payer,
                    People = e.ParticipantCount,
                    Amount = e.Amount,
                    PerPerson = e.ParticipantCount <= 0 ? 0 : e.Amount / e.ParticipantCount
                })
                .ToList();
            card.Controls.Add(MakeGrid(rows, 18, 108, 840, 180));
        }

        private void AddChecklistCard(FlowLayoutPanel content, ModeType mode, Action refresh)
        {
            Panel card = AddCard(content, mode == ModeType.Travel ? "짐 체크리스트" : "데일리 준비물 체크리스트", 340);
            Button add = MakeButton("항목 추가", blue, Color.White, 18, 58, 110, 38);
            add.Click += delegate { AddChecklistItem(mode, 0, null, null, refresh); };
            card.Controls.Add(add);

            Button recommend = MakeButton("추천 추가", Color.FromArgb(229, 237, 255), blue, 140, 58, 110, 38);
            recommend.Click += delegate { AddRecommendedChecklist(mode, 0, refresh); };
            card.Controls.Add(recommend);

            Button toggle = MakeButton("체크 변경", Color.FromArgb(229, 237, 255), blue, 262, 58, 110, 38);
            card.Controls.Add(toggle);

            List<ChecklistItem> items = MyChecklist(mode).ToList();
            int done = items.Count(c => c.IsChecked == StatusType.Done);
            Label summary = new Label();
            summary.Text = "진행률 " + done + " / " + items.Count;
            summary.Left = 520;
            summary.Top = 66;
            summary.Width = 300;
            summary.Height = 24;
            summary.ForeColor = textMuted;
            card.Controls.Add(summary);

            var rows = items
                .OrderBy(c => c.ItemId)
                .Select(c => new
                {
                    ID = c.ItemId,
                    Item = c.ItemName,
                    Category = c.Category,
                    Checked = StatusText(c.IsChecked),
                    Shared = c.IsShared == 1 ? "공유" : "-"
                })
                .ToList();
            DataGridView grid = MakeGrid(rows, 18, 108, 840, 190);
            card.Controls.Add(grid);

            toggle.Click += delegate
            {
                int id = SelectedId(grid);
                ChecklistItem item = MyChecklist(mode).FirstOrDefault(c => c.ItemId == id);
                if (item == null)
                {
                    ShowSelectMessage();
                    return;
                }
                item.IsChecked = item.IsChecked == StatusType.Done ? StatusType.NotDone : StatusType.Done;
                store.SaveAll();
                refresh();
            };
        }

        private void AddWeatherCard(FlowLayoutPanel content, ModeType mode, Action refresh)
        {
            Panel card = AddCard(content, mode == ModeType.Travel ? "여행지 우산 알림" : "일상 우산 알림", 300);
            Button add = MakeButton("날씨 추가", blue, Color.White, 18, 58, 110, 38);
            add.Click += delegate { AddWeather(refresh); };
            card.Controls.Add(add);

            Button alert = MakeButton("우산 확인", Color.FromArgb(229, 237, 255), blue, 140, 58, 110, 38);
            alert.Click += delegate { CheckWeatherAlert(mode, refresh); };
            card.Controls.Add(alert);

            var rows = store.WeatherList
                .OrderBy(w => w.Date)
                .Select(w => new
                {
                    ID = w.WeatherId,
                    Date = w.Date,
                    Region = w.Region,
                    Weather = WeatherText(w.Weather),
                    Rain = w.RainProbability + "%"
                })
                .ToList();
            card.Controls.Add(MakeGrid(rows, 18, 108, 840, 160));
        }

        private void AddTodoCard(FlowLayoutPanel content)
        {
            Panel card = AddCard(content, "Todo 관리", 330);
            Button add = MakeButton("Todo 추가", blue, Color.White, 18, 58, 110, 38);
            add.Click += delegate { AddTodo(ShowDailyView); };
            card.Controls.Add(add);

            Button toggle = MakeButton("완료 변경", Color.FromArgb(229, 237, 255), blue, 140, 58, 110, 38);
            card.Controls.Add(toggle);

            List<Todo> todos = MyTodos().OrderBy(t => t.DueDate).ToList();
            int done = todos.Count(t => t.IsCompleted == StatusType.Done);
            Label summary = new Label();
            summary.Text = "완료율 " + done + " / " + todos.Count;
            summary.Left = 270;
            summary.Top = 66;
            summary.Width = 300;
            summary.Height = 24;
            summary.ForeColor = textMuted;
            card.Controls.Add(summary);

            var rows = todos.Select(t => new
            {
                ID = t.TodoId,
                Due = t.DueDate,
                Title = t.Title,
                Importance = t.Importance,
                Status = StatusText(t.IsCompleted)
            }).ToList();
            DataGridView grid = MakeGrid(rows, 18, 108, 840, 190);
            card.Controls.Add(grid);

            toggle.Click += delegate
            {
                int id = SelectedId(grid);
                Todo todo = MyTodos().FirstOrDefault(t => t.TodoId == id);
                if (todo == null)
                {
                    ShowSelectMessage();
                    return;
                }
                todo.IsCompleted = todo.IsCompleted == StatusType.Done ? StatusType.NotDone : StatusType.Done;
                store.SaveAll();
                ShowDailyView();
            };
        }

        private void AddJobCard(FlowLayoutPanel content)
        {
            Panel card = AddCard(content, "알바비 계산", 310);
            Button add = MakeButton("근무 추가", blue, Color.White, 18, 58, 110, 38);
            add.Click += delegate { AddJob(ShowDailyView); };
            card.Controls.Add(add);

            int total = MyJobs().Sum(j => j.TotalPay);
            Label summary = new Label();
            summary.Text = "예상 급여 합계 " + total.ToString("N0") + "원";
            summary.Left = 150;
            summary.Top = 66;
            summary.Width = 360;
            summary.Height = 24;
            summary.ForeColor = textMuted;
            card.Controls.Add(summary);

            var rows = MyJobs()
                .OrderBy(j => j.WorkDate)
                .Select(j => new
                {
                    ID = j.JobId,
                    Date = j.WorkDate,
                    Job = j.JobName,
                    Time = j.StartTime + "-" + j.EndTime,
                    Hours = j.WorkHours,
                    Wage = j.HourlyWage,
                    Pay = j.TotalPay
                })
                .ToList();
            card.Controls.Add(MakeGrid(rows, 18, 108, 840, 170));
        }

        private void AddSleepCard(FlowLayoutPanel content)
        {
            Panel card = AddCard(content, "수면/기상 알림", 280);
            Button add = MakeButton("알림 추가", blue, Color.White, 18, 58, 110, 38);
            add.Click += delegate { AddSleepAlarm(ShowDailyView); };
            card.Controls.Add(add);

            var rows = MySleepAlarms()
                .OrderBy(s => s.Date)
                .Select(s => new
                {
                    ID = s.SleepId,
                    Date = s.Date,
                    Wake = s.WakeUpTime,
                    SleepHours = s.TargetSleepHours,
                    BedTime = s.RecommendedBedTime,
                    Alarm = s.IsAlarmOn == 1 ? "ON" : "OFF"
                })
                .ToList();
            card.Controls.Add(MakeGrid(rows, 18, 108, 840, 140));
        }

        private void ShowFriendsView()
        {
            ShowShell("친구/공유 관리", "Friends", delegate (FlowLayoutPanel content)
            {
                Panel friends = AddCard(content, "친구 추가", 300);
                Button add = MakeButton("친구 추가", blue, Color.White, 18, 58, 110, 38);
                add.Click += delegate { AddFriend(); };
                friends.Controls.Add(add);

                var friendRows = MyFriends().Select(f => new
                {
                    ID = f.FriendId,
                    UserID = f.FriendUserId,
                    Nickname = f.FriendNickname,
                    ScheduleShared = f.IsScheduleShared == 1 ? "공유" : "-"
                }).ToList();
                friends.Controls.Add(MakeGrid(friendRows, 18, 108, 840, 160));

                Panel requests = AddCard(content, "받은 친구 요청", 260);
                Button accept = MakeButton("요청 수락", blue, Color.White, 18, 58, 120, 38);
                requests.Controls.Add(accept);

                var requestRows = MyPendingRequests().Select(f => new
                {
                    ID = f.FriendId,
                    Requester = f.UserId,
                    Nickname = store.FindUser(f.UserId) == null ? f.UserId : store.FindUser(f.UserId).Nickname
                }).ToList();
                DataGridView requestGrid = MakeGrid(requestRows, 18, 108, 840, 120);
                requests.Controls.Add(requestGrid);
                accept.Click += delegate
                {
                    int id = SelectedId(requestGrid);
                    AcceptFriendRequest(id);
                };

                Panel shared = AddCard(content, "여행 전체 공유", 300);
                Button travel = MakeButton("친구 공유", blue, Color.White, 18, 58, 120, 38);
                travel.Click += delegate { ShareTripPackage(0); };
                shared.Controls.Add(travel);

                Button publicShare = MakeButton("공개 공유", Color.FromArgb(229, 237, 255), blue, 152, 58, 120, 38);
                publicShare.Click += delegate { ShareTripPublic(); };
                shared.Controls.Add(publicShare);

                var sharedRows = MyTripShares()
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new
                    {
                        ID = s.ShareId,
                        Trip = SharedTripTitle(s),
                        TripID = s.TripScheduleId,
                        Target = s.FriendUserId == PublicShareTarget ? "전체 공개" : s.FriendUserId,
                        Rating = s.Rating,
                        Description = s.Description,
                        Date = s.CreatedDate
                    })
                    .ToList();
                shared.Controls.Add(MakeGrid(sharedRows, 18, 108, 840, 160));

                Panel received = AddCard(content, "받은 여행 공유", 300);
                Button openReceived = MakeButton("공유 상세 보기", blue, Color.White, 18, 58, 140, 38);
                received.Controls.Add(openReceived);
                Button importReceived = MakeButton("불러오기", Color.FromArgb(229, 237, 255), blue, 172, 58, 110, 38);
                received.Controls.Add(importReceived);
                var receivedRows = SharedToMe()
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new
                    {
                        ID = s.ShareId,
                        Owner = s.OwnerUserId,
                        Trip = SharedTripTitle(s),
                        TripID = s.TripScheduleId,
                        Rating = s.Rating,
                        Description = s.Description,
                        Date = s.CreatedDate
                    })
                    .ToList();
                DataGridView receivedGrid = MakeGrid(receivedRows, 18, 108, 840, 160);
                received.Controls.Add(receivedGrid);
                openReceived.Click += delegate
                {
                    int id = SelectedId(receivedGrid);
                    OpenSharedTripDetail(id);
                };
                importReceived.Click += delegate
                {
                    int id = SelectedId(receivedGrid);
                    ImportSharedTrip(id);
                };

                Panel publicList = AddCard(content, "공개 여행 목록", 300);
                Button openPublic = MakeButton("공개 상세 보기", blue, Color.White, 18, 58, 140, 38);
                publicList.Controls.Add(openPublic);
                Button importPublic = MakeButton("불러오기", Color.FromArgb(229, 237, 255), blue, 172, 58, 110, 38);
                publicList.Controls.Add(importPublic);
                var publicRows = PublicTripShares()
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new
                    {
                        ID = s.ShareId,
                        Owner = s.OwnerUserId,
                        Trip = SharedTripTitle(s),
                        Rating = s.Rating,
                        Description = s.Description,
                        Date = s.CreatedDate
                    })
                    .ToList();
                DataGridView publicGrid = MakeGrid(publicRows, 18, 108, 840, 160);
                publicList.Controls.Add(publicGrid);
                openPublic.Click += delegate
                {
                    int id = SelectedId(publicGrid);
                    OpenSharedTripDetail(id);
                };
                importPublic.Click += delegate
                {
                    int id = SelectedId(publicGrid);
                    ImportSharedTrip(id);
                };
            });
        }

        private void ShowReviewsView()
        {
            ShowShell("리뷰 보기 및 작성", "Reviews", delegate (FlowLayoutPanel content)
            {
                Panel search = AddCard(content, "리뷰 검색", 320);
                TextBox keyword = new TextBox();
                keyword.Left = 18;
                keyword.Top = 62;
                keyword.Width = 300;
                keyword.Height = 30;
                keyword.Text = reviewKeyword;
                search.Controls.Add(keyword);

                Button searchButton = MakeButton("검색", blue, Color.White, 330, 58, 90, 36);
                searchButton.Click += delegate { reviewKeyword = keyword.Text.Trim(); ShowReviewsView(); };
                search.Controls.Add(searchButton);

                Button add = MakeButton("리뷰 작성", Color.FromArgb(229, 237, 255), blue, 432, 58, 110, 36);
                add.Click += delegate { AddReview(ShowReviewsView); };
                search.Controls.Add(add);

                IEnumerable<Review> query = store.Reviews;
                if (!String.IsNullOrEmpty(reviewKeyword))
                {
                    query = query.Where(r => ContainsText(r.PlaceName, reviewKeyword));
                }
                List<Review> found = query.ToList();
                double average = found.Count == 0 ? 0 : found.Average(r => r.Rating);
                Label avg = new Label();
                avg.Text = "평균 별점 " + average.ToString("0.00");
                avg.Left = 560;
                avg.Top = 66;
                avg.Width = 220;
                avg.Height = 24;
                avg.ForeColor = textMuted;
                search.Controls.Add(avg);

                var rows = found
                    .OrderByDescending(r => r.CreatedDate)
                    .Select(r => new
                    {
                        ID = r.ReviewId,
                        Place = r.PlaceName,
                        Rating = r.Rating,
                        Writer = r.UserId,
                        Date = r.CreatedDate,
                        Content = r.Content
                    })
                    .ToList();
                search.Controls.Add(MakeGrid(rows, 18, 108, 840, 180));

                Panel latest = AddCard(content, "최신 리뷰", 300);
                var latestRows = store.Reviews
                    .OrderByDescending(r => r.CreatedDate)
                    .ThenByDescending(r => r.ReviewId)
                    .Take(10)
                    .Select(r => new
                    {
                        ID = r.ReviewId,
                        Place = r.PlaceName,
                        Rating = r.Rating,
                        Writer = r.UserId,
                        Date = r.CreatedDate,
                        Content = r.Content
                    })
                    .ToList();
                latest.Controls.Add(MakeGrid(latestRows, 18, 60, 840, 200));
            });
        }

        private void ShowSettingsView()
        {
            ShowShell("설정 및 로그아웃", "Settings", delegate (FlowLayoutPanel content)
            {
                Panel card = AddCard(content, "사용자 정보와 데이터 저장", 260);
                Label info = new Label();
                info.Text = "아이디: " + currentUser.UserId + "\n닉네임: " + currentUser.Nickname + "\n데이터 폴더: " + store.DataDirectory;
                info.Left = 22;
                info.Top = 62;
                info.Width = 820;
                info.Height = 72;
                info.ForeColor = textDark;
                card.Controls.Add(info);

                Button save = MakeButton("데이터 저장", blue, Color.White, 22, 154, 130, 40);
                save.Click += delegate
                {
                    store.SaveAll();
                    MessageBox.Show(this, "모든 데이터가 저장되었습니다.", "저장", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                card.Controls.Add(save);

                Button backup = MakeButton("백업 생성", Color.FromArgb(229, 237, 255), blue, 170, 154, 130, 40);
                backup.Click += delegate
                {
                    store.Backup();
                    MessageBox.Show(this, "백업이 생성되었습니다.", "백업", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                card.Controls.Add(backup);

                Button logout = MakeButton("로그아웃", Color.FromArgb(232, 236, 244), textDark, 318, 154, 130, 40);
                logout.Click += delegate
                {
                    store.SaveAll();
                    currentUser = null;
                    ShowAuthView();
                };
                card.Controls.Add(logout);
            });
        }

        private void AddSchedule(ModeType mode, Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("date", "날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("start", "시작 시간") { DefaultValue = "09:00" },
                new FieldSpec("end", "종료 시간") { DefaultValue = "10:00" },
                new FieldSpec("title", "일정명"),
                new FieldSpec("place", "장소"),
                new FieldSpec("category", "분류") { DefaultValue = mode == ModeType.Travel ? "travel" : "personal" },
                new FieldSpec("importance", "중요도") { Options = new string[] { "3", "1", "2", "4", "5" } },
                new FieldSpec("memo", "메모") { IsMultiline = true }
            };
            if (!EntryDialog.TryShow(this, "일정 추가", fields, out v))
            {
                return;
            }

            Schedule schedule = new Schedule();
            schedule.ScheduleId = store.NextScheduleId();
            schedule.UserId = currentUser.UserId;
            schedule.Mode = mode;
            schedule.Date = v["date"];
            schedule.StartTime = v["start"];
            schedule.EndTime = v["end"];
            schedule.Title = v["title"];
            schedule.Location = v["place"];
            schedule.Category = v["category"];
            schedule.Importance = ParseInt(v["importance"], 3);
            schedule.Memo = v["memo"];
            schedule.IsCompleted = StatusType.NotDone;

            if (HasConflict(schedule))
            {
                DialogResult result = MessageBox.Show(this, "같은 날짜에 시간이 겹치는 일정이 있습니다. 그래도 저장할까요?", "일정 충돌", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            store.Schedules.Add(schedule);
            store.SaveAll();
            refresh();
        }

        private void AddReservation(int tripScheduleId)
        {
            Dictionary<string, string> v;
            string[] tripOptions = TravelScheduleOptions();
            if (tripOptions.Length == 0)
            {
                MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "예약 등록");
                return;
            }
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = TravelScheduleOptionFor(tripScheduleId) },
                new FieldSpec("type", "예약 종류") { Options = new string[] { "flight", "hotel", "rental", "etc" } },
                new FieldSpec("name", "예약명"),
                new FieldSpec("startDate", "시작 날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("startTime", "시작 시간") { DefaultValue = "09:00" },
                new FieldSpec("endDate", "종료 날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("endTime", "종료 시간") { DefaultValue = "10:00" },
                new FieldSpec("location", "위치"),
                new FieldSpec("price", "금액") { DefaultValue = "0" },
                new FieldSpec("memo", "메모") { IsMultiline = true }
            };
            if (!EntryDialog.TryShow(this, "예약 추가", fields, out v))
            {
                return;
            }

            int selectedScheduleId = ParseScheduleOption(v["schedule"]);
            if (MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel) == null)
            {
                MessageBox.Show(this, "여행 일정을 선택하세요.", "예약 등록");
                return;
            }

            Reservation reservation = new Reservation();
            reservation.ReservationId = store.NextReservationId();
            reservation.TripScheduleId = selectedScheduleId;
            reservation.UserId = currentUser.UserId;
            reservation.ReservationType = v["type"];
            reservation.ReservationName = v["name"];
            reservation.StartDate = v["startDate"];
            reservation.StartTime = v["startTime"];
            reservation.EndDate = v["endDate"];
            reservation.EndTime = v["endTime"];
            reservation.Location = v["location"];
            reservation.Price = ParseInt(v["price"], 0);
            reservation.Memo = v["memo"];
            store.Reservations.Add(reservation);

            store.SaveAll();
            ShowTravelView();
        }

        private void AddPlace(Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("region", "지역") { DefaultValue = "Jeju" },
                new FieldSpec("name", "장소명"),
                new FieldSpec("address", "주소"),
                new FieldSpec("category", "분류") { DefaultValue = "Sightseeing" },
                new FieldSpec("priority", "우선순위") { DefaultValue = "1" },
                new FieldSpec("distance", "출발지 거리") { DefaultValue = "0" },
                new FieldSpec("score", "추천 점수") { DefaultValue = "80" }
            };
            if (!EntryDialog.TryShow(this, "장소 추가", fields, out v))
            {
                return;
            }

            store.Places.Add(new Place
            {
                PlaceId = store.NextPlaceId(),
                Region = v["region"],
                PlaceName = v["name"],
                Address = v["address"],
                Category = v["category"],
                Priority = ParseInt(v["priority"], 1),
                DistanceFromStart = ParseInt(v["distance"], 0),
                RecommendScore = ParseInt(v["score"], 0)
            });
            store.SaveAll();
            refresh();
        }

        private void AddPlaceAndAttachToTrip()
        {
            string[] tripOptions = TravelScheduleOptions();
            if (tripOptions.Length == 0)
            {
                MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "장소 직접 추가");
                return;
            }

            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = FirstTravelScheduleOption() },
                new FieldSpec("region", "나라/지역") { Options = CountryOptions(), DefaultValue = "japan" },
                new FieldSpec("name", "장소명"),
                new FieldSpec("address", "주소"),
                new FieldSpec("category", "분류") { DefaultValue = "Sightseeing" },
                new FieldSpec("distance", "거리") { DefaultValue = "0" },
                new FieldSpec("score", "인기도") { DefaultValue = "80" }
            };

            if (!EntryDialog.TryShow(this, "장소 직접 추가", fields, out v))
            {
                return;
            }

            int scheduleId = ParseScheduleOption(v["schedule"]);
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == scheduleId && s.Mode == ModeType.Travel);
            if (schedule == null)
            {
                MessageBox.Show(this, "여행 일정을 선택하세요.", "장소 직접 추가");
                return;
            }

            Place place = new Place
            {
                PlaceId = store.NextPlaceId(),
                Region = v["region"],
                PlaceName = v["name"],
                Address = v["address"],
                Category = v["category"],
                Priority = 3,
                DistanceFromStart = ParseInt(v["distance"], 0),
                RecommendScore = ParseInt(v["score"], 80)
            };
            store.Places.Add(place);

            int nextOrder = MyRouteItems()
                .Where(r => r.TripScheduleId == schedule.ScheduleId)
                .Select(r => r.DisplayOrder)
                .DefaultIfEmpty(0)
                .Max() + 1;

            store.RouteItems.Add(new RouteItem
            {
                RouteId = store.NextRouteId(),
                UserId = currentUser.UserId,
                TripScheduleId = schedule.ScheduleId,
                Country = place.Region,
                PlaceName = place.PlaceName,
                Category = place.Category,
                DistanceFromStart = place.DistanceFromStart,
                Popularity = place.RecommendScore,
                SortType = "직접추가",
                DisplayOrder = nextOrder
            });

            store.SaveAll();
            MessageBox.Show(this, "장소를 추가하고 " + schedule.Title + " 상세 동선에 연결했습니다.", "장소 직접 추가");
            ShowTravelView();
        }

        private void AddRouteForTrip(int tripScheduleId)
        {
            string[] tripOptions = TravelScheduleOptions();
            if (tripOptions.Length == 0)
            {
                MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "동선 추가");
                return;
            }

            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = TravelScheduleOptionFor(tripScheduleId) },
                new FieldSpec("country", "나라/지역") { Options = CountryOptions(), DefaultValue = "japan" }
            };
            if (!EntryDialog.TryShow(this, "동선 추가", fields, out v))
            {
                return;
            }

            int selectedScheduleId = ParseScheduleOption(v["schedule"]);
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel);
            if (schedule == null)
            {
                MessageBox.Show(this, "여행 일정을 선택하세요.", "동선 추가");
                return;
            }

            string country = v["country"];
            List<Place> places = store.Places
                .Where(p => String.Equals(p.Region, country, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (places.Count == 0)
            {
                MessageBox.Show(this, "해당 나라/지역의 장소 더미 데이터가 없습니다.", "동선 추가");
                return;
            }

            using (RouteSelectForm selector = new RouteSelectForm(country, places))
            {
                if (selector.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<Place> selectedPlaces = selector.UseRandom
                    ? places.OrderBy(p => random.Next()).Take(selector.RandomCount).ToList()
                    : selector.SelectedPlaces.ToList();

                if (selectedPlaces.Count == 0)
                {
                    MessageBox.Show(this, "저장할 장소가 없습니다.", "동선 추가");
                    return;
                }

                SaveRouteItemsForSchedule(schedule, country, selectedPlaces, selector.SortType);
            }
        }

        private void SaveRouteItemsForSchedule(Schedule schedule, string country, List<Place> places, string sortType)
        {
            List<Place> ordered = sortType == "인기순"
                ? places.OrderByDescending(p => p.RecommendScore).ThenBy(p => p.DistanceFromStart).ToList()
                : places.OrderBy(p => p.DistanceFromStart).ThenByDescending(p => p.RecommendScore).ToList();

            store.RouteItems.RemoveAll(r => r.UserId == currentUser.UserId && r.TripScheduleId == schedule.ScheduleId);

            int order = 1;
            foreach (Place place in ordered)
            {
                store.RouteItems.Add(new RouteItem
                {
                    RouteId = store.NextRouteId(),
                    UserId = currentUser.UserId,
                    TripScheduleId = schedule.ScheduleId,
                    Country = country,
                    PlaceName = place.PlaceName,
                    Category = place.Category,
                    DistanceFromStart = place.DistanceFromStart,
                    Popularity = place.RecommendScore,
                    SortType = sortType,
                    DisplayOrder = order++
                });
            }

            store.SaveAll();
            MessageBox.Show(this, schedule.Title + " 일정에 동선을 저장했습니다.", "동선 추가");
            ShowTravelView();
        }

        private void RecommendRoute(bool byPriority)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[] { new FieldSpec("region", "지역 필터") { DefaultValue = "Jeju" } };
            if (!EntryDialog.TryShow(this, byPriority ? "우선순위 동선 추천" : "거리순 동선 추천", fields, out v))
            {
                return;
            }

            IEnumerable<Place> query = store.Places;
            if (!String.IsNullOrEmpty(v["region"]))
            {
                query = query.Where(p => String.Equals(p.Region, v["region"], StringComparison.OrdinalIgnoreCase));
            }
            query = byPriority
                ? query.OrderBy(p => p.Priority).ThenByDescending(p => p.RecommendScore)
                : query.OrderBy(p => p.DistanceFromStart).ThenByDescending(p => p.RecommendScore);

            List<Place> places = query.ToList();
            if (places.Count == 0)
            {
                MessageBox.Show(this, "해당 지역의 장소 데이터가 없습니다.", "동선 추천");
                return;
            }

            string message = "";
            int i = 1;
            foreach (Place p in places)
            {
                message += i + ". " + p.PlaceName + " (" + p.Category + ", 거리 " + p.DistanceFromStart + ", 우선순위 " + p.Priority + ")\n";
                i++;
            }
            MessageBox.Show(this, message, "추천 방문 순서");
        }

        private void SaveRandomRouteForSchedule(int tripScheduleId)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("sort", "정렬 기준") { Options = new string[] { "거리순", "인기순" } }
            };
            if (!EntryDialog.TryShow(this, "동선 정렬 선택", fields, out v))
            {
                return;
            }

            SaveRandomRouteForSchedule(tripScheduleId, v["sort"] == "인기순");
        }

        private void SaveRandomRouteForSchedule(int tripScheduleId, bool byPopularity)
        {
            Dictionary<string, string> v;
            string defaultScheduleId = tripScheduleId > 0 ? tripScheduleId.ToString() : FirstTravelScheduleIdText();
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("scheduleId", "여행 일정 ID") { DefaultValue = defaultScheduleId },
                new FieldSpec("country", "나라/지역") { Options = new string[] { "japan", "jeju", "seoul", "busan", "china", "australia" } }
            };
            if (!EntryDialog.TryShow(this, byPopularity ? "인기순 동선 저장" : "거리순 동선 저장", fields, out v))
            {
                return;
            }

            int selectedScheduleId = ParseInt(v["scheduleId"], tripScheduleId);
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel);
            if (schedule == null)
            {
                MessageBox.Show(this, "여행 일정 ID를 확인하세요.", "동선 저장");
                return;
            }

            string country = v["country"];
            List<Place> candidates = store.Places
                .Where(p => String.Equals(p.Region, country, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => random.Next())
                .Take(5)
                .ToList();

            if (candidates.Count == 0)
            {
                MessageBox.Show(this, "해당 나라/지역의 더미 장소가 없습니다.", "동선 저장");
                return;
            }

            candidates = byPopularity
                ? candidates.OrderByDescending(p => p.RecommendScore).ToList()
                : candidates.OrderBy(p => p.DistanceFromStart).ToList();

            store.RouteItems.RemoveAll(r => r.UserId == currentUser.UserId && r.TripScheduleId == selectedScheduleId);

            string sortType = byPopularity ? "인기순" : "거리순";
            int order = 1;
            string routePreview = "";
            foreach (Place place in candidates)
            {
                store.RouteItems.Add(new RouteItem
                {
                    RouteId = store.NextRouteId(),
                    UserId = currentUser.UserId,
                    TripScheduleId = selectedScheduleId,
                    Country = country,
                    PlaceName = place.PlaceName,
                    Category = place.Category,
                    DistanceFromStart = place.DistanceFromStart,
                    Popularity = place.RecommendScore,
                    SortType = sortType,
                    DisplayOrder = order++
                });
                routePreview += (order - 1) + ". " + place.PlaceName + " / distance " + place.DistanceFromStart + " / popularity " + place.RecommendScore + "\n";
            }

            store.SaveAll();
            MessageBox.Show(this, schedule.Title + " 일정에 " + sortType + " 동선 5개를 저장했습니다.\n\n" + routePreview, "동선 저장");
            ShowTravelView();
        }

        private void AddExpense(ModeType mode, Action refresh)
        {
            Dictionary<string, string> v;
            string friendHint = String.Join(",", MyFriends().Select(f => f.FriendUserId).ToArray());
            List<FieldSpec> specs = new List<FieldSpec>();
            if (mode == ModeType.Travel)
            {
                string[] tripOptions = TravelScheduleOptions();
                if (tripOptions.Length == 0)
                {
                    MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "경비 등록");
                    return;
                }
                specs.Add(new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = FirstTravelScheduleOption() });
            }
            specs.AddRange(new FieldSpec[]
            {
                new FieldSpec("date", "날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("category", "분류") { DefaultValue = mode == ModeType.Travel ? "food" : "daily" },
                new FieldSpec("payer", "결제자") { DefaultValue = currentUser.Nickname },
                new FieldSpec("friends", "친구 ID") { DefaultValue = friendHint },
                new FieldSpec("amount", "금액") { DefaultValue = "0" },
                new FieldSpec("memo", "메모") { IsMultiline = true }
            });
            if (!EntryDialog.TryShow(this, "지출 추가", specs.ToArray(), out v))
            {
                return;
            }

            int tripScheduleId = mode == ModeType.Travel ? ParseScheduleOption(v["schedule"]) : 0;
            if (mode == ModeType.Travel && tripScheduleId > 0 &&
                MySchedules().FirstOrDefault(s => s.ScheduleId == tripScheduleId && s.Mode == ModeType.Travel) == null)
            {
                MessageBox.Show(this, "여행 일정을 선택하세요.", "경비 등록");
                return;
            }

            List<string> participantUserIds = new List<string>();
            participantUserIds.Add(currentUser.UserId);
            foreach (string raw in v["friends"].Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string friendId = raw.Trim();
                if (MyFriends().Any(f => f.FriendUserId == friendId) && !participantUserIds.Contains(friendId))
                {
                    participantUserIds.Add(friendId);
                }
            }

            foreach (string userId in participantUserIds)
            {
                store.Expenses.Add(new Expense
                {
                    ExpenseId = store.NextExpenseId(),
                    TripScheduleId = tripScheduleId,
                    UserId = userId,
                    Mode = mode,
                    Date = v["date"],
                    Category = v["category"],
                    Payer = v["payer"],
                    ParticipantCount = participantUserIds.Count,
                    Amount = ParseInt(v["amount"], 0),
                    Memo = v["memo"]
                });
            }
            store.SaveAll();
            refresh();
        }

        private void AddChecklistItem(ModeType mode, int tripScheduleId, string name, string category, Action refresh)
        {
            int selectedScheduleId = tripScheduleId;
            if (name == null)
            {
                Dictionary<string, string> v;
                List<FieldSpec> specs = new List<FieldSpec>();
                if (mode == ModeType.Travel)
                {
                    string[] tripOptions = TravelScheduleOptions();
                    if (tripOptions.Length == 0)
                    {
                        MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "체크리스트");
                        return;
                    }
                    specs.Add(new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = TravelScheduleOptionFor(tripScheduleId) });
                }
                specs.AddRange(new FieldSpec[]
                {
                    new FieldSpec("name", "준비물명"),
                    new FieldSpec("category", "분류") { DefaultValue = "General" }
                });
                if (!EntryDialog.TryShow(this, "체크리스트 항목 추가", specs.ToArray(), out v))
                {
                    return;
                }
                if (mode == ModeType.Travel)
                {
                    selectedScheduleId = ParseScheduleOption(v["schedule"]);
                }
                name = v["name"];
                category = v["category"];
            }

            if (mode == ModeType.Travel &&
                MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel) == null)
            {
                MessageBox.Show(this, "여행 일정을 선택하세요.", "체크리스트");
                return;
            }

            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            store.Checklist.Add(new ChecklistItem
            {
                ItemId = store.NextChecklistId(),
                TripScheduleId = mode == ModeType.Travel ? selectedScheduleId : 0,
                UserId = currentUser.UserId,
                Mode = mode,
                ItemName = name,
                Category = category ?? "General",
                IsChecked = StatusType.NotDone,
                IsShared = 0
            });
            store.SaveAll();
            refresh();
        }

        private void AddRecommendedChecklist(ModeType mode, int tripScheduleId, Action refresh)
        {
            int selectedScheduleId = tripScheduleId;
            if (mode == ModeType.Travel)
            {
                Dictionary<string, string> v;
                string[] tripOptions = TravelScheduleOptions();
                if (tripOptions.Length == 0)
                {
                    MessageBox.Show(this, "먼저 여행 일정 관리에서 여행 일정을 등록하세요.", "추천 체크리스트");
                    return;
                }
                FieldSpec[] fields = new FieldSpec[]
                {
                    new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = TravelScheduleOptionFor(tripScheduleId) }
                };
                if (!EntryDialog.TryShow(this, "추천 체크리스트 저장", fields, out v))
                {
                    return;
                }
                selectedScheduleId = ParseScheduleOption(v["schedule"]);
                if (MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel) == null)
                {
                    MessageBox.Show(this, "여행 일정을 선택하세요.", "추천 체크리스트");
                    return;
                }
            }

            string[,] travel = new string[,] { { "Passport", "Document" }, { "Wallet", "Money" }, { "Charger", "Electronics" }, { "Medicine", "Health" }, { "Umbrella", "Weather" } };
            string[,] daily = new string[,] { { "Notebook", "School" }, { "Textbook", "School" }, { "Student ID", "Document" }, { "Charger", "Electronics" }, { "Umbrella", "Weather" } };
            string[,] source = mode == ModeType.Travel ? travel : daily;

            for (int i = 0; i < source.GetLength(0); i++)
            {
                string itemName = source[i, 0];
                if (!MyChecklist(mode).Any(c => c.TripScheduleId == selectedScheduleId && String.Equals(c.ItemName, itemName, StringComparison.OrdinalIgnoreCase)))
                {
                    store.Checklist.Add(new ChecklistItem
                    {
                        ItemId = store.NextChecklistId(),
                        TripScheduleId = mode == ModeType.Travel ? selectedScheduleId : 0,
                        UserId = currentUser.UserId,
                        Mode = mode,
                        ItemName = itemName,
                        Category = source[i, 1],
                        IsChecked = StatusType.NotDone,
                        IsShared = 0
                    });
                }
            }
            store.SaveAll();
            refresh();
        }

        private void AddWeather(Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("date", "날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("region", "지역") { DefaultValue = "Seoul" },
                new FieldSpec("weather", "날씨") { Options = new string[] { "Sunny", "Cloudy", "Rainy", "Snowy" } },
                new FieldSpec("rain", "강수확률") { DefaultValue = "50" }
            };
            if (!EntryDialog.TryShow(this, "날씨 데이터 추가", fields, out v))
            {
                return;
            }

            store.WeatherList.Add(new WeatherInfo
            {
                WeatherId = store.NextWeatherId(),
                Date = v["date"],
                Region = v["region"],
                Weather = ParseWeather(v["weather"]),
                RainProbability = ParseInt(v["rain"], 0)
            });
            store.SaveAll();
            refresh();
        }

        private void CheckWeatherAlert(ModeType mode, Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("date", "날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("region", "지역") { DefaultValue = "Seoul" }
            };
            if (!EntryDialog.TryShow(this, "우산 알림 확인", fields, out v))
            {
                return;
            }

            WeatherInfo weather = store.WeatherList.FirstOrDefault(w =>
                w.Date == v["date"] && String.Equals(w.Region, v["region"], StringComparison.OrdinalIgnoreCase));
            if (weather == null)
            {
                MessageBox.Show(this, "해당 날짜/지역의 날씨 데이터가 없습니다.", "우산 알림");
                return;
            }

            if (weather.Weather == WeatherType.Rainy || weather.RainProbability >= 50)
            {
                if (!MyChecklist(mode).Any(c => String.Equals(c.ItemName, "Umbrella", StringComparison.OrdinalIgnoreCase)))
                {
                    store.Checklist.Add(new ChecklistItem
                    {
                        ItemId = store.NextChecklistId(),
                        UserId = currentUser.UserId,
                        Mode = mode,
                        ItemName = "Umbrella",
                        Category = "Weather",
                        IsChecked = StatusType.NotDone,
                        IsShared = 0
                    });
                }
                store.SaveAll();
                MessageBox.Show(this, "비 예보가 있습니다. 우산을 준비물에 추가/확인했습니다.", "우산 알림");
            }
            else
            {
                MessageBox.Show(this, "강수확률이 낮아서 우산 알림이 없습니다.", "우산 알림");
            }
            refresh();
        }

        private void AddTodo(Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("title", "Todo 제목"),
                new FieldSpec("due", "마감일") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("importance", "중요도") { Options = new string[] { "3", "1", "2", "4", "5" } },
                new FieldSpec("memo", "메모") { IsMultiline = true }
            };
            if (!EntryDialog.TryShow(this, "Todo 추가", fields, out v))
            {
                return;
            }

            store.Todos.Add(new Todo
            {
                TodoId = store.NextTodoId(),
                UserId = currentUser.UserId,
                Title = v["title"],
                DueDate = v["due"],
                Importance = ParseInt(v["importance"], 3),
                IsCompleted = StatusType.NotDone,
                Memo = v["memo"]
            });
            store.SaveAll();
            refresh();
        }

        private void AddJob(Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("name", "알바명"),
                new FieldSpec("date", "근무일") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("start", "시작 시간") { DefaultValue = "09:00" },
                new FieldSpec("end", "종료 시간") { DefaultValue = "13:00" },
                new FieldSpec("wage", "시급") { DefaultValue = "10000" }
            };
            if (!EntryDialog.TryShow(this, "알바비 계산", fields, out v))
            {
                return;
            }

            int hours = CalculateHours(v["start"], v["end"]);
            if (hours <= 0)
            {
                hours = 1;
            }
            int wage = ParseInt(v["wage"], 0);
            store.Jobs.Add(new PartTimeJob
            {
                JobId = store.NextJobId(),
                UserId = currentUser.UserId,
                JobName = v["name"],
                WorkDate = v["date"],
                StartTime = v["start"],
                EndTime = v["end"],
                HourlyWage = wage,
                WorkHours = hours,
                TotalPay = wage * hours
            });
            store.SaveAll();
            refresh();
        }

        private void AddSleepAlarm(Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("date", "날짜") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") },
                new FieldSpec("wake", "기상 시간") { DefaultValue = "07:00" },
                new FieldSpec("hours", "목표 수면") { DefaultValue = "7" },
                new FieldSpec("alarm", "알림") { Options = new string[] { "ON", "OFF" } }
            };
            if (!EntryDialog.TryShow(this, "수면/기상 알림", fields, out v))
            {
                return;
            }

            int sleepHours = ParseInt(v["hours"], 7);
            store.SleepAlarms.Add(new SleepAlarm
            {
                SleepId = store.NextSleepId(),
                UserId = currentUser.UserId,
                Date = v["date"],
                WakeUpTime = v["wake"],
                TargetSleepHours = sleepHours,
                RecommendedBedTime = RecommendBedTime(v["wake"], sleepHours),
                IsAlarmOn = v["alarm"] == "ON" ? 1 : 0
            });
            store.SaveAll();
            refresh();
        }

        private void AddFriend()
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[] { new FieldSpec("friendId", "친구 ID") };
            if (!EntryDialog.TryShow(this, "친구 추가", fields, out v))
            {
                return;
            }

            string friendId = v["friendId"];
            User friend = store.FindUser(friendId);
            if (friend == null || friend.UserId == currentUser.UserId)
            {
                MessageBox.Show(this, "등록된 다른 사용자 ID를 입력하세요.", "친구 추가");
                return;
            }
            bool alreadyRelated = store.Friends.Any(f =>
                ((f.UserId == currentUser.UserId && f.FriendUserId == friend.UserId) ||
                (f.UserId == friend.UserId && f.FriendUserId == currentUser.UserId)) &&
                f.Status == 1);
            bool alreadyPending = store.Friends.Any(f =>
                f.UserId == currentUser.UserId &&
                f.FriendUserId == friend.UserId &&
                f.Status == 0);
            if (alreadyRelated || alreadyPending)
            {
                MessageBox.Show(this, "이미 친구이거나 요청을 보낸 상태입니다.", "친구 추가");
                return;
            }

            store.Friends.Add(new Friend
            {
                FriendId = store.NextFriendId(),
                UserId = currentUser.UserId,
                FriendUserId = friend.UserId,
                FriendNickname = friend.Nickname,
                IsScheduleShared = 0,
                Status = 0,
                RequestedBy = currentUser.UserId
            });
            store.SaveAll();
            MessageBox.Show(this, friend.Nickname + "님에게 친구 요청을 보냈습니다.", "친구 요청");
            ShowFriendsView();
        }

        private void AcceptFriendRequest(int friendId)
        {
            Friend request = MyPendingRequests().FirstOrDefault(f => f.FriendId == friendId);
            if (request == null)
            {
                ShowSelectMessage();
                return;
            }

            User requester = store.FindUser(request.UserId);
            if (requester == null)
            {
                MessageBox.Show(this, "요청한 사용자를 찾을 수 없습니다.", "친구 요청");
                return;
            }

            request.Status = 1;
            request.FriendNickname = currentUser.Nickname;
            request.IsScheduleShared = 0;

            bool reverseExists = store.Friends.Any(f =>
                f.UserId == currentUser.UserId &&
                f.FriendUserId == requester.UserId &&
                f.Status == 1);
            if (!reverseExists)
            {
                store.Friends.Add(new Friend
                {
                    FriendId = store.NextFriendId(),
                    UserId = currentUser.UserId,
                    FriendUserId = requester.UserId,
                    FriendNickname = requester.Nickname,
                    IsScheduleShared = 0,
                    Status = 1,
                    RequestedBy = request.RequestedBy
                });
            }

            store.SaveAll();
            MessageBox.Show(this, requester.Nickname + "님과 친구가 되었습니다.", "친구 요청 수락");
            ShowFriendsView();
        }

        private void ShareSchedule()
        {
            if (MyFriends().Count() == 0)
            {
                MessageBox.Show(this, "먼저 친구를 추가하세요.", "일정 공유");
                return;
            }
            if (MySchedules().Count() == 0)
            {
                MessageBox.Show(this, "공유할 일정이 없습니다.", "일정 공유");
                return;
            }

            Dictionary<string, string> v;
            string friendOptions = String.Join(", ", MyFriends().Select(f => f.FriendId + ":" + f.FriendNickname).ToArray());
            string scheduleOptions = String.Join(", ", MySchedules().Select(s => s.ScheduleId + ":" + s.Title).ToArray());
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("friend", "친구 ID") { DefaultValue = friendOptions.Length > 0 ? friendOptions.Split(',')[0].Split(':')[0] : "" },
                new FieldSpec("schedule", "일정 ID") { DefaultValue = scheduleOptions.Length > 0 ? scheduleOptions.Split(',')[0].Split(':')[0] : "" }
            };
            if (!EntryDialog.TryShow(this, "일정 공유", fields, out v))
            {
                return;
            }

            int friendId = ParseInt(v["friend"], 0);
            int scheduleId = ParseInt(v["schedule"], 0);
            Friend friend = MyFriends().FirstOrDefault(f => f.FriendId == friendId);
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == scheduleId);
            if (friend == null || schedule == null)
            {
                MessageBox.Show(this, "친구 ID와 일정 ID를 확인하세요.", "일정 공유");
                return;
            }

            friend.IsScheduleShared = 1;
            store.SaveAll();
            MessageBox.Show(this, "일정 '" + schedule.Title + "'을(를) " + friend.FriendNickname + "에게 공유 표시했습니다.", "일정 공유");
            ShowFriendsView();
        }

        private void ShareTripPackage(int tripScheduleId)
        {
            if (MyFriends().Count() == 0)
            {
                MessageBox.Show(this, "먼저 친구를 추가하고 상대가 수락해야 공유할 수 있습니다.", "여행 공유");
                return;
            }

            string[] tripOptions = TravelScheduleOptions();
            if (tripOptions.Length == 0)
            {
                MessageBox.Show(this, "공유할 여행 일정이 없습니다.", "여행 공유");
                return;
            }

            string[] friendOptions = MyFriends().Select(f => FriendOptionText(f)).ToArray();
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = TravelScheduleOptionFor(tripScheduleId) },
                new FieldSpec("friend", "친구") { Options = friendOptions },
                new FieldSpec("rating", "별점") { Options = new string[] { "5", "4", "3", "2", "1" } },
                new FieldSpec("description", "설명") { IsMultiline = true }
            };
            if (!EntryDialog.TryShow(this, "여행 패키지 공유", fields, out v))
            {
                return;
            }

            int selectedScheduleId = ParseScheduleOption(v["schedule"]);
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel);
            if (schedule == null)
            {
                MessageBox.Show(this, "공유할 여행 일정을 선택하세요.", "여행 공유");
                return;
            }

            Friend friend = MyFriends().FirstOrDefault(f => FriendOptionText(f) == v["friend"]);
            if (friend == null)
            {
                MessageBox.Show(this, "친구를 선택하세요.", "여행 공유");
                return;
            }

            friend.IsScheduleShared = 1;
            foreach (ChecklistItem item in MyChecklist(ModeType.Travel).Where(c => c.TripScheduleId == selectedScheduleId || c.TripScheduleId == 0))
            {
                item.IsShared = 1;
            }

            store.TripShares.Add(new TripShare
            {
                ShareId = store.NextShareId(),
                OwnerUserId = currentUser.UserId,
                FriendUserId = friend.FriendUserId,
                TripScheduleId = selectedScheduleId,
                Rating = ParseInt(v["rating"], 5),
                Description = v["description"],
                CreatedDate = DateTime.Today.ToString("yyyy-MM-dd")
            });

            store.SaveAll();
            MessageBox.Show(this, schedule.Title + " 일정과 예약/동선/체크리스트를 공유했습니다.", "여행 공유");
            ShowFriendsView();
        }

        private void ShareTripPublic()
        {
            string[] tripOptions = TravelScheduleOptions();
            if (tripOptions.Length == 0)
            {
                MessageBox.Show(this, "공개할 여행 일정이 없습니다.", "공개 공유");
                return;
            }

            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("schedule", "여행 일정") { Options = tripOptions, DefaultValue = FirstTravelScheduleOption() },
                new FieldSpec("rating", "별점") { Options = new string[] { "5", "4", "3", "2", "1" } },
                new FieldSpec("description", "설명") { IsMultiline = true }
            };
            if (!EntryDialog.TryShow(this, "공개 여행 공유", fields, out v))
            {
                return;
            }

            int selectedScheduleId = ParseScheduleOption(v["schedule"]);
            Schedule schedule = MySchedules().FirstOrDefault(s => s.ScheduleId == selectedScheduleId && s.Mode == ModeType.Travel);
            if (schedule == null)
            {
                MessageBox.Show(this, "공개할 여행 일정을 선택하세요.", "공개 공유");
                return;
            }

            foreach (ChecklistItem item in MyChecklist(ModeType.Travel).Where(c => c.TripScheduleId == selectedScheduleId || c.TripScheduleId == 0))
            {
                item.IsShared = 1;
            }

            store.TripShares.Add(new TripShare
            {
                ShareId = store.NextShareId(),
                OwnerUserId = currentUser.UserId,
                FriendUserId = PublicShareTarget,
                TripScheduleId = selectedScheduleId,
                Rating = ParseInt(v["rating"], 5),
                Description = v["description"],
                CreatedDate = DateTime.Today.ToString("yyyy-MM-dd")
            });

            store.SaveAll();
            MessageBox.Show(this, schedule.Title + " 일정을 공개 여행 목록에 공유했습니다.", "공개 공유");
            ShowFriendsView();
        }

        private string FriendOptionText(Friend friend)
        {
            return friend.FriendNickname + " (" + friend.FriendUserId + ")";
        }

        private void MarkChecklistSharedByDialog(ModeType mode)
        {
            List<ChecklistItem> items = MyChecklist(mode).ToList();
            if (items.Count == 0)
            {
                MessageBox.Show(this, "공유할 체크리스트가 없습니다.", "체크리스트 공유");
                return;
            }

            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[] { new FieldSpec("item", "항목 ID") { DefaultValue = items[0].ItemId.ToString() } };
            if (!EntryDialog.TryShow(this, "체크리스트 공유", fields, out v))
            {
                return;
            }

            int itemId = ParseInt(v["item"], 0);
            ChecklistItem item = items.FirstOrDefault(c => c.ItemId == itemId);
            if (item == null)
            {
                MessageBox.Show(this, "항목 ID를 확인하세요.", "체크리스트 공유");
                return;
            }
            item.IsShared = 1;
            store.SaveAll();
            ShowFriendsView();
        }

        private void AddReview(Action refresh)
        {
            Dictionary<string, string> v;
            FieldSpec[] fields = new FieldSpec[]
            {
                new FieldSpec("place", "장소명"),
                new FieldSpec("rating", "별점") { Options = new string[] { "5", "4", "3", "2", "1" } },
                new FieldSpec("content", "후기") { IsMultiline = true },
                new FieldSpec("date", "작성일") { DefaultValue = DateTime.Today.ToString("yyyy-MM-dd") }
            };
            if (!EntryDialog.TryShow(this, "리뷰 작성", fields, out v))
            {
                return;
            }

            store.Reviews.Add(new Review
            {
                ReviewId = store.NextReviewId(),
                UserId = currentUser.UserId,
                PlaceName = v["place"],
                Rating = ParseInt(v["rating"], 5),
                Content = v["content"],
                CreatedDate = v["date"]
            });
            store.SaveAll();
            refresh();
        }

        private IEnumerable<Schedule> MySchedules()
        {
            return store.Schedules.Where(s => currentUser != null && s.UserId == currentUser.UserId);
        }

        private IEnumerable<Todo> MyTodos()
        {
            return store.Todos.Where(t => currentUser != null && t.UserId == currentUser.UserId);
        }

        private IEnumerable<Reservation> MyReservations()
        {
            return store.Reservations.Where(r => currentUser != null && r.UserId == currentUser.UserId);
        }

        private IEnumerable<Expense> MyExpenses(ModeType mode)
        {
            return store.Expenses.Where(e => currentUser != null && e.UserId == currentUser.UserId && e.Mode == mode);
        }

        private IEnumerable<PartTimeJob> MyJobs()
        {
            return store.Jobs.Where(j => currentUser != null && j.UserId == currentUser.UserId);
        }

        private IEnumerable<ChecklistItem> MyChecklist(ModeType? mode)
        {
            IEnumerable<ChecklistItem> items = store.Checklist.Where(c => currentUser != null && c.UserId == currentUser.UserId);
            if (mode.HasValue)
            {
                items = items.Where(c => c.Mode == mode.Value);
            }
            return items;
        }

        private IEnumerable<Friend> MyFriends()
        {
            return store.Friends.Where(f => currentUser != null && f.UserId == currentUser.UserId && f.Status == 1);
        }

        private IEnumerable<Friend> MyPendingRequests()
        {
            return store.Friends.Where(f => currentUser != null && f.FriendUserId == currentUser.UserId && f.Status == 0);
        }

        private IEnumerable<RouteItem> MyRouteItems()
        {
            return store.RouteItems.Where(r => currentUser != null && r.UserId == currentUser.UserId);
        }

        private IEnumerable<TripShare> MyTripShares()
        {
            return store.TripShares.Where(s => currentUser != null && s.OwnerUserId == currentUser.UserId);
        }

        private IEnumerable<TripShare> SharedToMe()
        {
            return store.TripShares.Where(s => currentUser != null && s.FriendUserId == currentUser.UserId);
        }

        private IEnumerable<TripShare> PublicTripShares()
        {
            return store.TripShares.Where(s => currentUser != null && s.FriendUserId == PublicShareTarget);
        }

        private IEnumerable<TripShare> AccessibleTripShares()
        {
            return store.TripShares.Where(s =>
                currentUser != null &&
                (s.OwnerUserId == currentUser.UserId ||
                 s.FriendUserId == currentUser.UserId ||
                 s.FriendUserId == PublicShareTarget));
        }

        private string SharedTripTitle(TripShare share)
        {
            Schedule schedule = store.Schedules.FirstOrDefault(s =>
                s.UserId == share.OwnerUserId &&
                s.ScheduleId == share.TripScheduleId &&
                s.Mode == ModeType.Travel);
            return schedule == null ? "(삭제된 여행)" : schedule.Title;
        }

        private IEnumerable<SleepAlarm> MySleepAlarms()
        {
            return store.SleepAlarms.Where(s => currentUser != null && s.UserId == currentUser.UserId);
        }

        private bool HasConflict(Schedule target)
        {
            int start = TimeToMinutes(target.StartTime);
            int end = TimeToMinutes(target.EndTime);
            if (start < 0 || end <= start)
            {
                return false;
            }

            foreach (Schedule schedule in MySchedules())
            {
                if (schedule.Date != target.Date)
                {
                    continue;
                }
                int otherStart = TimeToMinutes(schedule.StartTime);
                int otherEnd = TimeToMinutes(schedule.EndTime);
                if (otherStart >= 0 && otherEnd > otherStart && start < otherEnd && end > otherStart)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsText(string source, string keyword)
        {
            if (source == null)
            {
                source = "";
            }
            if (keyword == null)
            {
                keyword = "";
            }
            return source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string CleanInput(string value)
        {
            return (value ?? "").Replace("|", "/").Trim();
        }

        private static int ParseInt(string value, int defaultValue)
        {
            int number;
            return Int32.TryParse(value, out number) ? number : defaultValue;
        }

        private static int TimeToMinutes(string time)
        {
            if (String.IsNullOrEmpty(time))
            {
                return -1;
            }
            string[] parts = time.Split(':');
            if (parts.Length != 2)
            {
                return -1;
            }
            int hour;
            int minute;
            if (!Int32.TryParse(parts[0], out hour) || !Int32.TryParse(parts[1], out minute))
            {
                return -1;
            }
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
            {
                return -1;
            }
            return hour * 60 + minute;
        }

        private static int CalculateHours(string start, string end)
        {
            int startMinutes = TimeToMinutes(start);
            int endMinutes = TimeToMinutes(end);
            if (startMinutes < 0 || endMinutes <= startMinutes)
            {
                return 0;
            }
            int minutes = endMinutes - startMinutes;
            return minutes / 60 + (minutes % 60 >= 30 ? 1 : 0);
        }

        private static string RecommendBedTime(string wake, int hours)
        {
            int wakeMinutes = TimeToMinutes(wake);
            if (wakeMinutes < 0)
            {
                return "unknown";
            }
            int bed = wakeMinutes - hours * 60;
            while (bed < 0)
            {
                bed += 24 * 60;
            }
            bed = bed % (24 * 60);
            return String.Format("{0:00}:{1:00}", bed / 60, bed % 60);
        }

        private static bool ReservationCreatesSchedule(string type)
        {
            return ContainsText(type, "flight") || ContainsText(type, "air") ||
                ContainsText(type, "hotel") || ContainsText(type, "lodging") ||
                ContainsText(type, "rental") || ContainsText(type, "항공") ||
                ContainsText(type, "숙소") || ContainsText(type, "렌터카");
        }

        private static WeatherType ParseWeather(string text)
        {
            if (text == "Cloudy")
            {
                return WeatherType.Cloudy;
            }
            if (text == "Rainy")
            {
                return WeatherType.Rainy;
            }
            if (text == "Snowy")
            {
                return WeatherType.Snowy;
            }
            return WeatherType.Sunny;
        }

        private static string WeatherText(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Cloudy:
                    return "Cloudy";
                case WeatherType.Rainy:
                    return "Rainy";
                case WeatherType.Snowy:
                    return "Snowy";
                default:
                    return "Sunny";
            }
        }

        private static string ModeText(ModeType mode)
        {
            return mode == ModeType.Travel ? "여행" : "데일리";
        }

        private static string StatusText(StatusType status)
        {
            return status == StatusType.Done ? "완료" : "미완료";
        }

        private void CheckDueAlarms()
        {
            if (currentUser == null)
            {
                return;
            }

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            SleepAlarm alarm = MySleepAlarms()
                .Where(a => a.IsAlarmOn == 1 && a.Date == today && !shownAlarms.Contains(a.SleepId))
                .OrderBy(a => a.WakeUpTime)
                .FirstOrDefault(a =>
                {
                    int alarmMinutes = TimeToMinutes(a.WakeUpTime);
                    return alarmMinutes >= 0 && alarmMinutes <= now;
                });

            if (alarm == null)
            {
                return;
            }

            shownAlarms.Add(alarm.SleepId);
            using (AlarmForm popup = new AlarmForm(alarm))
            {
                popup.ShowDialog(this);
            }
            alarm.IsAlarmOn = 0;
            store.SaveAll();
            if (currentUser != null)
            {
                ShowDailyView();
            }
        }

        private void ShowSelectMessage()
        {
            MessageBox.Show(this, "목록에서 항목을 먼저 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
