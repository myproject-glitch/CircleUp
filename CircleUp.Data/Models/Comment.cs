using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DatetUpdated { get; set; }

        //Foreign key for User
        public int UserId { get; set; }

        //Foreign key for Post
        public int PostId { get; set; }

        //Navigation properties


        public User User { get; set; }
        public Post Post{ get; set; }
    }
}
