using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum VerificationCodeTypes
{
    [Description("Register")]
    Register = 1,
    [Description("ForgotPassword")]
    ForgotPassword = 2,
    [Description("Withdrawal")]
    Withdrawal = 3,
    [Description("VerifyOldEmail")]
    VerifyOldEmail = 4,
    [Description("UpdateEmail")]
    UpdateEmail = 5,
}