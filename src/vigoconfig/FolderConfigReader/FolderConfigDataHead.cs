namespace vigoconfig;

public class FolderConfigDataHead
{
    public bool? KeepEmptyFolder { get; set; }

    public List<FolderConfigDataRule> Rules { get; } = [];
  
}