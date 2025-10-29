using Application.Interface.Infrastructure;
using Application.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System.Net;

namespace Infrastructure.Services
{
    public class IpWhitelistingService : IIpWhitelistingService
    {
        private readonly List<string> _whitelistedIps;
        private readonly AppSettings _appSettings;
        public IpWhitelistingService (IConfiguration configuration, IOptions<AppSettings> appsettings)
        {
            _appSettings = appsettings.Value;
            var whitelistedIps = _appSettings.WhitelistedIPAddresses;
            _whitelistedIps = whitelistedIps.Split (',').ToList ();
        }
        public bool IsWhitelisted (IPAddress ipAddress)
        {
            return _whitelistedIps.Contains (ipAddress.ToString ());
        }
    }
}
