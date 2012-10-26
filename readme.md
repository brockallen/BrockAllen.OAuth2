A claims-based OAuth2 library and sample application.

I built the OAuth2 code from scratch and it turns out to be very simple to implement. All this requires is similar config to the OAuthWebSecurity APIs and then one action method to trigger the login (and this is almost all the MVC code needed):

public ActionResult Login(ProviderType name)
{
    // 1st param is which OAuth2 provider to use (Google, Facebook, Live)
    // 2nd param is what URL to send the user once all the login magic is done
    return new OAuth2ActionResult(name, Url.Action("Index"));
}

The hosting application doesn't have to define an endpoint for the OAuth2 callback. I handle this internally by using an AreaRegistration within the helper library. This way the consuming application doesn't have to get involved with the callback messiness. Then in the callback the user is logged in with the SAM (WIF SessionAuthenticationModule) and the user's claims (from the OAuth2 provider) are available from ClaimsPrincipal.Current.Claims.
