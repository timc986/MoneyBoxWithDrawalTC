using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App
{
    public class Account
    {
        private INotificationService notificationService;

        public Account(INotificationService NotificationService)
        {
            notificationService = NotificationService;
        }

        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public void WithdrawFrom(decimal amount)
        {
            if (Balance - amount < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if (Balance - amount < 500m)
            {
                notificationService.NotifyFundsLow(User.Email);
            }

            Balance = Balance - amount;
            Withdrawn = Withdrawn - amount;
        }

        public void TransferTo(decimal amount)
        {
            if (PaidIn + amount > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (PayInLimit - PaidIn < 500m)
            {
                notificationService.NotifyApproachingPayInLimit(User.Email);
            }

            Balance = Balance + amount;
            PaidIn = PaidIn + amount;
        }
    }
}
