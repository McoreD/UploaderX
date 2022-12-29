using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;

namespace UploaderX;

public class Program
{
    internal static ApplicationConfig? Settings { get; set; }
    internal static UploadersConfig? UploadersConfig { get; set; }
    internal static TaskSettings DefaultTaskSettings { get; set; }

    public static void Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            })
            .Build();

        host.Run();
    }
}
