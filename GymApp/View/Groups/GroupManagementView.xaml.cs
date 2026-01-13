using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using GymApp.View.Nutritions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GymApp.View.Groups
{
    public partial class GroupManagementView : UserControl
    {
        public ObservableCollection<GroupItem> Groups { get; set; }

        public GroupManagementView()
        {
            InitializeComponent();

            Groups = new ObservableCollection<GroupItem>();
            this.DataContext = this;

            LoadGroups();
        }

        // ================= LOAD GROUPS =================
        private void LoadGroups()
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập lại!");
                return;
            }

            Groups.Clear();

            using var context = new GymDbContext();

            var userId = UserSession.CurrentUser!.id;

            // Lấy tất cả group_user của user hiện tại
            var groupUsers = context.GroupUsers
                .Include(gu => gu.Group)
                .Where(gu => gu.UserId == userId)
                .ToList();

            foreach (var gu in groupUsers)
            {
                Groups.Add(new GroupItem
                {
                    GroupId = gu.GroupId,
                    Name = gu.Group.name
                });
            }
        }

        // ================= BUTTONS =================
        private void BtnCreateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Bạn chưa đăng nhập!",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Mở cửa sổ AddGroupView
            var addGroupWindow = new AddGroupView();
            addGroupWindow.Owner = Window.GetWindow(this);

            // mở dạng modal
            addGroupWindow.ShowDialog();

            // reload lại danh sách sau khi đóng window
            LoadGroups();
        }

        // ================= CLICK GROUP CARD =================
        private void GroupCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is GroupItem group)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null) return;

                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new DetailGroupView(group.GroupId)
                );
            }
        }
    }

    // ================= VIEW MODEL =================
    public class GroupItem
    {
        public int GroupId { get; set; }   // dùng để navigate / click group

        public string Name { get; set; }

        // Avatar chữ cái đầu
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
