using API_Hotels.DataContext;
using API_Hotels.Models;
using API_Hotels.Models.Inputs;
using API_Hotels.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace API_Hotels.Repositories
{
    public class HotelManagementService : IHotelManagementService
    {
        private readonly DapperContext _context;

        public HotelManagementService(DapperContext dapperContext)
        {
            _context = dapperContext;
        }

        public async Task<Guid> AddRoomToHotel(Guid hotelId, AddRoomRequestInput roomData)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"INSERT INTO Rooms 
                                            (RoomId, HotelId, RoomType, BaseCost, TaxPercentage, Status, CreatedAt)
                                       VALUES (@RoomId, @HotelId, @RoomType, @BaseCost, @TaxPercentage, @Status, @CreatedAt);";

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", Guid.NewGuid(), DbType.Guid);
                parameters.Add("@HotelId", hotelId, DbType.Guid);
                parameters.Add("@RoomType", roomData.RoomType, DbType.String);
                parameters.Add("@BaseCost", roomData.BaseCost, DbType.Decimal);
                parameters.Add("@TaxPercentage", roomData.TaxPercentage, DbType.Decimal);
                parameters.Add("@Status", roomData.Status, DbType.Boolean);
                parameters.Add("@CreatedAt", DateTime.UtcNow, DbType.DateTime);

                await db.ExecuteAsync(query, parameters);

                return parameters.Get<Guid>("@RoomId");

            }
            catch (Exception ex)
            {
                throw new Exception("Error adding room to hotel", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<Guid> CreateHotel(HotelCreateRequestInput hotel)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"INSERT INTO Hotels 
                                        (HotelId, Name, Location, BasePrice, Status, CreatedAt)
                                    VALUES (@HotelId, @Name, @Location, @BasePrice, @Status, @CreatedAt);";

                var parameters = new DynamicParameters();
                parameters.Add("@HotelId", Guid.NewGuid(), DbType.Guid);
                parameters.Add("@Name", hotel.Name, DbType.String);
                parameters.Add("@Location", hotel.Location, DbType.String);
                parameters.Add("@BasePrice", hotel.BasePrice, DbType.Decimal);
                parameters.Add("@Status", true, DbType.Boolean);
                parameters.Add("@CreatedAt", DateTime.UtcNow, DbType.DateTime);

                await db.ExecuteAsync(query, parameters);

                return parameters.Get<Guid>("@HotelId");
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating hotel", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<Hotels> GetHotelById(Guid hotelId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT HotelId, Name, Location, BasePrice, Status, CreatedAt, UpdatedAt
                                    FROM Hotels
                                  WHERE HotelId = @HotelId;";

                var parameters = new DynamicParameters();
                parameters.Add("@HotelId", hotelId, DbType.Guid);

                return await db.QuerySingleOrDefaultAsync<Hotels>(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving hotel with ID: {hotelId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<List<Hotels>> GetHotels()
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT HotelId, Name, Location, BasePrice, Status, CreatedAt, UpdatedAt
                                    FROM Hotels";


                var hotels = await db.QueryAsync<Hotels>(query);
                return hotels.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving hotels", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<List<Rooms>> GetRoomsByHotel(Guid hotelId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();
                const string query = @"SELECT RoomId, HotelId, RoomType, BaseCost, TaxPercentage, Status, CreatedAt, UpdatedAt
                                    FROM Rooms
                                    WHERE HotelId = @HotelId;";

                var parameters = new DynamicParameters();
                parameters.Add("@HotelId", hotelId, DbType.Guid);
                var rooms = await db.QueryAsync<Rooms>(query, parameters);
                return rooms.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving rooms for hotel with ID: {hotelId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<bool> ToggleHotelStatus(Guid hotelId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string updateQuery = @"UPDATE Hotels
                                        SET Status = ~Status,
                                            UpdatedAt = @UpdatedAt
                                        WHERE HotelId = @HotelId;";

                const string selectQuery = @"SELECT Status 
                                            FROM Hotels 
                                        WHERE HotelId = @HotelId;";

                var parameters = new DynamicParameters();
                parameters.Add("@HotelId", hotelId, DbType.Guid);
                parameters.Add("@UpdatedAt", DateTime.UtcNow, DbType.DateTime);


                await db.ExecuteAsync(updateQuery, parameters);
                return await db.QuerySingleOrDefaultAsync<bool>(selectQuery, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling hotel status for HotelId: {hotelId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<bool> ToggleRoomStatus(Guid roomId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                const string updateQuery = @"UPDATE Rooms
                                SET Status = ~Status, -- Toggles the boolean status (1 -> 0 or 0 -> 1)
                                    UpdatedAt = @UpdatedAt
                                WHERE RoomId = @RoomId;";

                const string selectQuery = @"SELECT Status 
                                            FROM Rooms 
                                            WHERE RoomId = @RoomId;";

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", roomId, DbType.Guid);
                parameters.Add("@UpdatedAt", DateTime.UtcNow, DbType.DateTime);

                await db.ExecuteAsync(updateQuery, parameters);
                return await db.QuerySingleOrDefaultAsync<bool>(selectQuery, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling status for room with ID: {roomId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task UpdateHotel(Guid hotelId, HotelUpdateRequestInput request)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"UPDATE Hotels
                                    SET Name = @Name,
                                        Location = @Location,
                                        BasePrice = @BasePrice,
                                        Status = @Status,
                                        UpdatedAt = @UpdatedAt
                                    WHERE HotelId = @HotelId;";

                var parameters = new DynamicParameters();
                parameters.Add("@HotelId", hotelId, DbType.Guid);
                parameters.Add("@Name", request.Name, DbType.String);
                parameters.Add("@Location", request.Location, DbType.String);
                parameters.Add("@BasePrice", request.BasePrice, DbType.Decimal);
                parameters.Add("@Status", request.Status, DbType.Boolean);
                parameters.Add("@UpdatedAt", DateTime.UtcNow, DbType.DateTime);


                await db.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating hotel with ID: {hotelId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task UpdateRoom(Guid roomId, UpdateRoomRequestInput roomData)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"
                                UPDATE Rooms
                                SET RoomType = @RoomType,
                                    BaseCost = @BaseCost,
                                    TaxPercentage = @TaxPercentage,
                                    Status = @Status,
                                    UpdatedAt = @UpdatedAt
                                WHERE RoomId = @RoomId;
                            ";

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", roomId, DbType.Guid);
                parameters.Add("@RoomType", roomData.RoomType, DbType.String);
                parameters.Add("@BaseCost", roomData.BaseCost, DbType.Decimal);
                parameters.Add("@TaxPercentage", roomData.TaxPercentage, DbType.Decimal);
                parameters.Add("@Status", roomData.Status, DbType.Boolean);
                parameters.Add("@UpdatedAt", DateTime.UtcNow, DbType.DateTime);

                var affectedRows = await db.ExecuteAsync(query, parameters);

                if (affectedRows == 0)
                {
                    throw new Exception($"No room found with ID: {roomId}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating room with ID: {roomId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }
    }
}
