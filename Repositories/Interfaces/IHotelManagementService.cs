using API_Hotels.Models;
using API_Hotels.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_Hotels.Repositories.Interfaces
{
    public interface IHotelManagementService
    {
        // Hotel Management
        Task<bool> ToggleHotelStatus(Guid hotelId);
        Task<Guid> CreateHotel(HotelCreateRequestInput hotel);
        Task UpdateHotel(Guid hotelId, HotelUpdateRequestInput request);
        Task<List<Hotels>> GetHotels();
        Task<Hotels?> GetHotelById(Guid hotelId);

        // Room Management
        Task<Guid> AddRoomToHotel(Guid hotelId, AddRoomRequestInput roomData);
        Task UpdateRoom(Guid roomId, UpdateRoomRequestInput roomData);
        Task<bool> ToggleRoomStatus(Guid roomId);
        Task<List<Rooms>> GetRoomsByHotel(Guid hotelId);

        // Reservation Management
        Task<List<Hotels>> SearchHotels(string? city, DateTime checkInDate, DateTime checkOutDate, int numGuests);
        Task<Guid> CreateReservation(CreateReservationRequestInput reservationData);
        Task<List<Reservations>> GetReservationsByHotel(Guid hotelId);
        Task<Reservations?> GetReservationDetails(Guid reservationId);

        // Guest Management
        Task AddGuestsToReservation(Guid reservationId, int numberOfAdditionalGuests);
        Task AddEmergencyContact(Guid reservationId, AddEmergencyContactRequestInput emergencyContactRequest);
        Task<List<Guests>> GetGuestsByReservation(Guid reservationId);
    }
}
