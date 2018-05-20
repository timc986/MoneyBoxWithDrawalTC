using System;

namespace Moneybox.App.Features
{
    public interface ITransferMoney
    {
        void Execute(Guid fromAccountId, Guid toAccountId, decimal amount);
    }
}
