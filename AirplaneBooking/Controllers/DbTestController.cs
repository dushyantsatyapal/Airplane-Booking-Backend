using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // For StatusCodes
using System; // For Exception
using global::AirplaneBooking.Application.DTOs;
using global::AirplaneBooking.Application.Services;

namespace AirplaneBooking.API.Controllers;



[ApiController]
    [Route("api")] // This will make the route /api/DbTest
    public class DbTestController : ControllerBase
    {
        private readonly DbTestService _dbTestService;

        public DbTestController(DbTestService dbTestService)
        {
            _dbTestService = dbTestService ?? throw new ArgumentNullException(nameof(dbTestService));
        }

        /// <summary>
        /// Posts a sample data entry to both Firebase and MongoDB to test database connectivity.
        /// </summary>
        [HttpPost("test-write")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TestDbWrite([FromBody] TestDbEntryDto data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _dbTestService.TestDbConnectionAndWrite(data);
                // The service returns a string indicating success or specific errors
                if (result.StartsWith("Success"))
                {
                    return Ok(result);
                }
                else
                {
                    // If the service returns an error string, it implies a problem
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions during the controller or service call
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred during DB test: {ex.Message}");
            }
        }
    }
