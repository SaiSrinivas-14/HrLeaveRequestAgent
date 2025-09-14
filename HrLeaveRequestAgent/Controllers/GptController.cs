using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HrLeaveRequestAgent.Services;

namespace HrLeaveRequestAgent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GptController : ControllerBase
    {
        private readonly GptService _gptService;
        private readonly ILeaveService _leaveService;

        public GptController(GptService gptService, ILeaveService leaveService)
        {
            _gptService = gptService;
            _leaveService = leaveService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto dto)
        {
            int employeeId = 1; // Replace with authenticated user ID logic as needed

            string message = dto.Message?.Trim().ToLower() ?? "";

            if (message.Contains("leave from") || message.Contains("want leave from") || message.Contains("need leave from"))
            {
                DateTime? startDate = null;
                DateTime? endDate = null;

                // Attempt to parse dates from message in different formats
                var formats = new[] { "dd MMM yyyy", "d MMM yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
                var words = message.Split(' ');

                int fromIndex = Array.FindIndex(words, w => w == "from");
                int toIndex = Array.FindIndex(words, w => w == "to");

                if (fromIndex >= 0 && toIndex > fromIndex && toIndex < words.Length)
                {
                    // Join date words for parsing (some dates have spaces like "16 sep 2025")
                    var startDateStr = JoinDateParts(words, fromIndex + 1, toIndex - 1);
                    var endDateStr = JoinDateParts(words, toIndex + 1, words.Length - 1);

                    if (!TryParseDate(startDateStr, formats, out DateTime sd) ||
                        !TryParseDate(endDateStr, formats, out DateTime ed))
                    {
                        return Ok("Couldn't parse the dates. Please provide them in formats like '16 sep 2025', '16/09/2025', or '2025-09-16'.");
                    }

                    startDate = sd;
                    endDate = ed;

                    if (endDate < startDate)
                    {
                        return Ok("The end date cannot be earlier than the start date. Please check the dates and try again.");
                    }
                }
                else
                {
                    return Ok("Please specify the leave dates in the format 'leave from <start date> to <end date>'.");
                }

                var leaveBalance = await _leaveService.GetLeaveBalanceAsync(employeeId);
                int daysRequested = (endDate.Value - startDate.Value).Days + 1;

                if (leaveBalance == null)
                    return Ok("Sorry, leave balance information is not available.");

                if (daysRequested <= leaveBalance.CasualLeaveRemaining)
                {
                    // Auto approve leave and deduct days
                    await _leaveService.CreateLeaveRequestAsync(employeeId, startDate.Value, endDate.Value);
                    return Ok($"Your leave from {startDate:MMMM dd, yyyy} to {endDate:MMMM dd, yyyy} has been approved.");
                }
                else
                {
                    return Ok($"You have insufficient leave balance to cover {daysRequested} days. You currently have {leaveBalance.CasualLeaveRemaining} casual leaves available.");
                }
            }
            else if (message.Contains("how many leaves") || message.Contains("leave balance"))
            {
                var leaveBalance = await _leaveService.GetLeaveBalanceAsync(employeeId);
                if (leaveBalance == null)
                    return Ok("Sorry, leave balance information is not available.");

                return Ok($"You currently have {leaveBalance.CasualLeaveRemaining} casual leaves available.");
            }

            var currentBalance = (await _leaveService.GetLeaveBalanceAsync(employeeId))?.CasualLeaveRemaining ?? 0;
            var aiResponse = await _gptService.GptResponseWithContextAsync(dto.Message, currentBalance);

            return Ok(aiResponse);
        }

        private bool TryParseDate(string text, string[] formats, out DateTime parsedDate)
        {
            return DateTime.TryParseExact(text,
                                          formats,
                                          CultureInfo.InvariantCulture,
                                          DateTimeStyles.None,
                                          out parsedDate) ||
                   DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
        }

        private string JoinDateParts(string[] words, int start, int end)
        {
            if (start > end) return string.Empty;
            return string.Join(" ", words.Skip(start).Take(end - start + 1));
        }
    }

    public class ChatRequestDto
    {
        public string Message { get; set; }
    }
}
