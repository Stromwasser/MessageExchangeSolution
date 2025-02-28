using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MessageExchangeAPI.Hubs
{
    public class MessageHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Логируем подключение клиента
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Логируем отключение клиента
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Метод для отправки сообщения
        public async Task BroadcastMessage(int sequenceNumber, string text, DateTime createdAt)
        {
            Console.WriteLine($"Broadcasting: {text}");
            await Clients.All.SendAsync("receiveMsg", sequenceNumber, text, createdAt);
        }
    }
}
