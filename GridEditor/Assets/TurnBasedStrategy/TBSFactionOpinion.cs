namespace Assets.TurnBasedStrategy
{
    /// <summary>
    /// Opinion relative to other factions
    /// </summary>
    public class TBSFactionOpinion
    {
        public TBSFaction faction;
        public TBSFaction target;
        public float opinion;
        public bool Enemy { get { return this.opinion < 0.0f; } }
    }
}
