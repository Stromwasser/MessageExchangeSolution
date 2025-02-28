using MessageExchangeDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageExchangeDAL.Repositories
{
    public interface IMessageRepository
    {
        Task AddMessageAsync(MessageDTO message);

        Task<IEnumerable<MessageDTO>>GetMessagesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
