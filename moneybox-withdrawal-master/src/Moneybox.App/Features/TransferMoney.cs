using Common.Logging;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney : ITransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;
        private ILog log;

        public TransferMoney(IAccountRepository AccountRepository, INotificationService NotificationService, ILog Log)
        {
            accountRepository = AccountRepository;
            notificationService = NotificationService;
            log = Log;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            try
            {
                var from = accountRepository.GetAccountById(fromAccountId);
                var to = accountRepository.GetAccountById(toAccountId);

                from.WithdrawFrom(amount, notificationService);
                to.TransferTo(amount, notificationService);

                accountRepository.Update(from);
                accountRepository.Update(to);
            }
            catch(Exception ex)
            {
                log.Error("Exception in TransferMoney", ex);

                throw;
            }            
        }
    }
}
