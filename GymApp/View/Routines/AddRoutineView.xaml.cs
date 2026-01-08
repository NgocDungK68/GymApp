using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;

namespace GymApp.View.Routines
{
    public partial class AddRoutineView : Window
    {
        private ObservableCollection<RoutineExerciseItem> _exerciseList;

        public AddRoutineView()
        {
            InitializeComponent();
            Init();
        }

        public AddRoutineView(int exerciseId)
        {
            InitializeComponent();
            Init();

            // xử lý exercise được truyền vào
            AddExerciseToListById(exerciseId);
        }

        private void Init()
        {
            _exerciseList = new ObservableCollection<RoutineExerciseItem>();
            dgExercises.ItemsSource = _exerciseList;

            dgExercises.CellEditEnding += DgExercises_CellEditEnding;
        }

        // ===================== ADD EXERCISE =====================
        private void BtnAddExercise_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            int? selectedExerciseId = null;

            var exerciseView = new ExerciseManagementView();

            var selectExerciseWindow = new Window
            {
                Title = "Chọn bài tập",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = exerciseView,
                Owner = this
            };

            // lắng nghe khi user bấm "Tạo lịch"
            exerciseView.ExerciseSelected += id =>
            {
                selectedExerciseId = id;
            };

            if (selectExerciseWindow.ShowDialog() == true && selectedExerciseId.HasValue)
            {
                // HỨNG ĐƯỢC exerciseId TẠI ĐÂY
                AddExerciseToListById(selectedExerciseId.Value);
            }

            this.Show();
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

            // ===== CHECK LOGIN =====
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập!",
                    "Chưa đăng nhập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUser = UserSession.CurrentUser!;

            using (var context = new GymDbContext())
            {
                // ===================== SAVE ROUTINE =====================
                var routine = new Routine
                {
                    name = routineName,
                    description = description,
                    level = selectedLevel.Content.ToString()!
                };

                context.Routines.Add(routine);
                context.SaveChanges(); // sinh routine.id

                // ===================== SAVE ROUTINE_EXERCISE =====================
                foreach (var item in _exerciseList)
                {
                    context.RoutineExercises.Add(new RoutineExercise
                    {
                        RoutineId = routine.id,
                        ExerciseId = item.ExerciseId,
                        sets = item.Sets
                    });
                }

                // ===================== SAVE USER_ROUTINE =====================
                context.UserRoutines.Add(new UserRoutine
                {
                    RoutineId = routine.id,
                    UserId = currentUser.id,
                    owner_id = currentUser.id
                });

                context.SaveChanges();
            }

            MessageBox.Show("Tạo lịch tập thành công!",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        // ===================== CANCEL =====================
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddExerciseToListById(int exerciseId)
        {
            using var context = new GymDbContext();

            var exercise = context.Exercises
                                  .AsNoTracking()
                                  .FirstOrDefault(e => e.id == exerciseId);

            if (exercise == null)
            {
                MessageBox.Show("Không tìm thấy bài tập!",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _exerciseList.Add(new RoutineExerciseItem
            {
                Index = _exerciseList.Count + 1,
                ExerciseId = exercise.id,
                ExerciseName = exercise.name,
                Sets = 1
            });
        }

        private void DgExercises_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // chỉ validate cột "Số hiệp"
            if (e.Column.Header?.ToString() != "Số hiệp")
                return;

            if (e.EditingElement is TextBox textBox)
            {
                var rowItem = e.Row.Item as RoutineExerciseItem;
                if (rowItem == null) return;

                string input = textBox.Text.Trim();

                // ===== VALIDATE =====
                if (!int.TryParse(input, out int newSets) || newSets <= 0)
                {
                    MessageBox.Show(
                        "Số hiệp phải là số nguyên dương!",
                        "Dữ liệu không hợp lệ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    // rollback lại giá trị cũ
                    textBox.Text = rowItem.Sets.ToString();

                    // hủy commit
                    e.Cancel = true;
                    return;
                }

                // ===== UPDATE =====
                rowItem.Sets = newSets;
            }
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // chỉ cho nhập số
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }
    }

    public class RoutineExerciseItem : INotifyPropertyChanged
    {
        private int _sets = 1;
        public int Index { get; set; }

        public int ExerciseId { get; set; }   // GIỮ ID GỐC

        public string ExerciseName { get; set; } = string.Empty;

        public int Sets
        {
            get => _sets;
            set
            {
                if (_sets != value)
                {
                    _sets = value;
                    OnPropertyChanged(nameof(Sets));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
