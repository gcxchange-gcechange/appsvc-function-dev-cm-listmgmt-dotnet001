﻿using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public static class ClaimsPrincipalParser
{
    private class ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public string Type { get; set; }
        [JsonPropertyName("val")]
        public string Value { get; set; }
    }

    private class ClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public string IdentityProvider { get; set; }
        [JsonPropertyName("name_typ")]
        public string NameClaimType { get; set; }
        [JsonPropertyName("role_typ")]
        public string RoleClaimType { get; set; }
        [JsonPropertyName("claims")]
        public IEnumerable<ClientPrincipalClaim> Claims { get; set; }
    }

    public static bool CanUpdate(HttpRequest req, string ContactEmail, ILogger logger)
    {
        string UserEmail = GetUserEmail(req, logger);
        return UserEmail.ToLower() == ContactEmail.ToLower();
    }

    public static string GetUserEmail(HttpRequest req, ILogger logger)
    {
        ClaimsPrincipal principal = ClaimsPrincipalParser.Parse(req);

        string email;

        try
        {
            // non-guest user email is stored in the Upn claim
            Claim claim = principal.FindFirst(ClaimTypes.Upn);

            if (claim == null)
            {
                // look for guest user email under a different claim
                claim = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            }

            if (claim != null)
            {
                email = claim?.Value.ToString();
            }
            else
            {
                email = "";
                logger.LogWarning("Could not get email address from claims.");
            }     
        }
        catch (Exception ex)
        {
            email = "";
            logger.LogInformation($"Error getting Upn claim: {ex.Message}");
        }

        return email;
    }

    public static ClaimsPrincipal Parse(HttpRequest req)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /** 
         *  At this point, the code can iterate through `principal.Claims` to
         *  check claims as part of validation. Alternatively, you can convert
         *  it into a standard object with which to perform those checks later
         *  in the request pipeline. That object can also be leveraged for 
         *  associating user data, and so on. The rest of this function performs such
         *  a conversion to create a `ClaimsPrincipal` as might be used in 
         *  other .NET code.
         */

        var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);
        identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));

        return new ClaimsPrincipal(identity);
    }
}