﻿using System.IO;
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

        // score variabelen
        int level = 1;
        int score = 0;

        // beweging variabelen
        double speed = 3;
        bool goLeft = false;
        bool goRight = false;
        bool goUp = false;
        bool goDown = false;

        // timer variabelen
        int time = 60;
        int timeDecrease = 10;
        DispatcherTimer timer;

        // andere variabelen
        bool isGameRunning = false;

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
        }

        // timer
        private void Timer_Tick(object sender, EventArgs e)
        {
            time--;

            if (time >= 0)
            {
                timerTextBlock.Text = $"Time: {time} s";
            }
            else
            {
                timer.Stop();
                MessageBox.Show("Tijd is op!", "Tijd op", MessageBoxButton.OK, MessageBoxImage.Information);

                CheckForNewHighScore();

                SaveScoresAndLevel();

                isGameRunning = false;

                return;
            }

            // Only execute the rest of the logic if time hasn't run out
            // Update the position of the ball and check collision
            UpdatePosition(sender, e);
        }

        // (button click event) starten / stoppen van het spel
        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!isGameRunning)
            {
                isGameRunning = true;
                StartStop.Content = "stop spel / reset ";

                time = 60;
                level = 1;
                score = 0;


                Canvas.SetLeft(myBall, 10);
                Canvas.SetTop(myBall, 10);
                ChangeLevel();
                level--;

                scoresTextBlock.Text = $"Level: {level} Score: {score}";
                timerTextBlock.Text = $"Time: {time} s";

                timer.Start();
            }
            else
            {
                isGameRunning = false;
                timer.Stop();
                StartStop.Content = "start spel";

                goLeft = false;
                goRight = false;
                goUp = false;
                goDown = false;

                SaveScoresAndLevel();

                MessageBox.Show("You can start a new level. (score saved)", "Game Stopped", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // scores
        // opslaan score / level / datum,tijd
        public void SaveScoresAndLevel()
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
            scoreList.Add(new ScoreData { Score = score, Level = level, DateAchieved = DateTime.Now });

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
        // beste scores nagaan
        private void CheckForNewHighScore()
        {

            // Lees de bestaande scores van het JSON-bestand
            List<ScoreData> scoreList;
            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                scoreList = JsonConvert.DeserializeObject<List<ScoreData>>(jsonData);

                // Check if the list is not null and has any elements
                if (scoreList != null && scoreList.Count > 0)
                {
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
                    bool newAllTimeHighScore = score > allTimeHighScore;
                    bool newAllTimeHighLevel = level > allTimeHighLevel;
                    bool newDailyHighScore = score > dailyHighScore;
                    bool newDailyHighLevel = level > dailyHighLevel;

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
                else
                {
                    MessageBox.Show("You are the first to get a score on this devise!", "Congratulations!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Als het bestand niet bestaat, maak dan een lege lijst
                scoreList = new List<ScoreData>();
            }
        }
        // (button click event) hooghste scores
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
        // (button click event) hooghste scores van de dag
        private void DScore_Click(object sender, RoutedEventArgs e)
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
            }
            else
            {
                MessageBox.Show("The scores file does not exist");
            }
        }
        // Dataklasse opslaan scores / level / datum,tijd
        public class ScoreData
        {
            public int Score { get; set; }
            public int Level { get; set; }
            public DateTime DateAchieved { get; set; }
        }

        // bewegen van speler
        // bepalen welke beweging
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
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
        // positie veranderen speler
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

            // controleren op botsingen
            CheckCollision(myBall, levelBall);
            CheckCollision(myBall, scoreBall);
            CheckCollision(myBall, wall1);
            CheckCollision(myBall, wall2);
        }

        // botsingen in het spel
        // bepalen soort botsing
        private void CheckCollision(Ellipse ball1, object other)
        {
            if (other is Ellipse)
            {
                Ellipse ball2 = (Ellipse)other;
                CollisionEllipse(ball1, ball2);
            }
            else if (other is Rectangle)
            {
                Rectangle wall = (Rectangle)other;
                CollisionWall(ball1, wall);
            }
            else
            {
                throw new NotSupportedException("Unsupported collision object type");
            }
        }
        // botsen met Ellipse
        private void CollisionEllipse(Ellipse ball, Ellipse collisionBall)
        {
            Rect rect1 = new Rect(Canvas.GetLeft(ball), Canvas.GetTop(ball), ball.Width, ball.Height);
            Rect rect2 = new Rect(Canvas.GetLeft(collisionBall), Canvas.GetTop(collisionBall), collisionBall.Width, collisionBall.Height);

            if (rect1.IntersectsWith(rect2))
            {
                if (collisionBall == levelBall)
                {
                    ChangeLevel();
                }
                else if (collisionBall == scoreBall)
                {
                    Canvas.SetLeft(scoreBall, new Random().Next(0, (int)(canvas1.ActualWidth - scoreBall.Width)));
                    Canvas.SetTop(scoreBall, new Random().Next(0, (int)(canvas1.ActualHeight - scoreBall.Height)));

                    score++;
                    scoresTextBlock.Text = $"Level: {level} Score: {score}";
                }
            }
        }
        // botsen met rectangle
        private void CollisionWall(Ellipse ball, Rectangle wall)
        {
            Rect rect1 = new Rect(Canvas.GetLeft(ball), Canvas.GetTop(ball), ball.Width, ball.Height);
            Rect rect2 = new Rect(Canvas.GetLeft(wall), Canvas.GetTop(wall), wall.Width, wall.Height);

            if (rect1.IntersectsWith(rect2))
            {
                if (isGameRunning == true)
                {
                    timer.Stop();
                    MessageBox.Show("Je bent er aan!", "muur geraakt", MessageBoxButton.OK, MessageBoxImage.Information);

                    goLeft = false;
                    goRight = false;
                    goUp = false;
                    goDown = false;

                    CheckForNewHighScore();

                    SaveScoresAndLevel();

                    isGameRunning = false;
                }
            }
        }

        // nieuw level aanmaken
        private void ChangeLevel()
        {
            Canvas.SetLeft(levelBall, new Random().Next(0, (int)(canvas1.ActualWidth - levelBall.Width)));
            Canvas.SetTop(levelBall, new Random().Next(0, (int)(canvas1.ActualHeight - levelBall.Height)));
            Canvas.SetLeft(scoreBall, new Random().Next(0, (int)(canvas1.ActualWidth - scoreBall.Width)));
            Canvas.SetTop(scoreBall, new Random().Next(0, (int)(canvas1.ActualHeight - scoreBall.Height)));

            Canvas.SetLeft(wall1, new Random().Next(0, (int)(canvas1.ActualWidth - wall1.Width)));
            Canvas.SetTop(wall1, new Random().Next(0, (int)(canvas1.ActualHeight - wall1.Height)));
            Canvas.SetLeft(wall2, new Random().Next(0, (int)(canvas1.ActualWidth - wall2.Width)));
            Canvas.SetTop(wall2, new Random().Next(0, (int)(canvas1.ActualHeight - wall2.Height)));

            time = 60 - (level - 1) * 10;
            timerTextBlock.Text = $"Time: {time} s";

            level++;
            scoresTextBlock.Text = $"Level: {level} Score: {score}";
        }
    }
}