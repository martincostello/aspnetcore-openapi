# ASP.NET Core 🤝 OpenAPI

[![Build status][build-badge]][build-status]

## Introduction

This sample project demonstrates three different ways that you can expose
[OpenAPI specifications][openapi] for ASP.NET Core applications:

1. [ASP.NET Core OpenAPI][aspnetcore-openapi]
1. [NSwag][nswag]
1. [Swasbuckle.AspNetCore][swashbuckle]

The sample application is a simple Todo list application that allows you to
create, read, update, and delete Todo items. The application is implemented
using ASP.NET Core 9.0 with Minimal APIs and Razor Pages.

## Building and Testing

Compiling the application yourself requires Git and the [.NET SDK][dotnet-sdk] to be installed.

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/aspnetcore-openapi.git
cd aspnetcore-openapi
./build.ps1
```

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/aspnetcore-openapi.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[aspnetcore-openapi]: https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/aspnetcore-openapi "Get started with Microsoft.AspNetCore.OpenApi"
[build-badge]: https://github.com/martincostello/aspnetcore-openapi/actions/workflows/build.yml/badge.svg?branch=main&event=push "Build status for this project"
[build-status]: https://github.com/martincostello/aspnetcore-openapi/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush "Continuous Integration for this project"
[dotnet-sdk]: https://dotnet.microsoft.com/download "Download the .NET SDK"
[issues]: https://github.com/martincostello/aspnetcore-openapi/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
[nswag]: https://github.com/RicoSuter/NSwag "The NSwag repository"
[openapi]: https://swagger.io/specification/ "The OpenAPI website"
[repo]: https://github.com/martincostello/aspnetcore-openapi "This project on GitHub.com"
[swashbuckle]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore "The Swashbuckle.AspNetCore repository"
