using System.Security.Claims;
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
        logger.LogError($"HttpRequest req = {req}");

        ClaimsPrincipal principal = ClaimsPrincipalParser.Parse(req, logger);

        logger.LogError($"Is the principal null? {principal == null}");

        string email;

        try
        {
            // non-guest user email is stored in the Upn claim
            Claim claim = principal.FindFirst(ClaimTypes.Upn);

            if (claim == null)
            {
                logger.LogError($"Upn claim is null, check for emailaddress...");
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
                logger.LogError("Could not get email address from claims.");
            }
        }
        catch (Exception ex)
        {
            email = "";
            logger.LogError($"Error getting Upn claim: {ex.Message}");
        }

        logger.LogError($"GetUserEmail: email = {email}");

        return email;
    }

    public static ClaimsPrincipal Parse(HttpRequest req, ILogger logger)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            logger.LogError("Could not find header x-ms-client-principal");
        }


        logger.LogError("Headers:");
        foreach (var h in req.Headers)
        {
            logger.LogError($"{h.Key} = {h.Value}");
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

        if (principal == null)
        {
            logger.LogError("Parse: principal is null");
        }
        else
        {
            logger.LogError("Claims:");
            logger.LogError($"principal.IdentityProvider = {principal.IdentityProvider}");
            logger.LogError($"principal.NameClaimType = {principal.NameClaimType}");
            logger.LogError($"principal.RoleClaimType = {principal.RoleClaimType}");

            foreach (var c in principal.Claims)
            {
                logger.LogError($"{c.Type} = {c.Value}");
                //try {  }
                //catch (Exception e)
                //{
                //    logger.LogError("error showing claim");
                //}
            }
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);
        identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));

        return new ClaimsPrincipal(identity);
    }
}