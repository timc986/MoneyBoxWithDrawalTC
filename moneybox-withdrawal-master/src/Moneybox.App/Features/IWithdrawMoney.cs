using System;

namespace Moneybox.App.Features
{
    public interface IWithdrawMoney
    {
        void Execute(Guid fromAccountId, decimal amount);
    }
}
