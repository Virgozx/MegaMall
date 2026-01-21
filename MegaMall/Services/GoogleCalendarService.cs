using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using MegaMall.Interfaces;

namespace MegaMall.Services
{
    public class GoogleCalendarService : ICalendarService
    {
        private readonly string _apiKey;
        private readonly ILogger<GoogleCalendarService> _logger;

        // ID của lịch ngày lễ Việt Nam trên Google Calendar
        private const string VIETNAM_HOLIDAY_CALENDAR_ID = "vi.vietnamese#holiday@group.v.calendar.google.com";

        public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
        {
            _apiKey = configuration["GoogleCalendar:ApiKey"] ?? string.Empty;
            _logger = logger;
        }

        public async Task<List<HolidayEvent>> GetUpcomingVietnameseHolidaysAsync(int monthsAhead = 3)
        {
            var holidays = new List<HolidayEvent>();

            try
            {
                // Kiểm tra API key
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("Google Calendar API key is not configured");
                    return GetFallbackHolidays(monthsAhead);
                }

                // Khởi tạo Calendar service
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    ApiKey = _apiKey,
                    ApplicationName = "MegaMall"
                });

                // Thiết lập thời gian truy vấn
                var timeMin = DateTime.Now;
                var timeMax = DateTime.Now.AddMonths(monthsAhead);

                // Tạo request để lấy events
                var request = service.Events.List(VIETNAM_HOLIDAY_CALENDAR_ID);
                request.TimeMinDateTimeOffset = timeMin;
                request.TimeMaxDateTimeOffset = timeMax;
                request.SingleEvents = true;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // Thực hiện request
                var events = await request.ExecuteAsync();

                // Chuyển đổi sang HolidayEvent
                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach (var eventItem in events.Items)
                    {
                        DateTime eventDate;
                        
                        // Parse ngày từ event (có thể là Date hoặc DateTime)
                        if (!string.IsNullOrEmpty(eventItem.Start.Date))
                        {
                            eventDate = DateTime.Parse(eventItem.Start.Date);
                        }
                        else if (eventItem.Start.DateTimeDateTimeOffset.HasValue)
                        {
                            eventDate = eventItem.Start.DateTimeDateTimeOffset.Value.DateTime;
                        }
                        else
                        {
                            continue;
                        }

                        var daysUntil = (eventDate - DateTime.Now).Days;

                        holidays.Add(new HolidayEvent
                        {
                            Name = eventItem.Summary ?? "Ngày lễ",
                            Date = eventDate,
                            Description = eventItem.Description ?? "",
                            DaysUntil = daysUntil
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("No holidays found from Google Calendar API, using fallback");
                    return GetFallbackHolidays(monthsAhead);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching holidays from Google Calendar API");
                return GetFallbackHolidays(monthsAhead);
            }

            return holidays;
        }

        /// <summary>
        /// Danh sách ngày lễ dự phòng khi không kết nối được API
        /// </summary>
        private List<HolidayEvent> GetFallbackHolidays(int monthsAhead)
        {
            var currentYear = DateTime.Now.Year;
            var nextYear = currentYear + 1;
            var timeMax = DateTime.Now.AddMonths(monthsAhead);

            var allHolidays = new List<HolidayEvent>
            {
                // Các ngày lễ cố định hàng năm
                new HolidayEvent { Name = "Tết Dương Lịch", Date = new DateTime(currentYear, 1, 1), Description = "Năm mới dương lịch" },
                new HolidayEvent { Name = "Valentine", Date = new DateTime(currentYear, 2, 14), Description = "Ngày lễ tình nhân" },
                new HolidayEvent { Name = "Quốc tế Phụ nữ 8/3", Date = new DateTime(currentYear, 3, 8), Description = "Ngày Quốc tế Phụ nữ" },
                new HolidayEvent { Name = "Giỗ Tổ Hùng Vương", Date = new DateTime(currentYear, 4, 18), Description = "10/3 Âm lịch" },
                new HolidayEvent { Name = "Ngày Giải phóng miền Nam 30/4", Date = new DateTime(currentYear, 4, 30), Description = "Ngày thống nhất đất nước" },
                new HolidayEvent { Name = "Quốc tế Lao động 1/5", Date = new DateTime(currentYear, 5, 1), Description = "Ngày Quốc tế Lao động" },
                new HolidayEvent { Name = "Ngày của Mẹ", Date = GetSecondSundayOfMay(currentYear), Description = "Mother's Day" },
                new HolidayEvent { Name = "Ngày Quốc tế Thiếu nhi 1/6", Date = new DateTime(currentYear, 6, 1), Description = "Ngày Thiếu nhi" },
                new HolidayEvent { Name = "Ngày của Cha", Date = GetThirdSundayOfJune(currentYear), Description = "Father's Day" },
                new HolidayEvent { Name = "Quốc Khánh 2/9", Date = new DateTime(currentYear, 9, 2), Description = "Ngày Quốc khánh Việt Nam" },
                new HolidayEvent { Name = "Trung Thu", Date = new DateTime(currentYear, 9, 17), Description = "15/8 Âm lịch" },
                new HolidayEvent { Name = "Ngày Phụ nữ Việt Nam 20/10", Date = new DateTime(currentYear, 10, 20), Description = "Ngày Phụ nữ Việt Nam" },
                new HolidayEvent { Name = "Black Friday", Date = GetBlackFriday(currentYear), Description = "Ngày mua sắm lớn nhất năm" },
                new HolidayEvent { Name = "Giáng Sinh", Date = new DateTime(currentYear, 12, 24), Description = "Lễ Giáng Sinh" },
                new HolidayEvent { Name = "Tết Nguyên Đán 2025", Date = new DateTime(currentYear, 1, 29), Description = "Tết Âm lịch 2025" },
                
                // Năm sau
                new HolidayEvent { Name = "Tết Dương Lịch", Date = new DateTime(nextYear, 1, 1), Description = "Năm mới dương lịch" },
                new HolidayEvent { Name = "Valentine", Date = new DateTime(nextYear, 2, 14), Description = "Ngày lễ tình nhân" },
                new HolidayEvent { Name = "Quốc tế Phụ nữ 8/3", Date = new DateTime(nextYear, 3, 8), Description = "Ngày Quốc tế Phụ nữ" },
                new HolidayEvent { Name = "Tết Nguyên Đán 2026", Date = new DateTime(nextYear, 2, 17), Description = "Tết Âm lịch 2026" }
            };

            // Lọc các ngày lễ trong khoảng thời gian
            var upcomingHolidays = allHolidays
                .Where(h => h.Date >= DateTime.Now && h.Date <= timeMax)
                .OrderBy(h => h.Date)
                .ToList();

            // Tính số ngày còn lại
            foreach (var holiday in upcomingHolidays)
            {
                holiday.DaysUntil = (holiday.Date - DateTime.Now).Days;
            }

            return upcomingHolidays;
        }

        private DateTime GetSecondSundayOfMay(int year)
        {
            var firstDayOfMay = new DateTime(year, 5, 1);
            var firstSunday = firstDayOfMay;
            
            while (firstSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                firstSunday = firstSunday.AddDays(1);
            }
            
            return firstSunday.AddDays(7); // Second Sunday
        }

        private DateTime GetThirdSundayOfJune(int year)
        {
            var firstDayOfJune = new DateTime(year, 6, 1);
            var firstSunday = firstDayOfJune;
            
            while (firstSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                firstSunday = firstSunday.AddDays(1);
            }
            
            return firstSunday.AddDays(14); // Third Sunday
        }

        private DateTime GetBlackFriday(int year)
        {
            // Black Friday is the Friday after Thanksgiving (4th Thursday of November)
            var firstDayOfNovember = new DateTime(year, 11, 1);
            var firstThursday = firstDayOfNovember;
            
            while (firstThursday.DayOfWeek != DayOfWeek.Thursday)
            {
                firstThursday = firstThursday.AddDays(1);
            }
            
            var fourthThursday = firstThursday.AddDays(21); // 4th Thursday
            return fourthThursday.AddDays(1); // Friday after
        }
    }
}
