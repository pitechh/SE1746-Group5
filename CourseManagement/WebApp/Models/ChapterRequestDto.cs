﻿

namespace WebApp.Models
{
    public class ChapterRequestDto
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
    }
}
