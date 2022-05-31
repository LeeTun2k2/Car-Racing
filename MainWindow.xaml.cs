using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows.Threading;

namespace Car_Racing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer gameTimer = new DispatcherTimer();
        List<Rectangle> itemRemover = new List<Rectangle>();

        Random rand = new Random();

        ImageBrush playerImage = new ImageBrush();
        ImageBrush starImage = new ImageBrush();

        Rect playerHitBox;

        // set the game info
        int speed = 15;
        int playerSpeed = 10;
        int carNum;
        int starCounter = 30;
        int powerModeCounter = 200;

        // counter
        double score;
        double i;
        double highest;
        private double crossSpeed = 810;

        // Game event
        bool moveLeft, moveRight, powerMode;

        // Music

        public MainWindow()
        {
            InitializeComponent();

            myCanvas.Focus();

            gameTimer.Tick += GameLoop!; // link timer to game loop
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);

            if (!this.readData())
                this.highest = 0;

            this.lbRecord.Content += this.highest.ToString();

            playSound();
        }

        private void playSound()
        {
            sound.Source = new Uri(System.IO.Path.GetFullPath(@"./assets/sound/sound.mp3"));
            sound.Play();
        }

        private bool readData()
        {
            bool check = false;
            try
            {
                int number;
                check = int.TryParse(System.IO.File.ReadAllText(@"./HighestScore.txt"), out number);
                this.highest = number;
            }
            catch
            {

            }

            return check;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            score += .05;

            starCounter -= 1;

            scoreText.Content = $"Highest: {this.highest}\nScore: {score.ToString("#.#")}";

            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            if (moveLeft == true && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight == true && Canvas.GetLeft(player) + 90 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }

            if (starCounter < 1)
            {
                MakeStar();
                starCounter = rand.Next(600, 900);
            }

            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                if ((string)x.Tag == "roadMarks")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + speed);
                    if (Canvas.GetTop(x) > this.crossSpeed)
                    {
                        Canvas.SetTop(x, -150);
                    }
                }
                else if ((string)x.Tag == "Car")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + speed);
                    if (Canvas.GetTop(x) > this.Height)
                    {
                        ChangeCars(x);
                    }
                    Rect carHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (playerHitBox.IntersectsWith(carHitBox) && powerMode == true)
                    {
                        ChangeCars(x);
                    }
                    else if (playerHitBox.IntersectsWith(carHitBox) && powerMode == false)
                    {
                        menu.Visibility = Visibility.Visible;
                        if (this.score > this.highest)
                        {
                            this.highest = (int)this.score;
                            this.lbRecord.Content = "Top record: " + this.highest.ToString();
                            System.IO.File.WriteAllText("./HighestScore.txt", ((int)this.score).ToString());
                        }

                        foreach (var child in myCanvas.Children.OfType<Rectangle>())
                        {
                            if ((string)child.Tag == "star")
                            {
                                child.Visibility = Visibility.Hidden;
                            }
                        }
                        gameTimer.Stop();
                    }
                }
                else if ((string)x.Tag == "star")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 5);

                    Rect starHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (playerHitBox.IntersectsWith(starHitBox))
                    {
                        itemRemover.Add(x);

                        powerMode = true;

                        powerModeCounter = 200;

                    }

                    if (Canvas.GetTop(x) > this.Height)
                    {
                        itemRemover.Add(x);
                    }

                }
            }

            if (powerMode == true)
            {
                powerModeCounter -= 1;
                PowerUp();
                if (powerModeCounter < 1)
                {
                    powerMode = false;
                }
            }
            else
            {
                playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/playerImage.png"));
                myCanvas.Background = Brushes.Gray;
            }

            foreach (Rectangle y in itemRemover)
            {
                myCanvas.Children.Remove(y);
            }

            if ((int)score % 6 == 5 && score - (int)score > 0.95)
            {
                this.speed++;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }
        }

        private void OnKeyUP(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }
        }

        private void StartGame()
        {

            speed = 8;
            gameTimer.Start();

            moveLeft = false;
            moveRight = false;
            powerMode = false;

            score = 0;
            scoreText.Content = "Score: 0";
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/playerImage.png"));
            starImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/star.png"));
            player.Fill = playerImage;
            myCanvas.Background = Brushes.Gray;


            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                if ((string)x.Tag == "Car")
                {
                    Canvas.SetTop(x, (rand.Next(100, 900) * -1));
                    Canvas.SetLeft(x, rand.Next(0, 900));
                    ChangeCars(x);
                }

                if ((string)x.Tag == "star")
                {
                    itemRemover.Add(x);
                }

            }
            itemRemover.Clear();
        }

        private void ChangeCars(Rectangle car)
        {

            carNum = rand.Next(1, 6);

            ImageBrush carImage = new ImageBrush();

            switch (carNum)
            {
                case 1:
                    carImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/car1.png"));
                    break;
                case 2:
                    carImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/car2.png"));
                    break;
                case 3:
                    carImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/car3.png"));
                    break;
                case 4:
                    carImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/car4.png"));
                    break;
                case 5:
                    carImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/car5.png"));
                    break;
                case 6:
                    carImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/car6.png"));
                    break;
            }

            car.Fill = carImage;
            int top = (rand.Next(100, 900) * -1);
            int left = rand.Next(0, 880);
            Canvas.SetTop(car, top);
            Canvas.SetLeft(car, left);
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            this.menu.Visibility = Visibility.Hidden;
            StartGame();
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            sound.Stop();
            Application.Current.Shutdown();
        }

        private void PowerUp()
        {

            i += .5;


            if (i > 4)
            {
                i = 1;
            }


            switch (i)
            {
                case 1:
                    playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/powermode1.png"));
                    break;
                case 2:
                    playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/powermode2.png"));
                    break;
                case 3:
                    playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/powermode3.png"));
                    break;
                case 4:
                    playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/powermode4.png"));
                    break;
            }

            if ((int)(score * 10) % 40 < 10)
                myCanvas.Background = Brushes.DarkCyan;
            else if ((int)(score * 10) % 40 < 20)
                myCanvas.Background = Brushes.LightCoral;
            else if ((int)(score * 10) % 40 < 30)
                myCanvas.Background = Brushes.Orange;
            else
                myCanvas.Background = Brushes.LightGreen;

        }

        private void MakeStar()
        {
            Rectangle newStar = new Rectangle
            {
                Height = 50,
                Width = 50,
                Tag = "star",
                Fill = starImage
            };

            Canvas.SetLeft(newStar, rand.Next(0, 900));
            Canvas.SetTop(newStar, (rand.Next(100, 900) * -1));

            myCanvas.Children.Add(newStar);

        }
    }
}
