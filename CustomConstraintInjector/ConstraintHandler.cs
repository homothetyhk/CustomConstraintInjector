using RandomizerCore;
using RandomizerCore.Randomization;
using RandomizerMod.RC;

namespace CustomConstraintInjector
{
    public record ConstraintHandler : DefaultGroupPlacementStrategy.Constraint
    {
        public readonly Dictionary<string, List<CustomConstraint>> _itemConstraints = new();
        public readonly Dictionary<string, List<CustomConstraint>> _locationConstraints = new();

        public ConstraintHandler(RequestBuilder rb, IEnumerable<ConstraintPack> packs) : base()
        {
            base.Test = this.Test;
            base.Fail = this.Fail;
            base.Label = "CCI Constraint Handler";

            foreach (StageBuilder sb in rb.Stages)
            {
                foreach (GroupBuilder gb in sb.Groups)
                {
                    IEnumerable<string> itemNames = gb switch
                    {
                        ItemGroupBuilder igb => igb.Items.EnumerateDistinct(),
                        TransitionGroupBuilder tgb => tgb.Targets.EnumerateDistinct(),
                        SymmetricTransitionGroupBuilder stgb => stgb.Group1.EnumerateDistinct().Concat(stgb.Group2.EnumerateDistinct()),
                        SelfDualTransitionGroupBuilder sdtgb => sdtgb.Transitions.EnumerateDistinct(),
                        _ => Enumerable.Empty<string>(),
                    };
                    IEnumerable<string> locationNames = gb switch
                    {
                        ItemGroupBuilder igb => igb.Locations.EnumerateDistinct(),
                        TransitionGroupBuilder tgb => tgb.Sources.EnumerateDistinct(),
                        SymmetricTransitionGroupBuilder stgb => stgb.Group1.EnumerateDistinct().Concat(stgb.Group2.EnumerateDistinct()),
                        SelfDualTransitionGroupBuilder sdtgb => sdtgb.Transitions.EnumerateDistinct(),
                        _ => Enumerable.Empty<string>(),
                    };
                    foreach (string i in itemNames)
                    {
                        if (_itemConstraints.ContainsKey(i)) continue;
                        List<CustomConstraint>? cs = null;

                        foreach (ConstraintPack pack in packs)
                        {
                            foreach (CustomConstraint c in pack.Constraints)
                            {
                                if (c.Target.TryMatchItem(i))
                                {
                                    cs ??= new();
                                    cs.Add(c);
                                }
                            }
                        }
                        if (cs is not null) _itemConstraints.Add(i, cs);
                    }
                    foreach (string l in locationNames)
                    {
                        if (_locationConstraints.ContainsKey(l)) continue;
                        List<CustomConstraint>? cs = null;

                        foreach (ConstraintPack pack in packs)
                        {
                            foreach (CustomConstraint c in pack.Constraints)
                            {
                                if (c.Target.TryMatchLocation(l))
                                {
                                    cs ??= new();
                                    cs.Add(c);
                                }
                            }
                        }
                        if (cs is not null) _locationConstraints.Add(l, cs);
                    }
                }
            }
        }

        new public bool Test(IRandoItem ri, IRandoLocation rl)
        {
            List<CustomConstraint> cs;
            if (_itemConstraints.TryGetValue(ri.Name, out cs))
            {
                foreach (CustomConstraint c in cs) if (!c.Evaluate(ri, rl)) return false;
            }
            if (_locationConstraints.TryGetValue(rl.Name, out cs))
            {
                foreach (CustomConstraint c in cs) if (!c.Evaluate(ri, rl)) return false;
            }
            return true;
        }

        new public void Fail(IRandoItem ri, IRandoLocation rl)
        {
            List<CustomConstraint> cs;
            if (_itemConstraints.TryGetValue(ri.Name, out cs))
            {
                foreach (CustomConstraint c in cs)
                {
                    if (c.Strict && !c.Evaluate(ri, rl))
                    {
                        c.Throw(ri, rl);
                    }
                    else if (!c.Evaluate(ri, rl))
                    {
                        LogHelper.Log(c.GetFailMessage(ri, rl));
                    }
                }
            }
            if (_locationConstraints.TryGetValue(rl.Name, out cs))
            {
                foreach (CustomConstraint c in cs)
                {
                    if (c.Strict && !c.Evaluate(ri, rl))
                    {
                        c.Throw(ri, rl);
                    }
                    else if (!c.Evaluate(ri, rl))
                    {
                        LogHelper.Log(c.GetFailMessage(ri, rl));
                    }
                }
            }
        }
    }
}
