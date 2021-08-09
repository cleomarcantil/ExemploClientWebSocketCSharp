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

				var uri = (Console.ReadKey().KeyChar) switch
				{
					'1' => "wss://echo.websocket.org",
					'2' => "wss://exemplowebsocketcsharp.azurewebsites.net/ws",
					_ => throw new Exception("Opção inválida!"),
				};

				Console.WriteLine();

				Console.WriteLine("Conectando...");
				using var ws = new SimpleClientWebSocket(uri, new ClientWebSocketEvents());

				await ws.OpenAsync(CancellationToken.None);

				Console.WriteLine("Fim do programa.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro: {ex.Message}");
			}
		}


		class ClientWebSocketEvents : IClientWebSocketEvents
		{
			public async Task OnConnected(SimpleClientWebSocket clientWebSocket, CancellationToken cancellationToken)
			{
				Console.WriteLine("Conectado");
				Console.WriteLine("Digite algo para enviar... (ou ENTER para sair)");

				while (clientWebSocket.Connected && !cancellationToken.IsCancellationRequested)
				{
					var mensagem = Console.ReadLine();

					if (string.IsNullOrWhiteSpace(mensagem))
					{
						await clientWebSocket.CloseAsync(cancellationToken);
						break;
					}

					await clientWebSocket.SendMessageAsync(mensagem, cancellationToken);
				}
			}

			public async Task OnDisconnected(CancellationToken cancellationToken)
			{
				Console.WriteLine("Desconectado");
			}

			public async Task OnReceiveMessage(string msg, CancellationToken cancellationToken)
			{
				Console.WriteLine($"Recebida: {msg}");
			}
		}

	}


}
