using GymApp.Data;
using GymApp.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using GymApp.Core;

namespace GymApp.View.Routines
{
    public partial class WorkoutView : UserControl
    {
        private readonly int _routineId;

        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private bool _isPaused = false;

        public WorkoutView(int routineId)
        {
            InitializeComponent();

            _routineId = routineId;

            LoadRoutineInfo();
            LoadExercises();
            StartTimer();
        }

        // ================= TIMER =================
        private void StartTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (s, e) =>
            {
                _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
                txtWorkoutTime.Text = _elapsedTime.ToString(@"hh\:mm\:ss");
            };

            _timer.Start();
        }

        private void PauseTimer()
        {
            _timer?.Stop();
            _isPaused = true;
        }

        private void ResumeTimer()
        {
            _timer?.Start();
            _isPaused = false;
        }

        // ================= LOAD ROUTINE =================
        private void LoadRoutineInfo()
        {
            using var context = new GymDbContext();

            var routine = context.Routines
                .AsNoTracking()
                .FirstOrDefault(r => r.id == _routineId);

            if (routine != null)
            {
                txtRoutineName.Text = routine.name;
            }
        }

        // ================= LOAD EXERCISES =================
        private void LoadExercises()
        {
            using var context = new GymDbContext();

            var routineExercises = context.RoutineExercises
                .AsNoTracking()
                .Include(re => re.Exercise)
                .Where(re => re.RoutineId == _routineId)
                .OrderBy(re => re.id) // sort theo id RoutineExercise
                .ToList();

            var result = new List<WorkoutExerciseItem>();

            int index = 1;
            foreach (var re in routineExercises)
            {
                result.Add(new WorkoutExerciseItem
                {
                    Index = index++,
                    Name = re.Exercise.name,
                    MuscleGroup = re.Exercise.muscles,
                    Sets = re.sets
                });
            }

            dgWorkoutExercises.ItemsSource = result;
        }

        // ================= BUTTON EVENTS =================
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (!_isPaused)
            {
                PauseTimer();

                btn.Content = "▶ Tiếp tục buổi tập";
                btn.Background = new SolidColorBrush(Color.FromRgb(67, 160, 71)); // xanh lá
                btn.Foreground = Brushes.White;
            }
            else
            {
                ResumeTimer();

                btn.Content = "⏸ Dừng buổi tập";
                btn.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // vàng
                btn.Foreground = Brushes.Black;
            }
        }

        private void FinishWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Bạn có chắc muốn kết thúc buổi tập?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            // dừng timer
            _timer?.Stop();

            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("Vui lòng đăng nhập lại!");
                return;
            }

            using var context = new GymDbContext();
            var now = DateTime.Now;
            var today = now.Date;

            // ================= 1. NUTRITION =================
            var nutrition = context.Nutritions
                .FirstOrDefault(n =>
                    n.UserId == UserSession.CurrentUser.id &&
                    n.date.Date == today);

            if (nutrition == null)
            {
                nutrition = new Nutrition
                {
                    UserId = UserSession.CurrentUser.id,
                    date = now,
                    goal_calories = 0,
                    note = ""
                };

                context.Nutritions.Add(nutrition);
                context.SaveChanges(); // để có nutrition.id
            }

            // ================= 2. TÍNH DURATION (GIÂY) =================
            int durationInSeconds = (int)_elapsedTime.TotalSeconds;

            // ================= 3. TÍNH CALORIES =================
            // 7 kcal / phút
            double caloriesBurned = Math.Round(_elapsedTime.TotalMinutes * 7, 1);

            // ================= 4. TẠO WORKOUT =================
            var workout = new Workout
            {
                name = "",
                note = "",
                duration = durationInSeconds,
                date = now,
                calories_burned = caloriesBurned,
                RoutineId = _routineId,
                NutritionId = nutrition.id
            };

            context.Workouts.Add(workout);
            context.SaveChanges();

            MessageBox.Show(
                $"🎉 Hoàn thành buổi tập!\n" +
                $"⏱ Thời gian: {_elapsedTime:hh\\:mm\\:ss}\n" +
                $"🔥 Calories đốt cháy: {caloriesBurned} kcal",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // ================= 5. QUAY VỀ DANH SÁCH =================
            GoBackToRoutineList();
        }

        private void GoBackToRoutineList()
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            mainWindow.MainContent.Children.Clear();
            mainWindow.MainContent.Children.Add(new RoutineManagementView());
        }

    }

    public class WorkoutExerciseItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string MuscleGroup { get; set; }
        public int Sets { get; set; }
    }
}
