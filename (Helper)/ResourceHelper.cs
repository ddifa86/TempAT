
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ResourceHelper
    {
        private Dictionary<string, ATResource> _resources = new Dictionary<string, ATResource>();
        private Dictionary<string, ATResourceGroup> _resourceGroups = new Dictionary<string, ATResourceGroup>();

        public bool AddResourceGroup(ATResourceGroup obj)
        {
            if (_resourceGroups.ContainsKey(obj.GroupID))
                return false;

            _resourceGroups.Add(obj.GroupID, obj);
            return true;
        }

        public ATResourceGroup GetResourceGroup(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_resourceGroups.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public IEnumerable<ATResourceGroup> GetResourceGroups()
        {
            return this._resourceGroups.Values;
        }

        public void RemoveResourceGroup(ATResourceGroup resGroup)
        {
            this._resourceGroups.Remove(resGroup.GroupID);
        }

        public bool AddResource(ATResource resource)
        {
            if (_resources.ContainsKey(resource.ResourceID) == true)
                return false;

            _resources.Add(resource.ResourceID, resource);

            return true;
        }

        public ATResource GetResource(string resourceID)
        {
            if (string.IsNullOrEmpty(resourceID))
                return null;

            ATResource resource;
            if (_resources.TryGetValue(resourceID, out resource) == false)
                return null;

            return resource;
        }

        public List<ATResource> GetResources()
        {
            List<ATResource> returnValue = new List<ATResource>();
            foreach (var item in _resources)
            {
                returnValue.Add(item.Value); //왜 새로 만들어서 보내는지 확인
            }

            return returnValue;
        }

    }
}
