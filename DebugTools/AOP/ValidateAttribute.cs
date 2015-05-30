using System;

namespace DebugTools
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ValidateAttribute : Attribute
    {
        public ValidationTarget Targets { get; private set; }

        public ValidateAttribute(ValidationTarget targets)
        {
            Targets = targets;
        }
    }

    [Flags]
    public enum ValidationTarget
    {
        TargetData = 1,
        Runtime = 2
    }
}
