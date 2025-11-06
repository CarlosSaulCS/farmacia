using Farmacia.UI.Wpf.Models;

namespace Farmacia.UI.Wpf.Services;

public class UserSessionService
{
    private UserSession? _current;

    public event EventHandler<UserSession?>? SessionChanged;

    public UserSession? Current => _current;

    public void SetSession(UserSession session)
    {
        _current = session;
        SessionChanged?.Invoke(this, _current);
    }

    public void Clear()
    {
        _current = null;
        SessionChanged?.Invoke(this, _current);
    }
}
