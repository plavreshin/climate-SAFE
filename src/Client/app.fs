module App

open Elmish

open Fable
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.Recharts
open Fable.Recharts.Props
module R = Fable.Helpers.React
module P = R.Props

open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Elements.Form
open Fulma.Extra.FontAwesome
open Fulma.Components
open Fulma.BulmaClasses

open Shared


type Report = {
    currentWeatherState: CurrentWeatherResponse
    co2LevelData: Co2LevelResponse[]
    dewPointTemperatureData: DewPointTemperatureResponse[]
    relativeHumidityData: RelativeHumidityResponse[]
}

type ServerState = Idle | Loading | ServerError of string

type Model =
    { ServerState : ServerState
      Report : Report option }

type Msg =
    | GetReport
    | GotReport of Report
    | ErrorMsg of exn
    | ClearResult

let init () =
    { Report = None
      ServerState = Idle }, Cmd.ofMsg (GetReport)

let getResponse () = promise {
    let! weather = Fetch.fetchAs<CurrentWeatherResponse> (sprintf "api/currentWeatherState") []
    let! co2Level = Fetch.fetchAs<Co2LevelResponse[]> (sprintf "api/co2Level") []
    let! dewPointData = Fetch.fetchAs<DewPointTemperatureResponse[]> (sprintf "api/dewPointTemperature") []
    let! relativeHumidityData = Fetch.fetchAs<RelativeHumidityResponse[]> (sprintf "api/relativeHumidity") []
    return { currentWeatherState = weather; co2LevelData = co2Level; dewPointTemperatureData = dewPointData; relativeHumidityData = relativeHumidityData}
    }


let update msg model =
    match model, msg with
    | _, GetReport -> model, Cmd.ofPromise getResponse () GotReport ErrorMsg
    | _, GotReport response ->
        {  Report = Some response
           ServerState = Idle }, Cmd.none
    | _, ErrorMsg e -> { model with ServerState = ServerError e.Message }, Cmd.none
    | _, ClearResult -> { model with Report = None }, Cmd.none


[<AutoOpen>]
module ViewParts =
    let basicTile title options content =
        Tile.tile options [
            Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ]
                (Heading.h3 [] [ str title ] :: content)
        ]

    let margin t r b l =
        Chart.Margin { top = t; bottom = b; right = r; left = l }

    let co2LevelChart (co2LevelData: Co2LevelResponse[]) =
        basicTile "Co2 Level increase" [ ] [
            lineChart
                [ margin 5. 5. 5. 0.
                  Chart.Width 800.
                  Chart.Height 400.
                  Chart.Data co2LevelData ]
                [ line
                    [ Cartesian.Type Monotone
                      Cartesian.DataKey "level"
                      P.Stroke "#8884d8"
                      P.StrokeWidth 2. ]
                    []
                  cartesianGrid
                    [ P.Stroke "#ccc"
                      P.StrokeDasharray "5 5" ]
                    []
                  xaxis [Cartesian.DataKey "time"] []
                  yaxis [] []
                  tooltip [] []
                ]
        ]


    let seaTemperatureChart (dewPointData: DewPointTemperatureResponse[]) =
        basicTile "DewPoint temperature" [ ] [
            lineChart
                [ margin 5. 5. 5. 0.
                  Chart.Width 800.
                  Chart.Height 400.
                  Chart.Data dewPointData ]
                [ line
                    [ Cartesian.Type Monotone
                      Cartesian.DataKey "dewPointTemp"
                      P.Stroke "#8884d8"
                      P.StrokeWidth 2. ]
                    []
                  cartesianGrid
                    [ P.Stroke "#ccc"
                      P.StrokeDasharray "5 5" ]
                    []
                  xaxis [Cartesian.DataKey "time"] []
                  yaxis [] []
                  tooltip [] []
                ]
        ]

    let relativeHumidityChart (relativeHumidityData: RelativeHumidityResponse[]) =
        basicTile "Relative humidity comparison" [ ] [
            areaChart
                [ margin 5. 5. 0. 0.
                  Chart.Width 800.
                  Chart.Height 400.
                  Chart.Data relativeHumidityData ]
                [
                  R.defs []
                    [ R.linearGradient
                        [ P.Id "relHumidity1"; P.X1 0.; P.Y1 0.; P.X2 0.; P.Y2 1.]
                        [ R.stop [ P.Offset "5%"; P.StopColor "#8884d8"; P.StopOpacity 0.8 ] []
                          R.stop [ P.Offset "95%"; P.StopColor "#8884d8"; P.StopOpacity 0 ] [] ]
                      R.linearGradient
                        [ P.Id "relHumidity3"; P.X1 0.; P.Y1 0.; P.X2 0.; P.Y2 1.]
                        [ R.stop [ P.Offset "5%"; P.StopColor "#82ca9d"; P.StopOpacity 0.8 ] []
                          R.stop [ P.Offset "95%"; P.StopColor "#82ca9d"; P.StopOpacity 0 ] [] ] ]
                  xaxis [ Cartesian.DataKey "time" ] []
                  yaxis [] []
                  cartesianGrid [P.StrokeDasharray "3 3"] []
                  tooltip [] []
                  area
                    [ Cartesian.Type Monotone
                      Cartesian.DataKey "relHumidity1"
                      Cartesian.Stroke "#8884d8"
                      P.Fill "url(#relHumidity1)"
                      P.FillOpacity 1 ] []
                  area
                    [ Cartesian.Type Monotone
                      Cartesian.DataKey "relHumidity3"
                      Cartesian.Stroke "#82ca9d"
                      P.Fill "url(#relHumidity3)"
                      P.FillOpacity 1 ] []
                ]
        ]

    let weatherState(c: CurrentWeatherResponse) =
         let hum = c.humidity * 100.00
         Level.level [ ]
                    [ Level.item [ Level.Item.HasTextCentered ]
                        [ div [ ]
                            [ Level.heading [ ]
                                [ str "Humidity" ]
                              Level.title [ ]
                                [ str (sprintf "%.2f%%" hum) ] ] ]
                      Level.item [ Level.Item.HasTextCentered ]
                        [ div [ ]
                            [ Level.heading [ ]
                                [ str "Weather Summary" ]
                              Level.title [ ]
                                [ str c.summary ] ] ]
                      Level.item [ Level.Item.HasTextCentered ]
                        [ div [ ]
                            [ Level.heading [ ]
                                [ str "Current temp C / Feels like temp C" ]
                              Level.title [ ]
                                [ str (sprintf "%d / %d" c.averageTemp c.feelsLikeTemp) ] ] ] ]

let view model dispatch =
    div [] [
        Section.section [ ]
                [ Container.container [ Container.IsFluid ]
                        [ Heading.h2 [ ]
                            [ str "Climate change story" ]
                          Heading.h4 [ Heading.IsSubtitle ]
                            [ str "At first glance it might seem we have totally fine weather" ]
                          Heading.h5 [ Heading.IsSubtitle ]
                            [ str "Following are three graphs about Co2 level, sea water temperature and air quality" ]
                          hr [ ]
                         ]
                ]
        Container.container [] [
            yield
                Field.div [ Field.IsGrouped ] [
                ]

            match model with
            | { Report = None; ServerState = (Idle | Loading) } -> ()
            | { ServerState = ServerError error } ->
                yield
                    Field.div [] [
                        Tag.list [ Tag.List.HasAddons; Tag.List.IsCentered ] [
                            Tag.tag [ Tag.Color Color.IsDanger; Tag.Size IsMedium ] [
                                str error
                            ]
                        ]
                    ]
            | { Report = Some model } ->
                yield
                    weatherState model.currentWeatherState

                yield
                    Tile.ancestor [ ] [
                        Tile.parent [ Tile.Size Tile.Is12 ] [
                            co2LevelChart model.co2LevelData
                        ]
                    ]

                yield
                    Tile.ancestor [ ] [
                        Tile.parent [ Tile.Size Tile.Is12 ] [
                            seaTemperatureChart model.dewPointTemperatureData
                        ]
                    ]


                yield
                    Tile.ancestor [ ] [
                        Tile.parent [ Tile.Size Tile.Is12 ] [
                            relativeHumidityChart model.relativeHumidityData
                        ]
                    ]
        ]

        br [ ]

        Footer.footer [] [
            Content.content
                [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
                [ safeComponents ]
        ]
    ]
