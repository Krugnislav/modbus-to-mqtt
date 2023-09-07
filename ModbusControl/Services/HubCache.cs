using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusControl.Services;

public class HubCache
{
	private readonly ConcurrentDictionary<byte, AddressCache> _addressesCache;
	public HubCache()
	{
		_addressesCache = new ConcurrentDictionary<byte, AddressCache>();
	}

	public bool CheckAndUpdateValue(byte address, ushort register, ushort value)
	{
		return GetAddressCache(address).CheckAndUpdateValue(register, value);
	}

	public bool CheckAndUpdateValue(byte address, ushort register, bool value)
	{
		return GetAddressCache(address).CheckAndUpdateValue(register, value);
	}

	private AddressCache GetAddressCache(byte address)
	{
		if (!_addressesCache.TryGetValue(address, out var cache))
		{
			cache = new AddressCache();
			_addressesCache.TryAdd(address, cache);
		}
		return cache;
	}
}

public class AddressCache
{
	private readonly ConcurrentDictionary<ushort, ushort> _addressHoldingRegistersCache;
	private readonly ConcurrentDictionary<ushort, bool> _addressCoilRegistersCache;
	public AddressCache() 
	{
		_addressHoldingRegistersCache = new ConcurrentDictionary<ushort, ushort>();
		_addressCoilRegistersCache = new ConcurrentDictionary<ushort, bool>();
	}

	public bool CheckAndUpdateValue(ushort register, ushort value)
	{
		if (!_addressHoldingRegistersCache.TryGetValue(register, out var cache))
		{
			_addressHoldingRegistersCache[register] = value;
			return false;
		}

		_addressHoldingRegistersCache[register] = value;

		return cache == value;
	}

	public bool CheckAndUpdateValue(ushort register, bool value)
	{
		if (!_addressCoilRegistersCache.TryGetValue(register, out var cache))
		{
			_addressCoilRegistersCache[register] = value;
			return false;
		}

		_addressCoilRegistersCache[register] = value;

		return cache == value;
	}
}
