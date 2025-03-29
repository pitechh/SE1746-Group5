﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public DateTime IssueDate { get; set; }
        public string? CertificateNumber { get; set; }
        public string? CertificateUrl { get; set; }
    }
}
