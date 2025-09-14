using System;
using System.Threading.Tasks;
using HrLeaveRequestAgent.Models;

namespace HrLeaveRequestAgent.Services
{
    public interface ILeaveService
    {
        Task<LeaveBalance> GetLeaveBalanceAsync(int employeeId);
        Task<LeaveRequest> CreateLeaveRequestAsync(int employeeId, DateTime start, DateTime end);
    }
}
