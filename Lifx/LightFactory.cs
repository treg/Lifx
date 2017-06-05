using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Lifx.Communication;
using Lifx.Communication.Requests;
using Lifx.Communication.Responses;
using Lifx.Communication.Responses.Payloads;

namespace Lifx
{
	public sealed class LightFactory
	{
		private const int DefaultPort = 56700;

		private static readonly TimeSpan ResponseExpiry = TimeSpan.FromSeconds(5);
		private static readonly IResponseParser ResponseParser = new ResponseParser(
			new StateVersionResponsePayloadParser(),
			new StateResponsePayloadParser()
		);

		private readonly int _port;

		public LightFactory(int port)
		{
			_port = port;
		}

		public LightFactory() : this(DefaultPort) { }

		public async Task<ILight> CreateLightAsync(IPAddress address)
		{
			return await CreateLightAsync(address, CancellationToken.None);
		}

		public async Task<ILight> CreateLightAsync(IPAddress address, CancellationToken cancellationToken)
		{
			if (address == null)
			{
				throw new ArgumentNullException(nameof(address));
			}

			var endPoint = new IPEndPoint(address, _port);
			var communicator = new Communicator(ResponseParser, endPoint, ResponseExpiry);
			var requestFactory = new RequestFactory();

			var request = requestFactory.CreateGetVersionRequest();
			var payload = await communicator.CommunicateAsync<StateVersionResponsePayload>(request, cancellationToken);

			return new Light(address, payload.Product, payload.Version, communicator, requestFactory);
		}
	}
}
