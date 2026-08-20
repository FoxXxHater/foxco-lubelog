namespace CarCareTracker.Models
{
    public class ReminderRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public int Mileage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } = false;
        public bool UseCustomThresholds { get; set; } = false;
        public bool FixedIntervals { get; set; } = false;
        public ReminderUrgencyConfig CustomThresholds { get; set; } = new ReminderUrgencyConfig();
        public int CustomMileageInterval { get; set; } = 0;
        public int CustomMonthInterval { get; set; } = 0;
        public ReminderIntervalUnit CustomMonthIntervalUnit { get; set; } = ReminderIntervalUnit.Months;
        public ReminderMileageInterval ReminderMileageInterval { get; set; } = ReminderMileageInterval.FiveThousandMiles;
        public ReminderMonthInterval ReminderMonthInterval { get; set; } = ReminderMonthInterval.OneYear;
        public ReminderMetric Metric { get; set; } = ReminderMetric.Date;
        public List<string> Tags { get; set; } = new List<string>();
        /// <summary>
        /// Manually set urgency. When enabled it replaces the calculated urgency entirely,
        /// regardless of date or odometer.
        /// </summary>
        public bool UseUrgencyOverride { get; set; } = false;
        public ReminderUrgency UrgencyOverride { get; set; } = ReminderUrgency.NotUrgent;
        /// <summary>
        /// Non-recurring reminders are archived instead of pushed back when marked as done.
        /// </summary>
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
    }
}
