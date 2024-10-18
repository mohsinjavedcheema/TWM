using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum OrderState
    {
        [Description("Accepted")]
        Accepted = 0,
        [Description("Cancelled")]
        Cancelled = 1,
        [Description("Filled")]
        Filled = 2,
        [Description("Initialized")]
        Initialized = 3,
        PartFilled = 4,
        CancelSubmitted = 5,
        [Description("ChangeSubmitted")]
        ChangeSubmitted = 6,
        Submitted = 7,
        TriggerPending = 8,
        Rejected = 9,
        [Description("Working")]
        Working = 10, 
        CancelPending = 11, 
        ChangePending = 12,
        [Description("Expired")]
        Expired = 13,
        AcceptedByRisk = 50,
        Unknown = 99, 
    }
}