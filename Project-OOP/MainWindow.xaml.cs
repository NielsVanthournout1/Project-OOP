using System.Drawing;
using System.IO;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Project_OOP
{
    public partial class MainWindow : Window
    {
        private string jsonFilePath = Environment.CurrentDirectory + "\\scores.json";


        // Variabelen voor het bijhouden van de scores en beweging
        int levelTouches = 0;
        int scoreTouches = 0;
        double speed = 3;
        bool goLeft = false;
        bool goRight = false;
        bool goUp = false;
        bool goDown = false;

        // Timer variabelen
        int remainingTime = 60;
        int levelTimeDecrease = 10;
        int currentLevel = 1;
        int currentLevelTouches = 0;
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

        private void SaveScoresAndLevel()
        {
            List<ScoreData> scoreList;

            // Check if the file exists
            if (File.Exists(jsonFilePath))
            {
                // Read existing data from the file
                string jsonData = File.ReadAllText(jsonFilePath);

                // Check if the file is empty
                if (string.IsNullOrEmpty(jsonData))
                {
                    // Create a new list if the file is empty
                    scoreList = new List<ScoreData>();
                }
                else
                {
                    // Deserialize existing data into a list
                    scoreList = JsonConvert.DeserializeObject<List<ScoreData>>(jsonData);
                }
            }
            else
            {
                // Create a new list if the file doesn't exist
                scoreList = new List<ScoreData>();
            }

            // Add the current score and level to the list
            scoreList.Add(new ScoreData { Score = scoreTouches, Level = currentLevel, DateAchieved = DateTime.Now });

            // Serialize the list and save it to the file with each object on a new line
            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            string updatedJsonData = string.Join(",\n", scoreList.Select(JsonConvert.SerializeObject));

            File.WriteAllText(jsonFilePath, $"[\n{updatedJsonData}\n]");
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
            Rect rect1 = new Rect(Canvas.GetLeft(ball1), Canvas.GetTop(ball1), ball1.Width, ball1.Height);
            Rect rect2 = new Rect(Canvas.GetLeft(ball2), Canvas.GetTop(ball2), ball2.Width, ball2.Height);

            if (rect1.IntersectsWith(rect2))
            {
                // Move the ball2 (either levelBall or scoreBall) to a new random position
                Canvas.SetLeft(ball2, new Random().Next(0, (int)(canvas1.ActualWidth - ball2.Width)));
                Canvas.SetTop(ball2, new Random().Next(0, (int)(canvas1.ActualHeight - ball2.Height)));

                if (ball2 == levelBall)
                {
                    // Update the remaining time based on the current level
                    remainingTime = 50 - (currentLevel - 1) * 10;
                    // Update the timer text to display the remaining time
                    timerTextBlock.Text = $"Time: {remainingTime} s";

                    // Increase the current level only if it hasn't been increased already
                    if (currentLevelTouches < levelTouches)
                    {
                        currentLevel++;
                        currentLevelTouches = levelTouches; // Update the current level touches
                    }
                    // Update the levelTouches variable
                    levelTouches++;
                }
                else if (ball2 == scoreBall)
                {
                    // Update the scoreTouches variable
                    scoreTouches++;
                    // Update the score text
                    scoresTextBlock.Text = $"Level: {levelTouches} Score: {scoreTouches}";
                }
            }
        }


        // Event handler voor de timer tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime >= 0)
            {
                timerTextBlock.Text = $"Time: {remainingTime} s";
            }
            else
            {
                timer.Stop();
                MessageBox.Show("Tijd is op! Level niet voltooid.", "Tijd op", MessageBoxButton.OK, MessageBoxImage.Information);

                // Roep CheckForNewHighScore aan om te controleren of er een nieuwe hoogste score is bereikt
                CheckForNewHighScore();

                // Save the scores when the time runs out
                SaveScoresAndLevel();

                // Reset remainingTime for the next level or game
                remainingTime = 60; // Reset to initial time for the next level

                return;
            }

            // Only execute the rest of the logic if time hasn't run out
            // Update the position of the ball and check collision
            UpdatePosition(sender, e);
        }

        // Methode voor het bijwerken van de scores
        private void UpdateScores()
        {
            // Update de tekst voor scores
            scoresTextBlock.Text = $"Level: {levelTouches} Score: {scoreTouches}";
        }

        // Dataklasse voor het opslaan van scores en level
        public class ScoreData
        {
            public int Score { get; set; }
            public int Level { get; set; }
            public DateTime DateAchieved { get; set; }
        }

        private void HScore_Click(object sender, RoutedEventArgs e)
        {
            // Check if the file exists
            if (File.Exists(jsonFilePath))
            {
                // Read existing data from the file
                string jsonData = File.ReadAllText(jsonFilePath);
                // Deserialize the JSON data into a list of ScoreData
                List<ScoreData> scoreList = JsonConvert.DeserializeObject<List<ScoreData>>(jsonData);

                // Check if the list is not null and has any elements
                if (scoreList != null && scoreList.Count > 0)
                {
                    // Sort the score list in descending order based on the scores
                    scoreList = scoreList.OrderByDescending(s => s.Score).ToList();
                    // Take the top 10 scores
                    List<ScoreData> top10Scores = scoreList.Take(10).ToList();

                    // Initialize the StringBuilder for the message
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine("Top 10 highest scores:");

                    // Add the top 10 scores and format the date and time
                    for (int i = 0; i < top10Scores.Count; i++)
                    {
                        // Format the date and time
                        string formattedDate = top10Scores[i].DateAchieved.ToString("dd/MM/yyyy HH:mm:ss");
                        // Add the date and time to the message
                        messageBuilder.AppendLine($"({i + 1}) Score: {top10Scores[i].Score}, level: {top10Scores[i].Level}, date: {formattedDate}");
                    }

                    // Sort the list in descending order based on levels
                    scoreList = scoreList.OrderByDescending(s => s.Level).ToList();
                    // Take the top 10 levels
                    List<ScoreData> top10Levels = scoreList.Take(10).ToList();

                    // Add the top 10 levels and format the date and time
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Top 10 highest levels:");
                    for (int i = 0; i < top10Levels.Count; i++)
                    {
                        // Format the date and time
                        string formattedDate = top10Levels[i].DateAchieved.ToString("dd/MM/yyyy HH:mm:ss");
                        // Add the date and time to the message
                        messageBuilder.AppendLine($"({i + 1}) Level: {top10Levels[i].Level}, score: {top10Levels[i].Score}, date: {formattedDate}");
                    }

                    // Display the message
                    MessageBox.Show(messageBuilder.ToString(), "Top 10 Scores and Levels", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No scores found");
                }
            }
            else
            {
                MessageBox.Show("The scores file does not exist");
            }
        }

        private void DScore_Click(object sender, RoutedEventArgs e)
        {
            // Check if the file exists
            if (File.Exists(jsonFilePath))
            {
                // Read existing data from the file
                string jsonData = File.ReadAllText(jsonFilePath);
                // Deserialize the JSON data into a list of ScoreData
                List<ScoreData> scoreList = JsonConvert.DeserializeObject<List<ScoreData>>(jsonData);

                // Get today's date
                DateTime today = DateTime.Today;

                // Filter the list to only include scores from today
                List<ScoreData> todayScores = scoreList
                    .Where(score => score.DateAchieved.Date == today)
                    .ToList();

                // Sort the filtered list by score in descending order
                todayScores = todayScores.OrderByDescending(s => s.Score).ToList();

                // Take the top 10 scores
                List<ScoreData> top10Scores = todayScores.Take(10).ToList();

                // Initialize the StringBuilder for the message
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Top 10 highest scores today:");

                // Add the top 10 scores and format the date and time
                for (int i = 0; i < top10Scores.Count; i++)
                {
                    // Format the date and time
                    string formattedDate = top10Scores[i].DateAchieved.ToString("dd/MM/yyyy HH:mm:ss");
                    // Add the date and time to the message
                    messageBuilder.AppendLine($"({i + 1}) Score: {top10Scores[i].Score}, level: {top10Scores[i].Level}, date: {formattedDate}");
                }

                // Sort the filtered list by level in descending order
                todayScores = todayScores.OrderByDescending(s => s.Level).ToList();

                // Take the top 10 levels
                List<ScoreData> top10Levels = todayScores.Take(10).ToList();

                // Add the top 10 levels and format the date and time
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("Top 10 highest levels today:");
                for (int i = 0; i < top10Levels.Count; i++)
                {
                    // Format the date and time
                    string formattedDate = top10Levels[i].DateAchieved.ToString("dd/MM/yyyy HH:mm:ss");
                    // Add the date and time to the message
                    messageBuilder.AppendLine($"({i + 1}) Level: {top10Levels[i].Level}, score: {top10Levels[i].Score}, date: {formattedDate}");
                }

                // Display the message
                MessageBox.Show(messageBuilder.ToString(), "Top 10 Scores and Levels Today", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("The scores file does not exist");
            }
        }

        private void CheckForNewHighScore()
        {
            // Lees de bestaande scores van het JSON-bestand
            List<ScoreData> scoreList;
            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                scoreList = JsonConvert.DeserializeObject<List<ScoreData>>(jsonData);
            }
            else
            {
                // Als het bestand niet bestaat, maak dan een lege lijst
                scoreList = new List<ScoreData>();
            }

            // Initialiseer variabelen voor de hoogste scores
            int allTimeHighScore = 0;
            int allTimeHighLevel = 0;
            int dailyHighScore = 0;
            int dailyHighLevel = 0;

            // Bereken de hoogste scores aller tijden
            foreach (var scoreData in scoreList)
            {
                if (scoreData.Score > allTimeHighScore)
                {
                    allTimeHighScore = scoreData.Score;
                }
                if (scoreData.Level > allTimeHighLevel)
                {
                    allTimeHighLevel = scoreData.Level;
                }

                // Controleer of de datum van de score vandaag is
                if (scoreData.DateAchieved.Date == DateTime.Today)
                {
                    if (scoreData.Score > dailyHighScore)
                    {
                        dailyHighScore = scoreData.Score;
                    }
                    if (scoreData.Level > dailyHighLevel)
                    {
                        dailyHighLevel = scoreData.Level;
                    }
                }
            }

            // Controleer of de huidige score of level hoger is dan de hoogste scores aller tijden of dagelijks
            bool newAllTimeHighScore = scoreTouches > allTimeHighScore;
            bool newAllTimeHighLevel = levelTouches > allTimeHighLevel;
            bool newDailyHighScore = scoreTouches > dailyHighScore;
            bool newDailyHighLevel = levelTouches > dailyHighLevel;

            // Toon een messagebox als er een nieuw hoogste score of level is bereikt
            if (newAllTimeHighScore)
            {
                MessageBox.Show("New all-time highest score achieved!", "Congratulations!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (newAllTimeHighLevel)
            {
                MessageBox.Show("New all-time highest level achieved!", "Congratulations!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (newDailyHighScore)
            {
                MessageBox.Show("New daily highest score achieved!", "Congratulations!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (newDailyHighLevel)
            {
                MessageBox.Show("New daily highest level achieved!", "Congratulations!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}