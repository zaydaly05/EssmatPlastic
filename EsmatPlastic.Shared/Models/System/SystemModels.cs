namespace EsmatPlastic.Shared.Models.System;

public class HealthStatusResponse
{
    public string Status { get; set; } = "Healthy";
    public string Architecture { get; set; } = "";
    public string Mode { get; set; } = "LocalNetworkPrimary";
    public bool IsOnline { get; set; } = true;
    public bool IsNeonBackupOnline { get; set; } = false;
    public string PrimaryDatabase { get; set; } = "";
    public DateTime? LastSyncUtc { get; set; }
}
