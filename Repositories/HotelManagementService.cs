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

        public async Task AddEmergencyContact(Guid reservationId, AddEmergencyContactRequestInput emergencyContactRequest)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string queryGetGuestId = @"SELECT TOP 1 GuestId
                                             FROM Guests
                                             WHERE ReservationId = @ReservationId;"
                ;

                var guestId = await db.ExecuteScalarAsync<Guid?>(queryGetGuestId, new { ReservationId = reservationId });

                if (guestId == null)
                {
                    throw new Exception($"No guest found for ReservationId: {reservationId}");
                }

                const string queryInsertEmergencyContact = @"INSERT INTO EmergencyContacts (ContactId, GuestId, FullName, Phone, Relationship)
                                                          VALUES (@ContactId, @GuestId, @FullName, @Phone, @Relationship);";

                var insertParameters = new DynamicParameters();
                insertParameters.Add("@ContactId", Guid.NewGuid(), DbType.Guid);
                insertParameters.Add("@GuestId", guestId, DbType.Guid);
                insertParameters.Add("@FullName", emergencyContactRequest.FullName, DbType.String);
                insertParameters.Add("@Phone", emergencyContactRequest.Phone, DbType.String);
                insertParameters.Add("@Relationship", emergencyContactRequest.Relationship, DbType.String);

                await db.ExecuteAsync(queryInsertEmergencyContact, insertParameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding emergency contact for reservation with ID: {reservationId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task AddGuestsToReservation(Guid reservationId, int numberOfAdditionalGuests)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string queryCheckReservation = @"SELECT TotalGuests
                                                        FROM Reservations
                                                       WHERE ReservationId = @ReservationId;";

                const string queryUpdateGuests = @"UPDATE Reservations
                                                    SET TotalGuests = TotalGuests + @NumberOfAdditionalGuests
                                                   WHERE ReservationId = @ReservationId;";

                var parameters = new DynamicParameters();
                parameters.Add("@ReservationId", reservationId, DbType.Guid);
                parameters.Add("@NumberOfAdditionalGuests", numberOfAdditionalGuests, DbType.Int32);

                /// Verificar si la reserva existe
                int? currentNumberOfGuests = await db.ExecuteScalarAsync<int?>(queryCheckReservation, parameters);
                if (currentNumberOfGuests == null)
                {
                    throw new Exception($"Reservation with ID: {reservationId} not found.");
                }

                /// Actualizar el número de huéspedes
                await db.ExecuteAsync(queryUpdateGuests, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding guests to reservation with ID: {reservationId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
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

        public async Task CreateReservation(CreateReservationRequestInput reservationData)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string checkRoomQuery = @" SELECT COUNT(1)
                                                 FROM Rooms r
                                                 WHERE r.RoomId = @RoomId
                                                    AND r.HotelId = @HotelId
                                                    AND r.Status = 1;";

                const string checkAvailabilityQuery = @"SELECT COUNT(1)
                                                FROM Reservations res
                                                WHERE res.RoomId = @RoomId
                                                    AND NOT (@CheckOutDate <= res.CheckInDate OR @CheckInDate >= res.CheckOutDate);";

                const string createReservationQuery = @"INSERT INTO Reservations (ReservationId, RoomId, CheckInDate, CheckOutDate, TotalGuests, CreatedAt, TotalCost, EmailNotification)
                                                        VALUES (@ReservationId, @RoomId, @CheckInDate, @CheckOutDate, @TotalGuests, @CreatedAt, @TotalCost, @EmailNotification);";

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", reservationData.RoomId, DbType.Guid);
                parameters.Add("@HotelId", reservationData.HotelId, DbType.Guid);
                parameters.Add("@CheckInDate", reservationData.CheckInDate, DbType.DateTime);
                parameters.Add("@CheckOutDate", reservationData.CheckOutDate, DbType.DateTime);

                /// Verificar que la habitación pertenece al hotel y está activa
                int roomExists = await db.ExecuteScalarAsync<int>(checkRoomQuery, parameters);
                if (roomExists == 0)
                {
                    throw new Exception("The specified room does not exist or does not belong to the specified hotel.");
                }

                /// Verificar disponibilidad de la habitación en las fechas especificadas
                int conflicts = await db.ExecuteScalarAsync<int>(checkAvailabilityQuery, parameters);
                if (conflicts > 0)
                {
                    throw new Exception("The room is not available for the selected dates.");
                }

                /// Crear la reserva
                parameters.Add("@ReservationId", Guid.NewGuid(), DbType.Guid);
                parameters.Add("@GuestName", reservationData.GuestName, DbType.String);
                parameters.Add("@TotalGuests", reservationData.NumberOfGuests, DbType.Int32);
                parameters.Add("@TotalCost", 500, DbType.Decimal);
                parameters.Add("@EmailNotification", true, DbType.Boolean);
                parameters.Add("@CreatedAt", DateTime.UtcNow, DbType.DateTime);

                await db.ExecuteAsync(createReservationQuery, parameters);

            }
            catch (Exception ex)
            {
                throw new Exception("Error creating reservation", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<List<Guests>> GetGuestsByReservation(Guid reservationId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT g.GuestId, g.ReservationId, g.FullName, g.DateOfBirth, g.Gender, g.DocumentType, g.DocumentNumber, g.Email, g.Phone
                                            FROM Guests g
                                            WHERE g.ReservationId = @ReservationId;";

                var parameters = new DynamicParameters();
                parameters.Add("@ReservationId", reservationId, DbType.Guid);

                var guests = await db.QueryAsync<Guests>(query, parameters);
                return guests.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving guests for reservation with ID: {reservationId}", ex);
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

        public async Task<Reservations> GetReservationDetails(Guid reservationId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT r.ReservationId, r.RoomId, r.CheckInDate, r.CheckOutDate, r.TotalGuests, r.TotalCost, r.EmailNotification, r.CreatedAt
                                            FROM Reservations r
                                            WHERE r.ReservationId = @ReservationId;";

                var parameters = new DynamicParameters();
                parameters.Add("@ReservationId", reservationId, DbType.Guid);

                return await db.QuerySingleOrDefaultAsync<Reservations>(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving details for reservation with ID: {reservationId}", ex);
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
        }

        public async Task<List<Reservations>> GetReservationsByHotel(Guid hotelId)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT r.ReservationId, r.RoomId, r.CheckInDate, r.CheckOutDate, r.TotalGuests, r.TotalCost, r.EmailNotification, r.CreatedAt
                                                FROM Reservations r
                                                INNER JOIN Rooms rm ON r.RoomId = rm.RoomId
                                                WHERE rm.HotelId = @HotelId
                                                ORDER BY r.CheckInDate;";

                var parameters = new DynamicParameters();
                parameters.Add("@HotelId", hotelId, DbType.Guid);

                var reservations = await db.QueryAsync<Reservations>(query, parameters);
                return reservations.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reservations for hotel with ID: {hotelId}", ex);
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

        public async Task<List<Hotels>> SearchHotels(string city, DateTime checkInDate, DateTime checkOutDate, int numGuests)
        {
            using IDbConnection db = _context.CreateConnection();
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT h.HotelId, h.Name, h.Location, h.BasePrice, h.Status, h.CreatedAt, h.UpdatedAt
                                        FROM Hotels h
                                        WHERE (@City IS NULL OR h.Location = @City)
                                          AND h.Status = 1
                                          AND EXISTS (
                                              SELECT 1
                                              FROM Rooms r
                                              WHERE r.HotelId = h.HotelId
                                                AND r.Status = 1
                                          )
                                        ORDER BY h.Name;";

                var parameters = new DynamicParameters();
                parameters.Add("@City", string.IsNullOrWhiteSpace(city) ? null : city, DbType.String);
                parameters.Add("@CheckInDate", checkInDate, DbType.DateTime);
                parameters.Add("@CheckOutDate", checkOutDate, DbType.DateTime);
                parameters.Add("@NumGuests", numGuests, DbType.Int32);

                var hotels = await db.QueryAsync<Hotels>(query, parameters);
                return hotels.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching for hotels", ex);
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
                if (db.State == ConnectionState.Closed)
                    db.Open();

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
