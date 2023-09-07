using SharpModbus;

namespace ModbusControl.Services;

public class ModbusHub: IDisposable
{
	private readonly ModbusMaster _client;

	private ModbusHub(ModbusMaster client)
	{
		_client = client;
	}

	public static ModbusHub GetModbusHub(string ip, int port)
	{
		var master = ModbusMaster.TCP(ip, port);
		var hub = new ModbusHub(master);
		return hub;
	}

	public Task<bool[]> ReadCoilsAsync(byte slave, ushort address, ushort count)
	{
		return Task.Run(() => _client.ReadCoils(slave, address, count));
	}

	public Task<ushort[]> ReadHoldingsAsync(byte slave, ushort address, ushort count)
	{
		return Task.Run(() => _client.ReadHoldingRegisters(slave, address, count));
	}

	public void Dispose()
	{
		_client?.Dispose();
	}
}
