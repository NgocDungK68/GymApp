using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Routines
{
    public partial class AddRoutineView : Window
    {
        private ObservableCollection<RoutineExerciseItem> _exerciseList;

        public AddRoutineView()
        {
            InitializeComponent();

            _exerciseList = new ObservableCollection<RoutineExerciseItem>();
            dgExercises.ItemsSource = _exerciseList;
        }

        // ===================== ADD EXERCISE =====================
        private void BtnAddExercise_Click(object sender, RoutedEventArgs e)
        {
            // Ẩn window hiện tại
            this.Hide();

            // Mở UserControl ExerciseManagementView trong Window trung gian
            var selectExerciseWindow = new Window
            {
                Title = "Chọn bài tập",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new ExerciseManagementView()
            };

            // Giả sử ExerciseManagementView có event trả về bài tập đã chọn
            if (selectExerciseWindow.ShowDialog() == true)
            {
                // Demo dữ liệu – sau này bạn thay bằng dữ liệu thật
                AddExerciseToList("Push Up");
            }

            // Hiện lại AddRoutineView
            this.Show();
        }

        private void AddExerciseToList(string exerciseName)
        {
            _exerciseList.Add(new RoutineExerciseItem
            {
                Index = _exerciseList.Count + 1,
                ExerciseName = exerciseName,
                Sets = 1
            });
        }

        // ===================== CONFIRM =====================
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // ===== VALIDATE INPUT =====
            string routineName = txtRoutineName.Text.Trim();
            string description = txtDescription.Text.Trim();
            ComboBoxItem? selectedLevel = cbLevel.SelectedItem as ComboBoxItem;

            if (string.IsNullOrEmpty(routineName))
            {
                MessageBox.Show("Tên lịch tập không được để trống!",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtRoutineName.Focus();
                return;
            }

            if (selectedLevel == null)
            {
                MessageBox.Show("Vui lòng chọn mức độ!",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbLevel.Focus();
                return;
            }

            if (_exerciseList.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất 1 bài tập vào lịch tập!",
                    "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ===== TODO: SAVE TO DB (VIẾT SAU) =====
            MessageBox.Show("Tạo lịch tập thành công!",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        // ===================== CANCEL =====================
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class RoutineExerciseItem
    {
        public int Index { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public int Sets { get; set; } = 1;
    }
}
