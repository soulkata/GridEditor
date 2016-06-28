using System.Collections.Generic;
using UnityEngine;

namespace Assets.TurnBasedStrategy
{
    /// <summary>
    /// Party is a small group of units, that act coordinated by a central UI, this UI can be a Player or a AI
    /// </summary>
    public class TBSParty
    {
        public TBSFaction faction;
        public Color color;
        public List<TBSUnitBehaviour> units = new List<TBSUnitBehaviour>();
    }
}
