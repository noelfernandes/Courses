using System;
using System.Collections.Generic;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;

namespace MicroRabbit.Banking.Data.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private BankingDbContext Context;

        public AccountRepository(BankingDbContext context)
        {
            Context = context;
        }

        public IEnumerable<Account> Getaccounts()
        {
            return Context.Accounts;
        }
    }
}
