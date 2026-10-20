using Core.DTOs.Data;
using Core.DTOs.Responses;
using Core.Exceptions;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TickrServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/sync")]
    public class SyncController(IHabitSyncService syncService) : Controller
    {
        private readonly IHabitSyncService _syncService = syncService;

        // current cloud state, used by the app to detect existing data at sign-in
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var state = await _syncService.GetStateAsync(userId);
            return Ok(Wrap(state));
        }

        // merge the device state into the cloud (normal background sync)
        [HttpPost]
        [RequestSizeLimit(1_000_000)]
        public async Task<IActionResult> Merge([FromBody] SyncStateDto request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var state = await _syncService.MergeAsync(userId, request);
                return Ok(Wrap(state));
            }
            catch (InvalidInputException ex)
            {
                return BadRequest(new ApiResponse<SyncStateDto> { Success = false, Message = ex.Message });
            }
        }

        // make the cloud exactly match the device ("keep this device's data")
        [HttpPut]
        [RequestSizeLimit(1_000_000)]
        public async Task<IActionResult> Replace([FromBody] SyncStateDto request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var state = await _syncService.ReplaceAsync(userId, request);
                return Ok(Wrap(state));
            }
            catch (InvalidInputException ex)
            {
                return BadRequest(new ApiResponse<SyncStateDto> { Success = false, Message = ex.Message });
            }
        }

        private static ApiResponse<SyncStateDto> Wrap(SyncStateDto state) => new()
        {
            Success = true,
            Details = state
        };

        private bool TryGetUserId(out int userId)
        {
            userId = 0;

            // password reset / email verification tokens are signed with the same key,
            // only a real authentication token may touch user data
            var tokenType = User.FindFirst("token_type")?.Value;
            if (tokenType != "authentication") return false;

            // depending on the JWT handler, "sub" is either kept or mapped to NameIdentifier
            var sub = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(sub, out userId);
        }
    }
}
