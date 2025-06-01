using Microsoft.AspNetCore.Mvc;
using TripApp.Application.DTOs;
using TripApp.Application.Services.Interfaces;
using TripApp.Core.Models;

namespace TripApp.Presentation.Controllers;

[ApiController]
[Route("api/trips")]
public class TripController(
    ITripService tripService) 
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<GetTripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<GetTripDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrips(
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page is null && pageSize is null)
        {
            var trips = await tripService.GetAllTripsAsync();
            return Ok(trips);
        }

        var paginatedTrips = await tripService.GetPaginatedTripsAsync(page ?? 1, pageSize ?? 10);
        return Ok(paginatedTrips);
    }

    [HttpPost("/{idTrip}/clients")]
    public async Task<IActionResult> AsignClient(int idTrip, [FromBody] AssighClientToDTO dto)
    {
        try
        {
            if (idTrip != dto.TripId)
            {
                return BadRequest("Invalid id");
            }
            await tripService.AssignClientToTripAsync(dto);
            return Ok("Client assigned");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}