using Common.Logging;
using Moneybox.App.DataAccess;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney: IWithdrawMoney
    {
        private IAccountRepository accountRepository;
        private ILog log;

        public WithdrawMoney(IAccountRepository AccountRepository, ILog Log)
        {
            accountRepository = AccountRepository;
            log = Log;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            try
            {
                var from = accountRepository.GetAccountById(fromAccountId);

                from.WithdrawFrom(amount);

                accountRepository.Update(from);
            }
            catch(Exception ex)
            {
                log.Error("Exception in WithdrawMoney", ex);

                throw ex;
            }            
        }
    }
}
