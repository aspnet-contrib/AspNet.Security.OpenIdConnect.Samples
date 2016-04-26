# AspNet.Security.OpenIdConnect.Samples
ASP.NET Core samples demonstrating how to use the [ASP.NET OpenID Connect server](https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server) with MVC or JS apps

# Samples

## Cordova

[Source](./samples/Cordova)

## MVC

[Source](./samples/Mvc)

This sample shows two separate web applications, potentially running on separate servers, playing the roles of a client and a server w.r.t authentication.

To the user, `Mvc.Client` is the main web application, offering for example a login page and a home page (see `HomeController`). 
It plays as the *client* from the point of view of authentication, as it does not implement authentication services on its own and it asks the `Mvc.Server` for those through REST requests.

To the user, `Mvc.Server` is almost invisible. 
It offers authentication services to client web application by means of its REST API endpoints, playing as the *authorization server* mentioned in OpenId scenarios.
In this role, it also offers a View directly to the user, a consent form (see `AuthorizationController`).

Besides from APIs to request and refresh tokens, `Mvc.Server` also offers an API to access a protected resource, whose data are only accessible to authenticated users (see `ResourceController`).
Thus, it also plays as the *resource server* of an OpenId scenario. 
Please keep in mind that, generally speaking, authorization server and resource server could also be split in two separate web applications.

## SignalR

[Source](./samples/SignalR)
