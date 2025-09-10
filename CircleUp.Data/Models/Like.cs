using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Models
{
    public class Like
    {
        public int Id { get; set; }

        //Foreing key
        public int PostId { get; set; }
        public int UserId { get; set; }

        //Navigation properties

        public Post Post { get; set; }
        public User User { get; set; }
    }
}
