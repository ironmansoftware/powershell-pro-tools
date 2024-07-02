using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace PowerShellToolsPro.Analytics
{
	public interface IAnalyticsClientFactory
	{
		IAnalyticsClient Client { get; }
	}

	public interface IAnalyticsClient
	{
		void Event(string eventType, string eventAction);
	}

	[Export(typeof(IAnalyticsClientFactory))]
	public class AnalyticsClientFactory : IAnalyticsClientFactory
	{
		private readonly bool _noopClient;

		public AnalyticsClientFactory(bool noopClient)
		{
			_noopClient = noopClient;
		}

		public AnalyticsClientFactory() { }

		public IAnalyticsClient Client
		{
			get
			{
				if (_noopClient) return new NoopAnalyticsClient();
				return new AnalyticsClient();
			}
		}

		public bool Enabled
		{
			get
			{
				using (var key = Registry.CurrentUser.CreateSubKey(@"Software\PowerShellProTools"))
				{
					var enabled = (int)key.GetValue("UsageCollection", 1);
					return enabled == 1;
				}
			}
			set
			{
				using (var key = Registry.CurrentUser.CreateSubKey(@"Software\PowerShellProTools"))
				{
					key.SetValue("UsageCollection", value ? 1 : 0);
				}
			}
		}
	}

	public class NoopAnalyticsClient : IAnalyticsClient
	{
		public void Event(string eventType, string eventAction)
		{
		}
	}

	public class AnalyticsClient : IAnalyticsClient
	{
		private static readonly Guid _clientId;

		static AnalyticsClient()
		{
			_clientId = Guid.NewGuid();
		}

		private const string PropertyId = "UA-8902901-9";
		private const string GoogleAnalyticsUrl = "https://www.google-analytics.com/collect";
		private const string ProtocolVersion = "1";
		public void Event(string type, string eventAction)
		{
			var payload = $"v={ProtocolVersion}&t=event&cid={_clientId}&tid={PropertyId}&ec={type}&ea={eventAction}";
			SendRequest(payload);
		}

		private static void SendRequest(string payload)
		{
			Task.Run(() =>
			{
				try
				{
					var webClient = new WebClient();
					webClient.UploadString(GoogleAnalyticsUrl, payload);
				}
				catch
				{
					
				}
			});
		}

	}
}
