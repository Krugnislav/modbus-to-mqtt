using Microsoft.Extensions.Options;
using ModbusControl.Models;
using ModbusControl.Services;
using SharpModbus;
using System.Collections.Concurrent;

namespace ModbusControl;

public class Worker : BackgroundService
{
	private readonly ILogger<Worker> _logger;
	private readonly MQTTConfiguration _mqttOptions;
	private readonly RegistersConfiguration _registersOptions;
	private ConcurrentDictionary<string, ModbusHub> _hubs;
	private ConcurrentDictionary<string, HubCache> _hubCache;

	public Worker(ILogger<Worker> logger, 
		IOptions<MQTTConfiguration> mqttOptions,
		IOptions<RegistersConfiguration> registersOptions)
	{
		_logger = logger;
		_mqttOptions = mqttOptions.Value;
		_registersOptions = registersOptions.Value;
		_hubs = new ConcurrentDictionary<string, ModbusHub>();
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.WhenAll(_registersOptions.Registers.Select(r => Task.Run(() => ReadRegistersAsync(r, stoppingToken))));
	}

	private async Task ReadRegistersAsync(Register register, CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			ModbusHub? client = null;
			try
			{
				ModbusHub? hub = _hubs.GetOrAdd(register.Hub, (string key) =>
				{
					var hubOpt = _registersOptions.Hubs.FirstOrDefault(h => h.Name == register.Hub);
					if (hubOpt == null)
						return null;
					Thread.Sleep(hubOpt.ConnectionDelay * 1000);
					return ModbusHub.GetModbusHub(hubOpt.Ip, hubOpt.Port);
				});

				if (hub == null)
				{
					_logger.LogError($"Can't find a hub with name {register.Hub}");
					return;
				}

				var newResultTask = client.ReadHoldingsAsync(register.Slave, register.StartAddress, register.RegisterCount);
				var winner = await Task.WhenAny(
					newResultTask,
					Task.Delay(TimeSpan.FromSeconds(2)));
				if (winner != newResultTask)
				{
					throw new TimeoutException();
				}
				var newResult = newResultTask.Result;

				if (newResultTask.IsFaulted || newResult == null || newResult.Length != register.RegisterCount)
				{
					throw new Exception("Failed to read registers");
				}

				var cache = _hubCache.GetOrAdd(register.Hub, (string key) => new HubCache());
				for (ushort i = 0; i < register.RegisterCount; i++)
				{
					if (!cache.CheckAndUpdateValue(register.Slave, (ushort)(register.StartAddress + i), newResult[i]))
					{
						await MqttClient.Publish_Application_Message(register.Slave, register.StartAddress + i, newResult[i]);
					}
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to read from hub");
				_hubs[register.Hub]?.Dispose();
				_hubs.TryRemove(register.Hub, out client);
			}
			await Task.Delay(register.Delay , stoppingToken);
		}
	}
}