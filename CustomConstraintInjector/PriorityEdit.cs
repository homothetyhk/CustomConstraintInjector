namespace CustomConstraintInjector
{
    public class PriorityEdit
    {
        public TargetMatch Target;
        public PriorityOperator Op;
        public float Param;

        internal float Transform(float input)
        {
            switch (Op)
            {
                case PriorityOperator.ADD: return input + Param;
                case PriorityOperator.MUL: return input * Param;
                case PriorityOperator.MAX: return Math.Max(input, Param);
                case PriorityOperator.MIN: return Math.Min(input, Param);
                case PriorityOperator.POW: return (float)(Math.Pow(Math.Abs(input), Param) * Math.Sign(input));
            }
            return input;
        }
    }
}
