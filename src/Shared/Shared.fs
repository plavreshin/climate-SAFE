namespace Shared

type CurrentWeatherResponse = { summary: string; humidity: float; averageTemp: int; feelsLikeTemp: int}

type Co2LevelResponse = {time: string; level: float}

type DewPointTemperatureResponse = {time: string; dewPointTemp: float}

type RelativeHumidityResponse = {time: string; relHumidity1: float; relHumidity3: float}
