﻿namespace backend.Models
{
    public class Album
    {
        public string Id { get; set; } = String.Empty;
        public string Name { get; set; }
        public DateTime CreatedTime { get; set; }
        public IEnumerable<Photo>? Photos { get; set; }
    }
}
