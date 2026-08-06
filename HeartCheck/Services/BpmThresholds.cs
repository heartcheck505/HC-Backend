namespace HeartCheck.Services
{
    internal static class BpmThresholds
    {
        internal static (int Low, int High) Get(string context)
        {
            return context.ToLowerInvariant() switch
            {
                "rest" => (60, 100),
                "active" => (80, 160),
                "sleep" => (40, 80),
                _ => (60, 100)
            };
        }
    }
}
