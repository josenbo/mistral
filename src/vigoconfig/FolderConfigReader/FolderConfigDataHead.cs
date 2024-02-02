using System.Runtime.Serialization;

namespace vigoconfig;

public class FolderConfigDataHead
{
    [DataMember(Name = "KeepEmptyFolder")]   
    public bool? KeepEmptyFolder { get; set; }

    [DataMember(Name = "ValidChars")]   
    public string? ValidCharacters { get; set; }

    [DataMember(Name = "FileType")]   
    public string? FileType { get; set; }

    [DataMember(Name = "StoredWithEncoding")]   
    public string? SourceFileEncoding { get; set; }

    [DataMember(Name = "DeployWithEncoding")]   
    public string? TargetFileEncoding { get; set; }

    [DataMember(Name = "Newline")]   
    public string? LineEnding { get; set; }

    [DataMember(Name = "FileMode")]   
    public string? FilePermission { get; set; }

    [DataMember(Name = "FixTrailingNewline")]   
    public bool? FixTrailingNewline { get; set; }
    
    [DataMember(Name = "TargetList")]   
    public string? Targets { get; set; }
    
    [DataMember(Name = "Rules")]   
    public List<FolderConfigDataRule> Rules { get; } = [];
  
}