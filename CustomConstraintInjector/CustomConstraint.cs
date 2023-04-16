using RandomizerCore;
using RandomizerCore.Exceptions;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace CustomConstraintInjector
{
    public class CustomConstraint
    {
        public TargetMatch Target;
        public ConstraintTypes Type;
        public string[]? Params = null;
        public bool Strict = false;

        /// <summary>
        /// Called if the constraint matches either the item or location parameter. Returns true to indicate that the constraint is satisfied.
        /// </summary>
        public bool Evaluate(IRandoItem ri, IRandoLocation rl)
        {
            return Type switch
            {
                ConstraintTypes.NOTLOCATION => !IsParam(rl.Name),
                ConstraintTypes.NOTITEM => !IsParam(ri.Name),

                ConstraintTypes.NOTREQUIREDITEM => !ri.Required,
                ConstraintTypes.REQUIREDITEM => ri.Required,

                ConstraintTypes.NOTFLEXIBLECOUNT => rl is not RandoModLocation rml || rml.LocationDef is not LocationDef lDef || !lDef.FlexibleCount,

                ConstraintTypes.NOTMAPAREA => rl is not RandoModLocation rml || rml.LocationDef is not LocationDef lDef || !IsParam(lDef.MapArea),
                ConstraintTypes.NOTTITLEDAREA => rl is not RandoModLocation rml || rml.LocationDef is not LocationDef lDef || !IsParam(lDef.TitledArea),
                
                ConstraintTypes.ATMAPAREA => rl is RandoModLocation rml && rml.LocationDef is LocationDef lDef && IsParam(lDef.MapArea),
                ConstraintTypes.ATTITLEDAREA => rl is RandoModLocation rml && rml.LocationDef is LocationDef lDef && IsParam(lDef.TitledArea),
                _ => true,
            };
        }

        public string GetParam(int index)
        {
            if (Params is null || Params.Length <= index) throw new InvalidOperationException($"Constraint of type {Type} for target {Target} has undefined params.");
            return Params[index];
        }

        public bool IsParam(string s)
        {
            if (Params is null) throw new InvalidOperationException($"Constraint of type {Type} for target {Target} has undefined params.");
            foreach (string p in Params) if (p == s) return true;
            return false;
        }

        public void Throw(IRandoItem ri, IRandoLocation rl)
        {
            throw new OutOfLocationsException(GetFailMessage(ri, rl));
        }

        public string GetFailMessage(IRandoItem ri, IRandoLocation rl)
        {
            return $"Forced placement {ri.Name} at {rl.Name} failed to satisfy constraint of type {Type} and target {Target}";
        }

        public override string ToString()
        {
            return $"{Type}: {Target}";
        }
    }
}
