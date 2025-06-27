namespace backend.Service.Interfaces
{
    public interface IPointSyncToPosService
    {
        Task<bool> SyncEarnPointToPosAsync(string phoneNumber, int points);
        Task<bool> SyncRedeemPointToPosAsync(string phoneNumber, int points);


    }
}
