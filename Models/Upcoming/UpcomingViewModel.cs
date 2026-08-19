namespace CarCareTracker.Models
{
    /// <summary>
    /// Aggregates every reminder and every open plan across all vehicles the user has access to.
    /// </summary>
    public class UpcomingViewModel
    {
        public List<KioskReminderViewModel> Reminders { get; set; } = new List<KioskReminderViewModel>();
        public List<KioskPlanViewModel> Plans { get; set; } = new List<KioskPlanViewModel>();
    }
}
