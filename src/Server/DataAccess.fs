module DataAccess

open FSharp.Control.Tasks
open FSharp.Data
open Shared

let private apiKey = "d3437c70f288464da3e3dd9ab4b920d2"

module Co2Level =
    type Co2LevelPayload = JsonProvider<"https://api.planetos.com/v1/datasets/noaa_co2_obs_weekly/stations/Mauna+Loa+observing+station?origin=dataset-details&apikey=d3437c70f288464da3e3dd9ab4b920d2&var=increase_since_1800">

    let getCo2Level =
            apiKey
            |> sprintf "https://api.planetos.com/v1/datasets/noaa_co2_obs_weekly/stations/Mauna+Loa+observing+station?meta=true&count=50&time_order=desc&apikey=%s"
            |> Co2LevelPayload.AsyncLoad
            |> Async.StartAsTask

module DewPointTemperature =
    type DewPointTemperaturePayload = JsonProvider<"https://api.planetos.com/v1/datasets/noaa_ndbc_stdmet_stations/stations/62144?meta=true&count=50&time_order=desc&apikey=0f58fe2540fa4cf19a4aa68f290fd148">
    let private stationId = 62144

    let getDewPointTemperature =
        (stationId, apiKey)
        ||> sprintf "https://api.planetos.com/v1/datasets/noaa_ndbc_stdmet_stations/stations/%d?meta=true&count=50&time_order=desc&apikey=%s"
        |> DewPointTemperaturePayload.AsyncLoad
        |> Async.StartAsTask

module CurrentWeatherState =
    type CurrentWeatherStatePayload = JsonProvider<"https://api.darksky.net/forecast/226a573e8a3b34843e291ad87c145261/59.43696,24.75353">
    let (lat, long) = (59.43696, 24.75353)

    let getCurrentWeatherState =
        (lat, long)
        ||> sprintf "https://api.darksky.net/forecast/226a573e8a3b34843e291ad87c145261/%f,%f?units=auto"
        |> CurrentWeatherStatePayload.AsyncLoad
        |> Async.StartAsTask

module RelativeHumidity =
    type RelativeHumidityPayload = JsonProvider<"https://api.planetos.com/v1/datasets/noaa_rbsn_timeseries/stations/02750?meta=true&count=50&time_order=desc&apikey=0f58fe2540fa4cf19a4aa68f290fd148">

    let getRelativeHumidity =
        apiKey
        |> sprintf "https://api.planetos.com/v1/datasets/noaa_rbsn_timeseries/stations/02750?meta=true&count=50&time_order=desc&apikey=%s"
        |> RelativeHumidityPayload.AsyncLoad
        |> Async.StartAsTask