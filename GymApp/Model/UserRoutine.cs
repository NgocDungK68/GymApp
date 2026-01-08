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

        [Column("owner_id")]
        public int? owner_id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("routine_id")]
        public int RoutineId { get; set; }
        public Routine Routine { get; set; }
    }
}
