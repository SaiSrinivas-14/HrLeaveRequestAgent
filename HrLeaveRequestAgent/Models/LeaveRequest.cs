using System;

namespace HrLeaveRequestAgent.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public Employee Employee { get; set; }
    }
}
