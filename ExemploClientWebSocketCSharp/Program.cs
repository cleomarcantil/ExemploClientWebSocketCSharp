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
				Console.WriteLine("Escolha uma opção para conectar:");
				Console.WriteLine("  (1) - wss://echo.websocket.org");
				Console.WriteLine("  (2) - wss://exemplowebsocketcsharp.azurewebsites.net/ws");

				var url = (Console.ReadKey().KeyChar) switch
				{
					'1' => "wss://echo.websocket.org",
					'2' => "wss://exemplowebsocketcsharp.azurewebsites.net/ws",
					_ => throw new Exception("Opção inválida!"),
				};

				Console.WriteLine();

				using var cts = new CancellationTokenSource();
				using var clientWS = new ClientWebSocket();

				Console.WriteLine("Conectando...");
				await clientWS.ConnectAsync(new Uri(url), cts.Token);

				var receiveCallbackTask = StartReceiveCallback(clientWS, cts.Token);
				var sendCallbackTask = StartSendCallback(clientWS, cts.Token);

				await Task.WhenAny(receiveCallbackTask, sendCallbackTask);
				cts.Cancel();

				Console.WriteLine("Fim do programa.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro: {ex.Message}");
			}
		}


		static async Task StartSendCallback(ClientWebSocket clientWS, CancellationToken cancellationToken)
		{
			Console.WriteLine("Digite algo para enviar... (ou ENTER para sair)");

			while (clientWS.State == WebSocketState.Open)
			{
				var mensagem = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(mensagem))
					break;

				var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes(mensagem));
				await clientWS.SendAsync(sendData, WebSocketMessageType.Text, true, cancellationToken);
			}
		}

		static async Task StartReceiveCallback(ClientWebSocket clientWS, CancellationToken cancellationToken)
		{
			var buffer = new byte[4096];

			while (!cancellationToken.IsCancellationRequested)
			{
				var result = await clientWS.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

				if (result.CloseStatus.HasValue)
					break;

				var mensagemRecebida = Encoding.UTF8.GetString(buffer, 0, result.Count);
				Console.WriteLine($"Recebida: {mensagemRecebida}");
			}
		}

	}

}
