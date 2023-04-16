namespace CustomConstraintInjector
{
    public class ConstraintPack
    {
        public string Name;
        public List<CustomConstraint> Constraints = new();
        public List<PriorityEdit> PriorityEdits = new();
    }
}
