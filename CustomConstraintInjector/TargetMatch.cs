using Newtonsoft.Json;
using RandomizerCore;
using System.Text.RegularExpressions;

namespace CustomConstraintInjector
{
    public readonly struct TargetMatch
    {
        readonly object _pattern;
        readonly bool _item;

        [JsonConstructor]
        public TargetMatch(MatchingType match, string name)
        {
            _item = match switch
            {
                MatchingType.ITEM or MatchingType.ITEM_WILDCARD or MatchingType.ITEM_REGEX => true,
                _ => false
            };
            _pattern = match switch
            {
                MatchingType.ITEM_WILDCARD or MatchingType.LOCATION_WILDCARD => new Regex($"^{Regex.Escape(name).Replace("\\?", ".").Replace("\\*", ".*")}$"),
                MatchingType.ITEM_REGEX or MatchingType.LOCATION_REGEX => new Regex(name),
                _ => name
            };
        }

        public bool TryMatchName(string name)
        {
            return _pattern is string s && s == name
                || _pattern is Regex r && r.IsMatch(name);
        }

        public bool TryMatchItem(string name)
        {
            return _item && TryMatchName(name);
                
        }

        public bool TryMatchLocation(string name)
        {
            return !_item && TryMatchName(name);
        }

        public override string ToString()
        {
            return $"{_pattern} ({(_item ? "ITEM" : "LOCATION")})";
        }

    }
}
