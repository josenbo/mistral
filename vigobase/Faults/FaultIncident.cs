using JetBrains.Annotations;
using Serilog.Events;

namespace vigobase;

[PublicAPI]
public record FaultIncident(string IncidentId, string Message, string FaultKey, string? SupportInfo, LogEventLevel Severity, DateTime Timestamp, string MemberName, string SourceFilePath, int SourceLineNumber);