using API_Hotels.DataContext;
using API_Hotels.Models;
using API_Hotels.Models.Inputs;
using API_Hotels.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();
                const string queryGetGuestId = @"SELECT GuestId 
                                         FROM Guests 
                                         WHERE ReservationId = @ReservationId 
                                         LIMIT 1;";

                var guestId = await db.ExecuteScalarAsync<Guid?>(queryGetGuestId, new { ReservationId = reservationId });

                if (guestId == null)
                {
                    throw new Exception($"No guest found for ReservationId: {reservationId}");
                }

                const string queryInsertEmergencyContact = @"INSERT INTO EmergencyContacts (ContactId, GuestId, FullName, Phone, Relationship) 
                                                     VALUES (@ContactId, @GuestId, @FullName, @Phone, @Relationship);";

                var insertParameters = new DynamicParameters(new
                {
                    ContactId = Guid.NewGuid(),
                    GuestId = guestId,
                    FullName = emergencyContactRequest.FullName,
                    Phone = emergencyContactRequest.Phone,
                    Relationship = emergencyContactRequest.Relationship
                });

                await db.ExecuteAsync(queryInsertEmergencyContact, insertParameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding emergency contact for reservation with ID: {reservationId}: {ex.Message}", ex);
            }
        }


        public async Task AddGuestsToReservation(Guid reservationId, int numberOfAdditionalGuests)
        {
            using var db = _context.CreateConnection();

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

                var parameters = new DynamicParameters(new
                {
                    ReservationId = reservationId,
                    NumberOfAdditionalGuests = numberOfAdditionalGuests
                });

                int? currentNumberOfGuests = await db.ExecuteScalarAsync<int?>(queryCheckReservation, parameters);
                if (currentNumberOfGuests == null)
                {
                    throw new KeyNotFoundException($"Reservation with ID: {reservationId} not found.");
                }

                await db.ExecuteAsync(queryUpdateGuests, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding guests to reservation with ID: {reservationId}: {ex.Message}", ex);
            }
        }


        public async Task<Guid> AddRoomToHotel(Guid hotelId, AddRoomRequestInput roomData)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"INSERT INTO Rooms 
                               (RoomId, HotelId, RoomType, BaseCost, TaxPercentage, Status, CreatedAt) 
                               VALUES (@RoomId, @HotelId, @RoomType, @BaseCost, @TaxPercentage, @Status, @CreatedAt) 
                               RETURNING RoomId;";

                var roomId = Guid.NewGuid();

                var parameters = new
                {
                    RoomId = roomId,
                    HotelId = hotelId,
                    RoomType = roomData.RoomType,
                    BaseCost = roomData.BaseCost,
                    TaxPercentage = roomData.TaxPercentage,
                    Status = roomData.Status,
                    CreatedAt = DateTime.UtcNow
                };

                var insertedRoomId = await db.ExecuteScalarAsync<Guid>(query, parameters);

                return insertedRoomId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding room to hotel with ID: {hotelId}: {ex.Message}", ex);
            }
        }


        public async Task<Guid> CreateHotel(HotelCreateRequestInput hotel)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"INSERT INTO Hotels 
                               (HotelId, Name, Location, BasePrice, Status, CreatedAt) 
                               VALUES (@HotelId, @Name, @Location, @BasePrice, @Status, @CreatedAt) 
                               RETURNING HotelId;"; // 🔹 Retorna el ID insertado

                var hotelId = Guid.NewGuid();

                var parameters = new
                {
                    HotelId = hotelId,
                    Name = hotel.Name,
                    Location = hotel.Location,
                    BasePrice = hotel.BasePrice,
                    Status = true, // Siempre activo al crear
                    CreatedAt = DateTime.UtcNow
                };

                var insertedHotelId = await db.ExecuteScalarAsync<Guid>(query, parameters);
                return insertedHotelId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating hotel", ex);
            }
        }


        public async Task<Guid> CreateReservation(CreateReservationRequestInput reservationData)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string checkRoomQuery = @"SELECT COUNT(1) 
                                        FROM Rooms r 
                                        WHERE r.RoomId = @RoomId 
                                            AND r.HotelId = @HotelId 
                                            AND r.Status = TRUE;";

                const string checkAvailabilityQuery = @"SELECT COUNT(1) 
                                                FROM Reservations res 
                                                WHERE res.RoomId = @RoomId 
                                                    AND NOT (@CheckOutDate <= res.CheckInDate 
                                                             OR @CheckInDate >= res.CheckOutDate);";

                const string createReservationQuery = @"INSERT INTO Reservations 
                                                (ReservationId, RoomId, CheckInDate, CheckOutDate, TotalGuests, CreatedAt, TotalCost, EmailNotification) 
                                                VALUES (@ReservationId, @RoomId, @CheckInDate, @CheckOutDate, @TotalGuests, @CreatedAt, @TotalCost, @EmailNotification) 
                                                RETURNING ReservationId;"; // 🔹 Retorna el ID insertado

                var reservationId = Guid.NewGuid();

                var parameters = new
                {
                    RoomId = reservationData.RoomId,
                    HotelId = reservationData.HotelId,
                    CheckInDate = reservationData.CheckInDate,
                    CheckOutDate = reservationData.CheckOutDate
                };

                // 🔹 Verificar que la habitación existe y pertenece al hotel
                int roomExists = await db.ExecuteScalarAsync<int>(checkRoomQuery, parameters);
                if (roomExists == 0)
                    throw new Exception("The specified room does not exist or does not belong to the specified hotel.");

                // 🔹 Verificar disponibilidad de la habitación en las fechas
                int conflicts = await db.ExecuteScalarAsync<int>(checkAvailabilityQuery, parameters);
                if (conflicts > 0)
                    throw new Exception("The room is not available for the selected dates.");

                // 🔹 Insertar la reserva
                var reservationParams = new
                {
                    ReservationId = reservationId,
                    RoomId = reservationData.RoomId,
                    CheckInDate = reservationData.CheckInDate,
                    CheckOutDate = reservationData.CheckOutDate,
                    TotalGuests = reservationData.NumberOfGuests,
                    TotalCost = 500, // 🔹 TODO: Calcular correctamente el costo
                    EmailNotification = true,
                    CreatedAt = DateTime.UtcNow
                };

                var insertedReservationId = await db.ExecuteScalarAsync<Guid>(createReservationQuery, reservationParams);
                return insertedReservationId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating reservation", ex);
            }
        }


        public async Task<List<Guests>> GetGuestsByReservation(Guid reservationId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT GuestId, ReservationId, FullName, DateOfBirth, Gender, DocumentType, DocumentNumber, Email, Phone 
                               FROM Guests 
                               WHERE ReservationId = @ReservationId;";

                var guests = await db.QueryAsync<Guests>(query, new { ReservationId = reservationId });

                return guests.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving guests for reservation with ID: {reservationId}", ex);
            }
        }


        public async Task<Hotels> GetHotelById(Guid hotelId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT HotelId, Name, Location, BasePrice, Status, CreatedAt, UpdatedAt
                               FROM Hotels
                               WHERE HotelId = @HotelId;";

                return await db.QuerySingleOrDefaultAsync<Hotels>(query, new { HotelId = hotelId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving hotel with ID: {hotelId}", ex);
            }
        }


        public async Task<List<Hotels>> GetHotels()
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT HotelId, Name, Location, BasePrice, Status, CreatedAt, UpdatedAt
                               FROM Hotels;";

                return (await db.QueryAsync<Hotels>(query)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving hotels", ex);
            }
        }


        public async Task<Reservations> GetReservationDetails(Guid reservationId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT r.ReservationId, r.RoomId, r.CheckInDate, r.CheckOutDate, 
                                      r.TotalGuests, r.TotalCost, r.EmailNotification, r.CreatedAt
                               FROM Reservations r
                               WHERE r.ReservationId = @ReservationId;";

                var parameters = new { ReservationId = reservationId };

                return await db.QuerySingleOrDefaultAsync<Reservations>(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reservation details for ID: {reservationId}", ex);
            }
        }


        public async Task<List<Reservations>> GetReservationsByHotel(Guid hotelId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT r.ReservationId, r.RoomId, r.CheckInDate, r.CheckOutDate, 
                                      r.TotalGuests, r.TotalCost, r.EmailNotification, r.CreatedAt
                               FROM Reservations r
                               INNER JOIN Rooms rm ON r.RoomId = rm.RoomId
                               WHERE rm.HotelId = @HotelId
                               ORDER BY r.CheckInDate;";

                var parameters = new { HotelId = hotelId };

                var reservations = await db.QueryAsync<Reservations>(query, parameters);
                return reservations.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reservations for hotel with ID: {hotelId}", ex);
            }
        }


        public async Task<List<Rooms>> GetRoomsByHotel(Guid hotelId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT RoomId, HotelId, RoomType, BaseCost, TaxPercentage, 
                                      Status, CreatedAt, UpdatedAt
                               FROM Rooms
                               WHERE HotelId = @HotelId;";

                var parameters = new { HotelId = hotelId };

                var rooms = await db.QueryAsync<Rooms>(query, parameters);
                return rooms.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving rooms for hotel with ID: {hotelId}", ex);
            }
        }


        public async Task<List<Hotels>> SearchHotels(string city, DateTime checkInDate, DateTime checkOutDate, int numGuests)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"SELECT DISTINCT h.HotelId, h.Name, h.Location, h.BasePrice, h.Status, h.CreatedAt, h.UpdatedAt
                                        FROM Hotels h
                                        INNER JOIN Rooms r ON r.HotelId = h.HotelId
                                        WHERE (@City IS NULL OR h.Location = @City)
                                          AND h.Status = 1
                                          AND r.Status = 1
                                          AND r.RoomId NOT IN (
                                              SELECT res.RoomId 
                                              FROM Reservations res 
                                              WHERE NOT (@CheckOutDate <= res.CheckInDate OR @CheckInDate >= res.CheckOutDate)
                                          )
                                          AND r.Capacity >= @NumGuests
                                        ORDER BY h.Name;";

                var parameters = new
                {
                    City = string.IsNullOrWhiteSpace(city) ? null : city,
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    NumGuests = numGuests
                };

                var hotels = await db.QueryAsync<Hotels>(query, parameters);
                return hotels.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching for hotels", ex);
            }
        }


        public async Task<bool> ToggleHotelStatus(Guid hotelId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"UPDATE Hotels
                        SET Status = CASE WHEN Status = 1 THEN 0 ELSE 1 END,
                            UpdatedAt = @UpdatedAt
                        OUTPUT INSERTED.Status
                        WHERE HotelId = @HotelId;";

                var parameters = new
                {
                    HotelId = hotelId,
                    UpdatedAt = DateTime.UtcNow
                };

                return await db.QuerySingleOrDefaultAsync<bool>(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling hotel status for HotelId: {hotelId}", ex);
            }
        }


        public async Task<bool> ToggleRoomStatus(Guid roomId)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"UPDATE Rooms
                            SET Status = NOT Status,
                                UpdatedAt = @UpdatedAt
                            WHERE RoomId = @RoomId
                            RETURNING Status;";

                var parameters = new
                {
                    RoomId = roomId,
                    UpdatedAt = DateTime.UtcNow
                };

                // Ejecuta el UPDATE y retorna el valor actualizado de Status
                return await db.QuerySingleOrDefaultAsync<bool>(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling status for room with ID: {roomId}", ex);
            }
        }


        public async Task UpdateHotel(Guid hotelId, HotelUpdateRequestInput request)
        {
            using var db = _context.CreateConnection();

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

                var parameters = new
                {
                    HotelId = hotelId,
                    Name = request.Name,
                    Location = request.Location,
                    BasePrice = request.BasePrice,
                    Status = request.Status,
                    UpdatedAt = DateTime.UtcNow
                };

                await db.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating hotel with ID: {hotelId}", ex);
            }
        }


        public async Task UpdateRoom(Guid roomId, UpdateRoomRequestInput roomData)
        {
            using var db = _context.CreateConnection();

            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                const string query = @"UPDATE Rooms
                                        SET RoomType = @RoomType,
                                            BaseCost = @BaseCost,
                                            TaxPercentage = @TaxPercentage,
                                            Status = @Status,
                                            UpdatedAt = @UpdatedAt
                                        WHERE RoomId = @RoomId;";

                var parameters = new
                {
                    RoomId = roomId,
                    RoomType = roomData.RoomType,
                    BaseCost = roomData.BaseCost,
                    TaxPercentage = roomData.TaxPercentage,
                    Status = roomData.Status,
                    UpdatedAt = DateTime.UtcNow
                };

                var affectedRows = await db.ExecuteAsync(query, parameters);

                if (affectedRows == 0)
                {
                    throw new KeyNotFoundException($"No room found with ID: {roomId}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating room with ID: {roomId}", ex);
            }
        }

    }
}
