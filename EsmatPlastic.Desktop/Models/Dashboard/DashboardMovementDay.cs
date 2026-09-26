namespace EsmatPlastic.Desktop.Models.Dashboard;

public sealed class DashboardMovementDay
{
    public string DayLabel { get; set; } = string.Empty;
    public decimal Incoming { get; set; }
    public decimal Outgoing { get; set; }
    public double IncomingPercent { get; set; }
    public double OutgoingPercent { get; set; }
}
