using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Group_User")]
    public class GroupUser
    {
        [Key]
        public int id { get; set; }

        public string role_in_group { get; set; }
        public DateTime joined_at { get; set; }

        public int group_id { get; set; }
        public Group Group { get; set; }

        public int user_id { get; set; }
        public User User { get; set; }
    }
}
