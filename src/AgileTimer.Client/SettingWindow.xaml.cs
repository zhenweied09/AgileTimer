using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace AgileTimer.Client
{
	public partial class SettingWindow : Window
	{
		public List<SettingDto> AllMessages { get; set; }
		public Timer t = new Timer
		{
			Interval = 10000 
		};
		public SettingWindow()
		{
			InitializeComponent();
		}
		private const string fileName = "list.txt";
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var list = new List<SettingDto>();
			DateTime alertTime;
			var alertTimeLength = int.Parse(txtLength.Text.Trim());
			if (alertTimeLength > 0) {
				alertTime = DateTime.Now.AddMinutes(alertTimeLength);
			}
			else {
			alertTime = DateTime.Now.AddSeconds(10);
			}
			var dto = new SettingDto() { Message = txtMessage.Text.Trim(), AlertTime = alertTime };
			SerializeObject(dto, fileName);
			list.Add(dto);
			AllMessages = list;
			TimeSpan blackScreenTime;
			var blackScreenTimeLength = int.Parse(txtBlackScreenLength.Text.Trim());
			if (blackScreenTimeLength > 0)
			{
				blackScreenTime = TimeSpan.FromMinutes(blackScreenTimeLength);
			}
			else
			{
				blackScreenTime = TimeSpan.FromSeconds(10);
			}
			BlackScreenTime = blackScreenTime;
			Hide();
			calcute();
		}

		private void calcute()
		{
			if (AllMessages.Count > 0)
			{
				t.Enabled = true;
				t.Elapsed += T_Elapsed;				
			}
		}

		private void T_Elapsed(object sender, ElapsedEventArgs e)
		{
			var msg = AllMessages[0];
			if (msg.AlertTime <= DateTime.Now)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					BlackScreen s = new BlackScreen(msg.Message, this);
					s.Show();
					t.Enabled = false;
				}));
			}
		}

		public bool IsClosed { get; private set; }
		public TimeSpan BlackScreenTime { get; set; }

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			IsClosed = true;
		}

		public void SerializeObject<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) { return; }

			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
				using (var stream = new FileStream(fileName, FileMode.Append))
				{
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
				}
			}
			catch (Exception ex)
			{
			}
		}
	}
	[Serializable]
	public class SettingDto
	{
		public string Message { get; set; }
		public DateTime AlertTime { get; set; }
	}
}
