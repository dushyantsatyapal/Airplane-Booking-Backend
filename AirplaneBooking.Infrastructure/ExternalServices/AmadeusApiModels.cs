
namespace AirplaneBooking.Infrastructure.ExternalServices;

// AirplaneBooking.Infrastructure/ExternalServices/AmadeusApiModels.cs

using System.Text.Json.Serialization;

    // --- Flight Offers Search API Models ---
    public class AmadeusFlightOffersResponse
    {
        [JsonPropertyName("data")]
        public List<AmadeusFlightOffer> Data { get; set; }

        [JsonPropertyName("dictionaries")]
        public AmadeusDictionaries Dictionaries { get; set; }
    }

    public class AmadeusFlightOffer
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

    public class AmadeusItinerary
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; }
        [JsonPropertyName("segments")]
        public List<AmadeusSegment> Segments { get; set; }
    }

    public class AmadeusSegment
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

    public class AmadeusAirportInfo
    {
        [JsonPropertyName("iataCode")]
        public string IataCode { get; set; }
        [JsonPropertyName("terminal")]
        public string Terminal { get; set; }
        [JsonPropertyName("at")] // THIS IS THE PROPERTY YOUR CLIENT CODE IS LOOKING FOR
        public DateTime At { get; set; }
    }

    public class AmadeusAircraft
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    public class AmadeusOperating
    {
        [JsonPropertyName("carrierCode")]
        public string CarrierCode { get; set; }
    }

    public class AmadeusPrice
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

    public class AmadeusFee
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class AmadeusTravelerPricing
    {
        [JsonPropertyName("travelerId")]
        public string TravelerId { get; set; }
        [JsonPropertyName("fareOption")]
        public string FareOption { get; set; }
        [JsonPropertyName("travelerType")]
        public string TravelerType { get; set; }
        [JsonPropertyName("price")]
        public AmadeusPrice Price { get; set; } // Can reuse AmadeusPrice
        [JsonPropertyName("fareDetailsBySegment")]
        public List<AmadeusFareDetailsBySegment> FareDetailsBySegment { get; set; }
    }

    public class AmadeusFareDetailsBySegment
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

    public class AmadeusLegDetails
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

    public class AmadeusAmenity
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

    public class AmadeusAmenityProvider
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [JsonPropertyName("carrierCode")]
        public string CarrierCode { get; set; }
    }

    public class AmadeusDictionaries
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

    public class AmadeusLocationInfo
    {
        [JsonPropertyName("cityCode")]
        public string CityCode { get; set; }
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }
    }

    // --- Flight Offers Price API Models ---
    public class AmadeusPriceConfirmationResponse
    {
        [JsonPropertyName("data")]
        public AmadeusPriceConfirmationData Data { get; set; }
        [JsonPropertyName("meta")]
        public AmadeusMeta Meta { get; set; }
        [JsonPropertyName("warnings")]
        public List<AmadeusWarning> Warnings { get; set; }
    }

    public class AmadeusPriceConfirmationData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("flightOffers")]
        public List<AmadeusFlightOffer> FlightOffers { get; set; } // Reuses the FlightOffer model
    }

    public class AmadeusMeta
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
        // Other meta properties
    }

    public class AmadeusWarning
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("detail")]
        public string Detail { get; set; }
        // Other warning properties
    }

    // --- Flight Create Orders API Models ---
    public class AmadeusBookingCreateOrderResponse
    {
        [JsonPropertyName("data")]
        public AmadeusFlightOrder Data { get; set; }
        // Other properties like warnings, errors etc.
    }

    public class AmadeusFlightOrder
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; } // This is the PNR / booking reference
        [JsonPropertyName("queuingOfficeId")]
        public string QueuingOfficeId { get; set; }
        [JsonPropertyName("associatedRecords")]
        public List<AmadeusAssociatedRecord> AssociatedRecords { get; set; }
        [JsonPropertyName("travelers")]
        public List<AmadeusTraveler> Travelers { get; set; }
        [JsonPropertyName("flightOffers")]
        public List<AmadeusFlightOffer> FlightOffers { get; set; } // Reuses FlightOffer
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
        public AmadeusPrice Pricing { get; set; } // Renamed to avoid clash with other 'Price'
    }

    public class AmadeusAssociatedRecord
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

    public class AmadeusTraveler
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

    public class AmadeusTravelerName
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }

    public class AmadeusContact
    {
        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }
        [JsonPropertyName("phones")]
        public List<AmadeusPhone> Phones { get; set; }
    }

    public class AmadeusPhone
    {
        [JsonPropertyName("deviceType")]
        public string DeviceType { get; set; }
        [JsonPropertyName("countryCallingCode")]
        public string CountryCallingCode { get; set; }
        [JsonPropertyName("number")]
        public string Number { get; set; }
    }

    public class AmadeusDocument
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

    public class AmadeusRemarks
    {
        [JsonPropertyName("general")]
        public List<AmadeusRemark> General { get; set; }
        // Other types of remarks
    }

    public class AmadeusRemark
    {
        [JsonPropertyName("subType")]
        public string SubType { get; set; }
        [JsonPropertyName("category")]
        public string Category { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class AmadeusTicketingAgreement
    {
        [JsonPropertyName("option")]
        public string Option { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
    }

    public class AmadeusFormOfPayment
    {
        [JsonPropertyName("other")]
        public AmadeusOtherPayment Other { get; set; }
        // Other payment forms
    }

    public class AmadeusOtherPayment
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("flightOfferId")]
        public string FlightOfferId { get; set; }
    }

    public class AmadeusPayment
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

    public class AmadeusChange
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

    public class AmadeusCancellation
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