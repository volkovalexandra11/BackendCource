using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IClientStore clientStore;
        private readonly IEventService events;
        private readonly ILogger<ExternalController> logger;

        public ExternalController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            ILogger<ExternalController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.interaction = interaction;
            this.clientStore = clientStore;
            this.events = events;
            this.logger = logger;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";
            if (Url.IsLocalUrl(returnUrl) == false && interaction.IsValidReturnUrl(returnUrl) == false)
            {
                throw new Exception("invalid return URL");
            }
            
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)), 
                Items =
                {
                    { "returnUrl", returnUrl }, 
                    { "scheme", scheme },
                }
            };

            return Challenge(props, scheme);
            
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                logger.LogDebug("External claims: {@claims}", externalClaims);
            }
            
            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
            user ??= await AutoProvisionUserAsync(provider, providerUserId, claims);
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);
            var principal = await signInManager.CreateUserPrincipalAsync(user);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;
            
            var isUser = new IdentityServerUser(user.Id)
            {
                DisplayName = name,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isUser, localSignInProps);
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";
            var context = await interaction.GetAuthorizationContextAsync(returnUrl);
            await events.RaiseAsync(new UserLoginSuccessEvent(provider, 
                providerUserId, 
                user.Id, 
                name, 
                true, 
                context?.Client.ClientId)
            );

            if (context == null) return Redirect(returnUrl);
            return context.IsNativeClient() 
                ? this.LoadingPage("Redirect", returnUrl) : Redirect(returnUrl);
        }

        private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
            FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;
            var user = await userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, 
            string providerUserId, 
            IEnumerable<Claim> claims
            )
        {
            var filtered = new List<Claim>();
            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                       claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            if (name != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, name));
            }
            else
            {
                var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
               claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }

            var user = new ApplicationUser
            {
                UserName = Guid.NewGuid().ToString(),
            };
            var identityResult = await userManager.CreateAsync(user);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            if (filtered.Any())
            {
                identityResult = await userManager.AddClaimsAsync(user, filtered);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
            }

            identityResult = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            return user;
        }

        private static void ProcessLoginCallback(AuthenticateResult externalResult, 
            ICollection<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }
            
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
    }
}