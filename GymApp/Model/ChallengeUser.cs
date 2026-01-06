using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Challenge_User")]
    public class ChallengeUser
    {
        [Key]
        public int id { get; set; }

        public double? progress_value { get; set; }
        public bool is_completed { get; set; }
        public DateTime joined_at { get; set; }

        public int user_id { get; set; }
        public User User { get; set; }

        public int challenge_id { get; set; }
        public Challenge Challenge { get; set; }
    }
}
