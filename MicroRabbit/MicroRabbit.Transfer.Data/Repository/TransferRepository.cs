using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System.Collections.Generic;

namespace MicroRabbit.Transfer.Data.Repository
{
    public class TransferRepository : ITransferRepository
    {
        private TransferDbContext Context;

        public TransferRepository(TransferDbContext context)
        {
            Context = context;
        }

        public void Add(TransferLog transferLog)
        {
            Context.TransferLogs.Add(transferLog);
            Context.SaveChanges();
        }

        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return Context.TransferLogs;
        }
    }
}
