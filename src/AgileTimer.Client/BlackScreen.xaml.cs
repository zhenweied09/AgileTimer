using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;

namespace AgileTimer.Client
{
	public partial class BlackScreen : Window
	{
		public SettingWindow SettingWindow { get; set; }

		public DateTime WindowOpenTime { get; set; }
		//public BlackScreen()
		//{
		//	InitializeComponent();
		//}

		public BlackScreen(string msg, SettingWindow main)
		{
			InitializeComponent();
            SettingWindow = main;
			WindowOpenTime = DateTime.Now;
			NewMethod(msg);
			closeBlackScreen();
		}

		public Timer t = new Timer
		{
			Interval = 1000
		};

		private void closeBlackScreen()
		{
			t.Enabled = true;
			t.Elapsed += T_Elapsed;
		}

		private void T_Elapsed(object sender, ElapsedEventArgs e)
		{
			if ((DateTime.Now - WindowOpenTime)>= SettingWindow.BlackScreenTime)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					t.Enabled = false;
					Close();
					if (!SettingWindow.IsClosed)
					{
                        SettingWindow.WindowState = WindowState.Minimized;
                        SettingWindow.ShowInTaskbar = true;
                        SettingWindow.Show();
					}
				}));
			}
		}


		private void NewMethod(string msg)
		{
			WindowStyle = WindowStyle.None;
			WindowState = WindowState.Maximized;
			Background = Brushes.Black;
			lablAlertMessage.Content = msg;
			lablAlertMessage.Foreground = Brushes.Red;
			lablAlertMessage.FontSize = 100;
			BitmapImage image = new BitmapImage(new Uri("Resources/setting.jpg", UriKind.Relative));
			SettingImg.Source = image;
			SettingImg.ToolTip = "Settings";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!SettingWindow.IsClosed) {
                SettingWindow.ShowInTaskbar = true;
                SettingWindow.Show();
			}
			Close();
		}
	}
}
