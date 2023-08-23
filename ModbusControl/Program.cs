using ModbusControl;
using ModbusControl.Models;

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostContext, services) =>
	{
		IConfiguration configuration = hostContext.Configuration;
		services.Configure<MQTTConfiguration>(configuration.GetSection(nameof(MQTTConfiguration)));
		services.AddHostedService<Worker>();
	})
	.Build();

host.Run();
