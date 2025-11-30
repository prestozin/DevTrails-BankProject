using DevTrails___BankProject.Validators;
using DevTrails___BankProject.DTOs;

namespace DevTrails.Tests
{
    public class ValidatorsTests
    {
        private readonly DepositValidator _depositValidator;

        public ValidatorsTests()
        {
            _depositValidator = new DepositValidator();
        }

        [Fact]
        public void Deposit_NegativeAmount_ShouldHaveError()
        {
            var model = new DepositInputModel { Amount = -10, AccountNumber = "123" };
            var result = _depositValidator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
        }

        [Fact]
        public void Deposit_ZeroAmount_ShouldHaveError()
        {
            var model = new DepositInputModel { Amount = 0, AccountNumber = "123" };
            var result = _depositValidator.Validate(model);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Deposit_PositiveAmount_ShouldBeValid()
        {
            var model = new DepositInputModel { Amount = 100, AccountNumber = "123" };
            var result = _depositValidator.Validate(model);
            Assert.True(result.IsValid);
        }
    }
}