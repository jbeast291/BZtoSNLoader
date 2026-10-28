namespace ComponentPortFramework;

public class LazyDropToolFix : DropTool
{
    private string _animToolName = "peeper";

    private void Start()
    {
        var prefabIdentifier = GetComponent<PrefabIdentifier>();
        if (prefabIdentifier != null)
        {
            if (prefabIdentifier.ClassId == "Triops")
                _animToolName = "eyeye";
        }
    }

    public override string animToolName => _animToolName;
}