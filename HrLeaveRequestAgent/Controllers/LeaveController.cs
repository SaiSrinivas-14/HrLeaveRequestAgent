using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HrLeaveRequestAgent.Services;

namespace HrLeaveRequestAgent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet("balance/{employeeId}")]
        public async Task<IActionResult> GetLeaveBalance(int employeeId)
        {
            var balance = await _leaveService.GetLeaveBalanceAsync(employeeId);
            if (balance == null) return NotFound("Leave balance not found.");
            return Ok(balance);
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestDto dto)
        {
            var request = await _leaveService.CreateLeaveRequestAsync(dto.EmployeeId, dto.StartDate, dto.EndDate);
            return Ok(request);
        }
    }

    public class LeaveRequestDto
    {
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
