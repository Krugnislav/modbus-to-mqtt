using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusControl.Models
{
	public class RegistersConfiguration
	{
		public Hub[] Hubs { get; set; }
		public Register[] Registers { get; set; }
	}

	public class Hub
	{
		public string Name { get; set; }
		public string Ip { get; set; }
		public int Port { get; set; }
		public int ConnectionDelay { get; set; }
		public int Timeout { get; set; }
	}

	public class Register
	{
		public string Hub { get; set; }
		public byte Slave { get; set; }
		public ushort StartAddress { get; set; }
		public ushort RegisterCount { get; set; }
		public RType ReadType { get; set; }
		public RType? WriteType { get; set;}
		public int Delay { get; set; }
	}

	public enum RType
	{
		Coil = 0,
		Holding = 1,
	}
}
