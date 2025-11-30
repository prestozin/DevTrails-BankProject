using System.ComponentModel;
using System.Runtime.Serialization;

namespace DevTrails___BankProject.Enums
{
    public enum AccountType
    {
        [Description("Conta Corrente")]
        Checking = 0,

        [Description("Conta Poupança")]
        Savings = 1,
    }
}
