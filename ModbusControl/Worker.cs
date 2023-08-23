using Microsoft.Extensions.Options;
using ModbusControl.Models;
using SharpModbus;
using System.Collections.Concurrent;

namespace ModbusControl;

public class Worker : BackgroundService
{
	private readonly ILogger<Worker> _logger;
	private readonly MQTTConfiguration _mqttOptions;
	private readonly RegistersConfiguration _registersOptions;
	private ConcurrentDictionary<string, ModbusMaster> _hubs;

	public Worker(ILogger<Worker> logger, 
		IOptions<MQTTConfiguration> mqttOptions,
		IOptions<RegistersConfiguration> registersOptions)
	{
		_logger = logger;
		_mqttOptions = mqttOptions.Value;
		_registersOptions = registersOptions.Value;
		_hubs = new ConcurrentDictionary<string, ModbusMaster>();
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.WhenAll(_registersOptions.Registers.Select(r => Task.Run(() => ReadRegisters(r, stoppingToken))));
	}

	private async Task ReadRegisters(Register register, CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				if (!_hubs.TryGetValue(register.Hub, out var client))
				{
					Thread.Sleep(2000);
					client = ModbusConnection.GetModbusClient();
					continue;
				}

				var newResultTask = ModbusConnection.ReadHoldingsAsync(client, register.Slave, register.StartAddress, register.RegisterCount);
				var winner = await Task.WhenAny(
					newResultTask,
					Task.Delay(TimeSpan.FromSeconds(2)));
				if (winner == newResultTask)
				{
					var newResult = newResultTask.Result;

					if (newResultTask.IsFaulted || newResult == null || newResult.Length != register.RegisterCount)
					{
						throw new Exception("Failed to read registers");
					}

					/*for (int i = 0; i < registerCount; i++)
					{
						if (result[i] != newResult[i])
						{
		//					Console.WriteLine($"Value register {startAddress + i} changed to {newResult[i]}");
							result[i] = newResult[i];
							await Client_Publish_Samples.Publish_Application_Message(slaveId, startAddress + i, newResult[i] ? (byte)1 : (byte)0);
						}
					}*/
				}
				else
				{
					Console.WriteLine($"Task is timed out");
					client?.Dispose();
					client = null;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to read from hub");
				Console.WriteLine($"{e.Message}");
				client?.Dispose();
				client = null;
			}
			var milliseconds = 500;
			await Task.Delay(register.Delay , stoppingToken);
		}
	}

	private int Get
}