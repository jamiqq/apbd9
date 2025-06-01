using Microsoft.EntityFrameworkCore;
using TripApp.Application.Repository;
using TripApp.Core.Models;

namespace TripApp.Infrastructure.Repository;

public class TripRepository(TripContext tripDbContext) : ITripRepository
{
    public async Task<PaginatedResult<Core.Models.Trip>> GetPaginatedTripsAsync(int page = 1, int pageSize = 10)
    {
        var tripsQuery = tripDbContext.Trips
            .Include(e => e.ClientTrips).ThenInclude(e => e.IdClientNavigation)
            .Include(e => e.IdCountries)
            .OrderByDescending(e => e.DateFrom);

        var tripsCount = await tripsQuery.CountAsync();
        var totalPages = tripsCount / pageSize;
        var trips = await tripsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Core.Models.Trip>
        {
            PageSize = pageSize,
            PageNum = page,
            AllPages = totalPages,
            Data = trips
        };
    }

    public async Task<List<Core.Models.Trip>> GetAllTripsAsync()
    {
        return await tripDbContext.Trips
            .Include(e => e.ClientTrips).ThenInclude(e => e.IdClientNavigation)
            .Include(e => e.IdCountries)
            .OrderBy(e => e.DateFrom)
            .ToListAsync();
    }

    public async Task<bool> TripExistsAndFutureAsync(int idTrip)
    {
        return await tripDbContext.Trips.AnyAsync(e => e.IdTrip == idTrip && e.DateFrom > DateTime.Today);
    }

    public async Task<bool> ClientExistsAsync(string pesel)
    {
        return await tripDbContext.Clients.AnyAsync(e => e.Pesel == pesel);
    }

    public async Task<bool> ClientAlreadyInTripAsync(string pesel, int tripId)
    {
        return await tripDbContext.Clients
            .Where(c=>c.Pesel == pesel)
            .SelectMany(c=>c.ClientTrips)
            .AnyAsync(ct=>ct.IdTrip == tripId);
    }

    public async Task<int> AddClientAsync(Client client)
    {
        tripDbContext.Clients.Add(client);
        await tripDbContext.SaveChangesAsync();
        return client.IdClient;
    }

    public async Task<Client> GetClientByPeselAsync(string pesel)
    {
        return await tripDbContext.Clients.FirstOrDefaultAsync(c => c.Pesel == pesel);
    }

    public async Task AssignClientToTripAsync(int idClient, int tripId, DateTime registeredAt, DateTime? paymentDate)
    {
        var clientTrip = new ClientTrip
        {
            IdClient = idClient,
            IdTrip = tripId,
            RegisteredAt = registeredAt,
            PaymentDate = paymentDate
        };
        tripDbContext.ClientTrips.Add(clientTrip);
        await tripDbContext.SaveChangesAsync();
    }
}