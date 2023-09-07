using MQTTnet;
using MQTTnet.Client;

namespace ModbusControl.Services;

public class MqttClient
{
	public static async Task Publish_Application_Message(int address, int register, ushort value)
	{
		var mqttFactory = new MqttFactory();

		using var mqttClient = mqttFactory.CreateMqttClient();

		var mqttClientOptions = new MqttClientOptionsBuilder()
			.WithTcpServer("192.168.1.76", 1883)
			.WithCredentials("mqtt", "mqtt")
			.Build();

		await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

		var applicationMessage = new MqttApplicationMessageBuilder()
			.WithTopic($"home/{address}/{register}")
			.WithPayload(value.ToString())
			.Build();

		await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

		await mqttClient.DisconnectAsync();

		Console.WriteLine("MQTT application message is published.");
	}
}
