using Common.Logging;
using Moneybox.App.DataAccess;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney : ITransferMoney
    {
        private IAccountRepository accountRepository;
        private ILog log;

        public TransferMoney(IAccountRepository AccountRepository, ILog Log)
        {
            accountRepository = AccountRepository;
            log = Log;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            try
            {
                var from = accountRepository.GetAccountById(fromAccountId);
                var to = accountRepository.GetAccountById(toAccountId);

                from.WithdrawFrom(amount);
                to.TransferTo(amount);

                accountRepository.Update(from);
                accountRepository.Update(to);
            }
            catch(Exception ex)
            {
                log.Error("Exception in TransferMoney", ex);

                throw ex;
            }            
        }
    }
}
