# Climate metrics on top of SAFE-stack

This is sandbox project utilizing SAFE-stack to build isomorphic web-app in F#.

![App single page with metrics](https://i.imgur.com/Oyc2gIm.png)

## Server-side

Server side consists of `DataAccess.fs` and `Api.fs`.

There are two external APIs consumed by following application:

[PlanetOS](https://darksky.net/dev)

[DarkSky](https://data.planetos.com/)

APIs data is consumed using `FSharp.Data` and `Async`.

## Client-side

Is built on top of `Fable`, `Fulma` and `ReCharts`. There are 4 graphs visualising climate related data as well as current weather state.

## Shared

`Shared.fs` contains models shared between client and server-side.

## Running locally

Just type `sh build.sh run` and wait for `http://localhost:8080` to appear.

## Prerequisites

* [dotnet SDK 2.1.4](https://github.com/dotnet/cli/releases/tag/v2.1.4)
* [Yarn](https://yarnpkg.com/lang/en/docs/install/)
* [Node 8.x](https://nodejs.org/en/download/)
* [Mono](https://www.mono-project.com/docs/getting-started/install/)


