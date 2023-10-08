﻿using System.Linq.Expressions;
using AutoMapper;
using Invedia.DI.Attributes;
using Microsoft.EntityFrameworkCore;
using SWD_Laundry_Backend.Contract.Repository.Entity;
using SWD_Laundry_Backend.Contract.Repository.Interface;
using SWD_Laundry_Backend.Contract.Service.Interface;
using SWD_Laundry_Backend.Core.Models;
using SWD_Laundry_Backend.Core.Models.Common;

namespace SWD_Laundry_Backend.Service.Services;

[ScopedDependency(ServiceType = typeof(ILaundryStoreService))]
public class LaundryStoreService : Base_Service.Service, ILaundryStoreService
{
    private readonly ILaundryStoreRepository _repository;
    private readonly IMapper _mapper;

    public LaundryStoreService(ILaundryStoreRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<string> CreateAsync(LaundryStoreModel model, CancellationToken cancellationToken = default)
    {
        var query = await _repository.AddAsync(_mapper.Map<LaundryStore>(model), cancellationToken);
        var objectId = query.Id;
        return objectId;
    }

    public async Task<int> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var numberOfRows = await _repository.DeleteAsync(x => x.Id == id, cancellationToken: cancellationToken);
        return numberOfRows;
    }

    public async Task<ICollection<LaundryStore>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAsync(cancellationToken: cancellationToken, includes: x => x.ApplicationUser);
        return await list.ToListAsync(cancellationToken);
    }

    public async Task<LaundryStore?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetSingleAsync(c => c.Id == id, cancellationToken, x => x.ApplicationUser);
        return customer;
    }

    public Task<PaginatedList<LaundryStore>> GetPaginatedAsync(short pg, short size, Expression<Func<LaundryStore, object>>? orderBy = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdateAsync(string id, LaundryStoreModel model, CancellationToken cancellationToken = default)
    {
        var numberOfRows = await _repository.UpdateAsync(x => x.Id == id,
            x => x
            .SetProperty(x => x.StartTime, model.StartTime)
            .SetProperty(x => x.EndTime, model.EndTime)
            .SetProperty(x => x.Address, model.Address)
            .SetProperty(x => x.Status, model.Status)
            , cancellationToken);

        return numberOfRows;
    }
}