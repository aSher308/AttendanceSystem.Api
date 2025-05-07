namespace AttendanceSystem.Models
{
    public enum AttendanceStatus
    {
        OnTime,
        Late,
        Absent,
        Pending
    }

    public enum AttendanceType
    {
        GPS,
        QR,
        FaceRecognition,
        Manual
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum ScanType
    {
        CheckIn,
        CheckOut
    }

    public enum NotificationType
    {
        System,
        Reminder,
        Approval
    }

    public enum LeaveType
    {
        PaidLeave,          // Nghỉ phép có lương
        SickLeave,          // Nghỉ bệnh
        UnpaidLeave,        // Nghỉ không lương
        RemoteWork,         // Làm việc từ xa
        Other               // Khác
    }
}
