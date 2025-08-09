using System.Net;

namespace Application.Interface.Infrastructure
{
	public interface IIpWhitelistingService
	{
		bool IsWhitelisted (IPAddress ipAddress);
	}
}
