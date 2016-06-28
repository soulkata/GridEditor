using System;
using System.Collections.Generic;

namespace Assets.TurnBasedStrategy
{
    public class TBSBonnusValue
    {
        private List<TBSBonnusModifier> modifiers = new List<TBSBonnusModifier>();

        public void AddModifier(TBSBonnusType type, bool stack, int value)
        {
            Utils.Assert(value > 0, "Evaluating Bonnus Value", "Value is Less than Zero");

            foreach (TBSBonnusModifier c in this.modifiers)
            {
                if (c.type == type)
                {
                    if (type.stack)
                        c.value += value;
                    else
                        c.value = Math.Max(c.value, value);

                    return;
                }
            }

            this.modifiers.Add(new TBSBonnusModifier() { type = type, value = value });
        }

        public int Value
        {
            get
            {
                int ret = 0;
                foreach (TBSBonnusModifier m in this.modifiers)
                    ret += m.value;
                return ret;
            }
        }
    }
}
