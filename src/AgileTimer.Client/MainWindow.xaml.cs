using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;

namespace AgileTimer.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer;
        private TimeSpan _timeSpan = TimeSpan.FromMinutes(15);
        private bool _timerStarted;
        private HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                if (_timeSpan.TotalDays > 0)
                {
                    _timeSpan -= TimeSpan.FromSeconds(1);
                }
                else
                {
                    _timer.Stop();
                    System.Media.SystemSounds.Beep.Play();
                }

                TimerLabel.Dispatcher.Invoke(() => { TimerLabel.Content = _timeSpan.ToString(@"mm\:ss"); });
            };

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/NotificationHub")
                .Build();
            connection.On<string>("ReceivedToggleTimerMessage", (any)=> OnToggleTimerClicked(null,null));
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            connection.StartAsync();
        }

        private void OnAddMobberClicked(object sender, RoutedEventArgs e)
        {
            MobberPanel.Children.Add(new Button()
            {
                Content = "test mobber",
                FontSize = 48
            });
        }

        private void OnToggleTimerClicked(object sender, RoutedEventArgs e)
        {
            if (!_timerStarted)
            {
                startTimer();
            }
            else
            {
                stopTimer();
            }
        }

        private void OnResetTimerClicked(object sender, RoutedEventArgs e)
        {
            stopTimer();
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {
            var settingWindow = new SettingWindow();
            settingWindow.Show();
        }

        private void startTimer()
        {
            _timeSpan = TimeSpan.FromMinutes(15);
            _timer.Start();
            _timerStarted = true;

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    ToggleTimerButton.Content = "Stop";
                    TimerLabel.Content = _timeSpan.ToString(@"mm\:ss");
                });
            }
            else
            {
                ToggleTimerButton.Content = "Stop";
                TimerLabel.Content = _timeSpan.ToString(@"mm\:ss");
            }
        }
        private void stopTimer()
        {
            _timer.Stop();
            _timerStarted = false;
            _timeSpan = TimeSpan.FromMinutes(15);
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    ToggleTimerButton.Content = "Start";
                    TimerLabel.Content = _timeSpan.ToString(@"mm\:ss");
                });
            }
            else
            {
                ToggleTimerButton.Content = "Start";
                TimerLabel.Content = _timeSpan.ToString(@"mm\:ss");
            }
        }
    }
}
