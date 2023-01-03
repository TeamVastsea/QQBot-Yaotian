// See https://aka.ms/new-console-template for more information

namespace YaoTian;

public static class Program
{
    public static async Task Main()
    {
        await BotApp.Instance.Start();
    }
}