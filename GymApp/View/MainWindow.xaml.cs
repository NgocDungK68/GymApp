using GymApp.Data;
using GymApp.Model;
using GymApp.View;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GymApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadHome();
        }

        private void MenuTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string tag = btn.Tag?.ToString();
                switch (tag)
                {
                    case "Home":
                        LoadHome();
                        break;
                    case "Statistics":
                        break;
                    case "Ranking":
                        // new SettingsWindow().Show();
                        break;
                    case "Notification":
                        break;
                }
            }
        }

        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                MainContent.Children.Clear();
                string tag = btn.Tag?.ToString();

                switch (tag)
                {
                    case "UserManagement":
                        LoadUserManagementContent();
                        break;

                    case "Exercise":
                        LoadExerciseManagementContent();
                        break;

                    case "WorkoutSchedule":
                        break;

                    case "Nutrition":
                        break;

                    case "Group":
                        break;

                    case "Ranking":
                        break;

                    case "Notification":
                        break;
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                new LoginWindow().Show();
                this.Close();
            }
        }

        private void LoadHome()
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new HomeView());
        }

        private void LoadUserManagementContent()
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new UserManagementView());
        }

        private void LoadExerciseManagementContent()
        {
            MainContent.Children.Clear();

            var view = new ExerciseManagementView();
            Grid.SetRow(view, 0);
            MainContent.Children.Add(view);
        }
    }
}