using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnDayGui
{
    public class DataStore
    {
        public readonly List<User> Users = new List<User>();
        public readonly List<Schedule> Schedules = new List<Schedule>();
        public readonly List<Todo> Todos = new List<Todo>();
        public readonly List<Reservation> Reservations = new List<Reservation>();
        public readonly List<Expense> Expenses = new List<Expense>();
        public readonly List<PartTimeJob> Jobs = new List<PartTimeJob>();
        public readonly List<Place> Places = new List<Place>();
        public readonly List<ChecklistItem> Checklist = new List<ChecklistItem>();
        public readonly List<Friend> Friends = new List<Friend>();
        public readonly List<Review> Reviews = new List<Review>();
        public readonly List<WeatherInfo> WeatherList = new List<WeatherInfo>();
        public readonly List<SleepAlarm> SleepAlarms = new List<SleepAlarm>();
        public readonly List<RouteItem> RouteItems = new List<RouteItem>();
        public readonly List<TripShare> TripShares = new List<TripShare>();

        public string DataDirectory { get; private set; }

        public DataStore()
        {
            DataDirectory = FindDataDirectory();
        }

        public void LoadAll()
        {
            Directory.CreateDirectory(DataDirectory);
            Users.Clear();
            Schedules.Clear();
            Todos.Clear();
            Reservations.Clear();
            Expenses.Clear();
            Jobs.Clear();
            Places.Clear();
            Checklist.Clear();
            Friends.Clear();
            Reviews.Clear();
            WeatherList.Clear();
            SleepAlarms.Clear();
            RouteItems.Clear();
            TripShares.Clear();

            LoadUsers();
            LoadSchedules();
            LoadTodos();
            LoadReservations();
            LoadExpenses();
            LoadJobs();
            LoadPlaces();
            LoadChecklist();
            LoadFriends();
            LoadReviews();
            LoadWeather();
            LoadSleepAlarms();
            LoadRouteItems();
            LoadTripShares();
            RemoveReservationMirrorSchedules();
            EnsureDefaultPlaces();
            EnsureDefaultWeather();
            SaveAll();
        }

        public void SaveAll()
        {
            Directory.CreateDirectory(DataDirectory);
            SaveUsers();
            SaveSchedules();
            SaveTodos();
            SaveReservations();
            SaveExpenses();
            SaveJobs();
            SavePlaces();
            SaveChecklist();
            SaveFriends();
            SaveReviews();
            SaveWeather();
            SaveSleepAlarms();
            SaveRouteItems();
            SaveTripShares();
        }

        public User FindUser(string userId)
        {
            return Users.FirstOrDefault(u => String.Equals(u.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        public int NextScheduleId() { return Schedules.Count == 0 ? 1 : Schedules.Max(x => x.ScheduleId) + 1; }
        public int NextTodoId() { return Todos.Count == 0 ? 1 : Todos.Max(x => x.TodoId) + 1; }
        public int NextReservationId() { return Reservations.Count == 0 ? 1 : Reservations.Max(x => x.ReservationId) + 1; }
        public int NextExpenseId() { return Expenses.Count == 0 ? 1 : Expenses.Max(x => x.ExpenseId) + 1; }
        public int NextJobId() { return Jobs.Count == 0 ? 1 : Jobs.Max(x => x.JobId) + 1; }
        public int NextPlaceId() { return Places.Count == 0 ? 1 : Places.Max(x => x.PlaceId) + 1; }
        public int NextChecklistId() { return Checklist.Count == 0 ? 1 : Checklist.Max(x => x.ItemId) + 1; }
        public int NextFriendId() { return Friends.Count == 0 ? 1 : Friends.Max(x => x.FriendId) + 1; }
        public int NextReviewId() { return Reviews.Count == 0 ? 1 : Reviews.Max(x => x.ReviewId) + 1; }
        public int NextWeatherId() { return WeatherList.Count == 0 ? 1 : WeatherList.Max(x => x.WeatherId) + 1; }
        public int NextSleepId() { return SleepAlarms.Count == 0 ? 1 : SleepAlarms.Max(x => x.SleepId) + 1; }
        public int NextRouteId() { return RouteItems.Count == 0 ? 1 : RouteItems.Max(x => x.RouteId) + 1; }
        public int NextShareId() { return TripShares.Count == 0 ? 1 : TripShares.Max(x => x.ShareId) + 1; }

        public void Backup()
        {
            SaveAll();
            string root = Path.Combine(GetSolutionLikeRoot(), "backups");
            string folder = Path.Combine(root, "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(folder);
            foreach (string file in FileNames)
            {
                string src = Path.Combine(DataDirectory, file);
                string dst = Path.Combine(folder, file);
                if (File.Exists(src))
                {
                    File.Copy(src, dst, true);
                }
            }
        }

        private static readonly string[] FileNames = new string[]
        {
            "users.txt", "schedules.txt", "todos.txt", "reservations.txt",
            "expenses.txt", "jobs.txt", "places.txt", "checklist.txt",
            "friends.txt", "reviews.txt", "weather.txt", "sleep.txt",
            "routes.txt", "shares.txt"
        };

        private string FindDataDirectory()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                string consoleProject = Path.Combine(dir.FullName, "OnDay", "OnDay.vcxproj");
                if (File.Exists(consoleProject))
                {
                    return Path.Combine(dir.FullName, "OnDay", "data");
                }

                string currentProject = Path.Combine(dir.FullName, "OnDay.vcxproj");
                if (File.Exists(currentProject))
                {
                    return Path.Combine(dir.FullName, "data");
                }

                dir = dir.Parent;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        }

        private string GetSolutionLikeRoot()
        {
            DirectoryInfo dir = new DirectoryInfo(DataDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "OnDay.sln")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private string PathFor(string fileName)
        {
            return Path.Combine(DataDirectory, fileName);
        }

        private string[] ReadRows(string fileName)
        {
            string path = PathFor(fileName);
            if (!File.Exists(path))
            {
                return new string[0];
            }
            return File.ReadAllLines(path);
        }

        private void WriteRows(string fileName, IEnumerable<string> rows)
        {
            File.WriteAllLines(PathFor(fileName), rows.ToArray());
        }

        private static string Clean(string value)
        {
            if (value == null)
            {
                return "";
            }
            return value.Replace("|", "/").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static int IntAt(string[] fields, int index)
        {
            int value;
            if (index >= fields.Length || !Int32.TryParse(fields[index], out value))
            {
                return 0;
            }
            return value;
        }

        private static string StrAt(string[] fields, int index)
        {
            if (index >= fields.Length)
            {
                return "";
            }
            return fields[index];
        }

        private void LoadUsers()
        {
            foreach (string row in ReadRows("users.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 3)
                {
                    Users.Add(new User { UserId = f[0], Password = f[1], Nickname = f[2] });
                }
            }
        }

        private void SaveUsers()
        {
            WriteRows("users.txt", Users.Select(u => String.Join("|", Clean(u.UserId), Clean(u.Password), Clean(u.Nickname))));
        }

        private void LoadSchedules()
        {
            foreach (string row in ReadRows("schedules.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 12)
                {
                    Schedules.Add(new Schedule
                    {
                        ScheduleId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        Mode = (ModeType)IntAt(f, 2),
                        Title = StrAt(f, 3),
                        Date = StrAt(f, 4),
                        StartTime = StrAt(f, 5),
                        EndTime = StrAt(f, 6),
                        Location = StrAt(f, 7),
                        Category = StrAt(f, 8),
                        Importance = IntAt(f, 9),
                        Memo = StrAt(f, 10),
                        IsCompleted = (StatusType)IntAt(f, 11)
                    });
                }
            }
        }

        private void SaveSchedules()
        {
            WriteRows("schedules.txt", Schedules.Select(s => String.Join("|",
                s.ScheduleId, Clean(s.UserId), (int)s.Mode, Clean(s.Title), Clean(s.Date),
                Clean(s.StartTime), Clean(s.EndTime), Clean(s.Location), Clean(s.Category),
                s.Importance, Clean(s.Memo), (int)s.IsCompleted)));
        }

        private void LoadTodos()
        {
            foreach (string row in ReadRows("todos.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 7)
                {
                    Todos.Add(new Todo
                    {
                        TodoId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        Title = StrAt(f, 2),
                        DueDate = StrAt(f, 3),
                        Importance = IntAt(f, 4),
                        IsCompleted = (StatusType)IntAt(f, 5),
                        Memo = StrAt(f, 6)
                    });
                }
            }
        }

        private void SaveTodos()
        {
            WriteRows("todos.txt", Todos.Select(t => String.Join("|",
                t.TodoId, Clean(t.UserId), Clean(t.Title), Clean(t.DueDate),
                t.Importance, (int)t.IsCompleted, Clean(t.Memo))));
        }

        private void LoadReservations()
        {
            foreach (string row in ReadRows("reservations.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 11)
                {
                    Reservations.Add(new Reservation
                    {
                        ReservationId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        ReservationType = StrAt(f, 2),
                        ReservationName = StrAt(f, 3),
                        StartDate = StrAt(f, 4),
                        StartTime = StrAt(f, 5),
                        EndDate = StrAt(f, 6),
                        EndTime = StrAt(f, 7),
                        Location = StrAt(f, 8),
                        Price = IntAt(f, 9),
                        Memo = StrAt(f, 10),
                        TripScheduleId = IntAt(f, 11)
                    });
                }
            }
        }

        private void SaveReservations()
        {
            WriteRows("reservations.txt", Reservations.Select(r => String.Join("|",
                r.ReservationId, Clean(r.UserId), Clean(r.ReservationType), Clean(r.ReservationName),
                Clean(r.StartDate), Clean(r.StartTime), Clean(r.EndDate), Clean(r.EndTime),
                Clean(r.Location), r.Price, Clean(r.Memo), r.TripScheduleId)));
        }

        private void LoadExpenses()
        {
            foreach (string row in ReadRows("expenses.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 9)
                {
                    Expenses.Add(new Expense
                    {
                        ExpenseId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        Mode = (ModeType)IntAt(f, 2),
                        Date = StrAt(f, 3),
                        Category = StrAt(f, 4),
                        Payer = StrAt(f, 5),
                        ParticipantCount = Math.Max(1, IntAt(f, 6)),
                        Amount = IntAt(f, 7),
                        Memo = StrAt(f, 8),
                        TripScheduleId = IntAt(f, 9)
                    });
                }
            }
        }

        private void SaveExpenses()
        {
            WriteRows("expenses.txt", Expenses.Select(e => String.Join("|",
                e.ExpenseId, Clean(e.UserId), (int)e.Mode, Clean(e.Date), Clean(e.Category),
                Clean(e.Payer), e.ParticipantCount, e.Amount, Clean(e.Memo), e.TripScheduleId)));
        }

        private void LoadJobs()
        {
            foreach (string row in ReadRows("jobs.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 9)
                {
                    Jobs.Add(new PartTimeJob
                    {
                        JobId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        JobName = StrAt(f, 2),
                        WorkDate = StrAt(f, 3),
                        StartTime = StrAt(f, 4),
                        EndTime = StrAt(f, 5),
                        HourlyWage = IntAt(f, 6),
                        WorkHours = IntAt(f, 7),
                        TotalPay = IntAt(f, 8)
                    });
                }
            }
        }

        private void SaveJobs()
        {
            WriteRows("jobs.txt", Jobs.Select(j => String.Join("|",
                j.JobId, Clean(j.UserId), Clean(j.JobName), Clean(j.WorkDate),
                Clean(j.StartTime), Clean(j.EndTime), j.HourlyWage, j.WorkHours, j.TotalPay)));
        }

        private void LoadPlaces()
        {
            foreach (string row in ReadRows("places.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 8)
                {
                    Places.Add(new Place
                    {
                        PlaceId = IntAt(f, 0),
                        Region = StrAt(f, 1),
                        PlaceName = StrAt(f, 2),
                        Address = StrAt(f, 3),
                        Category = StrAt(f, 4),
                        Priority = IntAt(f, 5),
                        DistanceFromStart = IntAt(f, 6),
                        RecommendScore = IntAt(f, 7)
                    });
                }
            }
        }

        private void SavePlaces()
        {
            WriteRows("places.txt", Places.Select(p => String.Join("|",
                p.PlaceId, Clean(p.Region), Clean(p.PlaceName), Clean(p.Address),
                Clean(p.Category), p.Priority, p.DistanceFromStart, p.RecommendScore)));
        }

        private void LoadChecklist()
        {
            foreach (string row in ReadRows("checklist.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 7)
                {
                    Checklist.Add(new ChecklistItem
                    {
                        ItemId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        Mode = (ModeType)IntAt(f, 2),
                        ItemName = StrAt(f, 3),
                        Category = StrAt(f, 4),
                        IsChecked = (StatusType)IntAt(f, 5),
                        IsShared = IntAt(f, 6),
                        TripScheduleId = IntAt(f, 7)
                    });
                }
            }
        }

        private void SaveChecklist()
        {
            WriteRows("checklist.txt", Checklist.Select(c => String.Join("|",
                c.ItemId, Clean(c.UserId), (int)c.Mode, Clean(c.ItemName), Clean(c.Category),
                (int)c.IsChecked, c.IsShared, c.TripScheduleId)));
        }

        private void LoadFriends()
        {
            foreach (string row in ReadRows("friends.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 5)
                {
                    Friends.Add(new Friend
                    {
                        FriendId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        FriendUserId = StrAt(f, 2),
                        FriendNickname = StrAt(f, 3),
                        IsScheduleShared = IntAt(f, 4),
                        Status = f.Length >= 6 ? IntAt(f, 5) : 1,
                        RequestedBy = f.Length >= 7 ? StrAt(f, 6) : StrAt(f, 1)
                    });
                }
            }
        }

        private void SaveFriends()
        {
            WriteRows("friends.txt", Friends.Select(fr => String.Join("|",
                fr.FriendId, Clean(fr.UserId), Clean(fr.FriendUserId),
                Clean(fr.FriendNickname), fr.IsScheduleShared, fr.Status, Clean(fr.RequestedBy))));
        }

        private void LoadReviews()
        {
            foreach (string row in ReadRows("reviews.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 6)
                {
                    Reviews.Add(new Review
                    {
                        ReviewId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        PlaceName = StrAt(f, 2),
                        Rating = IntAt(f, 3),
                        Content = StrAt(f, 4),
                        CreatedDate = StrAt(f, 5)
                    });
                }
            }
        }

        private void SaveReviews()
        {
            WriteRows("reviews.txt", Reviews.Select(r => String.Join("|",
                r.ReviewId, Clean(r.UserId), Clean(r.PlaceName), r.Rating,
                Clean(r.Content), Clean(r.CreatedDate))));
        }

        private void LoadWeather()
        {
            foreach (string row in ReadRows("weather.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 5)
                {
                    WeatherList.Add(new WeatherInfo
                    {
                        WeatherId = IntAt(f, 0),
                        Date = StrAt(f, 1),
                        Region = StrAt(f, 2),
                        Weather = (WeatherType)IntAt(f, 3),
                        RainProbability = IntAt(f, 4)
                    });
                }
            }
        }

        private void SaveWeather()
        {
            WriteRows("weather.txt", WeatherList.Select(w => String.Join("|",
                w.WeatherId, Clean(w.Date), Clean(w.Region), (int)w.Weather, w.RainProbability)));
        }

        private void LoadSleepAlarms()
        {
            foreach (string row in ReadRows("sleep.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 7)
                {
                    SleepAlarms.Add(new SleepAlarm
                    {
                        SleepId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        Date = StrAt(f, 2),
                        WakeUpTime = StrAt(f, 3),
                        TargetSleepHours = IntAt(f, 4),
                        RecommendedBedTime = StrAt(f, 5),
                        IsAlarmOn = IntAt(f, 6)
                    });
                }
            }
        }

        private void SaveSleepAlarms()
        {
            WriteRows("sleep.txt", SleepAlarms.Select(s => String.Join("|",
                s.SleepId, Clean(s.UserId), Clean(s.Date), Clean(s.WakeUpTime),
                s.TargetSleepHours, Clean(s.RecommendedBedTime), s.IsAlarmOn)));
        }

        private void LoadRouteItems()
        {
            foreach (string row in ReadRows("routes.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 10)
                {
                    RouteItems.Add(new RouteItem
                    {
                        RouteId = IntAt(f, 0),
                        UserId = StrAt(f, 1),
                        TripScheduleId = IntAt(f, 2),
                        Country = StrAt(f, 3),
                        PlaceName = StrAt(f, 4),
                        Category = StrAt(f, 5),
                        DistanceFromStart = IntAt(f, 6),
                        Popularity = IntAt(f, 7),
                        SortType = StrAt(f, 8),
                        DisplayOrder = IntAt(f, 9)
                    });
                }
            }
        }

        private void SaveRouteItems()
        {
            WriteRows("routes.txt", RouteItems.Select(r => String.Join("|",
                r.RouteId, Clean(r.UserId), r.TripScheduleId, Clean(r.Country),
                Clean(r.PlaceName), Clean(r.Category), r.DistanceFromStart,
                r.Popularity, Clean(r.SortType), r.DisplayOrder)));
        }

        private void LoadTripShares()
        {
            foreach (string row in ReadRows("shares.txt"))
            {
                string[] f = row.Split('|');
                if (f.Length >= 7)
                {
                    TripShares.Add(new TripShare
                    {
                        ShareId = IntAt(f, 0),
                        OwnerUserId = StrAt(f, 1),
                        FriendUserId = StrAt(f, 2),
                        TripScheduleId = IntAt(f, 3),
                        Rating = IntAt(f, 4),
                        Description = StrAt(f, 5),
                        CreatedDate = StrAt(f, 6)
                    });
                }
            }
        }

        private void SaveTripShares()
        {
            WriteRows("shares.txt", TripShares.Select(s => String.Join("|",
                s.ShareId, Clean(s.OwnerUserId), Clean(s.FriendUserId),
                s.TripScheduleId, s.Rating, Clean(s.Description), Clean(s.CreatedDate))));
        }

        private void RemoveReservationMirrorSchedules()
        {
            Schedules.RemoveAll(s =>
                s.Mode == ModeType.Travel &&
                s.Title != null &&
                s.Title.StartsWith("예약:"));
        }

        private void EnsureDefaultPlaces()
        {
            AddDefaultPlace("japan", "Tokyo Tower", "Minato, Tokyo", "Sightseeing", 2, 12, 95);
            AddDefaultPlace("japan", "Sensoji Temple", "Asakusa, Tokyo", "Culture", 1, 9, 92);
            AddDefaultPlace("japan", "Shibuya Crossing", "Shibuya, Tokyo", "Landmark", 3, 18, 97);
            AddDefaultPlace("japan", "Fushimi Inari", "Kyoto", "Culture", 2, 35, 94);
            AddDefaultPlace("japan", "Dotonbori", "Osaka", "Food", 4, 41, 91);
            AddDefaultPlace("japan", "Arashiyama", "Kyoto", "Nature", 5, 48, 88);

            AddDefaultPlace("jeju", "Jeju Airport", "Jeju Airport", "Start", 1, 0, 100);
            AddDefaultPlace("jeju", "Dongmun Market", "Jeju-si", "Food", 2, 15, 90);
            AddDefaultPlace("jeju", "Hamdeok Beach", "Jeju-si", "Beach", 3, 35, 85);
            AddDefaultPlace("jeju", "Seongsan Ilchulbong", "Seogwipo", "Nature", 1, 47, 96);
            AddDefaultPlace("jeju", "Udo Island", "Jeju", "Island", 4, 55, 89);
            AddDefaultPlace("jeju", "Osulloc Tea Museum", "Seogwipo", "Cafe", 5, 39, 84);

            AddDefaultPlace("seoul", "Gyeongbokgung", "Jongno-gu", "Culture", 1, 8, 92);
            AddDefaultPlace("seoul", "Hongdae Cafe Street", "Mapo-gu", "Food", 2, 20, 88);
            AddDefaultPlace("seoul", "N Seoul Tower", "Yongsan-gu", "Landmark", 3, 14, 91);
            AddDefaultPlace("seoul", "Seongsu Cafe", "Seongdong-gu", "Cafe", 4, 16, 87);
            AddDefaultPlace("seoul", "Lotte World", "Songpa-gu", "Theme Park", 5, 28, 89);
            AddDefaultPlace("seoul", "DDP", "Jung-gu", "Design", 3, 11, 83);

            AddDefaultPlace("busan", "Haeundae Beach", "Haeundae-gu", "Beach", 1, 10, 94);
            AddDefaultPlace("busan", "Gamcheon Culture Village", "Saha-gu", "Culture", 2, 22, 90);
            AddDefaultPlace("busan", "Gwangalli Beach", "Suyeong-gu", "Beach", 3, 14, 91);
            AddDefaultPlace("busan", "Jagalchi Market", "Jung-gu", "Food", 4, 18, 87);
            AddDefaultPlace("busan", "Taejongdae", "Yeongdo-gu", "Nature", 5, 33, 86);
            AddDefaultPlace("busan", "Songdo Cable Car", "Seo-gu", "Activity", 4, 25, 84);

            AddDefaultPlace("china", "The Bund", "Shanghai", "Landmark", 1, 8, 93);
            AddDefaultPlace("china", "Yu Garden", "Shanghai", "Culture", 2, 12, 89);
            AddDefaultPlace("china", "Great Wall", "Beijing", "History", 1, 58, 97);
            AddDefaultPlace("china", "Forbidden City", "Beijing", "History", 2, 21, 95);
            AddDefaultPlace("china", "West Lake", "Hangzhou", "Nature", 4, 47, 88);
            AddDefaultPlace("china", "Chengdu Panda Base", "Chengdu", "Nature", 5, 39, 91);

            AddDefaultPlace("australia", "Sydney Opera House", "Sydney", "Landmark", 1, 10, 97);
            AddDefaultPlace("australia", "Bondi Beach", "Sydney", "Beach", 2, 18, 92);
            AddDefaultPlace("australia", "Great Barrier Reef", "Queensland", "Nature", 1, 70, 98);
            AddDefaultPlace("australia", "Queen Victoria Market", "Melbourne", "Food", 4, 22, 86);
            AddDefaultPlace("australia", "Blue Mountains", "New South Wales", "Nature", 3, 55, 90);
            AddDefaultPlace("australia", "Royal Botanic Garden", "Sydney", "Nature", 5, 8, 84);
        }

        private void AddDefaultPlace(string region, string placeName, string address, string category, int priority, int distance, int score)
        {
            bool exists = Places.Any(p =>
                String.Equals(p.Region, region, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(p.PlaceName, placeName, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                return;
            }

            Places.Add(new Place
            {
                PlaceId = NextPlaceId(),
                Region = region,
                PlaceName = placeName,
                Address = address,
                Category = category,
                Priority = priority,
                DistanceFromStart = distance,
                RecommendScore = score
            });
        }

        private void EnsureDefaultWeather()
        {
            if (WeatherList.Count > 0)
            {
                return;
            }

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            WeatherList.Add(new WeatherInfo { WeatherId = 1, Date = today, Region = "Seoul", Weather = WeatherType.Rainy, RainProbability = 70 });
            WeatherList.Add(new WeatherInfo { WeatherId = 2, Date = today, Region = "Jeju", Weather = WeatherType.Cloudy, RainProbability = 45 });
            WeatherList.Add(new WeatherInfo { WeatherId = 3, Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), Region = "Busan", Weather = WeatherType.Sunny, RainProbability = 10 });
            WeatherList.Add(new WeatherInfo { WeatherId = 4, Date = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), Region = "Seoul", Weather = WeatherType.Rainy, RainProbability = 80 });
        }
    }
}
