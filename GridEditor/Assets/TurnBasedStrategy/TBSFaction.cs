using System.Collections.Generic;
using UnityEngine;

namespace Assets.TurnBasedStrategy
{
    /// <summary>
    /// Faction are large groups of units, with the same thoughts aganist each other
    /// </summary>
    public class TBSFaction
    {
        public string name;
        public Color color;
        public List<TBSFactionOpinion> opinions = new List<TBSFactionOpinion>();
        public List<TBSParty> parties = new List<TBSParty>();
    }
}
