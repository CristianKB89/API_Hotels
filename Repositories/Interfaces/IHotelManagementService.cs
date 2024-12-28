using API_Hotels.Models;
using API_Hotels.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_Hotels.Repositories.Interfaces
{
    public interface IHotelManagementService
    {
        /// Hotel Management Service
        Task<bool> ToggleHotelStatus(Guid hotelId);
        Task<Guid> CreateHotel(HotelCreateRequestInput hotel);
        Task UpdateHotel(Guid hotelId, HotelUpdateRequestInput request);
        Task<List<Hotels>> GetHotels();
        Task<Hotels> GetHotelById(Guid hotelId);

        /// Room Management Service
        Task<Guid> AddRoomToHotel(Guid hotelId, AddRoomRequestInput roomData);
        Task UpdateRoom(Guid roomId, UpdateRoomRequestInput roomData);
        Task<bool> ToggleRoomStatus(Guid roomId);
        Task<List<Rooms>> GetRoomsByHotel(Guid hotelId);
    }
}
