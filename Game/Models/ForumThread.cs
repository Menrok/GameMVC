using System;
using System.Collections.Generic;

namespace Game.Models
{
    public class ForumThread
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } 
        public virtual ICollection<Reply> Replies { get; set; } 
    }
}
