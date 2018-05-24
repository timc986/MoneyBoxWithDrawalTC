using Common.Logging;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney: IWithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;
        private ILog log;

        public WithdrawMoney(IAccountRepository AccountRepository, INotificationService NotificationService, ILog Log)
        {
            accountRepository = AccountRepository;
            notificationService = NotificationService;
            log = Log;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            try
            {
                var from = accountRepository.GetAccountById(fromAccountId);

                from.WithdrawFrom(amount, notificationService);

                accountRepository.Update(from);
            }
            catch(Exception ex)
            {
                log.Error("Exception in WithdrawMoney", ex);

                throw;
            }            
        }
    }
}
