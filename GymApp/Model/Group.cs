using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Group")]
    public class Group
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string description { get; set; }

        public ICollection<GroupUser> GroupUsers { get; set; }
        public ICollection<Challenge> Challenges { get; set; }
    }
}
