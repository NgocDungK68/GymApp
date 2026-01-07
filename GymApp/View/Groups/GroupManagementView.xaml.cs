using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace GymApp.View.Groups
{
    public partial class GroupManagementView : UserControl
    {
        public ObservableCollection<GroupItem> Groups { get; set; }

        public GroupManagementView()
        {
            InitializeComponent();

            // Fake data
            Groups = new ObservableCollection<GroupItem>
            {
                new GroupItem { Name = "Lập trình C#" },
                new GroupItem { Name = "Gym buổi sáng" },
                new GroupItem { Name = "Ăn kiêng & Dinh dưỡng" },
                new GroupItem { Name = "Học SQL nâng cao" },
                new GroupItem { Name = "Chạy bộ cuối tuần" },
                new GroupItem { Name = "Fitness 2025" },
                new GroupItem { Name = "Nhóm bạn đại học" },
                new GroupItem { Name = "Workout tại nhà" }
            };

            // Gán DataContext cho View
            this.DataContext = this;
        }
    }

    // ================= MODEL =================
    public class GroupItem
    {
        public string Name { get; set; }

        // Lấy chữ cái đầu tiên
        public string FirstLetter
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return "?";

                return Name.Substring(0, 1).ToUpper();
            }
        }
    }
}
