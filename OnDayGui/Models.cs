namespace OnDayGui
{
    public enum ModeType
    {
        Travel = 1,
        Daily = 2
    }

    public enum StatusType
    {
        NotDone = 0,
        Done = 1
    }

    public enum WeatherType
    {
        Sunny = 1,
        Cloudy = 2,
        Rainy = 3,
        Snowy = 4
    }

    public class User
    {
        public string UserId = "";
        public string Password = "";
        public string Nickname = "";
    }

    public class Schedule
    {
        public int ScheduleId;
        public string UserId = "";
        public ModeType Mode;
        public string Title = "";
        public string Date = "";
        public string StartTime = "";
        public string EndTime = "";
        public string Location = "";
        public string Category = "";
        public int Importance;
        public string Memo = "";
        public StatusType IsCompleted;
    }

    public class Todo
    {
        public int TodoId;
        public string UserId = "";
        public string Title = "";
        public string DueDate = "";
        public int Importance;
        public StatusType IsCompleted;
        public string Memo = "";
    }

    public class Reservation
    {
        public int ReservationId;
        public int TripScheduleId;
        public string UserId = "";
        public string ReservationType = "";
        public string ReservationName = "";
        public string StartDate = "";
        public string StartTime = "";
        public string EndDate = "";
        public string EndTime = "";
        public string Location = "";
        public int Price;
        public string Memo = "";
    }

    public class Expense
    {
        public int ExpenseId;
        public int TripScheduleId;
        public string UserId = "";
        public ModeType Mode;
        public string Date = "";
        public string Category = "";
        public string Payer = "";
        public int ParticipantCount;
        public int Amount;
        public string Memo = "";
    }

    public class PartTimeJob
    {
        public int JobId;
        public string UserId = "";
        public string JobName = "";
        public string WorkDate = "";
        public string StartTime = "";
        public string EndTime = "";
        public int HourlyWage;
        public int WorkHours;
        public int TotalPay;
    }

    public class Place
    {
        public int PlaceId;
        public string Region = "";
        public string PlaceName = "";
        public string Address = "";
        public string Category = "";
        public int Priority;
        public int DistanceFromStart;
        public int RecommendScore;
    }

    public class ChecklistItem
    {
        public int ItemId;
        public int TripScheduleId;
        public string UserId = "";
        public ModeType Mode;
        public string ItemName = "";
        public string Category = "";
        public StatusType IsChecked;
        public int IsShared;
    }

    public class Friend
    {
        public int FriendId;
        public string UserId = "";
        public string FriendUserId = "";
        public string FriendNickname = "";
        public int IsScheduleShared;
        public int Status;
        public string RequestedBy = "";
    }

    public class Review
    {
        public int ReviewId;
        public string UserId = "";
        public string PlaceName = "";
        public int Rating;
        public string Content = "";
        public string CreatedDate = "";
    }

    public class WeatherInfo
    {
        public int WeatherId;
        public string Date = "";
        public string Region = "";
        public WeatherType Weather;
        public int RainProbability;
    }

    public class SleepAlarm
    {
        public int SleepId;
        public string UserId = "";
        public string Date = "";
        public string WakeUpTime = "";
        public int TargetSleepHours;
        public string RecommendedBedTime = "";
        public int IsAlarmOn;
    }

    public class RouteItem
    {
        public int RouteId;
        public string UserId = "";
        public int TripScheduleId;
        public string Country = "";
        public string PlaceName = "";
        public string Category = "";
        public int DistanceFromStart;
        public int Popularity;
        public string SortType = "";
        public int DisplayOrder;
    }

    public class TripShare
    {
        public int ShareId;
        public string OwnerUserId = "";
        public string FriendUserId = "";
        public int TripScheduleId;
        public int Rating;
        public string Description = "";
        public string CreatedDate = "";
    }
}
