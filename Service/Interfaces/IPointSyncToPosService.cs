namespace backend.Service.Interfaces
{
    public interface IPointSyncToPosService
    {
        Task<bool> SyncEarnPointToPosAsync(string phoneNumber, int points);
        Task<PosRedeemResponse> SyncRedeemPointToPosAsync(string phoneNumber, double points);


    }
}
