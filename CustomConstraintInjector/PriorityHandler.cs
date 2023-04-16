using RandomizerCore;
using RandomizerCore.Randomization;
using RandomizerMod.RC;

namespace CustomConstraintInjector
{
    public class PriorityHandler
    {
        public readonly Dictionary<string, List<PriorityEdit>> _itemEdits = new();
        public readonly Dictionary<string, List<PriorityEdit>> _locationEdits = new();

        public PriorityHandler(RequestBuilder rb, IEnumerable<ConstraintPack> packs) 
        {
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
                        if (_itemEdits.ContainsKey(i)) continue;
                        List<PriorityEdit>? es = null;

                        foreach (ConstraintPack pack in packs)
                        {
                            foreach (PriorityEdit e in pack.PriorityEdits)
                            {
                                if (e.Target.TryMatchItem(i))
                                {
                                    es ??= new();
                                    es.Add(e);
                                }
                            }
                        }
                        if (es is not null) _itemEdits.Add(i, es);
                    }
                    foreach (string l in locationNames)
                    {
                        if (_locationEdits.ContainsKey(l)) continue;
                        List<PriorityEdit>? es = null;

                        foreach (ConstraintPack pack in packs)
                        {
                            foreach (PriorityEdit e in pack.PriorityEdits)
                            {
                                if (e.Target.TryMatchLocation(l))
                                {
                                    es ??= new();
                                    es.Add(e);
                                }
                            }
                        }
                        if (es is not null) _locationEdits.Add(l, es);
                    }
                }
            }


            
            
        }

        public void OnPermute(Random rng, RandomizationGroup g)
        {
            foreach (IRandoItem ri in g.Items) if (_itemEdits.TryGetValue(ri.Name, out List<PriorityEdit> es))
                {
                    float f = ri.Priority;
                    foreach (PriorityEdit e in es) f = e.Transform(f);
                    ri.Priority = f;
                }
            foreach (IRandoLocation rl in g.Locations) if (_locationEdits.TryGetValue(rl.Name, out List<PriorityEdit> es))
                {
                    float f = rl.Priority;
                    foreach (PriorityEdit e in es) f = e.Transform(f);
                    rl.Priority = f;
                }
        }

    }
}
