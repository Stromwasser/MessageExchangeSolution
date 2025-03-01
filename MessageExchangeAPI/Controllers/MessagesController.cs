using MessageExchangeAPI.Hubs;
using MessageExchangeAPI.Models;
using MessageExchangeAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MessageExchangeAPI.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageRepository messageRepository, IHubContext<MessageHub> hubContext, ILogger<MessagesController> logger)
        {
            _messageRepository = messageRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Получает сообщение от клиента и отправляет через SignalR
        /// </summary>
        /// <param name="message">MessageDTO с текстом и порядковым номером</param>
        /// <returns>HTTP-статус</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostMessage([FromBody] MessageDTO message)
        {
            // Проверяем корректность модели
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                // Устанавливаем время создания сообщения
                message.CreatedAt = DateTime.UtcNow;

                // Сохраняем сообщение в базе данных
                await _messageRepository.AddMessageAsync(message);

                // Логируем успешное сохранение
                _logger.LogInformation("Message saved to database: {@Message}", message);

                // Отправляем сообщение через SignalR всем подключённым клиентам
                await _hubContext.Clients.All.SendAsync("receiveMsg", message.SequenceNumber, message.Text, message.CreatedAt);

                // Логируем успешную отправку через SignalR
                _logger.LogInformation("Message sent via SignalR: {@Message}", message);

                // Возвращаем статус 200 OK
                return Ok("Message received");
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                _logger.LogError(ex, "Error while processing message");

                // Возвращаем статус 500 Internal Server Error
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Получает список за последние 10 минут (по умолчанию) или за выбранный диапазон времени
        /// </summary>
        /// <returns>Список сообщений</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            // Если параметры не переданы, автоматически подставляем последние 10 минут
            var startTime = from ?? DateTime.UtcNow.AddMinutes(-10);
            var endTime = to ?? DateTime.UtcNow;

            var messages = await _messageRepository.GetMessagesByDateRangeAsync(startTime, endTime);

            return Ok(messages);
        }





    }
}
