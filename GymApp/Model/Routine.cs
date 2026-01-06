using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    public class Routine
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public string level { get; set; }
        public DateTime created_at { get; set; }

        public ICollection<RoutineExercise> RoutineExercises { get; set; }
        public ICollection<UserRoutine> UserRoutines { get; set; }
        public ICollection<Workout> Workouts { get; set; }
    }
}
