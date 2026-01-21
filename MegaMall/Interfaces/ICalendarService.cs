namespace MegaMall.Interfaces
{
    public class HolidayEvent
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DaysUntil { get; set; }
    }

    public interface ICalendarService
    {
        /// <summary>
        /// Lấy danh sách các ngày lễ lớn sắp tới ở Việt Nam
        /// </summary>
        /// <param name="monthsAhead">Số tháng muốn xem trước (mặc định 3 tháng)</param>
        /// <returns>Danh sách các ngày lễ</returns>
        Task<List<HolidayEvent>> GetUpcomingVietnameseHolidaysAsync(int monthsAhead = 3);
    }
}
