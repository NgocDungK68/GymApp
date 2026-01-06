using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    public class Notification
    {
        [Key]
        public int id { get; set; }

        public string title { get; set; }
        public string content { get; set; }
        public string source_type { get; set; }
        public int? source_id { get; set; }
        public DateTime created_at { get; set; }

        public int user_id { get; set; }
        public User User { get; set; }
    }
}
