using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
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

    public static bool CanUpdate(HttpRequestData req, string ContactEmail, ILogger logger)
    {
        string UserEmail = GetUserEmail(req, logger);
        return UserEmail.ToLower() == ContactEmail.ToLower();
    }

    public static string GetUserEmail(HttpRequestData req, ILogger logger)
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

        //logger.LogError($"GetUserEmail: email = {email}");

        return email;
    }

    public static ClaimsPrincipal Parse(HttpRequestData req, ILogger logger)
    {
        ClientPrincipal principal = null;

        if (req.Headers.TryGetValues("x-ms-client-principal", out var headerValues))
        {
            var data = headerValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(decoded);
                    principal = JsonSerializer.Deserialize<ClientPrincipal>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error parsing client principal: {ex.Message}");
                }
            }
            else
            {
                logger.LogError("Header x-ms-client-principal is empty");
            }
        }
        else
        {
            logger.LogError("Could not find header x-ms-client-principal");
        }

        if (principal == null)
        {
            logger.LogError("Parse: principal is null");
            principal = new ClientPrincipal(); // fallback to empty principal
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
            }
        }

        // Convert to standard ClaimsPrincipal
        var identity = new ClaimsIdentity(
            principal.IdentityProvider,
            principal.NameClaimType,
            principal.RoleClaimType);

        identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));

        return new ClaimsPrincipal(identity);
    }
}