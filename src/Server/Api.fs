module Api

open DataAccess
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Shared
open FSharp.Control.Tasks.ContextInsensitive
open FSharp.Data

let private tryGetProperty (data: JsonValue) (propertyName: string)  =
    match data.TryGetProperty(propertyName) with
        | Some value -> System.Math.Round (float (string value), 5)
        | None -> 0.0

let private asCo2LevelResponse (data: Co2Level.Co2LevelPayload.Root) =
    data.Entries
    |> Array.map (fun x -> { time = x.Axes.Time.Date.ToShortDateString(); level = tryGetProperty x.Data.JsonValue "CO2"} )
    |> Array.rev

let getCo2Level next (ctx: HttpContext) = task {
    let! resp = Co2Level.getCo2Level
    return! json (resp |> asCo2LevelResponse) next ctx
}

let private asDewPointTempResponse (data: DewPointTemperature.DewPointTemperaturePayload.Root) =
    data.Entries
    |> Array.map (fun x -> { time = x.Axes.Time.Date.ToShortDateString(); dewPointTemp = System.Math.Round (float x.Data.DewptTemperature, 3)} )
    |> Array.rev

let getDewPointTemperature next (ctx: HttpContext) = task {
    let! resp = DewPointTemperature.getDewPointTemperature
    return! json (resp |> asDewPointTempResponse) next ctx
}

let private round (f: float): float = System.Math.Round (f, 3)

let private asRelativeHumidityResponse (data: RelativeHumidity.RelativeHumidityPayload.Root) =
    data.Entries
    |> Array.map (fun x -> { time = x.Axes.Time.Date.ToShortDateString(); relHumidity1 = round (float x.Data.RelHumidity1); relHumidity3 = round (float x.Data.RelHumidity3) } )
    |> Array.rev

let getRelativeHumidityData next (ctx: HttpContext) = task {
    let! resp = RelativeHumidity.getRelativeHumidity
    return! json (resp |> asRelativeHumidityResponse) next ctx
}

let private asCurrentWeatherResponse (state: CurrentWeatherState.CurrentWeatherStatePayload.Root) =
    {   summary = state.Currently.Summary;
        humidity = float state.Currently.Humidity;
        averageTemp = int state.Currently.Temperature;
        feelsLikeTemp = int state.Currently.ApparentTemperature
    }

let getCurrentWeatherState next (ctx: HttpContext) = task {
    let! resp = CurrentWeatherState.getCurrentWeatherState
    return! json (resp |> asCurrentWeatherResponse) next ctx
}

let apiRouter = scope {
    pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
    get "/co2Level" getCo2Level
    get "/currentWeatherState" getCurrentWeatherState
    get "/dewPointTemperature" getDewPointTemperature
    get "/relativeHumidity" getRelativeHumidityData }