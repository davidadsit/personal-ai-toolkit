namespace PersonalDashboard.Page;

public static class TemperatureColor
{
    private const double Coldest = -40;
    private const double Neutral = 20;
    private const double Hottest = 40;
    private static readonly (double Red, double Green, double Blue) Cold = (140, 175, 250);
    private static readonly (double Red, double Green, double Blue) White = (255, 255, 255);
    private static readonly (double Red, double Green, double Blue) Hot = (242, 130, 115);

    public static string ForCelsius(double celsius)
    {
        var clamped = Math.Clamp(celsius, Coldest, Hottest);
        return clamped <= Neutral
            ? Mix(Cold, White, (clamped - Coldest) / (Neutral - Coldest))
            : Mix(White, Hot, (clamped - Neutral) / (Hottest - Neutral));
    }

    public static double ToCelsius(double fahrenheit) => (fahrenheit - 32) * 5 / 9;

    private static string Mix((double Red, double Green, double Blue) from, (double Red, double Green, double Blue) to, double share) =>
        $"rgb({Channel(from.Red, to.Red, share)}, {Channel(from.Green, to.Green, share)}, {Channel(from.Blue, to.Blue, share)})";

    private static int Channel(double from, double to, double share) =>
        (int)Math.Round(from + (to - from) * share, MidpointRounding.AwayFromZero);
}
