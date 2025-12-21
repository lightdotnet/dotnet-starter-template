namespace Monolith.Blazor.Services;

public class LayoutService
{
    /// <summary>
    /// Event that is raised when a major update occurs.
    /// </summary>
    public event EventHandler? MajorUpdateOccured;

    /// <summary>
    /// Gets or sets a value indicating whether the layout is in dark mode.
    /// </summary>
    public bool IsDarkMode { get; private set; }

    /// <summary>
    /// Sets the dark mode.
    /// </summary>
    /// <param name="value">The value indicating whether dark mode is enabled.</param>
    public void SetDarkMode(bool value)
    {
        IsDarkMode = value;
        OnMajorUpdateOccured();
    }

    /// <summary>
    /// Raises the MajorUpdateOccured event.
    /// </summary>
    private void OnMajorUpdateOccured() => MajorUpdateOccured?.Invoke(this, EventArgs.Empty);
}
