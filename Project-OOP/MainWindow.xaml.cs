using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Project_OOP
{
    public partial class MainWindow : Window
    {
        // Variabelen voor het bijhouden van de scores en beweging
        int levelTouches = 0;
        int scoreTouches = 0;
        double speed = 1;
        bool goLeft = false;
        bool goRight = false;
        bool goUp = false;
        bool goDown = false;

        // Timer variabelen
        int remainingTime = 60;
        int levelTimeDecrease = 10;
        int currentLevel = 1;
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            // Event handlers initialiseren
            this.KeyDown += KeyIsDown;
            this.KeyUp += KeyIsUp;
            CompositionTarget.Rendering += UpdatePosition;

            // Timer initialiseren
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // Event handlers voor het toetsenbord
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            // Logica voor het bewegen van de bal
            if (e.Key == Key.Left)
            {
                goLeft = true;
                goRight = false;
            }
            else if (e.Key == Key.Right)
            {
                goRight = true;
                goLeft = false;
            }
            else if (e.Key == Key.Up)
            {
                goUp = true;
                goDown = false;
            }
            else if (e.Key == Key.Down)
            {
                goDown = true;
                goUp = false;
            }
        }
        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            // Logica voor het stoppen van de beweging van de bal
            if (e.Key == Key.Left && !goRight)
            {
                goLeft = false;
            }
            else if (e.Key == Key.Right && !goLeft)
            {
                goRight = false;
            }
            else if (e.Key == Key.Up && !goDown)
            {
                goUp = false;
            }
            else if (e.Key == Key.Down && !goUp)
            {
                goDown = false;
            }
        }

        // Methode voor het bijwerken van de positie van de bal en het controleren op botsingen
        private void UpdatePosition(object sender, EventArgs e)
        {
            // Logica voor het bijwerken van de positie van de bal en het controleren op botsingen met de extra ballen
            if (goLeft)
            {
                Canvas.SetLeft(myBall, Canvas.GetLeft(myBall) - speed);
            }
            if (goRight)
            {
                Canvas.SetLeft(myBall, Canvas.GetLeft(myBall) + speed);
            }
            if (goUp)
            {
                Canvas.SetTop(myBall, Canvas.GetTop(myBall) - speed);
            }
            if (goDown)
            {
                Canvas.SetTop(myBall, Canvas.GetTop(myBall) + speed);
            }

            // Controleer of de hoofdbal een van de extra ballen raakt
            CheckCollision(myBall, levelBall, ref levelTouches);
            CheckCollision(myBall, scoreBall, ref scoreTouches);

            // Update de tekst voor scores
            scoresTextBlock.Text = $"Level: {levelTouches} Score: {scoreTouches}";
        }

        // Methode voor het controleren op botsingen tussen de hoofdbal en de extra ballen
        private void CheckCollision(Ellipse ball1, Ellipse ball2, ref int touchesCounter)
        {
            // Logica voor het controleren op botsingen tussen ballen
            Rect rect1 = new Rect(Canvas.GetLeft(ball1), Canvas.GetTop(ball1), ball1.Width, ball1.Height);
            Rect rect2 = new Rect(Canvas.GetLeft(ball2), Canvas.GetTop(ball2), ball2.Width, ball2.Height);

            if (rect1.IntersectsWith(rect2))
            {
                touchesCounter++;
                Canvas.SetLeft(ball2, new Random().Next(0, (int)(canvas1.ActualWidth - ball2.Width)));
                Canvas.SetTop(ball2, new Random().Next(0, (int)(canvas1.ActualHeight - ball2.Height)));

                // Reset de tijd naar een specifieke waarde per level
                remainingTime = 50 - (currentLevel - 1) * 10; // 50 - 10 * (currentLevel - 1)
                currentLevel++;
                timerTextBlock.Text = $"Time: {remainingTime} s"; // Update de weergegeven tijd
            }
        }

        // Event handler voor de timer tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Logica voor het bijwerken van de timer en het controleren of de tijd is verstreken
            remainingTime--;

            if (remainingTime >= 0)
            {
                timerTextBlock.Text = $"Time: {remainingTime} s";
            }
            else
            {
                timer.Stop();
                MessageBox.Show("Tijd is op! Level niet voltooid.", "Tijd op", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}