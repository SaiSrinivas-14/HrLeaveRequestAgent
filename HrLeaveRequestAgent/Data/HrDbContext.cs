using HrLeaveRequestAgent.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HrLeaveRequestAgent.Data
{
    public class HrDbContext : DbContext
    {
        public HrDbContext(DbContextOptions<HrDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
    }
}
