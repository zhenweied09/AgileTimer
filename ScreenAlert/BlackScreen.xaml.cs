using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;

namespace ScreenReminder
{
	public partial class BlackScreen : Window
	{
		public MainWindow MainWindow { get; set; }

		public DateTime WindowOpenTime { get; set; }
		//public BlackScreen()
		//{
		//	InitializeComponent();
		//}

		public BlackScreen(string msg, MainWindow main)
		{
			InitializeComponent();
			MainWindow = main;
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
			if ((DateTime.Now - WindowOpenTime)>= MainWindow.BlackScreenTime)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					t.Enabled = false;
					Close();
					if (!MainWindow.IsClosed)
					{
						MainWindow.WindowState = WindowState.Minimized;
						MainWindow.ShowInTaskbar = true;
						MainWindow.Show();
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
			if (!MainWindow.IsClosed) {
				MainWindow.ShowInTaskbar = true;
				MainWindow.Show();
			}
			Close();
		}
	}
}
