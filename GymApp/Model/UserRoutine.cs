using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("User_Routine")]
    public class UserRoutine
    {
        [Key]
        public int id { get; set; }

        public bool owner { get; set; }
        public DateTime created_at { get; set; }

        public int user_id { get; set; }
        public User User { get; set; }

        public int routine_id { get; set; }
        public Routine Routine { get; set; }
    }
}
