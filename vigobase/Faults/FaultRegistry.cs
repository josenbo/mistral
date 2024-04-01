using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Serilog;
using Serilog.Events;

namespace vigobase;

[PublicAPI]
public partial class FaultRegistry
{
    public IEnumerable<FaultIncident> Incidents => _incidents;
    
    public string Fatal(
        string faultKey,
        string? supportInfo,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (string.IsNullOrWhiteSpace(faultKey) || !RexFaultKey.IsMatch(faultKey))
        {
            faultKey = $"FX{_missingFaultKeyIndex,000}";
            _missingFaultKeyIndex = (_missingFaultKeyIndex + 1) % 100;
            if (_missingFaultKeyIndex == 0)
                _missingFaultKeyIndex = 1;
        }
             
        var incidentId = GetNextIncidentId(faultKey);
        
        if (string.IsNullOrWhiteSpace(message))
            message = $"Please notify support providing log files and the key {faultKey}";

        var incident = new FaultIncident(
            IncidentId: incidentId, 
            Message: message, 
            FaultKey: faultKey, 
            SupportInfo: supportInfo, 
            Severity: LogEventLevel.Fatal, 
            Timestamp: DateTime.Now,
            MemberName: memberName, 
            SourceFilePath: sourceFilePath, 
            SourceLineNumber: sourceLineNumber);
        _incidents.Add(incident);
        
        Log.Fatal("Registered {TheFaultKey} fault incident with the id {TheIncident} and severity {TheSeverity}", 
            incident.FaultKey, 
            incident.IncidentId,
            incident.Severity);

        Log.Fatal("{TheProperty} = {TheValue}", "Message".PadRight(21), incident.Message);
        Log.Fatal("{TheProperty} = {TheValue}", "Support info".PadRight(21), incident.SupportInfo);
        Log.Fatal("{TheProperty} = {TheValue}", "Timestamp".PadRight(21), incident.Timestamp);
        Log.Fatal("{TheProperty} = {TheValue}", "Member name".PadRight(21), incident.MemberName);
        Log.Fatal("{TheProperty} = {TheValue}", "Source file path".PadRight(21), incident.SourceFilePath);
        Log.Fatal("{TheProperty} = {TheValue}", "Source line number".PadRight(21), incident.SourceLineNumber);

        return incidentId;
    }

    private readonly List<FaultIncident> _incidents = [];
    
    private static string GetNextIncidentId(string faultKey)
    {
        var incidentId = _nextIncidentId++; 
        // ReSharper disable once StringLiteralTypo
        return $"{faultKey}_{incidentId}";
    }
    
    private static int _nextIncidentId = Random.Shared.Next(1000, 9000);
    private static int _missingFaultKeyIndex = 1;
    private static readonly Regex RexFaultKey = RexFaultKeyCompiled();

    [GeneratedRegex("^FX[0-9]{3}$")]
    private static partial Regex RexFaultKeyCompiled();
}