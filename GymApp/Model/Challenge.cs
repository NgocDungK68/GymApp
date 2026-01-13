using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Challenge")]
    public class Challenge
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public string challenge_type { get; set; }
        public double? challenge_target { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }

        [Column("group_id")]
        public int? GroupId { get; set; }
        public Group Group { get; set; }

        public ICollection<ChallengeUser> ChallengeUsers { get; set; }
    }
}
