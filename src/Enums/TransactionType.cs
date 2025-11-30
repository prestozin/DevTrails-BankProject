using System.ComponentModel;

namespace DevTrails___BankProject.Enums
{
    public enum TransactionType
    {
        [Description("Depósito")]
        Deposit = 0,

        [Description("Saque")]
        Withdraw = 1,

        [Description("Transferência")]
        Transfer = 2,

        [Description("Taxa de serviço")]
        ServiceFee = 3,
    }
}
