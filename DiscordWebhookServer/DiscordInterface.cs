using System;

namespace DiscordWebsocketServer
{
	public abstract class DiscordInterface
	{
		public string self_id;
		public string session_id;
		public event EventHandler<object> RespondWebsocketHandler;
		public event EventHandler<HTTPEventArgs> RespondHTTPHandler;
		protected virtual void OnRespondWebsocket(object obj) { RespondWebsocketHandler?.Invoke(this, obj); }
		protected virtual void OnRespondHTTP(object obj, Uri uri) { RespondHTTPHandler?.Invoke(this, new HTTPEventArgs() { uri = uri, data = obj }); }
		abstract public void Process(Payload payload, string rawData);

		public class HTTPEventArgs : EventArgs
		{
			public Uri uri;
			public object data;
		}
	}
}
