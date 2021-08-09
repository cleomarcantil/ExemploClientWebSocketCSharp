using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExemploClientWebSocketCSharp
{
	public interface IClientWebSocketEvents
	{
		public Task OnConnected(SimpleClientWebSocket clientWebSocket, CancellationToken cancellationToken);
		public Task OnDisconnected(CancellationToken cancellationToken);
		public Task OnReceiveMessage(string msg, CancellationToken cancellationToken);
	}

	public class SimpleClientWebSocket : IDisposable
	{
		public delegate Task OnReceiveMessageCallback(string msg, CancellationToken cancellationToken);

		private readonly ClientWebSocket clientWS;
		private readonly IClientWebSocketEvents clientWebSocketEvents;

		public SimpleClientWebSocket(string uri, IClientWebSocketEvents clientWebSocketEvents)
		{
			clientWS = new ClientWebSocket();
			Uri = uri;
			this.clientWebSocketEvents = clientWebSocketEvents;
		}

		public void Dispose() => clientWS.Dispose();

		public string Uri { get; }

		public bool Connected => (clientWS.State == WebSocketState.Open);

		public async Task OpenAsync(CancellationToken cancellationToken)
		{
			if (Connected)
				throw new Exception($"{nameof(ClientWebSocket)} já conecado!");

			await clientWS.ConnectAsync(new Uri(Uri), cancellationToken);

			while (!Connected && !cancellationToken.IsCancellationRequested)
				await Task.Delay(50, cancellationToken);

			if (!Connected)
				return;

			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			_ = Task.Run(async () => await clientWebSocketEvents.OnConnected(this, cts.Token));

			await ReceiveLoopAsync(cancellationToken);
			cts.Cancel();

			await clientWebSocketEvents.OnDisconnected(cancellationToken);
		}

		public async Task CloseAsync(CancellationToken cancellationToken)
			=> await clientWS.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);

		private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
		{
			var buffer = new byte[4096];

			while (Connected && !cancellationToken.IsCancellationRequested)
			{
				var result = await clientWS.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

				if (result.CloseStatus.HasValue)
					break;

				var mensagemRecebida = Encoding.UTF8.GetString(buffer, 0, result.Count);
				await clientWebSocketEvents.OnReceiveMessage(mensagemRecebida, cancellationToken);
			}
		}

		public async Task SendMessageAsync(string msg, CancellationToken cancellationToken)
		{
			if (!Connected)
				throw new Exception($"Não conectado a '{Uri}'!");

			var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
			await clientWS.SendAsync(sendData, WebSocketMessageType.Text, true, cancellationToken);
		}

	}

}
