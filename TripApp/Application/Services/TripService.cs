using TripApp.Application.DTOs;
using TripApp.Application.Mappers;
using TripApp.Application.Repository;
using TripApp.Application.Services.Interfaces;
using TripApp.Core.Models;

namespace TripApp.Application.Services;

public class TripService(ITripRepository tripRepository) : ITripService
{
    public async Task<PaginatedResult<GetTripDto>> GetPaginatedTripsAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 10) pageSize = 10;
        var result = await tripRepository.GetPaginatedTripsAsync(page, pageSize);

        var mappedTrips = new PaginatedResult<GetTripDto>
        {
            AllPages = result.AllPages,
            Data = result.Data.Select(trip => trip.MapToGetTripDto()).ToList(),
            PageNum = result.PageNum,
            PageSize = result.PageSize
        };

        return mappedTrips;
    }

    public async Task<List<GetTripDto>> GetAllTripsAsync()
    {
        var trips = await tripRepository.GetAllTripsAsync();
        var mappedTrips = trips.Select(trip => trip.MapToGetTripDto()).ToList();
        return mappedTrips;
    }

    public async Task AssignClientToTripAsync(AssighClientToDTO dto)
    {
        if (dto.TripId <= 0)
        {
            throw new Exception("Invalid trip id");
        }
        if (!await tripRepository.TripExistsAndFutureAsync(dto.TripId))
        {
            throw new Exception("Trip not found or already started");
        }

        if (await tripRepository.ClientAlreadyInTripAsync(dto.Pesel, dto.TripId))
        {
            throw new Exception("Client already in trip");
        }

        if (await tripRepository.ClientExistsAsync(dto.Pesel))
        {
            throw new Exception("Client already exists");
        }

        var client = new Client
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Pesel = dto.Pesel
        };
        
        var clientId = await tripRepository.AddClientAsync(client);
        
        await tripRepository.AssignClientToTripAsync(clientId, dto.TripId, DateTime.Now, dto.PaymentDate);
    }
}