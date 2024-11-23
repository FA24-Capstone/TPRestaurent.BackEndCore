namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum OrderStatus
    {
        TableAssigned = 1,
        DepositPaid = 2,
        TemporarilyCompleted = 3,
        Pending = 4,
        Processing = 5,
        ReadyForDelivery = 6,
        AssignedToShipper = 7,
        Delivering = 8,
        Completed = 9,
        Cancelled = 10,
    }
}