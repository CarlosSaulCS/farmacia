using CommunityToolkit.Mvvm.ComponentModel;
using Farmacia.Domain.Enums;

namespace Farmacia.UI.Wpf.Models;

public partial class AppointmentItemModel : ObservableObject
{
    [ObservableProperty]
    private int _appointmentId;

    [ObservableProperty]
    private DateTime _scheduledAt;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private string _patientName = string.Empty;

    [ObservableProperty]
    private string _doctorName = string.Empty;

    [ObservableProperty]
    private AppointmentStatus _status;

    [ObservableProperty]
    private string? _reason;

    [ObservableProperty]
    private string? _notes;
}
