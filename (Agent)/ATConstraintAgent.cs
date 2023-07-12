using System;
using Mozart.Task.Execution;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Planner;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATConstraintAgent : IAgent
    {
        private HashSet<IConstraint> _constraintObjects = new HashSet<IConstraint>();
        private Dictionary<string, ATConstraint> _constraints = new Dictionary<string, ATConstraint>();

        public static ATConstraintAgent Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATConstraintAgent>();
            }
        }

        public void Initialize()
        {
            var objects = this.GetConstraintObjects();
            _constraints = new Dictionary<string, ATConstraint>();

            foreach (var obj in objects)
            {
                foreach (var info in obj.ConstraintInfos)
                {
                    ATConstraint constraint;
                    if (_constraints.TryGetValue(info.ConstraintID, out constraint) == false)
                    {
                        constraint = new ATConstraint(info);
                        _constraints.Add(info.ConstraintID, constraint);
                    }

                    obj.Constraints.Add(constraint);
                }
            }
        }

        public void Dispose()
        {
            foreach (var obj in _constraintObjects)
            {
                obj.Constraints = new List<ATConstraint>();
            }

            _constraints = null;
        }

        public void AddConstraintObject(IConstraint obj)
        {
            _constraintObjects.Add(obj);
        }

        internal List<ATConstraint> GetContraints()
        {
            return _constraints.Values.ToList();
        }

        internal HashSet<IConstraint> GetConstraintObjects()
        {
            return _constraintObjects;
        }
    }
}
