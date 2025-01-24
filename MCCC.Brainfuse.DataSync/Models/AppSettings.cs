using System.ComponentModel.DataAnnotations;
using MCCC.Extensions.Configuration.Encryption;

namespace MCCC.Brainfuse.DataSync.Models;

public class AppSettings
{
    public const string Configuration = "Configuration";

    public bool UpdateMode { get; set; }
    public bool UseProxy { get; set; }

    [Required] public ConnectionString ConnectionStrings { get; set; }

    [Required] public string McccTutorCourseMappingFile { get; set; } = string.Empty;

    [Required] public string ExportLocation { get; set; } = string.Empty;

    public int? ReportLookBackPeriod { get; set; }
    
    public string? ReportStartDate { get; set; }
    
    public string? SpecificSearchDate { get; set; }
    public string? SpecificEndDate { get; set; }
    
    public string? ExportType { get; set; }
    public bool StarfishExport { get; set; }
    public string? StarfishExportLocation { get; set; }
    
    public BrainfuseApiSettings BrainfuseApiSettings { get; set; }
}

public class ConnectionString
{
    public string Colleague { get; set; } = string.Empty;
    public string Croa { get; set; } = string.Empty;
}

public class BrainfuseApiSettings
{
    [Required] public string BrainfuseApiBaseUrl { get; set; } = string.Empty;
    
    [Encrypted] [Required] public string BrainfuseApiKey { get; set; } = string.Empty;

    public int ThreadCount { get; set; }
    public int PageSize { get; set; }
}