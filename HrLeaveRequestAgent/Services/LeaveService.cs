using System;
using System.Threading.Tasks;
using HrLeaveRequestAgent.Data;
using HrLeaveRequestAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace HrLeaveRequestAgent.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly HrDbContext _context;

        public LeaveService(HrDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveBalance> GetLeaveBalanceAsync(int employeeId)
        {
            return await _context.LeaveBalances.Include(lb => lb.Employee)
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId);
        }

        public async Task<LeaveRequest> CreateLeaveRequestAsync(int employeeId, DateTime start, DateTime end)
        {
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                StartDate = start,
                EndDate = end,
                Status = "Approved" // Mark leave approved
            };
            _context.LeaveRequests.Add(leaveRequest);

            var leaveBalance = await GetLeaveBalanceAsync(employeeId);
            if (leaveBalance != null)
            {
                int daysRequested = (end - start).Days + 1;
                leaveBalance.CasualLeaveRemaining -= daysRequested;

                if (leaveBalance.CasualLeaveRemaining < 0)
                    leaveBalance.CasualLeaveRemaining = 0; // Ensure no negative balance
            }

            await _context.SaveChangesAsync();
            return leaveRequest;
        }

    }
}
