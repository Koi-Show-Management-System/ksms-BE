using KSMS.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly KoiShowManagementSystemContext _context;
        
        public TestController(KoiShowManagementSystemContext context)
        {
            _context = context;
        }
        
[HttpGet("test-connection")]
public async Task<IActionResult> TestConnection()
{
    try
    {
        bool canConnect = await _context.Database.CanConnectAsync();
        string version = null;
        
        if (canConnect)
        {
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT @@VERSION";
                version = (await command.ExecuteScalarAsync())?.ToString();
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }
        
        var connectionInfo = new
        {
            IsConnected = canConnect,
            DatabaseName = _context.Database.GetDbConnection().Database,
            ServerVersion = version,
            CurrentTime = DateTime.Now
        };
        
        return Ok(connectionInfo);
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            Error = ex.Message,
            InnerException = ex.InnerException?.Message,
            StackTrace = ex.StackTrace
        });
    }
}}
}