using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExemploClientWebSocketCSharp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Exemplo cliente de conexão a um WebSocket");

			try
			{
				using var cts = new CancellationTokenSource();
				using var clientWS = new ClientWebSocket();
				var wsUri = new Uri("wss://echo.websocket.org");
				
				await clientWS.ConnectAsync(wsUri, cts.Token);

				while (clientWS.State == WebSocketState.Open)
				{
					Console.Write("Digite uma mensagem para enviar: ");
					var mensagem = Console.ReadLine();

					if (string.IsNullOrWhiteSpace(mensagem))
					{
						cts.Cancel();
						break;
					}

					var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes(mensagem));
					await clientWS.SendAsync(sendData, WebSocketMessageType.Text, true, cts.Token);

					var responseBuffer = new byte[4096];

					while (true)
					{
						var response = await clientWS.ReceiveAsync(new ArraySegment<byte>(responseBuffer), cts.Token);

						if (response.EndOfMessage)
							break;
					}

					var responseText = Encoding.UTF8.GetString(responseBuffer);
					Console.WriteLine($"Resposta: {responseText}");
				}

				Console.WriteLine("Fim do programa.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro: {ex.Message}");
			}
		}
	}
}
