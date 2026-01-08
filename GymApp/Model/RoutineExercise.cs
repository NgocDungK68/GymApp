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

        [Column("routine_id")]
        public int RoutineId { get; set; }
        public Routine Routine { get; set; }

        [Column("exercise_id")]
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; }
    }
}
