using System;
using System.Collections.Generic;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Models;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using MicroRabbit.Domain.Core.Bus;

namespace MicroRabbit.Banking.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository AccountRepository;
        private readonly IEventBus Bus;

        public AccountService(IAccountRepository accountRepository, IEventBus bus)
        {
            AccountRepository = accountRepository;
            Bus = bus;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return AccountRepository.GetAccounts();
        }

        public void Transfer(AccountTransfer accountTransfer)
        {
            var createTransferCommand = new CreateTransferCommand(accountTransfer.FromAccount, accountTransfer.ToAccount, accountTransfer.Amount);
            Bus.SendCommand(createTransferCommand);
        }
    }
}
