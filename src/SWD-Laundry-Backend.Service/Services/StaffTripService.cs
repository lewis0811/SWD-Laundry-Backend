﻿using System.Linq.Expressions;
using AutoMapper;
using Invedia.DI.Attributes;
using Microsoft.EntityFrameworkCore;
using SWD_Laundry_Backend.Contract.Repository.Entity;
using SWD_Laundry_Backend.Contract.Repository.Interface;
using SWD_Laundry_Backend.Contract.Service.Interface;
using SWD_Laundry_Backend.Core.Models;
using SWD_Laundry_Backend.Core.Models.Common;
using SWD_Laundry_Backend.Core.QueryObject;
using SWD_Laundry_Backend.Core.Utils;

namespace SWD_Laundry_Backend.Service.Services;

[ScopedDependency(ServiceType = typeof(IStaffTripService))]
public class StaffTripService : Base_Service.Service, IStaffTripService
{
    private readonly Expression<Func<Staff_Trip, object>>[]? _items =
        {
            p => p.Staff,
            p => p.Building,
            p => p.TimeSchedule
        };

    private readonly IStaffTripRepository _repository;
    private readonly IMapper _mapper;

    public StaffTripService(IStaffTripRepository staffTripRepository, IMapper mapper)
    {
        _repository = staffTripRepository;
        _mapper = mapper;
    }

    public async Task<string> CreateAsync(StaffTripModel model, CancellationToken cancellationToken = default)
    {
        var query = await _repository.AddAsync(_mapper.Map<Staff_Trip>(model), cancellationToken);
        var objectId = query.Id;
        return objectId;
    }

    public async Task<int> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var numberOfRows = await _repository.DeleteAsync(x => x.Id == id, cancellationToken: cancellationToken);
        return numberOfRows;
    }

    public async Task<ICollection<Staff_Trip>> GetAllAsync(StaffTripQuery? query, CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAsync(
            cancellationToken: cancellationToken,
            includes: _items);
        return await list.ToListAsync(cancellationToken);
    }

    public async Task<Staff_Trip?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetSingleAsync(c => c.Id == id, cancellationToken, _items);
        return entity;
    }


    public async Task<PaginatedList<Staff_Trip>> GetPaginatedAsync(StaffTripQuery query, Expression<Func<Staff_Trip, object>>? orderBy = null, CancellationToken cancellationToken = default)
    {
        var list = await _repository
    .GetAsync(cancellationToken: cancellationToken);
        list = orderBy != null ?
            list.OrderBy(orderBy) :
            list.OrderBy(x => x.CreatedTime);
        var result = await list.PaginatedListAsync(query);
        return result;
    }

    public async Task<int> UpdateAsync(string id, StaffTripModel model, CancellationToken cancellationToken = default)
    {
        var numberOfRows = await _repository.UpdateAsync(x => x.Id == id,
            x => x
            .SetProperty(x => x.TripCollect, model.TripCollect)
            .SetProperty(x => x.TripType, model.TripType)
            .SetProperty(x => x.TimeScheduleID, model.TimeScheduleID)
            .SetProperty(x => x.BuildingID, model.BuildingID)
            .SetProperty(x => x.StaffID, model.StaffID)
            , cancellationToken);

        return numberOfRows;
    }

}