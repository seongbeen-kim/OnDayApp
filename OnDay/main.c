#include <ctype.h>
#include <direct.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#define MAX_USERS 50
#define MAX_SCHEDULES 200
#define MAX_TODOS 200
#define MAX_RESERVATIONS 100
#define MAX_EXPENSES 200
#define MAX_JOBS 100
#define MAX_PLACES 100
#define MAX_CHECKLIST 200
#define MAX_FRIENDS 100
#define MAX_REVIEWS 200
#define MAX_WEATHER 100

#define MAX_ID_LEN 30
#define MAX_PW_LEN 30
#define MAX_NAME_LEN 50
#define MAX_TITLE_LEN 100
#define MAX_MEMO_LEN 200
#define MAX_DATE_LEN 20
#define MAX_TIME_LEN 10
#define MAX_LOCATION_LEN 100
#define DATA_DIR "data"

typedef enum {
    MODE_TRAVEL = 1,
    MODE_DAILY = 2
} ModeType;

typedef enum {
    STATUS_NOT_DONE = 0,
    STATUS_DONE = 1
} StatusType;

typedef enum {
    WEATHER_SUNNY = 1,
    WEATHER_CLOUDY = 2,
    WEATHER_RAINY = 3,
    WEATHER_SNOWY = 4
} WeatherType;

typedef struct {
    char userId[MAX_ID_LEN];
    char password[MAX_PW_LEN];
    char nickname[MAX_NAME_LEN];
} User;

typedef struct {
    int scheduleId;
    char userId[MAX_ID_LEN];
    ModeType mode;
    char title[MAX_TITLE_LEN];
    char date[MAX_DATE_LEN];
    char startTime[MAX_TIME_LEN];
    char endTime[MAX_TIME_LEN];
    char location[MAX_LOCATION_LEN];
    char category[MAX_NAME_LEN];
    int importance;
    char memo[MAX_MEMO_LEN];
    StatusType isCompleted;
} Schedule;

typedef struct {
    int todoId;
    char userId[MAX_ID_LEN];
    char title[MAX_TITLE_LEN];
    char dueDate[MAX_DATE_LEN];
    int importance;
    StatusType isCompleted;
    char memo[MAX_MEMO_LEN];
} Todo;

typedef struct {
    int reservationId;
    char userId[MAX_ID_LEN];
    char reservationType[MAX_NAME_LEN];
    char reservationName[MAX_TITLE_LEN];
    char startDate[MAX_DATE_LEN];
    char startTime[MAX_TIME_LEN];
    char endDate[MAX_DATE_LEN];
    char endTime[MAX_TIME_LEN];
    char location[MAX_LOCATION_LEN];
    int price;
    char memo[MAX_MEMO_LEN];
} Reservation;

typedef struct {
    int expenseId;
    char userId[MAX_ID_LEN];
    ModeType mode;
    char date[MAX_DATE_LEN];
    char category[MAX_NAME_LEN];
    char payer[MAX_NAME_LEN];
    int participantCount;
    int amount;
    char memo[MAX_MEMO_LEN];
} Expense;

typedef struct {
    int jobId;
    char userId[MAX_ID_LEN];
    char jobName[MAX_TITLE_LEN];
    char workDate[MAX_DATE_LEN];
    char startTime[MAX_TIME_LEN];
    char endTime[MAX_TIME_LEN];
    int hourlyWage;
    int workHours;
    int totalPay;
} PartTimeJob;

typedef struct {
    int placeId;
    char region[MAX_LOCATION_LEN];
    char placeName[MAX_LOCATION_LEN];
    char address[MAX_LOCATION_LEN];
    char category[MAX_NAME_LEN];
    int priority;
    int distanceFromStart;
    int recommendScore;
} Place;

typedef struct {
    int itemId;
    char userId[MAX_ID_LEN];
    ModeType mode;
    char itemName[MAX_TITLE_LEN];
    char category[MAX_NAME_LEN];
    StatusType isChecked;
    int isShared;
} ChecklistItem;

typedef struct {
    int friendId;
    char userId[MAX_ID_LEN];
    char friendUserId[MAX_ID_LEN];
    char friendNickname[MAX_NAME_LEN];
    int isScheduleShared;
} Friend;

typedef struct {
    int reviewId;
    char userId[MAX_ID_LEN];
    char placeName[MAX_LOCATION_LEN];
    int rating;
    char content[MAX_MEMO_LEN];
    char createdDate[MAX_DATE_LEN];
} Review;

typedef struct {
    int weatherId;
    char date[MAX_DATE_LEN];
    char region[MAX_LOCATION_LEN];
    WeatherType weather;
    int rainProbability;
} WeatherInfo;

typedef struct {
    int sleepId;
    char userId[MAX_ID_LEN];
    char date[MAX_DATE_LEN];
    char wakeUpTime[MAX_TIME_LEN];
    int targetSleepHours;
    char recommendedBedTime[MAX_TIME_LEN];
    int isAlarmOn;
} SleepAlarm;

static User users[MAX_USERS];
static Schedule schedules[MAX_SCHEDULES];
static Todo todos[MAX_TODOS];
static Reservation reservations[MAX_RESERVATIONS];
static Expense expenses[MAX_EXPENSES];
static PartTimeJob jobs[MAX_JOBS];
static Place places[MAX_PLACES];
static ChecklistItem checklist[MAX_CHECKLIST];
static Friend friends[MAX_FRIENDS];
static Review reviews[MAX_REVIEWS];
static WeatherInfo weatherList[MAX_WEATHER];
static SleepAlarm sleepAlarms[MAX_USERS];

static int userCount = 0;
static int scheduleCount = 0;
static int todoCount = 0;
static int reservationCount = 0;
static int expenseCount = 0;
static int jobCount = 0;
static int placeCount = 0;
static int checklistCount = 0;
static int friendCount = 0;
static int reviewCount = 0;
static int weatherCount = 0;
static int sleepAlarmCount = 0;

static char currentUserId[MAX_ID_LEN] = "";
static char currentNickname[MAX_NAME_LEN] = "";

static void loadAll(void);
static void saveAll(void);
static void authMenu(void);
static void mainMenu(void);
static void travelMenu(void);
static void dailyMenu(void);
static void friendMenu(void);
static void reviewMenu(void);
static void settingsMenu(void);

static void trimNewline(char* text)
{
    size_t len = strlen(text);
    while (len > 0 && (text[len - 1] == '\n' || text[len - 1] == '\r')) {
        text[len - 1] = '\0';
        len--;
    }
}

static void setString(char* dst, size_t dstSize, const char* src)
{
    if (dstSize == 0) {
        return;
    }
    strncpy(dst, src, dstSize - 1);
    dst[dstSize - 1] = '\0';
}

static void sanitizeField(char* text)
{
    size_t i;
    for (i = 0; text[i] != '\0'; i++) {
        if (text[i] == '|') {
            text[i] = '/';
        }
    }
}

static void readLine(const char* prompt, char* buffer, size_t size)
{
    printf("%s", prompt);
    if (fgets(buffer, (int)size, stdin) == NULL) {
        clearerr(stdin);
        buffer[0] = '\0';
        return;
    }
    trimNewline(buffer);
    sanitizeField(buffer);
}

static int readInt(const char* prompt, int minValue, int maxValue)
{
    char buffer[64];
    char* end = NULL;
    long value;

    while (1) {
        readLine(prompt, buffer, sizeof(buffer));
        if (buffer[0] == '\0') {
            printf("Please enter a number.\n");
            continue;
        }

        errno = 0;
        value = strtol(buffer, &end, 10);
        while (end != NULL && *end != '\0' && isspace((unsigned char)*end)) {
            end++;
        }

        if (errno == 0 && end != NULL && *end == '\0') {
            if (minValue <= maxValue && (value < minValue || value > maxValue)) {
                printf("Enter a value between %d and %d.\n", minValue, maxValue);
                continue;
            }
            return (int)value;
        }

        printf("Invalid number.\n");
    }
}

static int confirm(const char* prompt)
{
    char answer[16];
    readLine(prompt, answer, sizeof(answer));
    return answer[0] == 'y' || answer[0] == 'Y';
}

static void pauseEnter(void)
{
    char buffer[8];
    printf("\nPress Enter to continue...");
    (void)fgets(buffer, sizeof(buffer), stdin);
}

static void printHeader(const char* title)
{
    printf("\n============================================================\n");
    printf(" OnDay - %s\n", title);
    printf("============================================================\n");
}

static void buildDataPath(const char* filename, char* path, size_t size)
{
    (void)snprintf(path, size, "%s\\%s", DATA_DIR, filename);
}

static FILE* openDataFile(const char* filename, const char* mode)
{
    char path[160];
    buildDataPath(filename, path, sizeof(path));
    return fopen(path, mode);
}

static int splitFields(char* line, char** fields, int maxFields)
{
    int count = 0;
    char* start = line;
    char* p = line;

    while (1) {
        if (*p == '|' || *p == '\0') {
            if (count < maxFields) {
                fields[count++] = start;
            }
            if (*p == '\0') {
                break;
            }
            *p = '\0';
            start = p + 1;
        }
        p++;
    }

    return count;
}

static int equalsIgnoreCase(const char* a, const char* b)
{
    while (*a != '\0' && *b != '\0') {
        if (tolower((unsigned char)*a) != tolower((unsigned char)*b)) {
            return 0;
        }
        a++;
        b++;
    }
    return *a == '\0' && *b == '\0';
}

static int containsIgnoreCase(const char* text, const char* keyword)
{
    size_t textLen = strlen(text);
    size_t keywordLen = strlen(keyword);
    size_t i;
    size_t j;

    if (keywordLen == 0) {
        return 1;
    }

    if (keywordLen > textLen) {
        return 0;
    }

    for (i = 0; i <= textLen - keywordLen; i++) {
        int same = 1;
        for (j = 0; j < keywordLen; j++) {
            if (tolower((unsigned char)text[i + j]) != tolower((unsigned char)keyword[j])) {
                same = 0;
                break;
            }
        }
        if (same) {
            return 1;
        }
    }

    return 0;
}

static const char* modeName(ModeType mode)
{
    return mode == MODE_TRAVEL ? "Travel" : "Daily";
}

static const char* statusName(StatusType status)
{
    return status == STATUS_DONE ? "Done" : "Not done";
}

static const char* weatherName(WeatherType weather)
{
    switch (weather) {
    case WEATHER_SUNNY:
        return "Sunny";
    case WEATHER_CLOUDY:
        return "Cloudy";
    case WEATHER_RAINY:
        return "Rainy";
    case WEATHER_SNOWY:
        return "Snowy";
    default:
        return "Unknown";
    }
}

static int parseTimeToMinutes(const char* timeText)
{
    int hour = 0;
    int minute = 0;
    if (sscanf(timeText, "%d:%d", &hour, &minute) != 2) {
        return -1;
    }
    if (hour < 0 || hour > 23 || minute < 0 || minute > 59) {
        return -1;
    }
    return hour * 60 + minute;
}

static void minutesToTime(int minutes, char* out, size_t outSize)
{
    while (minutes < 0) {
        minutes += 24 * 60;
    }
    minutes %= 24 * 60;
    (void)snprintf(out, outSize, "%02d:%02d", minutes / 60, minutes % 60);
}

static void todayString(char* out, size_t outSize)
{
    time_t now = time(NULL);
    struct tm localNow;
    localtime_s(&localNow, &now);
    (void)strftime(out, outSize, "%Y-%m-%d", &localNow);
}

static void nowStamp(char* out, size_t outSize)
{
    time_t now = time(NULL);
    struct tm localNow;
    localtime_s(&localNow, &now);
    (void)strftime(out, outSize, "%Y%m%d_%H%M%S", &localNow);
}

static int isCurrentUser(const char* userId)
{
    return strcmp(userId, currentUserId) == 0;
}

static int findUserIndex(const char* userId)
{
    int i;
    for (i = 0; i < userCount; i++) {
        if (strcmp(users[i].userId, userId) == 0) {
            return i;
        }
    }
    return -1;
}

static int nextScheduleId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < scheduleCount; i++) {
        if (schedules[i].scheduleId > maxId) {
            maxId = schedules[i].scheduleId;
        }
    }
    return maxId + 1;
}

static int nextTodoId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < todoCount; i++) {
        if (todos[i].todoId > maxId) {
            maxId = todos[i].todoId;
        }
    }
    return maxId + 1;
}

static int nextReservationId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < reservationCount; i++) {
        if (reservations[i].reservationId > maxId) {
            maxId = reservations[i].reservationId;
        }
    }
    return maxId + 1;
}

static int nextExpenseId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < expenseCount; i++) {
        if (expenses[i].expenseId > maxId) {
            maxId = expenses[i].expenseId;
        }
    }
    return maxId + 1;
}

static int nextJobId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < jobCount; i++) {
        if (jobs[i].jobId > maxId) {
            maxId = jobs[i].jobId;
        }
    }
    return maxId + 1;
}

static int nextPlaceId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < placeCount; i++) {
        if (places[i].placeId > maxId) {
            maxId = places[i].placeId;
        }
    }
    return maxId + 1;
}

static int nextChecklistId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < checklistCount; i++) {
        if (checklist[i].itemId > maxId) {
            maxId = checklist[i].itemId;
        }
    }
    return maxId + 1;
}

static int nextFriendId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < friendCount; i++) {
        if (friends[i].friendId > maxId) {
            maxId = friends[i].friendId;
        }
    }
    return maxId + 1;
}

static int nextReviewId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < reviewCount; i++) {
        if (reviews[i].reviewId > maxId) {
            maxId = reviews[i].reviewId;
        }
    }
    return maxId + 1;
}

static int nextWeatherId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < weatherCount; i++) {
        if (weatherList[i].weatherId > maxId) {
            maxId = weatherList[i].weatherId;
        }
    }
    return maxId + 1;
}

static int nextSleepId(void)
{
    int i;
    int maxId = 0;
    for (i = 0; i < sleepAlarmCount; i++) {
        if (sleepAlarms[i].sleepId > maxId) {
            maxId = sleepAlarms[i].sleepId;
        }
    }
    return maxId + 1;
}

static void loadUsers(void)
{
    FILE* fp = openDataFile("users.txt", "r");
    char line[512];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && userCount < MAX_USERS) {
        char* fields[3];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, fields, 3) >= 3) {
            setString(users[userCount].userId, sizeof(users[userCount].userId), fields[0]);
            setString(users[userCount].password, sizeof(users[userCount].password), fields[1]);
            setString(users[userCount].nickname, sizeof(users[userCount].nickname), fields[2]);
            userCount++;
        }
    }
    fclose(fp);
}

static void saveUsers(void)
{
    FILE* fp = openDataFile("users.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < userCount; i++) {
        fprintf(fp, "%s|%s|%s\n", users[i].userId, users[i].password, users[i].nickname);
    }
    fclose(fp);
}

static void loadSchedules(void)
{
    FILE* fp = openDataFile("schedules.txt", "r");
    char line[1024];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && scheduleCount < MAX_SCHEDULES) {
        char* f[12];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 12) >= 12) {
            Schedule* s = &schedules[scheduleCount++];
            s->scheduleId = atoi(f[0]);
            setString(s->userId, sizeof(s->userId), f[1]);
            s->mode = (ModeType)atoi(f[2]);
            setString(s->title, sizeof(s->title), f[3]);
            setString(s->date, sizeof(s->date), f[4]);
            setString(s->startTime, sizeof(s->startTime), f[5]);
            setString(s->endTime, sizeof(s->endTime), f[6]);
            setString(s->location, sizeof(s->location), f[7]);
            setString(s->category, sizeof(s->category), f[8]);
            s->importance = atoi(f[9]);
            setString(s->memo, sizeof(s->memo), f[10]);
            s->isCompleted = (StatusType)atoi(f[11]);
        }
    }
    fclose(fp);
}

static void saveSchedules(void)
{
    FILE* fp = openDataFile("schedules.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < scheduleCount; i++) {
        Schedule* s = &schedules[i];
        fprintf(fp, "%d|%s|%d|%s|%s|%s|%s|%s|%s|%d|%s|%d\n",
            s->scheduleId, s->userId, (int)s->mode, s->title, s->date,
            s->startTime, s->endTime, s->location, s->category,
            s->importance, s->memo, (int)s->isCompleted);
    }
    fclose(fp);
}

static void loadTodos(void)
{
    FILE* fp = openDataFile("todos.txt", "r");
    char line[768];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && todoCount < MAX_TODOS) {
        char* f[7];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 7) >= 7) {
            Todo* t = &todos[todoCount++];
            t->todoId = atoi(f[0]);
            setString(t->userId, sizeof(t->userId), f[1]);
            setString(t->title, sizeof(t->title), f[2]);
            setString(t->dueDate, sizeof(t->dueDate), f[3]);
            t->importance = atoi(f[4]);
            t->isCompleted = (StatusType)atoi(f[5]);
            setString(t->memo, sizeof(t->memo), f[6]);
        }
    }
    fclose(fp);
}

static void saveTodos(void)
{
    FILE* fp = openDataFile("todos.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < todoCount; i++) {
        Todo* t = &todos[i];
        fprintf(fp, "%d|%s|%s|%s|%d|%d|%s\n",
            t->todoId, t->userId, t->title, t->dueDate, t->importance,
            (int)t->isCompleted, t->memo);
    }
    fclose(fp);
}

static void loadReservations(void)
{
    FILE* fp = openDataFile("reservations.txt", "r");
    char line[1024];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && reservationCount < MAX_RESERVATIONS) {
        char* f[11];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 11) >= 11) {
            Reservation* r = &reservations[reservationCount++];
            r->reservationId = atoi(f[0]);
            setString(r->userId, sizeof(r->userId), f[1]);
            setString(r->reservationType, sizeof(r->reservationType), f[2]);
            setString(r->reservationName, sizeof(r->reservationName), f[3]);
            setString(r->startDate, sizeof(r->startDate), f[4]);
            setString(r->startTime, sizeof(r->startTime), f[5]);
            setString(r->endDate, sizeof(r->endDate), f[6]);
            setString(r->endTime, sizeof(r->endTime), f[7]);
            setString(r->location, sizeof(r->location), f[8]);
            r->price = atoi(f[9]);
            setString(r->memo, sizeof(r->memo), f[10]);
        }
    }
    fclose(fp);
}

static void saveReservations(void)
{
    FILE* fp = openDataFile("reservations.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < reservationCount; i++) {
        Reservation* r = &reservations[i];
        fprintf(fp, "%d|%s|%s|%s|%s|%s|%s|%s|%s|%d|%s\n",
            r->reservationId, r->userId, r->reservationType, r->reservationName,
            r->startDate, r->startTime, r->endDate, r->endTime,
            r->location, r->price, r->memo);
    }
    fclose(fp);
}

static void loadExpenses(void)
{
    FILE* fp = openDataFile("expenses.txt", "r");
    char line[768];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && expenseCount < MAX_EXPENSES) {
        char* f[9];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 9) >= 9) {
            Expense* e = &expenses[expenseCount++];
            e->expenseId = atoi(f[0]);
            setString(e->userId, sizeof(e->userId), f[1]);
            e->mode = (ModeType)atoi(f[2]);
            setString(e->date, sizeof(e->date), f[3]);
            setString(e->category, sizeof(e->category), f[4]);
            setString(e->payer, sizeof(e->payer), f[5]);
            e->participantCount = atoi(f[6]);
            e->amount = atoi(f[7]);
            setString(e->memo, sizeof(e->memo), f[8]);
        }
    }
    fclose(fp);
}

static void saveExpenses(void)
{
    FILE* fp = openDataFile("expenses.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < expenseCount; i++) {
        Expense* e = &expenses[i];
        fprintf(fp, "%d|%s|%d|%s|%s|%s|%d|%d|%s\n",
            e->expenseId, e->userId, (int)e->mode, e->date, e->category,
            e->payer, e->participantCount, e->amount, e->memo);
    }
    fclose(fp);
}

static void loadJobs(void)
{
    FILE* fp = openDataFile("jobs.txt", "r");
    char line[768];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && jobCount < MAX_JOBS) {
        char* f[9];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 9) >= 9) {
            PartTimeJob* j = &jobs[jobCount++];
            j->jobId = atoi(f[0]);
            setString(j->userId, sizeof(j->userId), f[1]);
            setString(j->jobName, sizeof(j->jobName), f[2]);
            setString(j->workDate, sizeof(j->workDate), f[3]);
            setString(j->startTime, sizeof(j->startTime), f[4]);
            setString(j->endTime, sizeof(j->endTime), f[5]);
            j->hourlyWage = atoi(f[6]);
            j->workHours = atoi(f[7]);
            j->totalPay = atoi(f[8]);
        }
    }
    fclose(fp);
}

static void saveJobs(void)
{
    FILE* fp = openDataFile("jobs.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < jobCount; i++) {
        PartTimeJob* j = &jobs[i];
        fprintf(fp, "%d|%s|%s|%s|%s|%s|%d|%d|%d\n",
            j->jobId, j->userId, j->jobName, j->workDate, j->startTime,
            j->endTime, j->hourlyWage, j->workHours, j->totalPay);
    }
    fclose(fp);
}

static void loadPlaces(void)
{
    FILE* fp = openDataFile("places.txt", "r");
    char line[768];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && placeCount < MAX_PLACES) {
        char* f[8];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 8) >= 8) {
            Place* p = &places[placeCount++];
            p->placeId = atoi(f[0]);
            setString(p->region, sizeof(p->region), f[1]);
            setString(p->placeName, sizeof(p->placeName), f[2]);
            setString(p->address, sizeof(p->address), f[3]);
            setString(p->category, sizeof(p->category), f[4]);
            p->priority = atoi(f[5]);
            p->distanceFromStart = atoi(f[6]);
            p->recommendScore = atoi(f[7]);
        }
    }
    fclose(fp);
}

static void savePlaces(void)
{
    FILE* fp = openDataFile("places.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < placeCount; i++) {
        Place* p = &places[i];
        fprintf(fp, "%d|%s|%s|%s|%s|%d|%d|%d\n",
            p->placeId, p->region, p->placeName, p->address, p->category,
            p->priority, p->distanceFromStart, p->recommendScore);
    }
    fclose(fp);
}

static void loadChecklist(void)
{
    FILE* fp = openDataFile("checklist.txt", "r");
    char line[768];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && checklistCount < MAX_CHECKLIST) {
        char* f[7];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 7) >= 7) {
            ChecklistItem* c = &checklist[checklistCount++];
            c->itemId = atoi(f[0]);
            setString(c->userId, sizeof(c->userId), f[1]);
            c->mode = (ModeType)atoi(f[2]);
            setString(c->itemName, sizeof(c->itemName), f[3]);
            setString(c->category, sizeof(c->category), f[4]);
            c->isChecked = (StatusType)atoi(f[5]);
            c->isShared = atoi(f[6]);
        }
    }
    fclose(fp);
}

static void saveChecklist(void)
{
    FILE* fp = openDataFile("checklist.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < checklistCount; i++) {
        ChecklistItem* c = &checklist[i];
        fprintf(fp, "%d|%s|%d|%s|%s|%d|%d\n",
            c->itemId, c->userId, (int)c->mode, c->itemName, c->category,
            (int)c->isChecked, c->isShared);
    }
    fclose(fp);
}

static void loadFriends(void)
{
    FILE* fp = openDataFile("friends.txt", "r");
    char line[512];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && friendCount < MAX_FRIENDS) {
        char* f[5];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 5) >= 5) {
            Friend* fr = &friends[friendCount++];
            fr->friendId = atoi(f[0]);
            setString(fr->userId, sizeof(fr->userId), f[1]);
            setString(fr->friendUserId, sizeof(fr->friendUserId), f[2]);
            setString(fr->friendNickname, sizeof(fr->friendNickname), f[3]);
            fr->isScheduleShared = atoi(f[4]);
        }
    }
    fclose(fp);
}

static void saveFriends(void)
{
    FILE* fp = openDataFile("friends.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < friendCount; i++) {
        Friend* fr = &friends[i];
        fprintf(fp, "%d|%s|%s|%s|%d\n",
            fr->friendId, fr->userId, fr->friendUserId, fr->friendNickname,
            fr->isScheduleShared);
    }
    fclose(fp);
}

static void loadReviews(void)
{
    FILE* fp = openDataFile("reviews.txt", "r");
    char line[768];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && reviewCount < MAX_REVIEWS) {
        char* f[6];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 6) >= 6) {
            Review* r = &reviews[reviewCount++];
            r->reviewId = atoi(f[0]);
            setString(r->userId, sizeof(r->userId), f[1]);
            setString(r->placeName, sizeof(r->placeName), f[2]);
            r->rating = atoi(f[3]);
            setString(r->content, sizeof(r->content), f[4]);
            setString(r->createdDate, sizeof(r->createdDate), f[5]);
        }
    }
    fclose(fp);
}

static void saveReviews(void)
{
    FILE* fp = openDataFile("reviews.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < reviewCount; i++) {
        Review* r = &reviews[i];
        fprintf(fp, "%d|%s|%s|%d|%s|%s\n",
            r->reviewId, r->userId, r->placeName, r->rating,
            r->content, r->createdDate);
    }
    fclose(fp);
}

static void loadWeather(void)
{
    FILE* fp = openDataFile("weather.txt", "r");
    char line[512];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && weatherCount < MAX_WEATHER) {
        char* f[5];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 5) >= 5) {
            WeatherInfo* w = &weatherList[weatherCount++];
            w->weatherId = atoi(f[0]);
            setString(w->date, sizeof(w->date), f[1]);
            setString(w->region, sizeof(w->region), f[2]);
            w->weather = (WeatherType)atoi(f[3]);
            w->rainProbability = atoi(f[4]);
        }
    }
    fclose(fp);
}

static void saveWeather(void)
{
    FILE* fp = openDataFile("weather.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < weatherCount; i++) {
        WeatherInfo* w = &weatherList[i];
        fprintf(fp, "%d|%s|%s|%d|%d\n",
            w->weatherId, w->date, w->region, (int)w->weather, w->rainProbability);
    }
    fclose(fp);
}

static void loadSleepAlarms(void)
{
    FILE* fp = openDataFile("sleep.txt", "r");
    char line[512];
    if (fp == NULL) {
        return;
    }
    while (fgets(line, sizeof(line), fp) != NULL && sleepAlarmCount < MAX_USERS) {
        char* f[7];
        trimNewline(line);
        if (line[0] == '\0') {
            continue;
        }
        if (splitFields(line, f, 7) >= 7) {
            SleepAlarm* s = &sleepAlarms[sleepAlarmCount++];
            s->sleepId = atoi(f[0]);
            setString(s->userId, sizeof(s->userId), f[1]);
            setString(s->date, sizeof(s->date), f[2]);
            setString(s->wakeUpTime, sizeof(s->wakeUpTime), f[3]);
            s->targetSleepHours = atoi(f[4]);
            setString(s->recommendedBedTime, sizeof(s->recommendedBedTime), f[5]);
            s->isAlarmOn = atoi(f[6]);
        }
    }
    fclose(fp);
}

static void saveSleepAlarms(void)
{
    FILE* fp = openDataFile("sleep.txt", "w");
    int i;
    if (fp == NULL) {
        return;
    }
    for (i = 0; i < sleepAlarmCount; i++) {
        SleepAlarm* s = &sleepAlarms[i];
        fprintf(fp, "%d|%s|%s|%s|%d|%s|%d\n",
            s->sleepId, s->userId, s->date, s->wakeUpTime,
            s->targetSleepHours, s->recommendedBedTime, s->isAlarmOn);
    }
    fclose(fp);
}

static void ensureDefaultPlaces(void)
{
    if (placeCount > 0) {
        return;
    }
    if (placeCount + 5 > MAX_PLACES) {
        return;
    }

    places[placeCount++] = (Place){ 1, "Jeju", "Airport", "Jeju Airport", "Start", 1, 0, 100 };
    places[placeCount++] = (Place){ 2, "Jeju", "Dongmun Market", "Jeju-si", "Food", 2, 15, 90 };
    places[placeCount++] = (Place){ 3, "Jeju", "Hamdeok Beach", "Jeju-si", "Sightseeing", 3, 35, 85 };
    places[placeCount++] = (Place){ 4, "Seoul", "Gyeongbokgung", "Jongno-gu", "Sightseeing", 1, 8, 92 };
    places[placeCount++] = (Place){ 5, "Seoul", "Hongdae Cafe", "Mapo-gu", "Food", 2, 20, 80 };
}

static void ensureDefaultWeather(void)
{
    char today[MAX_DATE_LEN];
    if (weatherCount > 0) {
        return;
    }
    todayString(today, sizeof(today));
    if (weatherCount + 4 > MAX_WEATHER) {
        return;
    }

    weatherList[weatherCount++] = (WeatherInfo){ 1, "", "Seoul", WEATHER_RAINY, 70 };
    setString(weatherList[weatherCount - 1].date, sizeof(weatherList[weatherCount - 1].date), today);
    weatherList[weatherCount++] = (WeatherInfo){ 2, "", "Jeju", WEATHER_CLOUDY, 45 };
    setString(weatherList[weatherCount - 1].date, sizeof(weatherList[weatherCount - 1].date), today);
    weatherList[weatherCount++] = (WeatherInfo){ 3, "2026-06-11", "Busan", WEATHER_SUNNY, 10 };
    weatherList[weatherCount++] = (WeatherInfo){ 4, "2026-06-12", "Seoul", WEATHER_RAINY, 80 };
}

static void loadAll(void)
{
    _mkdir(DATA_DIR);
    loadUsers();
    loadSchedules();
    loadTodos();
    loadReservations();
    loadExpenses();
    loadJobs();
    loadPlaces();
    loadChecklist();
    loadFriends();
    loadReviews();
    loadWeather();
    loadSleepAlarms();
    ensureDefaultPlaces();
    ensureDefaultWeather();
}

static void saveAll(void)
{
    _mkdir(DATA_DIR);
    saveUsers();
    saveSchedules();
    saveTodos();
    saveReservations();
    saveExpenses();
    saveJobs();
    savePlaces();
    saveChecklist();
    saveFriends();
    saveReviews();
    saveWeather();
    saveSleepAlarms();
}

static void showSplash(void)
{
    printHeader("Travel and daily schedule manager");
    printf("Travel mode, daily mode, sharing, reviews, checklist, weather alerts.\n");
    printf("Data is saved with C structs, arrays, and text files.\n");
}

static void signUp(void)
{
    User newUser;

    printHeader("Sign up");
    if (userCount >= MAX_USERS) {
        printf("User storage is full.\n");
        return;
    }

    readLine("New user ID: ", newUser.userId, sizeof(newUser.userId));
    if (newUser.userId[0] == '\0') {
        printf("User ID cannot be empty.\n");
        return;
    }
    if (findUserIndex(newUser.userId) >= 0) {
        printf("That ID already exists.\n");
        return;
    }

    readLine("Password: ", newUser.password, sizeof(newUser.password));
    readLine("Nickname: ", newUser.nickname, sizeof(newUser.nickname));
    if (newUser.password[0] == '\0' || newUser.nickname[0] == '\0') {
        printf("Password and nickname are required.\n");
        return;
    }

    users[userCount++] = newUser;
    setString(currentUserId, sizeof(currentUserId), newUser.userId);
    setString(currentNickname, sizeof(currentNickname), newUser.nickname);
    saveUsers();
    printf("Sign up complete. Logged in as %s.\n", currentNickname);
}

static void login(void)
{
    char id[MAX_ID_LEN];
    char pw[MAX_PW_LEN];
    int index;

    printHeader("Login");
    readLine("User ID: ", id, sizeof(id));
    readLine("Password: ", pw, sizeof(pw));

    index = findUserIndex(id);
    if (index >= 0 && strcmp(users[index].password, pw) == 0) {
        setString(currentUserId, sizeof(currentUserId), users[index].userId);
        setString(currentNickname, sizeof(currentNickname), users[index].nickname);
        printf("Login success. Welcome, %s.\n", currentNickname);
    } else {
        printf("Login failed. Check your ID and password.\n");
    }
}

static void authMenu(void)
{
    while (currentUserId[0] == '\0') {
        int choice;
        printHeader("Start");
        printf("1. Login\n");
        printf("2. Sign up\n");
        printf("0. Save and exit\n");
        choice = readInt("Select: ", 0, 2);

        if (choice == 1) {
            login();
            pauseEnter();
        } else if (choice == 2) {
            signUp();
            pauseEnter();
        } else {
            saveAll();
            exit(0);
        }
    }
}

static int compareSchedules(const Schedule* a, const Schedule* b)
{
    int cmp = strcmp(a->date, b->date);
    if (cmp != 0) {
        return cmp;
    }
    cmp = strcmp(a->startTime, b->startTime);
    if (cmp != 0) {
        return cmp;
    }
    return a->scheduleId - b->scheduleId;
}

static void sortedScheduleIndices(ModeType mode, int* indices, int* count)
{
    int i;
    int j;
    *count = 0;
    for (i = 0; i < scheduleCount; i++) {
        if (isCurrentUser(schedules[i].userId) && schedules[i].mode == mode) {
            indices[(*count)++] = i;
        }
    }

    for (i = 0; i < *count - 1; i++) {
        for (j = 0; j < *count - i - 1; j++) {
            if (compareSchedules(&schedules[indices[j]], &schedules[indices[j + 1]]) > 0) {
                int temp = indices[j];
                indices[j] = indices[j + 1];
                indices[j + 1] = temp;
            }
        }
    }
}

static void listSchedules(ModeType mode)
{
    int indices[MAX_SCHEDULES];
    int count;
    int i;

    sortedScheduleIndices(mode, indices, &count);
    printHeader(mode == MODE_TRAVEL ? "Travel schedules" : "Daily schedules");
    if (count == 0) {
        printf("No schedules yet.\n");
        return;
    }

    printf("ID | Date       | Time        | Title | Category | Location | Status\n");
    printf("---+------------+-------------+-------+----------+----------+--------\n");
    for (i = 0; i < count; i++) {
        Schedule* s = &schedules[indices[i]];
        printf("%2d | %-10s | %s-%s | %s | %s | %s | %s\n",
            s->scheduleId, s->date, s->startTime, s->endTime, s->title,
            s->category, s->location, statusName(s->isCompleted));
        if (s->memo[0] != '\0') {
            printf("   memo: %s\n", s->memo);
        }
    }
}

static int hasScheduleConflict(const Schedule* target)
{
    int i;
    int start = parseTimeToMinutes(target->startTime);
    int end = parseTimeToMinutes(target->endTime);
    if (start < 0 || end < 0 || end <= start) {
        return 0;
    }

    for (i = 0; i < scheduleCount; i++) {
        Schedule* s = &schedules[i];
        int otherStart;
        int otherEnd;
        if (!isCurrentUser(s->userId) || strcmp(s->date, target->date) != 0) {
            continue;
        }
        otherStart = parseTimeToMinutes(s->startTime);
        otherEnd = parseTimeToMinutes(s->endTime);
        if (otherStart < 0 || otherEnd < 0 || otherEnd <= otherStart) {
            continue;
        }
        if (start < otherEnd && end > otherStart) {
            printf("Time conflict with schedule #%d: %s %s-%s %s\n",
                s->scheduleId, s->date, s->startTime, s->endTime, s->title);
            return 1;
        }
    }
    return 0;
}

static void addSchedule(ModeType mode)
{
    Schedule s;
    printHeader(mode == MODE_TRAVEL ? "Add travel schedule" : "Add daily schedule");
    if (scheduleCount >= MAX_SCHEDULES) {
        printf("Schedule storage is full.\n");
        return;
    }

    memset(&s, 0, sizeof(s));
    s.scheduleId = nextScheduleId();
    setString(s.userId, sizeof(s.userId), currentUserId);
    s.mode = mode;
    readLine("Date (YYYY-MM-DD): ", s.date, sizeof(s.date));
    readLine("Start time (HH:MM): ", s.startTime, sizeof(s.startTime));
    readLine("End time (HH:MM): ", s.endTime, sizeof(s.endTime));
    readLine("Title: ", s.title, sizeof(s.title));
    readLine("Location: ", s.location, sizeof(s.location));
    readLine("Category (flight/class/work/personal/etc): ", s.category, sizeof(s.category));
    s.importance = readInt("Importance (1-5): ", 1, 5);
    readLine("Memo: ", s.memo, sizeof(s.memo));
    s.isCompleted = STATUS_NOT_DONE;

    if (hasScheduleConflict(&s) && !confirm("Save anyway? (y/n): ")) {
        printf("Schedule was not saved.\n");
        return;
    }

    schedules[scheduleCount++] = s;
    saveSchedules();
    printf("Schedule saved with ID %d.\n", s.scheduleId);
}

static void toggleScheduleComplete(ModeType mode)
{
    int id;
    int i;

    listSchedules(mode);
    id = readInt("Schedule ID to toggle: ", 1, 999999);
    for (i = 0; i < scheduleCount; i++) {
        if (schedules[i].scheduleId == id && isCurrentUser(schedules[i].userId) && schedules[i].mode == mode) {
            schedules[i].isCompleted = schedules[i].isCompleted == STATUS_DONE ? STATUS_NOT_DONE : STATUS_DONE;
            saveSchedules();
            printf("Schedule #%d is now %s.\n", id, statusName(schedules[i].isCompleted));
            return;
        }
    }
    printf("Schedule not found.\n");
}

static void scheduleMenu(ModeType mode)
{
    while (1) {
        int choice;
        printHeader(mode == MODE_TRAVEL ? "Travel schedule management" : "Daily timetable management");
        printf("1. Add schedule\n");
        printf("2. List schedules\n");
        printf("3. Complete / reopen schedule\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 3);
        if (choice == 1) {
            addSchedule(mode);
            pauseEnter();
        } else if (choice == 2) {
            listSchedules(mode);
            pauseEnter();
        } else if (choice == 3) {
            toggleScheduleComplete(mode);
            pauseEnter();
        } else {
            return;
        }
    }
}

static int reservationCreatesSchedule(const char* type)
{
    return containsIgnoreCase(type, "flight") ||
        containsIgnoreCase(type, "air") ||
        containsIgnoreCase(type, "hotel") ||
        containsIgnoreCase(type, "lodging") ||
        containsIgnoreCase(type, "rental");
}

static void addScheduleFromReservation(const Reservation* r)
{
    Schedule s;
    if (scheduleCount >= MAX_SCHEDULES) {
        printf("Reservation saved, but schedule storage is full.\n");
        return;
    }
    memset(&s, 0, sizeof(s));
    s.scheduleId = nextScheduleId();
    setString(s.userId, sizeof(s.userId), r->userId);
    s.mode = MODE_TRAVEL;
    (void)snprintf(s.title, sizeof(s.title), "Reservation: %s", r->reservationName);
    setString(s.date, sizeof(s.date), r->startDate);
    setString(s.startTime, sizeof(s.startTime), r->startTime);
    setString(s.endTime, sizeof(s.endTime), r->endTime);
    setString(s.location, sizeof(s.location), r->location);
    setString(s.category, sizeof(s.category), r->reservationType);
    s.importance = 5;
    setString(s.memo, sizeof(s.memo), r->memo);
    s.isCompleted = STATUS_NOT_DONE;
    schedules[scheduleCount++] = s;
    printf("Auto-added travel schedule #%d from reservation.\n", s.scheduleId);
}

static void addReservation(void)
{
    Reservation r;
    printHeader("Reservation auto registration");
    if (reservationCount >= MAX_RESERVATIONS) {
        printf("Reservation storage is full.\n");
        return;
    }

    memset(&r, 0, sizeof(r));
    r.reservationId = nextReservationId();
    setString(r.userId, sizeof(r.userId), currentUserId);
    readLine("Type (flight/hotel/rental/etc): ", r.reservationType, sizeof(r.reservationType));
    readLine("Reservation name: ", r.reservationName, sizeof(r.reservationName));
    readLine("Start date (YYYY-MM-DD): ", r.startDate, sizeof(r.startDate));
    readLine("Start time (HH:MM): ", r.startTime, sizeof(r.startTime));
    readLine("End date (YYYY-MM-DD): ", r.endDate, sizeof(r.endDate));
    readLine("End time (HH:MM): ", r.endTime, sizeof(r.endTime));
    readLine("Location: ", r.location, sizeof(r.location));
    r.price = readInt("Price: ", 0, 999999999);
    readLine("Memo: ", r.memo, sizeof(r.memo));

    reservations[reservationCount++] = r;
    if (reservationCreatesSchedule(r.reservationType)) {
        addScheduleFromReservation(&r);
    }
    saveReservations();
    saveSchedules();
    printf("Reservation saved with ID %d.\n", r.reservationId);
}

static void listReservations(void)
{
    int i;
    int found = 0;
    printHeader("Reservation list");
    for (i = 0; i < reservationCount; i++) {
        Reservation* r = &reservations[i];
        if (!isCurrentUser(r->userId)) {
            continue;
        }
        found = 1;
        printf("#%d [%s] %s | %s %s - %s %s | %s | %d\n",
            r->reservationId, r->reservationType, r->reservationName,
            r->startDate, r->startTime, r->endDate, r->endTime,
            r->location, r->price);
        if (r->memo[0] != '\0') {
            printf("   memo: %s\n", r->memo);
        }
    }
    if (!found) {
        printf("No reservations yet.\n");
    }
}

static void reservationMenu(void)
{
    while (1) {
        int choice;
        printHeader("Reservation management");
        printf("1. Add reservation and reflect it in schedule\n");
        printf("2. List reservations\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 2);
        if (choice == 1) {
            addReservation();
            pauseEnter();
        } else if (choice == 2) {
            listReservations();
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addPlace(void)
{
    Place p;
    printHeader("Add route place");
    if (placeCount >= MAX_PLACES) {
        printf("Place storage is full.\n");
        return;
    }
    memset(&p, 0, sizeof(p));
    p.placeId = nextPlaceId();
    readLine("Region: ", p.region, sizeof(p.region));
    readLine("Place name: ", p.placeName, sizeof(p.placeName));
    readLine("Address: ", p.address, sizeof(p.address));
    readLine("Category: ", p.category, sizeof(p.category));
    p.priority = readInt("Priority (1 is highest): ", 1, 99);
    p.distanceFromStart = readInt("Distance from start: ", 0, 999999);
    p.recommendScore = readInt("Recommend score (0-100): ", 0, 100);
    places[placeCount++] = p;
    savePlaces();
    printf("Place saved with ID %d.\n", p.placeId);
}

static void listPlaces(void)
{
    int i;
    printHeader("Place data");
    if (placeCount == 0) {
        printf("No place data.\n");
        return;
    }
    for (i = 0; i < placeCount; i++) {
        Place* p = &places[i];
        printf("#%d [%s] %s | %s | priority %d | distance %d | score %d\n",
            p->placeId, p->region, p->placeName, p->category,
            p->priority, p->distanceFromStart, p->recommendScore);
    }
}

static int comparePlaceForRoute(const Place* a, const Place* b, int byPriority)
{
    if (byPriority) {
        if (a->priority != b->priority) {
            return a->priority - b->priority;
        }
        if (a->recommendScore != b->recommendScore) {
            return b->recommendScore - a->recommendScore;
        }
    }
    if (a->distanceFromStart != b->distanceFromStart) {
        return a->distanceFromStart - b->distanceFromStart;
    }
    return b->recommendScore - a->recommendScore;
}

static void recommendRoute(int byPriority)
{
    char region[MAX_LOCATION_LEN];
    int indices[MAX_PLACES];
    int count = 0;
    int i;
    int j;
    int totalDistance = 0;

    printHeader(byPriority ? "Route recommendation by priority" : "Route recommendation by distance");
    readLine("Region filter (blank for all): ", region, sizeof(region));
    for (i = 0; i < placeCount; i++) {
        if (region[0] == '\0' || equalsIgnoreCase(places[i].region, region)) {
            indices[count++] = i;
        }
    }

    for (i = 0; i < count - 1; i++) {
        for (j = 0; j < count - i - 1; j++) {
            if (comparePlaceForRoute(&places[indices[j]], &places[indices[j + 1]], byPriority) > 0) {
                int temp = indices[j];
                indices[j] = indices[j + 1];
                indices[j + 1] = temp;
            }
        }
    }

    if (count == 0) {
        printf("No matching place data.\n");
        return;
    }

    printf("Recommended visit order:\n");
    for (i = 0; i < count; i++) {
        Place* p = &places[indices[i]];
        totalDistance += p->distanceFromStart;
        printf("%d. %s (%s) - priority %d, distance %d, score %d\n",
            i + 1, p->placeName, p->category, p->priority,
            p->distanceFromStart, p->recommendScore);
    }
    printf("Estimated total distance score: %d\n", totalDistance);
}

static void routeMenu(void)
{
    while (1) {
        int choice;
        printHeader("Optimal route recommendation");
        printf("1. Add place data\n");
        printf("2. List place data\n");
        printf("3. Recommend by distance\n");
        printf("4. Recommend by priority\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 4);
        if (choice == 1) {
            addPlace();
            pauseEnter();
        } else if (choice == 2) {
            listPlaces();
            pauseEnter();
        } else if (choice == 3) {
            recommendRoute(0);
            pauseEnter();
        } else if (choice == 4) {
            recommendRoute(1);
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addExpense(ModeType mode)
{
    Expense e;
    printHeader(mode == MODE_TRAVEL ? "Add travel expense" : "Add daily expense");
    if (expenseCount >= MAX_EXPENSES) {
        printf("Expense storage is full.\n");
        return;
    }
    memset(&e, 0, sizeof(e));
    e.expenseId = nextExpenseId();
    setString(e.userId, sizeof(e.userId), currentUserId);
    e.mode = mode;
    readLine("Date (YYYY-MM-DD): ", e.date, sizeof(e.date));
    readLine("Category: ", e.category, sizeof(e.category));
    readLine("Payer: ", e.payer, sizeof(e.payer));
    e.participantCount = readInt("Participant count: ", 1, 999);
    e.amount = readInt("Amount: ", 0, 999999999);
    readLine("Memo: ", e.memo, sizeof(e.memo));
    expenses[expenseCount++] = e;
    saveExpenses();
    printf("Expense saved with ID %d.\n", e.expenseId);
}

static void listExpenses(ModeType mode)
{
    int i;
    int found = 0;
    int total = 0;
    double totalShare = 0.0;
    printHeader(mode == MODE_TRAVEL ? "Travel expense settlement" : "Daily spending");
    for (i = 0; i < expenseCount; i++) {
        Expense* e = &expenses[i];
        if (!isCurrentUser(e->userId) || e->mode != mode) {
            continue;
        }
        found = 1;
        total += e->amount;
        totalShare += (double)e->amount / (double)e->participantCount;
        printf("#%d %s | %s | payer %s | %d won / %d people | per person %.0f\n",
            e->expenseId, e->date, e->category, e->payer, e->amount,
            e->participantCount, (double)e->amount / (double)e->participantCount);
        if (e->memo[0] != '\0') {
            printf("   memo: %s\n", e->memo);
        }
    }
    if (!found) {
        printf("No expenses yet.\n");
        return;
    }
    printf("\nTotal spending: %d\n", total);
    printf("Sum of each item per-person shares: %.0f\n", totalShare);
}

static void expenseMenu(ModeType mode)
{
    while (1) {
        int choice;
        printHeader(mode == MODE_TRAVEL ? "Travel expense calculator" : "Daily expense calculator");
        printf("1. Add expense\n");
        printf("2. List and calculate settlement\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 2);
        if (choice == 1) {
            addExpense(mode);
            pauseEnter();
        } else if (choice == 2) {
            listExpenses(mode);
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addChecklistItem(ModeType mode, const char* defaultName, const char* defaultCategory)
{
    ChecklistItem item;
    if (checklistCount >= MAX_CHECKLIST) {
        printf("Checklist storage is full.\n");
        return;
    }
    memset(&item, 0, sizeof(item));
    item.itemId = nextChecklistId();
    setString(item.userId, sizeof(item.userId), currentUserId);
    item.mode = mode;
    if (defaultName != NULL) {
        setString(item.itemName, sizeof(item.itemName), defaultName);
        setString(item.category, sizeof(item.category), defaultCategory == NULL ? "General" : defaultCategory);
    } else {
        readLine("Item name: ", item.itemName, sizeof(item.itemName));
        readLine("Category: ", item.category, sizeof(item.category));
    }
    item.isChecked = STATUS_NOT_DONE;
    item.isShared = 0;
    checklist[checklistCount++] = item;
}

static int checklistItemExists(ModeType mode, const char* name)
{
    int i;
    for (i = 0; i < checklistCount; i++) {
        if (isCurrentUser(checklist[i].userId) &&
            checklist[i].mode == mode &&
            equalsIgnoreCase(checklist[i].itemName, name)) {
            return 1;
        }
    }
    return 0;
}

static void addRecommendedChecklist(ModeType mode)
{
    const char* travelItems[][2] = {
        { "Passport", "Document" },
        { "Wallet", "Money" },
        { "Charger", "Electronics" },
        { "Medicine", "Health" },
        { "Umbrella", "Weather" }
    };
    const char* dailyItems[][2] = {
        { "Notebook", "School" },
        { "Textbook", "School" },
        { "Student ID", "Document" },
        { "Charger", "Electronics" },
        { "Umbrella", "Weather" }
    };
    int count = mode == MODE_TRAVEL ? 5 : 5;
    int added = 0;
    int i;

    for (i = 0; i < count; i++) {
        const char* name = mode == MODE_TRAVEL ? travelItems[i][0] : dailyItems[i][0];
        const char* category = mode == MODE_TRAVEL ? travelItems[i][1] : dailyItems[i][1];
        if (!checklistItemExists(mode, name)) {
            addChecklistItem(mode, name, category);
            added++;
        }
    }
    saveChecklist();
    printf("%d recommended checklist items were added.\n", added);
}

static void listChecklist(ModeType mode)
{
    int i;
    int total = 0;
    int done = 0;
    printHeader(mode == MODE_TRAVEL ? "Travel checklist" : "Daily checklist");
    for (i = 0; i < checklistCount; i++) {
        ChecklistItem* c = &checklist[i];
        if (!isCurrentUser(c->userId) || c->mode != mode) {
            continue;
        }
        total++;
        if (c->isChecked == STATUS_DONE) {
            done++;
        }
        printf("#%d [%c] %s | %s | shared %s\n",
            c->itemId, c->isChecked == STATUS_DONE ? 'X' : ' ',
            c->itemName, c->category, c->isShared ? "yes" : "no");
    }
    if (total == 0) {
        printf("No checklist items yet.\n");
    } else {
        printf("Progress: %d/%d (%.1f%%)\n", done, total, total == 0 ? 0.0 : (double)done * 100.0 / (double)total);
    }
}

static void toggleChecklist(ModeType mode)
{
    int id;
    int i;
    listChecklist(mode);
    id = readInt("Checklist ID to toggle: ", 1, 999999);
    for (i = 0; i < checklistCount; i++) {
        if (checklist[i].itemId == id && isCurrentUser(checklist[i].userId) && checklist[i].mode == mode) {
            checklist[i].isChecked = checklist[i].isChecked == STATUS_DONE ? STATUS_NOT_DONE : STATUS_DONE;
            saveChecklist();
            printf("Checklist item #%d is now %s.\n", id, statusName(checklist[i].isChecked));
            return;
        }
    }
    printf("Checklist item not found.\n");
}

static void shareChecklistItem(ModeType mode)
{
    int id;
    int i;
    listChecklist(mode);
    id = readInt("Checklist ID to share: ", 1, 999999);
    for (i = 0; i < checklistCount; i++) {
        if (checklist[i].itemId == id && isCurrentUser(checklist[i].userId) && checklist[i].mode == mode) {
            checklist[i].isShared = 1;
            saveChecklist();
            printf("Checklist item #%d is marked as shared.\n", id);
            return;
        }
    }
    printf("Checklist item not found.\n");
}

static WeatherInfo* findWeather(const char* date, const char* region)
{
    int i;
    for (i = 0; i < weatherCount; i++) {
        if (strcmp(weatherList[i].date, date) == 0 &&
            equalsIgnoreCase(weatherList[i].region, region)) {
            return &weatherList[i];
        }
    }
    return NULL;
}

static void ensureUmbrellaItem(ModeType mode)
{
    if (!checklistItemExists(mode, "Umbrella")) {
        addChecklistItem(mode, "Umbrella", "Weather");
        saveChecklist();
        printf("Umbrella was added to your %s checklist.\n", modeName(mode));
    }
}

static void addWeatherRecord(void)
{
    WeatherInfo w;
    printHeader("Add weather data");
    if (weatherCount >= MAX_WEATHER) {
        printf("Weather storage is full.\n");
        return;
    }
    memset(&w, 0, sizeof(w));
    w.weatherId = nextWeatherId();
    readLine("Date (YYYY-MM-DD): ", w.date, sizeof(w.date));
    readLine("Region: ", w.region, sizeof(w.region));
    printf("1. Sunny  2. Cloudy  3. Rainy  4. Snowy\n");
    w.weather = (WeatherType)readInt("Weather: ", 1, 4);
    w.rainProbability = readInt("Rain probability (0-100): ", 0, 100);
    weatherList[weatherCount++] = w;
    saveWeather();
    printf("Weather record saved with ID %d.\n", w.weatherId);
}

static void listWeather(void)
{
    int i;
    printHeader("Weather data");
    if (weatherCount == 0) {
        printf("No weather data.\n");
        return;
    }
    for (i = 0; i < weatherCount; i++) {
        WeatherInfo* w = &weatherList[i];
        printf("#%d %s | %s | %s | rain %d%%\n",
            w->weatherId, w->date, w->region, weatherName(w->weather), w->rainProbability);
    }
}

static void checkWeatherAlert(ModeType mode)
{
    char date[MAX_DATE_LEN];
    char region[MAX_LOCATION_LEN];
    WeatherInfo* w;
    printHeader(mode == MODE_TRAVEL ? "Travel umbrella alert" : "Daily umbrella alert");
    readLine("Date (YYYY-MM-DD): ", date, sizeof(date));
    readLine("Region: ", region, sizeof(region));
    w = findWeather(date, region);
    if (w == NULL) {
        printf("No exact weather data. Add data first or use sample regions like Seoul/Jeju.\n");
        return;
    }
    printf("%s %s: %s, rain probability %d%%\n",
        w->date, w->region, weatherName(w->weather), w->rainProbability);
    if (w->weather == WEATHER_RAINY || w->rainProbability >= 50) {
        printf("Alert: rain is likely. Prepare an umbrella.\n");
        ensureUmbrellaItem(mode);
    } else {
        printf("No umbrella alert for this data.\n");
    }
}

static void weatherMenu(ModeType mode)
{
    while (1) {
        int choice;
        printHeader(mode == MODE_TRAVEL ? "Travel weather alert" : "Daily weather alert");
        printf("1. Add weather data\n");
        printf("2. Check umbrella alert\n");
        printf("3. List weather data\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 3);
        if (choice == 1) {
            addWeatherRecord();
            pauseEnter();
        } else if (choice == 2) {
            checkWeatherAlert(mode);
            pauseEnter();
        } else if (choice == 3) {
            listWeather();
            pauseEnter();
        } else {
            return;
        }
    }
}

static void checklistMenu(ModeType mode)
{
    while (1) {
        int choice;
        printHeader(mode == MODE_TRAVEL ? "Travel packing checklist" : "Daily preparation checklist");
        printf("1. Add checklist item\n");
        printf("2. Add recommended items\n");
        printf("3. List checklist and progress\n");
        printf("4. Check / uncheck item\n");
        printf("5. Mark item shared\n");
        printf("6. Weather-based umbrella check\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 6);
        if (choice == 1) {
            addChecklistItem(mode, NULL, NULL);
            saveChecklist();
            printf("Checklist item saved.\n");
            pauseEnter();
        } else if (choice == 2) {
            addRecommendedChecklist(mode);
            pauseEnter();
        } else if (choice == 3) {
            listChecklist(mode);
            pauseEnter();
        } else if (choice == 4) {
            toggleChecklist(mode);
            pauseEnter();
        } else if (choice == 5) {
            shareChecklistItem(mode);
            pauseEnter();
        } else if (choice == 6) {
            checkWeatherAlert(mode);
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addTodo(void)
{
    Todo t;
    printHeader("Add Todo");
    if (todoCount >= MAX_TODOS) {
        printf("Todo storage is full.\n");
        return;
    }
    memset(&t, 0, sizeof(t));
    t.todoId = nextTodoId();
    setString(t.userId, sizeof(t.userId), currentUserId);
    readLine("Todo title: ", t.title, sizeof(t.title));
    readLine("Due date (YYYY-MM-DD): ", t.dueDate, sizeof(t.dueDate));
    t.importance = readInt("Importance (1-5): ", 1, 5);
    t.isCompleted = STATUS_NOT_DONE;
    readLine("Memo: ", t.memo, sizeof(t.memo));
    todos[todoCount++] = t;
    saveTodos();
    printf("Todo saved with ID %d.\n", t.todoId);
}

static void listTodos(void)
{
    int i;
    int total = 0;
    int done = 0;
    printHeader("Todo list");
    for (i = 0; i < todoCount; i++) {
        Todo* t = &todos[i];
        if (!isCurrentUser(t->userId)) {
            continue;
        }
        total++;
        if (t->isCompleted == STATUS_DONE) {
            done++;
        }
        printf("#%d [%c] %s | due %s | importance %d\n",
            t->todoId, t->isCompleted == STATUS_DONE ? 'X' : ' ',
            t->title, t->dueDate, t->importance);
        if (t->memo[0] != '\0') {
            printf("   memo: %s\n", t->memo);
        }
    }
    if (total == 0) {
        printf("No Todo items yet.\n");
    } else {
        printf("Completion rate: %d/%d (%.1f%%)\n", done, total, (double)done * 100.0 / (double)total);
    }
}

static void toggleTodo(void)
{
    int id;
    int i;
    listTodos();
    id = readInt("Todo ID to toggle: ", 1, 999999);
    for (i = 0; i < todoCount; i++) {
        if (todos[i].todoId == id && isCurrentUser(todos[i].userId)) {
            todos[i].isCompleted = todos[i].isCompleted == STATUS_DONE ? STATUS_NOT_DONE : STATUS_DONE;
            saveTodos();
            printf("Todo #%d is now %s.\n", id, statusName(todos[i].isCompleted));
            return;
        }
    }
    printf("Todo not found.\n");
}

static void todoMenu(void)
{
    while (1) {
        int choice;
        printHeader("Todo management");
        printf("1. Add Todo\n");
        printf("2. List Todo and completion rate\n");
        printf("3. Complete / reopen Todo\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 3);
        if (choice == 1) {
            addTodo();
            pauseEnter();
        } else if (choice == 2) {
            listTodos();
            pauseEnter();
        } else if (choice == 3) {
            toggleTodo();
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addJob(void)
{
    PartTimeJob j;
    int startMinutes;
    int endMinutes;
    printHeader("Part-time pay calculation");
    if (jobCount >= MAX_JOBS) {
        printf("Part-time job storage is full.\n");
        return;
    }
    memset(&j, 0, sizeof(j));
    j.jobId = nextJobId();
    setString(j.userId, sizeof(j.userId), currentUserId);
    readLine("Job name: ", j.jobName, sizeof(j.jobName));
    readLine("Work date (YYYY-MM-DD): ", j.workDate, sizeof(j.workDate));
    readLine("Start time (HH:MM): ", j.startTime, sizeof(j.startTime));
    readLine("End time (HH:MM): ", j.endTime, sizeof(j.endTime));
    j.hourlyWage = readInt("Hourly wage: ", 0, 9999999);
    startMinutes = parseTimeToMinutes(j.startTime);
    endMinutes = parseTimeToMinutes(j.endTime);
    if (startMinutes >= 0 && endMinutes > startMinutes) {
        j.workHours = (endMinutes - startMinutes) / 60;
        if ((endMinutes - startMinutes) % 60 >= 30) {
            j.workHours++;
        }
        printf("Calculated work hours: %d\n", j.workHours);
        if (j.workHours <= 0) {
            j.workHours = readInt("Work hours: ", 1, 24);
        }
    } else {
        j.workHours = readInt("Work hours: ", 1, 24);
    }
    j.totalPay = j.workHours * j.hourlyWage;
    jobs[jobCount++] = j;
    saveJobs();
    printf("Job saved. Estimated pay: %d\n", j.totalPay);
}

static void listJobs(void)
{
    int i;
    int found = 0;
    int total = 0;
    printHeader("Part-time job records");
    for (i = 0; i < jobCount; i++) {
        PartTimeJob* j = &jobs[i];
        if (!isCurrentUser(j->userId)) {
            continue;
        }
        found = 1;
        total += j->totalPay;
        printf("#%d %s | %s %s-%s | %d hours x %d = %d\n",
            j->jobId, j->jobName, j->workDate, j->startTime, j->endTime,
            j->workHours, j->hourlyWage, j->totalPay);
    }
    if (!found) {
        printf("No part-time records yet.\n");
    } else {
        printf("Total estimated pay: %d\n", total);
    }
}

static void summarizeJobsByMonth(void)
{
    char month[16];
    int i;
    int total = 0;
    int count = 0;
    printHeader("Monthly pay summary");
    readLine("Month prefix (YYYY-MM): ", month, sizeof(month));
    for (i = 0; i < jobCount; i++) {
        PartTimeJob* j = &jobs[i];
        if (isCurrentUser(j->userId) && strncmp(j->workDate, month, strlen(month)) == 0) {
            total += j->totalPay;
            count++;
        }
    }
    printf("%s records: %d, estimated pay: %d\n", month, count, total);
}

static void jobMenu(void)
{
    while (1) {
        int choice;
        printHeader("Part-time pay");
        printf("1. Add work record and calculate pay\n");
        printf("2. List work records\n");
        printf("3. Monthly pay summary\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 3);
        if (choice == 1) {
            addJob();
            pauseEnter();
        } else if (choice == 2) {
            listJobs();
            pauseEnter();
        } else if (choice == 3) {
            summarizeJobsByMonth();
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addSleepAlarm(void)
{
    SleepAlarm s;
    int wakeMinutes;
    printHeader("Sleep and wake-up alarm");
    if (sleepAlarmCount >= MAX_USERS) {
        printf("Sleep alarm storage is full.\n");
        return;
    }
    memset(&s, 0, sizeof(s));
    s.sleepId = nextSleepId();
    setString(s.userId, sizeof(s.userId), currentUserId);
    readLine("Date (YYYY-MM-DD): ", s.date, sizeof(s.date));
    readLine("Wake-up time (HH:MM): ", s.wakeUpTime, sizeof(s.wakeUpTime));
    s.targetSleepHours = readInt("Target sleep hours: ", 1, 24);
    s.isAlarmOn = readInt("Alarm on? (1 yes, 0 no): ", 0, 1);
    wakeMinutes = parseTimeToMinutes(s.wakeUpTime);
    if (wakeMinutes >= 0) {
        minutesToTime(wakeMinutes - s.targetSleepHours * 60, s.recommendedBedTime, sizeof(s.recommendedBedTime));
    } else {
        setString(s.recommendedBedTime, sizeof(s.recommendedBedTime), "unknown");
    }
    sleepAlarms[sleepAlarmCount++] = s;
    saveSleepAlarms();
    printf("Recommended bedtime: %s\n", s.recommendedBedTime);
    if (s.targetSleepHours < 6) {
        printf("Warning: target sleep time is short.\n");
    }
}

static void listSleepAlarms(void)
{
    int i;
    int found = 0;
    printHeader("Sleep alarm list");
    for (i = 0; i < sleepAlarmCount; i++) {
        SleepAlarm* s = &sleepAlarms[i];
        if (!isCurrentUser(s->userId)) {
            continue;
        }
        found = 1;
        printf("#%d %s | wake %s | target %d hours | bedtime %s | alarm %s\n",
            s->sleepId, s->date, s->wakeUpTime, s->targetSleepHours,
            s->recommendedBedTime, s->isAlarmOn ? "on" : "off");
    }
    if (!found) {
        printf("No sleep alarms yet.\n");
    }
}

static void sleepMenu(void)
{
    while (1) {
        int choice;
        printHeader("Sleep / wake-up alarm");
        printf("1. Add sleep alarm\n");
        printf("2. List sleep alarms\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 2);
        if (choice == 1) {
            addSleepAlarm();
            pauseEnter();
        } else if (choice == 2) {
            listSleepAlarms();
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addFriend(void)
{
    char friendId[MAX_ID_LEN];
    int userIndex;
    int i;
    Friend fr;

    printHeader("Add friend");
    if (friendCount >= MAX_FRIENDS) {
        printf("Friend storage is full.\n");
        return;
    }
    readLine("Friend user ID: ", friendId, sizeof(friendId));
    if (strcmp(friendId, currentUserId) == 0) {
        printf("You cannot add yourself.\n");
        return;
    }
    userIndex = findUserIndex(friendId);
    if (userIndex < 0) {
        printf("No registered user with that ID.\n");
        return;
    }
    for (i = 0; i < friendCount; i++) {
        if (isCurrentUser(friends[i].userId) && strcmp(friends[i].friendUserId, friendId) == 0) {
            printf("Already added.\n");
            return;
        }
    }

    memset(&fr, 0, sizeof(fr));
    fr.friendId = nextFriendId();
    setString(fr.userId, sizeof(fr.userId), currentUserId);
    setString(fr.friendUserId, sizeof(fr.friendUserId), users[userIndex].userId);
    setString(fr.friendNickname, sizeof(fr.friendNickname), users[userIndex].nickname);
    fr.isScheduleShared = 0;
    friends[friendCount++] = fr;
    saveFriends();
    printf("Friend added: %s (%s)\n", fr.friendNickname, fr.friendUserId);
}

static void listFriends(void)
{
    int i;
    int found = 0;
    printHeader("Friend list");
    for (i = 0; i < friendCount; i++) {
        Friend* fr = &friends[i];
        if (!isCurrentUser(fr->userId)) {
            continue;
        }
        found = 1;
        printf("#%d %s (%s) | schedule shared %s\n",
            fr->friendId, fr->friendNickname, fr->friendUserId,
            fr->isScheduleShared ? "yes" : "no");
    }
    if (!found) {
        printf("No friends yet.\n");
    }
}

static Friend* selectFriend(void)
{
    int id;
    int i;
    listFriends();
    id = readInt("Friend relation ID: ", 1, 999999);
    for (i = 0; i < friendCount; i++) {
        if (friends[i].friendId == id && isCurrentUser(friends[i].userId)) {
            return &friends[i];
        }
    }
    printf("Friend relation not found.\n");
    return NULL;
}

static void shareSchedule(void)
{
    Friend* fr;
    int id;
    int i;
    printHeader("Schedule sharing");
    fr = selectFriend();
    if (fr == NULL) {
        return;
    }
    listSchedules(MODE_TRAVEL);
    listSchedules(MODE_DAILY);
    id = readInt("Schedule ID to share: ", 1, 999999);
    for (i = 0; i < scheduleCount; i++) {
        if (schedules[i].scheduleId == id && isCurrentUser(schedules[i].userId)) {
            fr->isScheduleShared = 1;
            saveFriends();
            printf("Schedule #%d is shared with %s. The Friend.isScheduleShared field was saved as 1.\n",
                id, fr->friendNickname);
            return;
        }
    }
    printf("Schedule not found.\n");
}

static void showSharedChecklistStatus(void)
{
    int i;
    int friendTotal = 0;
    printHeader("Shared checklist status");
    for (i = 0; i < friendCount; i++) {
        if (isCurrentUser(friends[i].userId)) {
            friendTotal++;
        }
    }
    if (friendTotal == 0) {
        printf("No friends. Add friends before sharing.\n");
    }

    for (i = 0; i < checklistCount; i++) {
        ChecklistItem* c = &checklist[i];
        if (isCurrentUser(c->userId) && c->isShared) {
            printf("#%d %s (%s) | owner status %s | visible to %d friends\n",
                c->itemId, c->itemName, modeName(c->mode), statusName(c->isChecked), friendTotal);
        }
    }
}

static void friendMenu(void)
{
    while (1) {
        int choice;
        printHeader("Friend and sharing");
        printf("1. Add friend\n");
        printf("2. List friends\n");
        printf("3. Share schedule\n");
        printf("4. Share travel checklist item\n");
        printf("5. Share daily checklist item\n");
        printf("6. Shared checklist status\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 6);
        if (choice == 1) {
            addFriend();
            pauseEnter();
        } else if (choice == 2) {
            listFriends();
            pauseEnter();
        } else if (choice == 3) {
            shareSchedule();
            pauseEnter();
        } else if (choice == 4) {
            shareChecklistItem(MODE_TRAVEL);
            pauseEnter();
        } else if (choice == 5) {
            shareChecklistItem(MODE_DAILY);
            pauseEnter();
        } else if (choice == 6) {
            showSharedChecklistStatus();
            pauseEnter();
        } else {
            return;
        }
    }
}

static void addReview(void)
{
    Review r;
    printHeader("Write review");
    if (reviewCount >= MAX_REVIEWS) {
        printf("Review storage is full.\n");
        return;
    }
    memset(&r, 0, sizeof(r));
    r.reviewId = nextReviewId();
    setString(r.userId, sizeof(r.userId), currentUserId);
    readLine("Place name: ", r.placeName, sizeof(r.placeName));
    r.rating = readInt("Rating (1-5): ", 1, 5);
    readLine("Review content: ", r.content, sizeof(r.content));
    readLine("Created date (YYYY-MM-DD, blank for today): ", r.createdDate, sizeof(r.createdDate));
    if (r.createdDate[0] == '\0') {
        todayString(r.createdDate, sizeof(r.createdDate));
    }
    reviews[reviewCount++] = r;
    saveReviews();
    printf("Review saved with ID %d.\n", r.reviewId);
}

static void searchReviews(void)
{
    char keyword[MAX_LOCATION_LEN];
    int i;
    int found = 0;
    int ratingSum = 0;
    printHeader("Review search");
    readLine("Place keyword: ", keyword, sizeof(keyword));
    for (i = 0; i < reviewCount; i++) {
        Review* r = &reviews[i];
        if (containsIgnoreCase(r->placeName, keyword)) {
            found++;
            ratingSum += r->rating;
            printf("#%d %s | rating %d | by %s | %s\n",
                r->reviewId, r->placeName, r->rating, r->userId, r->createdDate);
            printf("   %s\n", r->content);
        }
    }
    if (!found) {
        printf("No matching reviews.\n");
    } else {
        printf("Average rating: %.2f\n", (double)ratingSum / (double)found);
    }
}

static void latestReviews(void)
{
    int indices[MAX_REVIEWS];
    int count = 0;
    int i;
    int j;
    printHeader("Latest reviews");
    for (i = 0; i < reviewCount; i++) {
        indices[count++] = i;
    }
    for (i = 0; i < count - 1; i++) {
        for (j = 0; j < count - i - 1; j++) {
            Review* a = &reviews[indices[j]];
            Review* b = &reviews[indices[j + 1]];
            if (strcmp(a->createdDate, b->createdDate) < 0) {
                int temp = indices[j];
                indices[j] = indices[j + 1];
                indices[j + 1] = temp;
            }
        }
    }
    if (count == 0) {
        printf("No reviews yet.\n");
        return;
    }
    for (i = 0; i < count && i < 10; i++) {
        Review* r = &reviews[indices[i]];
        printf("#%d %s | rating %d | by %s | %s\n",
            r->reviewId, r->placeName, r->rating, r->userId, r->createdDate);
        printf("   %s\n", r->content);
    }
}

static void reviewMenu(void)
{
    while (1) {
        int choice;
        printHeader("Review management");
        printf("1. Search reviews and average rating\n");
        printf("2. Write review\n");
        printf("3. Latest reviews\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 3);
        if (choice == 1) {
            searchReviews();
            pauseEnter();
        } else if (choice == 2) {
            addReview();
            pauseEnter();
        } else if (choice == 3) {
            latestReviews();
            pauseEnter();
        } else {
            return;
        }
    }
}

static int countSchedulesOnDate(const char* date)
{
    int i;
    int count = 0;
    for (i = 0; i < scheduleCount; i++) {
        if (isCurrentUser(schedules[i].userId) && strcmp(schedules[i].date, date) == 0) {
            count++;
        }
    }
    return count;
}

static void printDashboard(void)
{
    char today[MAX_DATE_LEN];
    int i;
    int todoTotal = 0;
    int todoDone = 0;
    int checklistTotal = 0;
    int checklistDone = 0;
    int travelExpense = 0;

    todayString(today, sizeof(today));
    printHeader("Home dashboard");
    printf("User: %s (%s)\n", currentNickname, currentUserId);
    printf("Today: %s\n", today);
    printf("Schedules today: %d\n", countSchedulesOnDate(today));

    for (i = 0; i < todoCount; i++) {
        if (isCurrentUser(todos[i].userId)) {
            todoTotal++;
            if (todos[i].isCompleted == STATUS_DONE) {
                todoDone++;
            }
        }
    }
    printf("Todo progress: %d/%d", todoDone, todoTotal);
    if (todoTotal > 0) {
        printf(" (%.1f%%)", (double)todoDone * 100.0 / (double)todoTotal);
    }
    printf("\n");

    for (i = 0; i < checklistCount; i++) {
        if (isCurrentUser(checklist[i].userId)) {
            checklistTotal++;
            if (checklist[i].isChecked == STATUS_DONE) {
                checklistDone++;
            }
        }
    }
    printf("Checklist progress: %d/%d\n", checklistDone, checklistTotal);

    for (i = 0; i < expenseCount; i++) {
        if (isCurrentUser(expenses[i].userId) && expenses[i].mode == MODE_TRAVEL) {
            travelExpense += expenses[i].amount;
        }
    }
    printf("Travel expense total: %d\n", travelExpense);

    printf("\nToday's schedule preview:\n");
    for (i = 0; i < scheduleCount; i++) {
        if (isCurrentUser(schedules[i].userId) && strcmp(schedules[i].date, today) == 0) {
            printf("- [%s] %s-%s %s (%s)\n",
                modeName(schedules[i].mode), schedules[i].startTime,
                schedules[i].endTime, schedules[i].title, schedules[i].location);
        }
    }
}

static void copyFileBinary(const char* src, const char* dst)
{
    FILE* in = fopen(src, "rb");
    FILE* out;
    char buffer[4096];
    size_t n;
    if (in == NULL) {
        return;
    }
    out = fopen(dst, "wb");
    if (out == NULL) {
        fclose(in);
        return;
    }
    while ((n = fread(buffer, 1, sizeof(buffer), in)) > 0) {
        fwrite(buffer, 1, n, out);
    }
    fclose(in);
    fclose(out);
}

static void backupData(void)
{
    const char* files[] = {
        "users.txt", "schedules.txt", "todos.txt", "reservations.txt",
        "expenses.txt", "jobs.txt", "places.txt", "checklist.txt",
        "friends.txt", "reviews.txt", "weather.txt", "sleep.txt"
    };
    char stamp[32];
    char backupRoot[80];
    char backupDir[120];
    int i;

    saveAll();
    nowStamp(stamp, sizeof(stamp));
    (void)snprintf(backupRoot, sizeof(backupRoot), "%s", "backups");
    _mkdir(backupRoot);
    (void)snprintf(backupDir, sizeof(backupDir), "%s\\backup_%s", backupRoot, stamp);
    _mkdir(backupDir);

    for (i = 0; i < (int)(sizeof(files) / sizeof(files[0])); i++) {
        char src[160];
        char dst[200];
        buildDataPath(files[i], src, sizeof(src));
        (void)snprintf(dst, sizeof(dst), "%s\\%s", backupDir, files[i]);
        copyFileBinary(src, dst);
    }
    printf("Backup created: %s\n", backupDir);
}

static void settingsMenu(void)
{
    while (1) {
        int choice;
        printHeader("Settings and logout");
        printf("1. Show user information\n");
        printf("2. Save all data\n");
        printf("3. Backup data files\n");
        printf("4. Logout\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 4);
        if (choice == 1) {
            printf("User ID: %s\n", currentUserId);
            printf("Nickname: %s\n", currentNickname);
            pauseEnter();
        } else if (choice == 2) {
            saveAll();
            printf("All data saved in the data folder.\n");
            pauseEnter();
        } else if (choice == 3) {
            backupData();
            pauseEnter();
        } else if (choice == 4) {
            saveAll();
            currentUserId[0] = '\0';
            currentNickname[0] = '\0';
            printf("Logged out.\n");
            pauseEnter();
            return;
        } else {
            return;
        }
    }
}

static void travelMenu(void)
{
    while (1) {
        int choice;
        printHeader("Travel mode hub");
        printf("1. Travel schedule management\n");
        printf("2. Reservation auto registration\n");
        printf("3. Optimal route recommendation\n");
        printf("4. Travel expense calculation\n");
        printf("5. Packing checklist\n");
        printf("6. Travel weather / umbrella alert\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 6);
        if (choice == 1) {
            scheduleMenu(MODE_TRAVEL);
        } else if (choice == 2) {
            reservationMenu();
        } else if (choice == 3) {
            routeMenu();
        } else if (choice == 4) {
            expenseMenu(MODE_TRAVEL);
        } else if (choice == 5) {
            checklistMenu(MODE_TRAVEL);
        } else if (choice == 6) {
            weatherMenu(MODE_TRAVEL);
        } else {
            return;
        }
    }
}

static void dailyMenu(void)
{
    while (1) {
        int choice;
        printHeader("Daily mode hub");
        printf("1. Timetable schedule management\n");
        printf("2. Todo management\n");
        printf("3. Part-time pay calculation\n");
        printf("4. Sleep / wake-up alarm\n");
        printf("5. Daily preparation checklist\n");
        printf("6. Daily weather / umbrella alert\n");
        printf("7. Daily expense calculation\n");
        printf("0. Back\n");
        choice = readInt("Select: ", 0, 7);
        if (choice == 1) {
            scheduleMenu(MODE_DAILY);
        } else if (choice == 2) {
            todoMenu();
        } else if (choice == 3) {
            jobMenu();
        } else if (choice == 4) {
            sleepMenu();
        } else if (choice == 5) {
            checklistMenu(MODE_DAILY);
        } else if (choice == 6) {
            weatherMenu(MODE_DAILY);
        } else if (choice == 7) {
            expenseMenu(MODE_DAILY);
        } else {
            return;
        }
    }
}

static void mainMenu(void)
{
    while (currentUserId[0] != '\0') {
        int choice;
        printHeader("Main home");
        printf("Logged in: %s (%s)\n\n", currentNickname, currentUserId);
        printf("1. Summary dashboard\n");
        printf("2. Travel mode\n");
        printf("3. Daily mode\n");
        printf("4. Friend / sharing management\n");
        printf("5. Review search and writing\n");
        printf("6. Settings / logout\n");
        printf("0. Save and exit\n");
        choice = readInt("Select: ", 0, 6);
        if (choice == 1) {
            printDashboard();
            pauseEnter();
        } else if (choice == 2) {
            travelMenu();
        } else if (choice == 3) {
            dailyMenu();
        } else if (choice == 4) {
            friendMenu();
        } else if (choice == 5) {
            reviewMenu();
        } else if (choice == 6) {
            settingsMenu();
        } else {
            saveAll();
            printf("Saved. Goodbye.\n");
            exit(0);
        }
    }
}

int main(void)
{
    loadAll();
    showSplash();
    while (1) {
        authMenu();
        mainMenu();
    }
}
