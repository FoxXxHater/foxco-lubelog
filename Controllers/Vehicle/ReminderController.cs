using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        private List<ReminderRecordViewModel> GetRemindersAndUrgency(int vehicleId, DateTime dateCompare)
        {
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            List<ReminderRecordViewModel> results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, dateCompare);
            return results;
        }
        private bool GetAndUpdateVehicleUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result.RemoveAll(x => x.IsCompleted);
            //check if user wants auto-refresh past-due reminders
            if (_config.GetUserConfig(User).EnableAutoReminderRefresh && _userLogic.UserCanEditVehicle(GetUserID(), vehicleId, HouseholdPermission.Edit))
            {
                //check for past due reminders that are eligible for recurring.
                var pastDueAndRecurring = result.Where(x => x.Urgency == ReminderUrgency.PastDue && x.IsRecurring);
                if (pastDueAndRecurring.Any())
                {
                    foreach (ReminderRecordViewModel reminderRecord in pastDueAndRecurring)
                    {
                        //update based on recurring intervals.
                        //pull reminderRecord based on ID
                        var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecord.Id);
                        existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder, null, null);
                        //save to db.
                        _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                        //set urgency to not urgent so it gets excluded in count.
                        reminderRecord.Urgency = ReminderUrgency.NotUrgent;
                    }
                }
            }
            //check for very urgent or past due reminders that were not eligible for recurring.
            var pastDueAndUrgentReminders = result.Where(x => x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue);
            if (pastDueAndUrgentReminders.Any())
            {
                return true;
            }
            return false;
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetVehicleHaveUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetAndUpdateVehicleUrgentOrPastDueReminders(vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result = result.OrderBy(x => x.IsCompleted).ThenByDescending(x => x.Urgency).ToList();
            return PartialView("Reminder/_ReminderRecords", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetRecurringReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result.RemoveAll(x => !x.IsRecurring || x.IsCompleted);
            result = result.OrderByDescending(x => x.Urgency).ThenBy(x => x.Description).ToList();
            return PartialView("_RecurringReminderSelector", result);
        }
        [HttpPost]
        public IActionResult PushbackRecurringReminderRecord(int reminderRecordId)
        {
            var result = PushbackRecurringReminderRecordWithChecks(reminderRecordId, null, null);
            return Json(result);
        }
        private OperationResponse PushbackRecurringReminderRecordWithChecks(int reminderRecordId, DateTime? currentDate, int? currentMileage)
        {
            try
            {
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
                if (existingReminder is not null && existingReminder.Id != default && existingReminder.IsRecurring)
                {
                    //security check
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingReminder.VehicleId, HouseholdPermission.Edit))
                    {
                        return OperationResponse.Failed("Access Denied");
                    }
                    existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder, currentDate, currentMileage);
                    //save to db.
                    var reminderUpdateResult = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                    if (!reminderUpdateResult)
                    {
                        _logger.LogError("Unable to update reminder either because the reminder no longer exists or is no longer recurring");
                        return OperationResponse.Failed("Unable to update reminder either because the reminder no longer exists or is no longer recurring");
                    }
                    return OperationResponse.Succeed();
                }
                else
                {
                    _logger.LogError("Unable to update reminder because it no longer exists.");
                    return OperationResponse.Failed("Unable to update reminder because it no longer exists.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return OperationResponse.Failed(StaticHelper.GenericErrorMessage);
            }
        }
        /// <summary>
        /// Recurring reminders move on to their next interval, everything else gets archived.
        /// Either way the urgency stops counting.
        /// </summary>
        [HttpPost]
        public IActionResult MarkReminderRecordAsDone(int reminderRecordId)
        {
            try
            {
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
                if (existingReminder is null || existingReminder.Id == default)
                {
                    _logger.LogError("Unable to update reminder because it no longer exists.");
                    return Json(OperationResponse.Failed("Unable to update reminder because it no longer exists."));
                }
                //security check
                if (!_userLogic.UserCanEditVehicle(GetUserID(), existingReminder.VehicleId, HouseholdPermission.Edit))
                {
                    return Json(OperationResponse.Failed("Access Denied"));
                }
                if (existingReminder.IsRecurring)
                {
                    return Json(PushbackRecurringReminderRecordWithChecks(reminderRecordId, null, null));
                }
                existingReminder.IsCompleted = true;
                existingReminder.CompletedDate = DateTime.Now;
                var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                if (result)
                {
                    _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(existingReminder, "reminderrecord.update", User.Identity?.Name ?? string.Empty));
                }
                return Json(OperationResponse.Conditional(result, "Reminder Completed", StaticHelper.GenericErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(OperationResponse.Failed(StaticHelper.GenericErrorMessage));
            }
        }
        [HttpPost]
        public IActionResult ReopenReminderRecord(int reminderRecordId)
        {
            try
            {
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
                if (existingReminder is null || existingReminder.Id == default)
                {
                    _logger.LogError("Unable to update reminder because it no longer exists.");
                    return Json(OperationResponse.Failed("Unable to update reminder because it no longer exists."));
                }
                //security check
                if (!_userLogic.UserCanEditVehicle(GetUserID(), existingReminder.VehicleId, HouseholdPermission.Edit))
                {
                    return Json(OperationResponse.Failed("Access Denied"));
                }
                existingReminder.IsCompleted = false;
                existingReminder.CompletedDate = null;
                var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                if (result)
                {
                    _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(existingReminder, "reminderrecord.update", User.Identity?.Name ?? string.Empty));
                }
                return Json(OperationResponse.Conditional(result, "Reminder Reopened", StaticHelper.GenericErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(OperationResponse.Failed(StaticHelper.GenericErrorMessage));
            }
        }
        [HttpPost]
        public IActionResult SaveReminderRecordToVehicleId(ReminderRecordInput reminderRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), reminderRecord.VehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            var reminderToSave = reminderRecord.ToReminderRecord();
            if (reminderToSave.Id != default)
            {
                //completion state is only changed through MarkReminderRecordAsDone/ReopenReminderRecord.
                var storedReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderToSave.Id);
                if (storedReminder is not null && storedReminder.Id != default)
                {
                    reminderToSave.IsCompleted = storedReminder.IsCompleted;
                    reminderToSave.CompletedDate = storedReminder.CompletedDate;
                }
            }
            var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(reminderToSave);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(reminderToSave, reminderRecord.Id == default ? "reminderrecord.add" : "reminderrecord.update", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [HttpPost]
        public IActionResult GetAddReminderRecordPartialView(ReminderRecordInputViewModel? reminderModel)
        {
            if (reminderModel is null)
            {
                reminderModel = new ReminderRecordInputViewModel();
            }
            if (reminderModel.VehicleId != default)
            {
                var vehicleData = _dataAccess.GetVehicleById(reminderModel.VehicleId);
                reminderModel.UseHours = vehicleData.UseHours;
            }
            return PartialView("Reminder/_ReminderRecordModal", reminderModel);
        }
        [HttpGet]
        public IActionResult GetReminderRecordForEditById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId, HouseholdPermission.View))
            {
                return Forbid();
            }
            var vehicleData = _dataAccess.GetVehicleById(result.VehicleId);
            var vehicleUseHours = vehicleData.UseHours;
            //convert to Input object.
            var convertedResult = new ReminderRecordInputViewModel
            {
                Id = result.Id,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Mileage = result.Mileage,
                Metric = result.Metric,
                IsRecurring = result.IsRecurring,
                FixedIntervals = result.FixedIntervals,
                UseCustomThresholds = result.UseCustomThresholds,
                CustomThresholds = result.CustomThresholds,
                ReminderMileageInterval = result.ReminderMileageInterval,
                ReminderMonthInterval = result.ReminderMonthInterval,
                CustomMileageInterval = result.CustomMileageInterval,
                CustomMonthInterval = result.CustomMonthInterval,
                CustomMonthIntervalUnit = result.CustomMonthIntervalUnit,
                Tags = result.Tags,
                UseHours = vehicleUseHours,
                UseUrgencyOverride = result.UseUrgencyOverride,
                UrgencyOverride = result.UrgencyOverride,
                IsCompleted = result.IsCompleted,
                CompletedDate = result.CompletedDate
            };
            return PartialView("Reminder/_ReminderRecordModal", convertedResult);
        }
        private OperationResponse DeleteReminderRecordWithChecks(int reminderRecordId)
        {
            var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                return OperationResponse.Failed("Access Denied");
            }
            var result = _reminderRecordDataAccess.DeleteReminderRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(existingRecord, "reminderrecord.delete", User.Identity?.Name ?? string.Empty));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteReminderRecordById(int reminderRecordId)
        {
            var result = DeleteReminderRecordWithChecks(reminderRecordId);
            return Json(result);
        }
    }
}
