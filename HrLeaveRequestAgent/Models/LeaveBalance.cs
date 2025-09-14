namespace HrLeaveRequestAgent.Models
{
    public class LeaveBalance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int CasualLeaveRemaining { get; set; }
        public Employee Employee { get; set; }
    }
}
