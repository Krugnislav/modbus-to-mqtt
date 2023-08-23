using SharpModbus;

namespace ModbusControl.Models;

public static class ModbusConnection
{
	public static ModbusMaster GetModbusClient()
	{
		try
		{
			var master = ModbusMaster.TCP("192.168.1.197", 8233);
			return master;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		return null;
	}

	public static Task<bool[]> ReadCoilsAsync(ModbusMaster client, byte slave, ushort address, ushort count)
	{
		var task = Task.Run(() => {
			try
			{
				return client.ReadCoils(slave, address, count);
			}
			catch (Exception ex)
			{
				return null;
			}
		});
		return task;
	}

	public static Task<ushort[]> ReadHoldingsAsync(ModbusMaster client, byte slave, ushort address, ushort count)
	{
		var task = Task.Run(() => {
			try
			{
				return client.ReadHoldingRegisters(slave, address, count);
			}
			catch (Exception ex)
			{
				return null;
			}
		});
		return task;
	}
}
