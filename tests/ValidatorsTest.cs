using Xunit;
using DevTrails___BankProject.Validators;
using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Enums;

namespace DevTrails.Tests
{
    public class ValidatorsTests
    {
        private readonly DepositValidator _depositValidator;
        private readonly AccountValidator _accountValidator = new AccountValidator();
        public ValidatorsTests()
        {
            _depositValidator = new DepositValidator();
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenAmountIsNegative()
        {
            var model = new DepositInputModel { Amount = -10, AccountNumber = "123" };
            var result = _depositValidator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenAmountIsZero()
        {
            var model = new DepositInputModel { Amount = 0, AccountNumber = "123" };
            var result = _depositValidator.Validate(model);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenAmountIsPositive()
        {
            var model = new DepositInputModel { Amount = 100, AccountNumber = "123" };
            var result = _depositValidator.Validate(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Account_ShouldHaveError_WhenCpfIsInvalid()
        {
            var model = new AccountCreateModel { CPF = "123", AccountType = AccountType.Checking };
            var result = _accountValidator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CPF");
        }

        [Fact]
        public void Account_ShouldBeValid_WhenDataIsCorrect()
        {
            var model = new AccountCreateModel { CPF = "12345678900", AccountType = AccountType.Savings };
            var result = _accountValidator.Validate(model);

            Assert.True(result.IsValid);
        }
    }
}