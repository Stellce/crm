namespace Api.Security;

public class RateLimitingOptions
{
    public FixedWindowRateLimitOptions Auth { get; set; } = new();
    public SlidingWindowRateLimitOptions UserApi { get; set; } = new();
}

public class FixedWindowRateLimitOptions
{
    public int PermitLimit { get; set; } = 5;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(5);
    public int QueueLimit { get; set; } = 0;
}

public class SlidingWindowRateLimitOptions
{
    public int PermitLimit { get; set; } = 60;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public int SegmentsPerWindow { get; set; } = 6;
    public int QueueLimit { get; set; } = 0;
}