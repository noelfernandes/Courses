using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Transfer.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository TransferRepository;
        private readonly IEventBus Bus;

        public TransferService(ITransferRepository transferRepository, IEventBus bus)
        {
            TransferRepository = transferRepository;
            Bus = bus;
        }

        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return TransferRepository.GetTransferLogs();
        }
    }
}
