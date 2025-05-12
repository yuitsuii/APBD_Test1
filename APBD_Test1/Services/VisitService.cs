using System.Data.Common;
using APBD_Test1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_Test1.Services;

public class VisitService : IVisitService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD_test1;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    public async Task<VisitRequestDTO> GetVisit(int visitId)
    {
        var query = @"
        SELECT v.date, c.first_name, c.last_name, c.date_of_birth, m.mechanic_id, m.licence_number,s.name, vs.service_fee FROM Visit v 
        JOIN Client c ON c.client_id = v.client_id JOIN Mechanic m ON m.mechanic_id = v.mechanic_id 
        JOIN Visit_Service vs ON vs.visit_id = v.visit_id JOIN Service s ON s.service_id = vs.service_id WHERE v.visit_id = @visitId";

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@visitId", visitId);

        try
        {
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            VisitRequestDTO? visit = null;
            while (await reader.ReadAsync())
            {
                visit ??= new VisitRequestDTO
                {
                    Date = reader.GetDateTime(0),
                    Client = new ClientDTO
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Mechanic = new MechanicDTO
                    {
                        MechanicId = reader.GetInt32(4),
                        LicenceNumber = reader.IsDBNull(5) ? null : reader.GetString(5)
                    },
                    Visits = new List<VisitServiceDTO>()
                };

                visit.Visits.Add(new VisitServiceDTO
                {
                    Name = reader.GetString(6),
                    ServiceFee = reader.GetDecimal(7)  
                });
            }

            return visit ?? throw new KeyNotFoundException($"Visit {visitId} not found.");
        }
        catch (SqlException ex)
        {
     
            Console.WriteLine($"SQL Error: {ex.Message}");
            throw new Exception("Database error occurred");
        }

    }


    public async Task<VisitPostDTO> PostVisit(VisitPostDTO visit)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            var visitCheckQuery = @"SELECT 1 FROM Visit WHERE visit_id = @visit_id";
            await using (var visitCheckCommand = new SqlCommand(visitCheckQuery, connection, (SqlTransaction)transaction))
            {
                visitCheckCommand.Parameters.AddWithValue("@visit_id", visit.VisitId);
                var visitExists = await visitCheckCommand.ExecuteScalarAsync();
                if (visitExists != null)
                    throw new InvalidOperationException("Visit with this ID already exists");
            }

            var clientCheckQuery = @"SELECT 1 FROM Client WHERE client_id = @client_id";
            await using (var clientCheckCommand = new SqlCommand(clientCheckQuery, connection, (SqlTransaction)transaction))
            {
                clientCheckCommand.Parameters.AddWithValue("@client_id", visit.ClientId);
                var clientExists = await clientCheckCommand.ExecuteScalarAsync();
                if (clientExists == null)
                    throw new KeyNotFoundException("Client not found");
            }

            var mechanicQuery = @"SELECT mechanic_id FROM Mechanic WHERE licence_number = @licence_number";
            int? mechanicId;
            await using (var mechanicCommand = new SqlCommand(mechanicQuery, connection, (SqlTransaction)transaction))
            {
                mechanicCommand.Parameters.AddWithValue("@licence_number", visit.LicenceNumber);
                mechanicId = await mechanicCommand.ExecuteScalarAsync() as int?;
                if (mechanicId == null)
                    throw new KeyNotFoundException("Mechanic not found");
            }

            var insertVisitQuery = @"INSERT INTO Visit (visit_id, client_id, mechanic_id, date) 
                                     VALUES (@visit_id, @client_id, @mechanic_id, GETDATE())";
            await using (var insertCommand = new SqlCommand(insertVisitQuery, connection, (SqlTransaction)transaction))
            {
                insertCommand.Parameters.AddWithValue("@visit_id", visit.VisitId);
                insertCommand.Parameters.AddWithValue("@client_id", visit.ClientId);
                insertCommand.Parameters.AddWithValue("@mechanic_id", mechanicId.Value);
                await insertCommand.ExecuteNonQueryAsync();
            }

            foreach (var service in visit.VisitServices)
            {
                var serviceCheckQuery = @"SELECT 1 FROM Service WHERE name = @service_name";
                await using (var serviceCheckCommand = new SqlCommand(serviceCheckQuery, connection, (SqlTransaction)transaction))
                {
                    serviceCheckCommand.Parameters.AddWithValue("@service_name", service.Name);
                    var serviceExists = await serviceCheckCommand.ExecuteScalarAsync();
                    if (serviceExists == null)
                        throw new KeyNotFoundException($"Service '{service.Name}' not found");
                }

                if (service.Fee <= 0)
                    throw new ArgumentException($"Invalid fee for service '{service.Name}'");

                var serviceIdQuery = @"SELECT service_id FROM Service WHERE name = @service_name";
                object serviceId;
                await using (var serviceIdCommand = new SqlCommand(serviceIdQuery, connection, (SqlTransaction)transaction))
                {
                    serviceIdCommand.Parameters.AddWithValue("@service_name", service.Name);
                    serviceId = await serviceIdCommand.ExecuteScalarAsync();
                }
                var insertServiceQuery = @"INSERT INTO Visit_Service 
                                     (visit_id, service_id, service_fee) 
                                     VALUES (@visit_id, @service_id, @service_fee)";
                await using (var insertServiceCommand = new SqlCommand(insertServiceQuery, connection, (SqlTransaction)transaction))
                {
                    insertServiceCommand.Parameters.AddWithValue("@visit_id", visit.VisitId);
                    insertServiceCommand.Parameters.AddWithValue("@service_id", serviceId);
                    insertServiceCommand.Parameters.AddWithValue("@service_fee", service.Fee);
                    await insertServiceCommand.ExecuteNonQueryAsync();
                }
            }
            await transaction.CommitAsync();
        return visit;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error creating visit: {ex.Message}");
            throw;
        }
    }

    
            
    
    
}