namespace PersonalDashboard.Weather;

public static class WeatherCondition
{
    public static string Name(int code) => Describe(code).Name;

    public static WeatherPicture Picture(int code) => Describe(code).Picture;

    private static (string Name, WeatherPicture Picture) Describe(int code) => code switch
    {
        0 => ("Clear", WeatherPicture.Sun),
        1 or 2 => ("Partly cloudy", WeatherPicture.PartSun),
        3 => ("Cloudy", WeatherPicture.Cloud),
        45 or 48 => ("Fog", WeatherPicture.Fog),
        >= 51 and <= 57 => ("Drizzle", WeatherPicture.Rain),
        >= 61 and <= 67 => ("Rain", WeatherPicture.Rain),
        >= 71 and <= 77 => ("Snow", WeatherPicture.Snow),
        >= 80 and <= 82 => ("Showers", WeatherPicture.Rain),
        85 or 86 => ("Snow showers", WeatherPicture.Snow),
        >= 95 and <= 99 => ("Thunderstorm", WeatherPicture.Storm),
        _ => ("", WeatherPicture.None),
    };
}
