namespace YaoTian.ModuleMgr;


[AttributeUsage(AttributeTargets.Class)]
public class ModuleAttribute : Attribute
{
    public string Name { get; set; }
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";

    public ModuleAttribute(string name)
    {
        Name = name;
    }
    

}