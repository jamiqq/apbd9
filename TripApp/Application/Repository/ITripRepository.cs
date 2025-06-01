using TripApp.Core.Models;

namespace TripApp.Application.Repository;

public interface ITripRepository
{
    Task<PaginatedResult<Core.Models.Trip>> GetPaginatedTripsAsync(int page = 1, int pageSize = 10);
    Task<List<Core.Models.Trip>> GetAllTripsAsync();
    Task<bool> TripExistsAndFutureAsync(int tripId);
    Task<bool> ClientExistsAsync(string pesel);
    Task<bool> ClientAlreadyInTripAsync(string pesel, int tripId);
    Task<int> AddClientAsync(Client client);
    Task<Client> GetClientByPeselAsync(string pesel);
    Task AssignClientToTripAsync(int clientId, int tripId, DateTime registeredAt, DateTime? paymentDate);
}