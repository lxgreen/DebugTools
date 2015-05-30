using System;
using System.Linq;
using System.Reflection;
using NLog;

namespace DebugTools
{
    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly)]
    public class IntersectMethodsMarkedByAttribute : Attribute
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Type[] _types;

        // Required
        public IntersectMethodsMarkedByAttribute()
        {
        }

        public IntersectMethodsMarkedByAttribute(params Type[] types)
        {
            _types = types;
            if (types.All(x => typeof(Attribute).IsAssignableFrom(x)))
            {
                throw new Exception("Fody: cannot intersect non-attributes");
            }
        }

        public void Init(object instance, MethodBase method, object[] args)
        {
            _logger.Trace(string.Format("Init: {0} [{1}]", method.DeclaringType.FullName + "." + method.Name, args.Length));
            if (!Validate(instance, method))
            {
                _logger.Error("DataTarget is not valid");
                throw new Exception("DataTarget is not valid");
            }
        }

        private static bool Validate(object instance, MethodBase method)
        {
            bool isValid = true;
            var validateDataAttribute = (from attr in method.GetCustomAttributes<ValidateAttribute>()
                                         select attr).FirstOrDefault();

            if (validateDataAttribute != null)
            {
                if ((validateDataAttribute.Targets | ValidationTarget.TargetData) == ValidationTarget.TargetData)
                {
                    var dataTargetField = method.DeclaringType.GetField("_dataTarget", BindingFlags.Instance | BindingFlags.NonPublic);

                    if (dataTargetField != null)
                    {
                        var dataTarget = dataTargetField.GetValue(instance);
                        isValid &= dataTarget != null;
                    }
                }
                if ((validateDataAttribute.Targets | ValidationTarget.Runtime) == ValidationTarget.Runtime)
                {
                }
            }

            return isValid;
        }

        public void OnEntry()
        {
            _logger.Trace("OnEntry");
        }

        public void OnExit()
        {
            _logger.Trace("OnExit");
        }

        public void OnException(Exception exception)
        {
            _logger.Error(string.Format("OnException: {0}: {1}", exception.GetType(), exception.Message));
        }
    }

    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Constructor)]
    public class LoggedAttribute : Attribute
    {
    }
}
