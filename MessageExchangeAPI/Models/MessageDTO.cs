using System;
using System.ComponentModel.DataAnnotations;

namespace MessageExchangeAPI.Models
{
    
    /// <summary>
    /// Представляет сообщение для обмена
    /// </summary>
    public class MessageDTO
    {
        /// <summary>Порядковый номер сообщения</summary>
        public int SequenceNumber { get; set; }

        /// <summary>Текст сообщения</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Дата и время создания сообщения</summary>
        public DateTime CreatedAt { get; set; }
    }

}
