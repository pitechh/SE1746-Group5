﻿namespace WebAPI.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public double Progress { get; set; }
        public int Status { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
