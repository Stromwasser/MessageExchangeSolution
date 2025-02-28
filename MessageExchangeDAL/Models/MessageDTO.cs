using System;
using System.ComponentModel.DataAnnotations;

namespace MessageExchangeDAL.Models
{
    
    /// <summary>
    /// Представляет сообщение для обмена
    /// </summary>
    public class MessageDTO
    {
        /// <summary>Порядковый номер сообщения</summary>
        public int SequenceNumber { get; set; }

        /// <summary>Текст сообщения</summary>
        public string Text { get; set; }

        /// <summary>Дата и время создания сообщения</summary>
        public DateTime CreatedAt { get; set; }
    }

}
