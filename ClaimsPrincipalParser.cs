using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.Security;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        string email = "";

        //logger.LogError($"HttpRequest req = {req}");

        //foreach (var header in req.Headers)
        //{
        //    string name = header.Key;
        //    var values = header.Value; // StringValues; may contain multiple values

        //    logger.LogError($"{name}: {string.Join(", ", values)}");
        //}


        //ClaimsPrincipal principal = ClaimsPrincipalParser.Parse(req, logger);
        //logger.LogError($"Is the principal null? {principal == null}");

        //try
        //{
        //    // non-guest user email is stored in the Upn claim
        //    Claim claim = principal.FindFirst(ClaimTypes.Upn);

        //    if (claim == null)
        //    {
        //        logger.LogError($"Upn claim is null, check for emailaddress...");
        //        // look for guest user email under a different claim
        //        claim = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        //    }

        //    if (claim != null)
        //    {
        //        email = claim?.Value.ToString();
        //    }
        //    else
        //    {
        //        email = "";
        //        logger.LogError("Could not get email address from claims.");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    email = "";
        //    logger.LogError($"Error getting Upn claim: {ex.Message}");
        //}

        string uniqueName = "";
        string upn = "";

        logger.LogError("Check Authorization header");

        try {
            var authorization = req.Headers.Authorization.ToString();
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var rawToken = authorization["Bearer ".Length..].Trim();
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(rawToken))
                {
                    var jwt = handler.ReadJwtToken(rawToken);
                    uniqueName = jwt.Claims.First(claim => claim.Type == "unique_name")?.Value;
                    upn = jwt.Claims.First(claim => claim.Type == "upn")?.Value;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error checking Authorization header: {ex.Message}");
            logger.LogError(ex.Message);
            if (ex.InnerException is not null) logger.LogError(ex.InnerException.Message);
            logger.LogError(ex.StackTrace);
        }

        logger.LogInformation($"unique_name: {uniqueName}");
        logger.LogInformation($"upn: {upn}");

        try
        {
            // non-guest user email is stored in the Upn claim
            if (upn != null && upn.Contains("@"))
            {
                email = upn;
            }
            else if(uniqueName != null && uniqueName.Contains("@"))
            {
                email = uniqueName;
            }
            else
            {
                logger.LogError("Could not get email address from Authorization header.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error getting Upn claim: {ex.Message}");
        }

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

            if (principal.Claims == null)
            {
                logger.LogError("principal.Claims is null");
            }
            else
            {
                foreach (var c in principal.Claims)
                {
                    logger.LogError($"{c.Type} = {c.Value}");
                }
            }
        }

        try {
            var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);
            identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to create ClaimsIdentity");
            logger.LogError(ex.Message);
            if (ex.InnerException is not null) logger.LogError(ex.InnerException.Message);
            logger.LogError(ex.StackTrace);
        }

        return null;
    }
}