using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;
using Farmacia.Domain.Validators;
using Farmacia.UI.Wpf.Models;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class PatientsViewModel : ViewModelBase
{
    private readonly PharmacyDbContext _context;
    private readonly PatientValidator _patientValidator = new();

    public ObservableCollection<PatientListItemModel> Patients { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _totalPatients;

    [ObservableProperty]
    private int _recentConsultations;

    [ObservableProperty]
    private int _upcomingBirthdays;

    [ObservableProperty]
    private int _withoutContactInfo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    private PatientListItemModel? _selectedPatient;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    private bool _detailActivated;

    [ObservableProperty]
    private string _newPatientName = string.Empty;

    [ObservableProperty]
    private string? _newPatientPhone;

    [ObservableProperty]
    private DateOnly? _newPatientBirthDate;

    [ObservableProperty]
    private string? _newPatientGeneralData;

    [ObservableProperty]
    private string? _newPatientAllergies;

    [ObservableProperty]
    private string? _newPatientChronicConditions;

    [ObservableProperty]
    private string? _newPatientNotes;

    public PatientsViewModel(PharmacyDbContext context)
    {
        _context = context;
    }

    public bool IsDetailVisible => DetailActivated && SelectedPatient is not null;

    partial void OnIsBusyChanged(bool value)
    {
        RegisterPatientCommand.NotifyCanExecuteChanged();
    DeletePatientCommand.NotifyCanExecuteChanged();
    }

    partial void OnNewPatientNameChanged(string value)
    {
        RegisterPatientCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedPatientChanged(PatientListItemModel? value)
    {
    DeletePatientCommand.NotifyCanExecuteChanged();

        if (value is not null)
        {
            if (!DetailActivated)
            {
                DetailActivated = true;
            }
        }
        else if (!Patients.Any())
        {
            DetailActivated = false;
        }
    }

    partial void OnDetailActivatedChanged(bool value)
    {
        if (!value)
        {
            SelectedPatient = null;
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await PopulateAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (!DetailActivated)
        {
            DetailActivated = true;
        }
        await PopulateAsync(SearchText);
    }

    [RelayCommand(CanExecute = nameof(CanRegisterPatient))]
    private async Task RegisterPatientAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = null;

            var patient = new Patient
            {
                Name = NewPatientName.Trim(),
                Phone = string.IsNullOrWhiteSpace(NewPatientPhone) ? null : NewPatientPhone.Trim(),
                BirthDate = NewPatientBirthDate,
                GeneralData = string.IsNullOrWhiteSpace(NewPatientGeneralData) ? null : NewPatientGeneralData.Trim(),
                Allergies = string.IsNullOrWhiteSpace(NewPatientAllergies) ? null : NewPatientAllergies.Trim(),
                ChronicConditions = string.IsNullOrWhiteSpace(NewPatientChronicConditions) ? null : NewPatientChronicConditions.Trim(),
                Notes = string.IsNullOrWhiteSpace(NewPatientNotes) ? null : NewPatientNotes.Trim()
            };

            var validationResult = _patientValidator.Validate(patient);
            if (!validationResult.IsValid)
            {
                StatusMessage = validationResult.Errors.First().ErrorMessage;
                return;
            }

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            if (!DetailActivated)
            {
                DetailActivated = true;
            }

            await PopulateAsync(SearchText);
            SelectedPatient = Patients.FirstOrDefault(p => p.PatientId == patient.Id) ?? Patients.FirstOrDefault();

            ClearNewPatientFormState();
            StatusMessage = "Paciente registrado correctamente.";
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
    private void ResetNewPatientForm()
    {
        ClearNewPatientFormState();
        StatusMessage = null;
    }

    private bool CanRegisterPatient()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(NewPatientName) && NewPatientName.Trim().Length >= 3;
    }

    private bool CanDeletePatient(PatientListItemModel? item)
    {
        return !IsBusy && (item ?? SelectedPatient) is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeletePatient))]
    private async Task DeletePatientAsync(PatientListItemModel? item)
    {
        var target = item ?? SelectedPatient;
        if (target is null)
        {
            StatusMessage = "Selecciona un paciente para eliminar.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            SelectedPatient = target;
            var patientId = target.PatientId;

            var hasConsultations = await _context.Consultations.AnyAsync(c => c.PatientId == patientId);
            if (hasConsultations)
            {
                StatusMessage = "No puedes eliminar pacientes con consultas registradas.";
                return;
            }

            var hasActiveAppointments = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patientId &&
                (a.Status == AppointmentStatus.Programada ||
                 a.Status == AppointmentStatus.Confirmada ||
                 a.Status == AppointmentStatus.EnProceso));

            if (hasActiveAppointments)
            {
                StatusMessage = "Cancela o finaliza las citas pendientes antes de eliminar al paciente.";
                return;
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient is null)
            {
                StatusMessage = "No se encontró el paciente seleccionado.";
                return;
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            await PopulateAsync(SearchText);

            StatusMessage = "Paciente eliminado.";
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

    private void ClearNewPatientFormState()
    {
        NewPatientName = string.Empty;
        NewPatientPhone = null;
        NewPatientBirthDate = null;
        NewPatientGeneralData = null;
        NewPatientAllergies = null;
        NewPatientChronicConditions = null;
        NewPatientNotes = null;
    }

    private async Task PopulateAsync(string? filter = null)
    {
        try
        {
            IsBusy = true;
            StatusMessage = null;
            var previousSelectionId = DetailActivated ? SelectedPatient?.PatientId : null;
            Patients.Clear();
            SelectedPatient = null;

            var query = _context.Patients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var term = filter.Trim();
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{term}%") || (p.Phone != null && EF.Functions.Like(p.Phone, $"%{term}%")));
            }

            var patients = await query
                .OrderBy(p => p.Name)
                .Take(200)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Phone,
                    p.BirthDate,
                    p.Notes,
                    LastConsultation = p.Consultations
                        .OrderByDescending(c => c.ConsultationDate)
                        .Select(c => (DateTime?)c.ConsultationDate)
                        .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var patient in patients)
            {
                var birthDate = patient.BirthDate;
                var nextBirthday = GetNextBirthday(birthDate);
                var isBirthdaySoon = nextBirthday.HasValue && nextBirthday.Value.ToDateTime(TimeOnly.MinValue) <= DateTime.Today.AddDays(30);

                Patients.Add(new PatientListItemModel
                {
                    PatientId = patient.Id,
                    Name = patient.Name,
                    Phone = patient.Phone,
                    BirthDate = patient.BirthDate,
                    Notes = patient.Notes,
                    LastConsultation = patient.LastConsultation,
                    IsBirthdaySoon = isBirthdaySoon
                });
            }

            UpdateMetrics();

            var shouldSelect = DetailActivated;

            if (shouldSelect && previousSelectionId.HasValue)
            {
                SelectedPatient = Patients.FirstOrDefault(p => p.PatientId == previousSelectionId.Value);
            }

            if (shouldSelect && SelectedPatient is null)
            {
                SelectedPatient = Patients.FirstOrDefault();
            }

            if (!Patients.Any())
            {
                StatusMessage = "No se encontraron pacientes.";
                DetailActivated = false;
            }
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
        TotalPatients = Patients.Count;
        RecentConsultations = Patients.Count(p => p.LastConsultation.HasValue && p.LastConsultation.Value >= DateTime.Today.AddDays(-90));
        UpcomingBirthdays = Patients.Count(p => p.IsBirthdaySoon);
        WithoutContactInfo = Patients.Count(p => !p.HasPhone);
    }

    private static DateOnly? GetNextBirthday(DateOnly? birthDate)
    {
        if (!birthDate.HasValue)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var thisYearBirthday = new DateOnly(today.Year, birthDate.Value.Month, Math.Min(birthDate.Value.Day, DateTime.DaysInMonth(today.Year, birthDate.Value.Month)));

        return thisYearBirthday < today
            ? new DateOnly(today.Year + 1, birthDate.Value.Month, Math.Min(birthDate.Value.Day, DateTime.DaysInMonth(today.Year + 1, birthDate.Value.Month)))
            : thisYearBirthday;
    }
}
