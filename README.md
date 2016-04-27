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

## Support

**Need help or wanna share your thoughts?** Don't hesitate to join our dedicated chat rooms:

- **JabbR: [https://jabbr.net/#/rooms/aspnet-contrib](https://jabbr.net/#/rooms/aspnet-contrib)**
- **Gitter: [https://gitter.im/aspnet-contrib/AspNet.Security.OpenIdConnect.Server](https://gitter.im/aspnet-contrib/AspNet.Security.OpenIdConnect.Server)** (sharing the same chat room as *AspNet.Security.OpenIdConnect.Server*)

## Contributors

**AspNet.Security.OpenIdConnect.Samples** is actively maintained by **[Kévin Chalet](https://github.com/PinpointTownes)**. Contributions are welcome and can be submitted using pull requests.

## License

This project is licensed under the **Apache License**. This means that you can use, modify and distribute it freely. See [http://www.apache.org/licenses/LICENSE-2.0.html](http://www.apache.org/licenses/LICENSE-2.0.html) for more details.
