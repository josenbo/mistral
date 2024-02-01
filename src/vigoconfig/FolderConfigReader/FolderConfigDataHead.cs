using System.Runtime.Serialization;

namespace vigoconfig;

public class FolderConfigDataHead
{
    [DataMember(Name = "KeepEmptyFolder")]   
    public bool? KeepEmptyFolder { get; set; }

    [DataMember(Name = "Rules")]   
    public List<FolderConfigDataRule> Rules { get; } = [];
  
}