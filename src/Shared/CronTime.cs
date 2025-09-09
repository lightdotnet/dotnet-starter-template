namespace Monolith;

public abstract class CronTime
{
    public static readonly string EVERY_5_MINS = EveryMins(5);

    public static readonly string EVERY_15_MINS = EveryMins(15);

    public static readonly string EVERY_30_MINS = EveryMins(30);

    public const string EVERY_1_HOUR = "0 * * * *";

    public static readonly string EVERY_3_HOURS = EveryHours(3);

    public static readonly string EVERY_DAY_AT_0_UTC_7 = EveryDayAt(17);

    public static readonly string EVERY_DAY_AT_1_UTC_7 = EveryDayAt(18);

    public static readonly string EVERY_DAY_AT_5_UTC_7 = EveryDayAt(22);

    public static readonly string EVERY_DAY_AT_6_UTC_7 = EveryDayAt(23);

    public static string EveryMins(int mins) => $"*/{mins} * * * *";

    public static string EveryHours(int hour) => $"0 */{hour} * * *";

    public static string EveryDayAt(int utcHour) => $"00 {utcHour} * * *";
}