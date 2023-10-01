﻿
namespace SWD_Laundry_Backend.Contract.Service.Base_Service_Interface;
public interface IGetAble<T, in TKey> where T : class
{
    Task<ICollection<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
}
