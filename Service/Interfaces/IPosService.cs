﻿using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IPosService
    {
        Task<PosCouponResponse?> GetCouponDetailsAsync(string couponCode);
        Task<bool> MarkCouponAsUsedAsync(string couponCode);

    }
}
