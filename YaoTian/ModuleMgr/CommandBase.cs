using System.Reflection;

namespace YaoTian.ModuleMgr;

public class CommandBase
{
    public ModuleBase MethodModule { get; init; }
    public CommandAttribute CommandInfo { get; init; }
    public MethodInfo InnerMethod { get; init; }
}