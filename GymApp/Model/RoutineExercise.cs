using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Routine_Exercise")]
    public class RoutineExercise
    {
        [Key]
        public int id { get; set; }

        public int sets { get; set; }

        public int routine_id { get; set; }
        public Routine Routine { get; set; }

        public int exercise_id { get; set; }
        public Exercise Exercise { get; set; }
    }
}
