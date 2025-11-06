using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;
using Farmacia.UI.Wpf.Models;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class AppointmentsViewModel : ViewModelBase
{
    private readonly PharmacyDbContext _context;

    public ObservableCollection<AppointmentItemModel> Appointments { get; } = new();
    public ObservableCollection<LookupItemModel> PatientOptions { get; } = new();
    public ObservableCollection<LookupItemModel> DoctorOptions { get; } = new();

    public IReadOnlyList<int> AppointmentDurationOptions { get; } = new[] { 15, 30, 45, 60 };
    public IReadOnlyList<TimeSpan> AvailableTimeSlots { get; } = GenerateTimeSlots();

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyPending = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _totalAppointments;

    [ObservableProperty]
    private int _pendingAppointments;

    [ObservableProperty]
    private int _completedAppointments;

    [ObservableProperty]
    private int _cancelledAppointments;

    [ObservableProperty]
    private int _noShowAppointments;

    [ObservableProperty]
    private AppointmentItemModel? _selectedAppointment;

    [ObservableProperty]
    private LookupItemModel? _selectedPatientOption;

    [ObservableProperty]
    private LookupItemModel? _selectedDoctorOption;

    [ObservableProperty]
    private DateOnly _newAppointmentDate = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private TimeSpan _newAppointmentTime = new(9, 0, 0);

    [ObservableProperty]
    private int _newAppointmentDurationMinutes = 30;

    [ObservableProperty]
    private string _newAppointmentReason = string.Empty;

    [ObservableProperty]
    private string? _newAppointmentNotes;

    public AppointmentsViewModel(PharmacyDbContext context)
    {
        _context = context;
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        if (LoadCommand.CanExecute(null))
        {
            _ = LoadCommand.ExecuteAsync(null);
        }
    }

    partial void OnSelectedPatientOptionChanged(LookupItemModel? value) => ScheduleAppointmentCommand.NotifyCanExecuteChanged();

    partial void OnSelectedDoctorOptionChanged(LookupItemModel? value) => ScheduleAppointmentCommand.NotifyCanExecuteChanged();

    partial void OnNewAppointmentDateChanged(DateOnly value) => ScheduleAppointmentCommand.NotifyCanExecuteChanged();

    partial void OnNewAppointmentTimeChanged(TimeSpan value) => ScheduleAppointmentCommand.NotifyCanExecuteChanged();

    partial void OnNewAppointmentDurationMinutesChanged(int value) => ScheduleAppointmentCommand.NotifyCanExecuteChanged();

    partial void OnNewAppointmentReasonChanged(string value) => ScheduleAppointmentCommand.NotifyCanExecuteChanged();

    partial void OnShowOnlyPendingChanged(bool value)
    {
        if (LoadCommand.CanExecute(null))
        {
            _ = LoadCommand.ExecuteAsync(null);
        }
    }

    partial void OnIsBusyChanged(bool value)
    {
        ScheduleAppointmentCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = null;
            await LoadReferenceDataAsync();
            await PopulateAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task MarkCompletedAsync(AppointmentItemModel? appointment)
    {
        await UpdateStatusAsync(appointment, AppointmentStatus.Completada);
    }

    [RelayCommand]
    private async Task MarkCancelledAsync(AppointmentItemModel? appointment)
    {
        await UpdateStatusAsync(appointment, AppointmentStatus.Cancelada);
    }

    [RelayCommand]
    private async Task MarkNoShowAsync(AppointmentItemModel? appointment)
    {
        await UpdateStatusAsync(appointment, AppointmentStatus.NoAsistio);
    }

    [RelayCommand(CanExecute = nameof(CanScheduleAppointment))]
    private async Task ScheduleAppointmentAsync()
    {
        if (SelectedPatientOption is null)
        {
            StatusMessage = "Selecciona un paciente.";
            return;
        }

        if (SelectedDoctorOption is null)
        {
            StatusMessage = "Selecciona un doctor.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            var appointmentDate = NewAppointmentDate;
            var appointmentStart = appointmentDate.ToDateTime(TimeOnly.FromTimeSpan(NewAppointmentTime));
            var appointmentEnd = appointmentStart.AddMinutes(NewAppointmentDurationMinutes);

            if (appointmentEnd <= appointmentStart)
            {
                StatusMessage = "La duración de la cita debe ser mayor a cero.";
                return;
            }

            if (appointmentStart < DateTime.Now.AddMinutes(-5))
            {
                StatusMessage = "No puedes agendar citas en el pasado.";
                return;
            }

            var sameDayAppointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.DoctorId == SelectedDoctorOption.Id)
                .Where(a => a.ScheduledAt.Date == appointmentStart.Date)
                .Where(a => a.Status != AppointmentStatus.Cancelada)
                .Select(a => new { a.ScheduledAt, a.Duration, a.Patient.Name })
                .ToListAsync();

            var hasOverlap = sameDayAppointments.Any(a =>
            {
                var existingStart = a.ScheduledAt;
                var existingEnd = existingStart + a.Duration;
                return existingStart < appointmentEnd && existingEnd > appointmentStart;
            });

            if (hasOverlap)
            {
                StatusMessage = "El doctor ya tiene una cita en ese horario.";
                return;
            }

            var appointment = new Appointment
            {
                PatientId = SelectedPatientOption.Id,
                DoctorId = SelectedDoctorOption.Id,
                ScheduledAt = appointmentStart,
                Duration = TimeSpan.FromMinutes(NewAppointmentDurationMinutes),
                Status = AppointmentStatus.Programada,
                Reason = string.IsNullOrWhiteSpace(NewAppointmentReason) ? null : NewAppointmentReason.Trim(),
                Notes = string.IsNullOrWhiteSpace(NewAppointmentNotes) ? null : NewAppointmentNotes.Trim()
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            SelectedDate = DateOnly.FromDateTime(appointmentStart);
            await PopulateAsync();

            SelectedAppointment = Appointments.FirstOrDefault(a => a.AppointmentId == appointment.Id)
                                   ?? Appointments.FirstOrDefault();

            ResetAppointmentDraftInternal(false);

            StatusMessage = "Cita agendada correctamente.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanScheduleAppointment()
    {
        return !IsBusy
               && SelectedPatientOption is not null
               && SelectedDoctorOption is not null
               && NewAppointmentDurationMinutes > 0
               && NewAppointmentReason.Trim().Length >= 3;
    }

    [RelayCommand]
    private void ResetAppointmentDraft()
    {
        ResetAppointmentDraftInternal(true);
    }

    private async Task PopulateAsync()
    {
        StatusMessage = null;
        SelectedAppointment = null;
        Appointments.Clear();

        var start = SelectedDate.ToDateTime(TimeOnly.MinValue);
        var end = SelectedDate.ToDateTime(TimeOnly.MaxValue);

        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.ScheduledAt >= start && a.ScheduledAt <= end);

        if (ShowOnlyPending)
        {
            query = query.Where(a => a.Status == AppointmentStatus.Programada || a.Status == AppointmentStatus.Confirmada || a.Status == AppointmentStatus.EnProceso);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(a => EF.Functions.Like(a.Patient.Name, $"%{term}%") || EF.Functions.Like(a.Doctor.FullName, $"%{term}%"));
        }

        var items = await query
            .OrderBy(a => a.ScheduledAt)
            .Take(200)
            .ToListAsync();
        foreach (var appointment in items)
        {
            Appointments.Add(new AppointmentItemModel
            {
                AppointmentId = appointment.Id,
                ScheduledAt = appointment.ScheduledAt,
                Duration = appointment.Duration,
                PatientName = appointment.Patient.Name,
                DoctorName = appointment.Doctor.FullName,
                Status = appointment.Status,
                Reason = appointment.Reason,
                Notes = appointment.Notes
            });
        }

        UpdateMetrics();

        if (SelectedAppointment is null)
        {
            SelectedAppointment = Appointments.FirstOrDefault();
        }

        if (!Appointments.Any())
        {
            StatusMessage = "No hay citas para la fecha seleccionada.";
        }
    }

    private async Task LoadReferenceDataAsync()
    {
        var previousPatientId = SelectedPatientOption?.Id;
        var previousDoctorId = SelectedDoctorOption?.Id;

        var patients = await _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new LookupItemModel
            {
                Id = p.Id,
                Name = p.Name
            })
            .Take(500)
            .ToListAsync();

        var doctors = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Medico)
            .OrderBy(u => u.FullName)
            .Select(u => new LookupItemModel
            {
                Id = u.Id,
                Name = u.FullName
            })
            .ToListAsync();

        PatientOptions.Clear();
        foreach (var patient in patients)
        {
            PatientOptions.Add(patient);
        }

        DoctorOptions.Clear();
        foreach (var doctor in doctors)
        {
            DoctorOptions.Add(doctor);
        }

        SelectedPatientOption = PatientOptions.FirstOrDefault(p => p.Id == previousPatientId) ?? PatientOptions.FirstOrDefault();
        SelectedDoctorOption = DoctorOptions.FirstOrDefault(d => d.Id == previousDoctorId) ?? DoctorOptions.FirstOrDefault();
    }

    private async Task UpdateStatusAsync(AppointmentItemModel? appointment, AppointmentStatus newStatus)
    {
        if (appointment is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var entity = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointment.AppointmentId);
            if (entity is null)
            {
                StatusMessage = "No se encontró la cita.";
                return;
            }

            entity.Status = newStatus;
            await _context.SaveChangesAsync();

            appointment.Status = newStatus;
            await PopulateAsync();
            SelectedAppointment = Appointments.FirstOrDefault(a => a.AppointmentId == appointment.AppointmentId)
                                   ?? Appointments.FirstOrDefault();
            StatusMessage = "Estado actualizado.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateMetrics()
    {
        TotalAppointments = Appointments.Count;
        PendingAppointments = Appointments.Count(a => a.Status is AppointmentStatus.Programada or AppointmentStatus.Confirmada or AppointmentStatus.EnProceso);
        CompletedAppointments = Appointments.Count(a => a.Status == AppointmentStatus.Completada);
        CancelledAppointments = Appointments.Count(a => a.Status == AppointmentStatus.Cancelada);
        NoShowAppointments = Appointments.Count(a => a.Status == AppointmentStatus.NoAsistio);
    }

    private static IReadOnlyList<TimeSpan> GenerateTimeSlots()
    {
        var slots = new List<TimeSpan>();
        for (var hour = 7; hour <= 20; hour++)
        {
            slots.Add(new TimeSpan(hour, 0, 0));
            slots.Add(new TimeSpan(hour, 30, 0));
        }

        return slots;
    }

    private void ResetAppointmentDraftInternal(bool resetSelections)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (NewAppointmentDate != today)
        {
            NewAppointmentDate = today;
        }

        var defaultSlot = new TimeSpan(9, 0, 0);
        if (AvailableTimeSlots.Count > 0)
        {
            var preferred = AvailableTimeSlots.FirstOrDefault(t => t == defaultSlot);
            defaultSlot = preferred != default ? preferred : AvailableTimeSlots[0];
        }

        NewAppointmentTime = defaultSlot;
        NewAppointmentDurationMinutes = AppointmentDurationOptions.Contains(30) ? 30 : AppointmentDurationOptions.FirstOrDefault();
        NewAppointmentReason = string.Empty;
        NewAppointmentNotes = null;

        if (resetSelections)
        {
            SelectedPatientOption = PatientOptions.FirstOrDefault();
            SelectedDoctorOption = DoctorOptions.FirstOrDefault();
        }

        ScheduleAppointmentCommand.NotifyCanExecuteChanged();
    }
}
