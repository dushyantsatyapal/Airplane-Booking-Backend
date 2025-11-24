// AirplaneBooking.Infrastructure/ExternalServices/AmadeusApiClient.cs

using AirplaneBooking.Application.DTOs;
using AirplaneBooking.Application.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AirplaneBooking.Infrastructure.ExternalServices
{
    public class AmadeusApiClient : IAmadeusService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AmadeusApiClient> _logger;
        private string _accessToken;
        private DateTime _tokenExpiration;
        private readonly object _lock = new object();

        public AmadeusApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<AmadeusApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.BaseAddress = new Uri(_configuration["Amadeus:BaseUrl"] ?? "https://test.api.amadeus.com");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task EnsureAuthenticatedAsync()
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow.AddMinutes(5))
                {
                    if (_httpClient.DefaultRequestHeaders.Authorization == null)
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                    }
                    return;
                }
            }
            _logger.LogInformation("Amadeus access token is missing or expired, refreshing...");
            var clientId = _configuration["Amadeus:ClientId"];
            var clientSecret = _configuration["Amadeus:ClientSecret"];
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("Amadeus API ClientId or ClientSecret is not configured.");
            }
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/security/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret }
                })
            };
            var response = await _httpClient.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
            lock (_lock)
            {
                _accessToken = tokenResponse.GetProperty("access_token").GetString();
                var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
                _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            _logger.LogInformation("Amadeus access token refreshed successfully.");
        }

        public async Task<IEnumerable<FlightOfferDto>> SearchFlightsAsync(FlightSearchRequestDto request)
        {
            await EnsureAuthenticatedAsync();
            var queryParams = new List<string>
            {
                $"originLocationCode={request.OriginLocationCode}",
                $"destinationLocationCode={request.DestinationLocationCode}",
                $"departureDate={request.DepartureDate:yyyy-MM-dd}",
                $"adults={request.Adults}"
            };
            if (request.ReturnDate.HasValue) queryParams.Add($"returnDate={request.ReturnDate.Value:yyyy-MM-dd}");
            if (request.Children > 0) queryParams.Add($"children={request.Children}");
            if (request.Infants > 0) queryParams.Add($"infants={request.Infants}");
            if (!string.IsNullOrWhiteSpace(request.TravelClass)) queryParams.Add($"travelClass={request.TravelClass.ToUpperInvariant()}");
            var queryString = string.Join("&", queryParams);
            _logger.LogInformation($"Sending Amadeus Flight Search request to: {_httpClient.BaseAddress}/v2/shopping/flight-offers?{queryString}");
            var response = await _httpClient.GetAsync($"/v2/shopping/flight-offers?{queryString}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Amadeus API returned an error ({response.StatusCode}): {errorContent}");
                throw new HttpRequestException($"Amadeus API request failed with status code {response.StatusCode}. Response: {errorContent}");
            }
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Amadeus API successful response content: {content}");

            var amadeusResponse = JsonSerializer.Deserialize<AmadeusFlightOffersResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var flightOffers = new List<FlightOfferDto>();
            if (amadeusResponse?.Data != null)
            {
                foreach (var offer in amadeusResponse.Data)
                {
                    var firstItinerary = offer.Itineraries.FirstOrDefault();
                    var firstSegment = firstItinerary?.Segments.FirstOrDefault();
                    if (firstSegment != null && offer.Price != null)
                    {
                        var rawOfferJson = JsonSerializer.Serialize(offer, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        flightOffers.Add(new FlightOfferDto
                        {
                            Id = offer.Id,
                            CarrierCode = firstSegment.CarrierCode,
                            FlightNumber = firstSegment.Number,
                            DepartureAirportCode = firstSegment.Departure.IataCode,
                            ArrivalAirportCode = firstSegment.Arrival.IataCode,
                            DepartureTime = firstSegment.Departure.At,
                            ArrivalTime = firstSegment.Arrival.At,
                            Price = decimal.TryParse(offer.Price.GrandTotal, out var price) ? price : 0,
                            Currency = offer.Price.Currency,
                            AvailableSeats = offer.NumberOfBookableSeats,
                            RawJsonOffer = rawOfferJson
                        });
                    }
                }
            }
            return flightOffers;
        }

        public async Task<BookingConfirmationDto> BookFlightAsync(string rawFlightOfferJson, List<PassengerDto> passengers)
        {
            // --- MODIFIED LOGIC: BYPASS ALL AMADEUS BOOKING/PRICING API CALLS ---
            _logger.LogInformation("Bypassing Amadeus booking API for test purposes.");

            // Generate a dummy PNR and confirmation DTO from the provided JSON
            var flightOfferJsonElement = JsonSerializer.Deserialize<JsonElement>(rawFlightOfferJson);
            var totalPriceString = flightOfferJsonElement.GetProperty("price").GetProperty("grandTotal").GetString();
            var totalPrice = decimal.Parse(totalPriceString);

            var dummyAmadeusReference = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            _logger.LogInformation($"Successfully simulated Amadeus booking. Reference: {dummyAmadeusReference}");

            return new BookingConfirmationDto
            {
                BookingId = Guid.NewGuid().ToString(),
                AmadeusBookingReference = dummyAmadeusReference,
                Status = "CONFIRMED",
                TotalPrice = totalPrice,
                BookingDate = DateTime.UtcNow
            };
        }

        public async Task<bool> CancelBookingAsync(string amadeusBookingReference)
        {
            await EnsureAuthenticatedAsync();
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/v1/booking/flight-orders/{amadeusBookingReference}");
            var response = await _httpClient.SendAsync(requestMessage);
            return response.IsSuccessStatusCode;
        }

        #region Internal DTOs for Amadeus API
        // This region contains all the internal DTOs used for deserializing Amadeus responses.
        // They are provided in full here to ensure a complete, compilable file.

        private class AmadeusFlightOffersResponse
        {
            [JsonPropertyName("data")]
            public List<AmadeusFlightOffer> Data { get; set; }
            [JsonPropertyName("dictionaries")]
            public AmadeusDictionaries Dictionaries { get; set; }
        }

        private class AmadeusFlightOffer
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("source")]
            public string Source { get; set; }
            [JsonPropertyName("instantTicketingRequired")]
            public bool InstantTicketingRequired { get; set; }
            [JsonPropertyName("nonHomogeneous")]
            public bool NonHomogeneous { get; set; }
            [JsonPropertyName("oneWayCombinable")]
            public bool OneWayCombinable { get; set; }
            [JsonPropertyName("lastTicketingDate")]
            public string LastTicketingDate { get; set; }
            [JsonPropertyName("lastTicketingDateTime")]
            public DateTime LastTicketingDateTime { get; set; }
            [JsonPropertyName("numberOfBookableSeats")]
            public int NumberOfBookableSeats { get; set; }
            [JsonPropertyName("itineraries")]
            public List<AmadeusItinerary> Itineraries { get; set; }
            [JsonPropertyName("price")]
            public AmadeusPrice Price { get; set; }
            [JsonPropertyName("travelerPricings")]
            public List<AmadeusTravelerPricing> TravelerPricings { get; set; }
        }

        private class AmadeusItinerary
        {
            [JsonPropertyName("duration")]
            public string Duration { get; set; }
            [JsonPropertyName("segments")]
            public List<AmadeusSegment> Segments { get; set; }
        }

        private class AmadeusSegment
        {
            [JsonPropertyName("departure")]
            public AmadeusAirportInfo Departure { get; set; }
            [JsonPropertyName("arrival")]
            public AmadeusAirportInfo Arrival { get; set; }
            [JsonPropertyName("carrierCode")]
            public string CarrierCode { get; set; }
            [JsonPropertyName("number")]
            public string Number { get; set; }
            [JsonPropertyName("aircraft")]
            public AmadeusAircraft Aircraft { get; set; }
            [JsonPropertyName("operating")]
            public AmadeusOperating Operating { get; set; }
            [JsonPropertyName("duration")]
            public string Duration { get; set; }
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("numberOfStops")]
            public int NumberOfStops { get; set; }
            [JsonPropertyName("blacklistedInEU")]
            public bool BlacklistedInEU { get; set; }
        }

        private class AmadeusAirportInfo
        {
            [JsonPropertyName("iataCode")]
            public string IataCode { get; set; }
            [JsonPropertyName("terminal")]
            public string Terminal { get; set; }
            [JsonPropertyName("at")]
            public DateTime At { get; set; }
        }

        private class AmadeusAircraft
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }
        }

        private class AmadeusOperating
        {
            [JsonPropertyName("carrierCode")]
            public string CarrierCode { get; set; }
        }

        private class AmadeusPrice
        {
            [JsonPropertyName("currency")]
            public string Currency { get; set; }
            [JsonPropertyName("total")]
            public string Total { get; set; }
            [JsonPropertyName("base")]
            public string Base { get; set; }
            [JsonPropertyName("fees")]
            public List<AmadeusFee> Fees { get; set; }
            [JsonPropertyName("grandTotal")]
            public string GrandTotal { get; set; }
        }

        private class AmadeusFee
        {
            [JsonPropertyName("amount")]
            public string Amount { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        private class AmadeusTravelerPricing
        {
            [JsonPropertyName("travelerId")]
            public string TravelerId { get; set; }
            [JsonPropertyName("fareOption")]
            public string FareOption { get; set; }
            [JsonPropertyName("travelerType")]
            public string TravelerType { get; set; }
            [JsonPropertyName("price")]
            public AmadeusPrice Price { get; set; }
            [JsonPropertyName("fareDetailsBySegment")]
            public List<AmadeusFareDetailsBySegment> FareDetailsBySegment { get; set; }
        }

        private class AmadeusFareDetailsBySegment
        {
            [JsonPropertyName("segmentId")]
            public string SegmentId { get; set; }
            [JsonPropertyName("cabin")]
            public string Cabin { get; set; }
            [JsonPropertyName("fareBasis")]
            public string FareBasis { get; set; }
            [JsonPropertyName("class")]
            public string Class { get; set; }
            [JsonPropertyName("isAllotment")]
            public bool IsAllotment { get; set; }
            [JsonPropertyName("legDetails")]
            public AmadeusLegDetails LegDetails { get; set; }
        }

        private class AmadeusLegDetails
        {
            [JsonPropertyName("departureDateTime")]
            public DateTime DepartureDateTime { get; set; }
            [JsonPropertyName("arrivalDateTime")]
            public DateTime ArrivalDateTime { get; set; }
            [JsonPropertyName("travelClass")]
            public string TravelClass { get; set; }
            [JsonPropertyName("amenities")]
            public List<AmadeusAmenity> Amenities { get; set; }
        }

        private class AmadeusAmenity
        {
            [JsonPropertyName("description")]
            public string Description { get; set; }
            [JsonPropertyName("isChargeable")]
            public bool IsChargeable { get; set; }
            [JsonPropertyName("amenityType")]
            public string AmenityType { get; set; }
            [JsonPropertyName("amenityProvider")]
            public AmadeusAmenityProvider AmenityProvider { get; set; }
        }

        private class AmadeusAmenityProvider
        {
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }
            [JsonPropertyName("carrierCode")]
            public string CarrierCode { get; set; }
        }

        private class AmadeusDictionaries
        {
            [JsonPropertyName("locations")]
            public Dictionary<string, AmadeusLocationInfo> Locations { get; set; }
            [JsonPropertyName("aircraft")]
            public Dictionary<string, string> Aircraft { get; set; }
            [JsonPropertyName("currencies")]
            public Dictionary<string, string> Currencies { get; set; }
            [JsonPropertyName("carriers")]
            public Dictionary<string, string> Carriers { get; set; }
        }

        private class AmadeusLocationInfo
        {
            [JsonPropertyName("cityCode")]
            public string CityCode { get; set; }
            [JsonPropertyName("countryCode")]
            public string CountryCode { get; set; }
        }

        private class AmadeusPriceConfirmationResponse
        {
            [JsonPropertyName("data")]
            public AmadeusPriceConfirmationData Data { get; set; }
            [JsonPropertyName("meta")]
            public AmadeusMeta Meta { get; set; }
            [JsonPropertyName("warnings")]
            public List<AmadeusWarning> Warnings { get; set; }
        }

        private class AmadeusPriceConfirmationData
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("flightOffers")]
            public List<AmadeusFlightOffer> FlightOffers { get; set; }
        }

        private class AmadeusMeta
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }
        }

        private class AmadeusWarning
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }
            [JsonPropertyName("title")]
            public string Title { get; set; }
            [JsonPropertyName("detail")]
            public string Detail { get; set; }
        }

        private class AmadeusBookingCreateOrderResponse
        {
            [JsonPropertyName("data")]
            public AmadeusFlightOrder Data { get; set; }
        }

        private class AmadeusFlightOrder
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("queuingOfficeId")]
            public string QueuingOfficeId { get; set; }
            [JsonPropertyName("associatedRecords")]
            public List<AmadeusAssociatedRecord> AssociatedRecords { get; set; }
            [JsonPropertyName("travelers")]
            public List<AmadeusTraveler> Travelers { get; set; }
            [JsonPropertyName("flightOffers")]
            public List<AmadeusFlightOffer> FlightOffers { get; set; }
            [JsonPropertyName("remarks")]
            public AmadeusRemarks Remarks { get; set; }
            [JsonPropertyName("ticketingAgreement")]
            public AmadeusTicketingAgreement TicketingAgreement { get; set; }
            [JsonPropertyName("contacts")]
            public List<AmadeusContact> Contacts { get; set; }
            [JsonPropertyName("documents")]
            public List<AmadeusDocument> Documents { get; set; }
            [JsonPropertyName("formOfPayments")]
            public List<AmadeusFormOfPayment> FormOfPayments { get; set; }
            [JsonPropertyName("payments")]
            public List<AmadeusPayment> Payments { get; set; }
            [JsonPropertyName("changes")]
            public List<AmadeusChange> Changes { get; set; }
            [JsonPropertyName("cancellations")]
            public List<AmadeusCancellation> Cancellations { get; set; }
            [JsonPropertyName("version")]
            public string Version { get; set; }
            [JsonPropertyName("creationDate")]
            public string CreationDate { get; set; }
            [JsonPropertyName("price")]
            public AmadeusPrice Pricing { get; set; }
        }

        private class AmadeusAssociatedRecord
        {
            [JsonPropertyName("reference")]
            public string Reference { get; set; }
            [JsonPropertyName("creationDate")]
            public string CreationDate { get; set; }
            [JsonPropertyName("originSystemCode")]
            public string OriginSystemCode { get; set; }
            [JsonPropertyName("flightOfferId")]
            public string FlightOfferId { get; set; }
        }

        private class AmadeusTraveler
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("dateOfBirth")]
            public string DateOfBirth { get; set; }
            [JsonPropertyName("name")]
            public AmadeusTravelerName Name { get; set; }
            [JsonPropertyName("contact")]
            public AmadeusContact Contact { get; set; }
            [JsonPropertyName("documents")]
            public List<AmadeusDocument> Documents { get; set; }
        }

        private class AmadeusTravelerName
        {
            [JsonPropertyName("firstName")]
            public string FirstName { get; set; }
            [JsonPropertyName("lastName")]
            public string LastName { get; set; }
        }

        private class AmadeusContact
        {
            [JsonPropertyName("emailAddress")]
            public string EmailAddress { get; set; }
            [JsonPropertyName("phones")]
            public List<AmadeusPhone> Phones { get; set; }
        }

        private class AmadeusPhone
        {
            [JsonPropertyName("deviceType")]
            public string DeviceType { get; set; }
            [JsonPropertyName("countryCallingCode")]
            public string CountryCallingCode { get; set; }
            [JsonPropertyName("number")]
            public string Number { get; set; }
        }

        private class AmadeusDocument
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("number")]
            public string Number { get; set; }
            [JsonPropertyName("expiryDate")]
            public string ExpiryDate { get; set; }
            [JsonPropertyName("issuanceDate")]
            public string IssuanceDate { get; set; }
            [JsonPropertyName("issuanceLocation")]
            public string IssuanceLocation { get; set; }
            [JsonPropertyName("issuanceCountry")]
            public string IssuanceCountry { get; set; }
            [JsonPropertyName("nationality")]
            public string Nationality { get; set; }
            [JsonPropertyName("holder")]
            public bool Holder { get; set; }
            [JsonPropertyName("birthPlace")]
            public string BirthPlace { get; set; }
            [JsonPropertyName("gender")]
            public string Gender { get; set; }
            [JsonPropertyName("airlineCheckInRequired")]
            public bool AirlineCheckInRequired { get; set; }
        }

        private class AmadeusRemarks
        {
            [JsonPropertyName("general")]
            public List<AmadeusRemark> General { get; set; }
        }

        private class AmadeusRemark
        {
            [JsonPropertyName("subType")]
            public string SubType { get; set; }
            [JsonPropertyName("category")]
            public string Category { get; set; }
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        private class AmadeusTicketingAgreement
        {
            [JsonPropertyName("option")]
            public string Option { get; set; }
            [JsonPropertyName("date")]
            public string Date { get; set; }
        }

        private class AmadeusFormOfPayment
        {
            [JsonPropertyName("other")]
            public AmadeusOtherPayment Other { get; set; }
        }

        private class AmadeusOtherPayment
        {
            [JsonPropertyName("method")]
            public string Method { get; set; }
            [JsonPropertyName("flightOfferId")]
            public string FlightOfferId { get; set; }
        }

        private class AmadeusPayment
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("method")]
            public string Method { get; set; }
            [JsonPropertyName("amount")]
            public string Amount { get; set; }
            [JsonPropertyName("currency")]
            public string Currency { get; set; }
        }

        private class AmadeusChange
        {
            [JsonPropertyName("flightOfferIds")]
            public List<string> FlightOfferIds { get; set; }
            [JsonPropertyName("travelerIds")]
            public List<string> TravelerIds { get; set; }
            [JsonPropertyName("action")]
            public string Action { get; set; }
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }

        private class AmadeusCancellation
        {
            [JsonPropertyName("flightOfferIds")]
            public List<string> FlightOfferIds { get; set; }
            [JsonPropertyName("travelerIds")]
            public List<string> TravelerIds { get; set; }
            [JsonPropertyName("action")]
            public string Action { get; set; }
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }

        #endregion
    }
}