using NUnit.Framework;
using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using System;
using Common.Logging;

namespace Moneybox.App.Test.Features
{
    [TestFixture]
    public class WithdrawMoneyTest
    {
        private Mock<IAccountRepository> accountRepository;
        private Mock<INotificationService> notificationService;
        private Mock<ILog> log;
        private IWithdrawMoney withdrawMoney;
        private Account account;

        [SetUp]
        public void SetUp()
        {
            //Arrange
            accountRepository = new Mock<IAccountRepository>();
            notificationService = new Mock<INotificationService>();
            log = new Mock<ILog>();
            withdrawMoney = new WithdrawMoney(accountRepository.Object, notificationService.Object, log.Object);

            account = new Account();
        }

        [Test]
        public void GIVEN_any_WEHN_Execute_is_called_Should_call_accountRepository_GetAccountById_once()
        {
            //Arrange
            var withdrawAmount = 80;

            account.Balance = 100000000;
            account.PaidIn = 10;
            account.Withdrawn = 500;
            account.Id = Guid.NewGuid();
            account.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "test user",
                Email = "testemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

            //Act
            withdrawMoney.Execute(account.Id, withdrawAmount);

            //Assert
            accountRepository.Verify(x => x.GetAccountById(account.Id), Times.Once);
        }

        [Test]
        public void GIVEN_balance_is_more_than_withdrawAmount_over_500m_WEHN_Execute_is_called_Should_call_accountRepository_Update_once()
        {
            //Arrange
            var withdrawAmount = 80;

            account.Balance = 100000000;
            account.PaidIn = 10;
            account.Withdrawn = 500;
            account.Id = Guid.NewGuid();
            account.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "test user",
                Email = "testemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

            //Act
            withdrawMoney.Execute(account.Id, withdrawAmount);

            //Assert
            accountRepository.Verify(x => x.Update(account), Times.Once);
        }

        [Test]
        public void GIVEN_balance_less_than_withdrawAmount_WEHN_Execute_is_called_Should_throw_InvalidOperationException_insufficient_funds()
        {
            void Test()
            {
                //Arrange
                var withdrawAmount = 80;

                account.Balance = 1;
                account.PaidIn = 10;
                account.Withdrawn = 500;
                account.Id = Guid.NewGuid();
                account.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "test user",
                    Email = "testemail@email.com"
                };

                accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

                //Act
                withdrawMoney.Execute(account.Id, withdrawAmount);
            }

            //Assert
            Assert.Throws(typeof(InvalidOperationException), Test, "Insufficient funds to make transfer");
        }

        [Test]
        public void GIVEN_balance_is_more_than_amount_for_less_than_500m_WEHN_Execute_is_called_Should_call_accountRepository_Update_once()
        {
            //Arrange
            var withdrawAmount = 80;

            account.Balance = 100;
            account.PaidIn = 10;
            account.Withdrawn = 500;
            account.Id = Guid.NewGuid();
            account.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "test user",
                Email = "testemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

            //Act
            withdrawMoney.Execute(account.Id, withdrawAmount);

            //Assert
            notificationService.Verify(x => x.NotifyFundsLow(account.User.Email), Times.Once);
            accountRepository.Verify(x => x.Update(account), Times.Once);
        }

        [Test]
        public void GIVEN_balance_is_more_than_amount_for_less_than_500m_WEHN_Execute_is_called_Should_call_notificationService_once()
        {
            //Arrange
            var withdrawAmount = 80;

            account.Balance = 100;
            account.PaidIn = 10;
            account.Withdrawn = 500;
            account.Id = Guid.NewGuid();
            account.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "test user",
                Email = "testemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

            //Act
            withdrawMoney.Execute(account.Id, withdrawAmount);

            //Assert
            notificationService.Verify(x => x.NotifyFundsLow(account.User.Email), Times.Once);
        }

        [Test]
        public void GIVEN_balance_is_more_than_withdrawAmount_WEHN_Execute_is_called_Should_balance_reduced_by_withdrawAmount()
        {
            //Arrange
            var withdrawAmount = 80;

            account.Balance = 100000000;
            account.PaidIn = 10;
            account.Withdrawn = 500;
            account.Id = Guid.NewGuid();
            account.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "test user",
                Email = "testemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

            //Act
            withdrawMoney.Execute(account.Id, withdrawAmount);

            //Assert
            accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance.Equals(100000000 - withdrawAmount))), Times.Once);
        }

        [Test]
        public void GIVEN_balance_is_more_than_withdrawAmount_WEHN_Execute_is_called_Should_withdrawn_reduced_by_withdrawAmount()
        {
            //Arrange
            var withdrawAmount = 80;

            account.Balance = 100000000;
            account.PaidIn = 10;
            account.Withdrawn = 500;
            account.Id = Guid.NewGuid();
            account.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "test user",
                Email = "testemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(account.Id)).Returns(account);

            //Act
            withdrawMoney.Execute(account.Id, withdrawAmount);

            //Assert
            accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Withdrawn.Equals(500 - withdrawAmount))), Times.Once);
        }
    }
}
