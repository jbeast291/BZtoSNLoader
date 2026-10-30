namespace ComponentPortFramework;

public class LazyDropToolFix : DropTool
{
    public string customAnimToolName = "peeper";
    
    public override string animToolName => customAnimToolName;
}