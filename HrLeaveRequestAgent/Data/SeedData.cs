using HrLeaveRequestAgent.Data;
using HrLeaveRequestAgent.Models;
using System.Linq;

namespace HrLeaveRequestAgent.Data
{
    public static class SeedData
    {
        public static void Initialize(HrDbContext context)
        {
            // Avoid seeding if data already exists
            if (context.Employees.Any()) return;

            var employee = new Employee
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com"
            };

            context.Employees.Add(employee);

            context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                CasualLeaveRemaining = 7
            });

            context.SaveChanges();
        }
    }
}
