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
    public class TransferMoneyTest
    {
        private Mock<IAccountRepository> accountRepository;
        private Mock<INotificationService> notificationService;
        private Mock<ILog> log;
        private ITransferMoney transferMoney;
        private Account fromAccount;
        private Account toAccount;

        [SetUp]
        public void SetUp()
        {
            //Arrange
            accountRepository = new Mock<IAccountRepository>();
            notificationService = new Mock<INotificationService>();
            log = new Mock<ILog>();
            transferMoney = new TransferMoney(accountRepository.Object, notificationService.Object, log.Object);

            //In reality will use IOC like Unity but might be overengineer for this project
            fromAccount = new Account();
            toAccount = new Account();
        }

        [Test]
        public void GIVEN_any_WEHN_Execute_is_called_Should_call_accountRepository_GetAccountById_twice()
        {
            //Arrange
            var transferAmount = 800;

            fromAccount.Balance = 10000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 10;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            accountRepository.Verify(x => x.GetAccountById(fromAccount.Id), Times.Once);
            accountRepository.Verify(x => x.GetAccountById(toAccount.Id), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_is_more_than_transferAmount_for_over_500m_and_ToPayInLimit_is_more_than_paidIn_for_over_500m_WEHN_Execute_is_called_Should_call_accountRepository_Update_Twice()
        {
            //Arrange
            var transferAmount = 800;

            fromAccount.Balance = 10000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 10;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            accountRepository.Verify(x => x.Update(fromAccount), Times.Once);
            accountRepository.Verify(x => x.Update(toAccount), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_less_than_transferAmount_WEHN_Execute_is_called_Should_throw_InvalidOperationException_insufficient_funds()
        {
            void Test()
            {
                //Arrange
                var transferAmount = 800;

                fromAccount.Balance = 1;
                fromAccount.PaidIn = 10;
                fromAccount.Withdrawn = 500;
                fromAccount.Id = Guid.NewGuid();
                fromAccount.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "from test user",
                    Email = "fromtestemail@email.com"
                };

                toAccount.Balance = 10000;
                toAccount.PaidIn = 10;
                toAccount.Withdrawn = 500;
                toAccount.Id = Guid.NewGuid();
                toAccount.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "to test user",
                    Email = "totestemail@email.com"
                };

                accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
                accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

                //Act
                transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);
            }

            //Assert
            Assert.Throws(typeof(InvalidOperationException), Test, "Insufficient funds to make transfer");
        }

        [Test]
        public void GIVEN_ToPaidIn_plus_transfer_amount_exceed_PayInLimit_WEHN_Execute_is_called_Should_throw_InvalidOperationException_limit_reached()
        {
            void Test()
            {
                //Arrange
                var transferAmount = 800;

                fromAccount.Balance = 100000000;
                fromAccount.PaidIn = 10;
                fromAccount.Withdrawn = 500;
                fromAccount.Id = Guid.NewGuid();
                fromAccount.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "from test user",
                    Email = "fromtestemail@email.com"
                };

                toAccount.Balance = 10000;
                toAccount.PaidIn = 1000000;
                toAccount.Withdrawn = 500;
                toAccount.Id = Guid.NewGuid();
                toAccount.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "to test user",
                    Email = "totestemail@email.com"
                };

                accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
                accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

                //Act
                transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);
            }

            //Assert
            Assert.Throws(typeof(InvalidOperationException), Test, "Account pay in limit reached");
        }

        [Test]
        public void GIVEN_Frombalance_less_than_transferAmount_and_ToPaidIn_plus_transfer_amount_exceed_PayInLimit_WEHN_Execute_is_called_Should_throw_InvalidOperationException_insufficient_funds()
        {
            void Test()
            {
                //Arrange
                var transferAmount = 800;

                fromAccount.Balance = 1;
                fromAccount.PaidIn = 10;
                fromAccount.Withdrawn = 500;
                fromAccount.Id = Guid.NewGuid();
                fromAccount.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "from test user",
                    Email = "fromtestemail@email.com"
                };

                toAccount.Balance = 10000;
                toAccount.PaidIn = 1000000;
                toAccount.Withdrawn = 500;
                toAccount.Id = Guid.NewGuid();
                toAccount.User = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "to test user",
                    Email = "totestemail@email.com"
                };

                accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
                accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

                //Act
                transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);
            }

            //Assert
            Assert.Throws(typeof(InvalidOperationException), Test, "Insufficient funds to make transfer");
        }

        [Test]
        public void GIVEN_Frombalance_is_more_than_transferAmount_for_less_than_500m_and_ToPayInLimit_is_more_than_paidIn_for_less_than_500m_WEHN_Execute_is_called_Should_call_accountRepository_Update_Twice()
        {
            //Arrange
            var transferAmount = 800;

            fromAccount.Balance = 10000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 10;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            accountRepository.Verify(x => x.Update(fromAccount), Times.Once);
            accountRepository.Verify(x => x.Update(toAccount), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_is_more_than_transferAmount_for_less_than_500m_and_ToPayInLimit_is_more_than_paidIn_for_less_than_500m_WEHN_Execute_is_called_Should_call_notificationService_once_Twice()
        {
            //Arrange
            var transferAmount = 80;

            fromAccount.Balance = 110;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 3600;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            notificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Once);
            notificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_more_than_transferAmount_over_500m_and_ToPayInLimit_is_more_than_paidIn_for_less_than_500m_WEHN_Execute_is_called_Should_call_accountRepository_Update_Twice()
        {
            //Arrange
            var transferAmount = 80;

            fromAccount.Balance = 10000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 3600;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            accountRepository.Verify(x => x.Update(fromAccount), Times.Once);
            accountRepository.Verify(x => x.Update(toAccount), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_more_than_transferAmount_over_500m_and_ToPayInLimit_is_more_than_paidIn_for_less_than_500m_WEHN_Execute_is_called_Should_call_notificationService_once()
        {
            //Arrange
            var transferAmount = 80;

            fromAccount.Balance = 10000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 3600;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            notificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_is_more_than_transferAmount_for_less_than_500m_and_ToPaidIn_plus_transferAmount_not_exceeding_PayInLimit_WEHN_Execute_is_called_Should_call_notificationService_once()
        {
            //Arrange
            var transferAmount = 80;

            fromAccount.Balance = 100;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 10;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            notificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_is_more_than_transferAmount_over_500m_and_ToPaidIn_plus_transferAmount_not_exceeding_PayInLimit_WEHN_Execute_is_called_Should_frombalance_reduced_by_transferAmount_and_toBalance_increased_by_transferAmount()
        {
            //Arrange
            var transferAmount = 80;

            fromAccount.Balance = 100000000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 10;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance.Equals(100000000 - transferAmount))), Times.Once);
            accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance.Equals(10000 + transferAmount))), Times.Once);
        }

        [Test]
        public void GIVEN_Frombalance_is_more_than_transferAmount_and_ToPaidIn_plus_transferAmount_not_exceeding_PayInLimit_WEHN_Execute_is_called_Should_withdrawn_reduced_by_transferAmount_and_ToPaidIn_increased_by_transferAmount()
        {
            //Arrange
            var transferAmount = 80;

            fromAccount.Balance = 100000000;
            fromAccount.PaidIn = 10;
            fromAccount.Withdrawn = 500;
            fromAccount.Id = Guid.NewGuid();
            fromAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "from test user",
                Email = "fromtestemail@email.com"
            };

            toAccount.Balance = 10000;
            toAccount.PaidIn = 10;
            toAccount.Withdrawn = 500;
            toAccount.Id = Guid.NewGuid();
            toAccount.User = new User()
            {
                Id = Guid.NewGuid(),
                Name = "to test user",
                Email = "totestemail@email.com"
            };

            accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            //Act
            transferMoney.Execute(fromAccount.Id, toAccount.Id, transferAmount);

            //Assert
            accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Withdrawn.Equals(500 - transferAmount))), Times.Once);
            accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.PaidIn.Equals(10 + transferAmount))), Times.Once);
        }
    }
}
