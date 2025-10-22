using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace StowawayStorage.Services
{
    /// <summary>
    /// Minimal USPS RateV4 client for domestic rates.
    /// Uses config keys:
    ///   USPS:UserId
    ///   USPS:BaseUrl (e.g., https://secure.shippingapis.com/ShippingAPI.dll)
    ///   USPS:FromZip (your origin ZIP)
    /// </summary>
    public class USPSShippingService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public USPSShippingService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        /// <summary>
        /// Get a Priority Mail rate for a given destination ZIP and weight (ounces).
        /// Signature chosen to match typical controller scaffolds.
        /// </summary>
        public async Task<decimal> GetRateAsync(string toZip, int weightOz)
        {
            var userId = _config["USPS:UserId"];
            var baseUrl = _config["USPS:BaseUrl"] ?? "https://secure.shippingapis.com/ShippingAPI.dll";
            var fromZip = _config["USPS:FromZip"] ?? "00000";

            if (string.IsNullOrWhiteSpace(userId))
                throw new System.InvalidOperationException("USPS:UserId is missing from configuration.");

            // USPS RateV4 needs pounds + ounces
            if (weightOz < 1) weightOz = 1;
            var pounds = weightOz / 16;
            var ounces = weightOz % 16;

            // Build XML payload (Package ID must be unique; we just use 1)
            var requestXml =
                $@"<RateV4Request USERID=""{System.Security.SecurityElement.Escape(userId)}"">
                    <Revision>2</Revision>
                    <Package ID=""1"">
                        <Service>PRIORITY</Service>
                        <ZipOrigination>{fromZip}</ZipOrigination>
                        <ZipDestination>{toZip}</ZipDestination>
                        <Pounds>{pounds}</Pounds>
                        <Ounces>{ounces}</Ounces>
                        <Container>VARIABLE</Container>
                        <Size>REGULAR</Size>
                        <Width></Width>
                        <Length></Length>
                        <Height></Height>
                        <Girth></Girth>
                        <Machinable>true</Machinable>
                    </Package>
                  </RateV4Request>";

            // USPS wants: ?API=RateV4&XML=...
            var url = $"{baseUrl}?API=RateV4&XML={Uri.EscapeDataString(requestXml)}";

            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var xml = await resp.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xml);

            // Check for API-level error
            var error = doc.Root?.Element("Package")?.Element("Error") ?? doc.Root?.Element("Error");
            if (error != null)
            {
                var msg = error.Element("Description")?.Value ?? "USPS error";
                throw new System.Exception($"USPS Rate error: {msg}");
            }

            // Parse first postage/rate
            var rateText = doc.Root?
                .Element("Package")?
                .Element("Postage")?
                .Element("Rate")?.Value;

            if (string.IsNullOrWhiteSpace(rateText))
                throw new System.Exception("Unable to parse USPS rate.");

            if (!decimal.TryParse(rateText, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
                throw new System.Exception($"Invalid rate '{rateText}' returned by USPS.");

            return rate;
        }
    }
}
