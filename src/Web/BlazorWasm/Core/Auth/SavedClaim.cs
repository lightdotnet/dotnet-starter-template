namespace Monolith.Core.Auth
{
    public class SavedClaim
    {
        public SavedClaim() { }

        public SavedClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; } = null!;

        public string Value { get; set; } = null!;
    }
}
