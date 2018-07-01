using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Configuration
{
    public class PluginHandlerConfigurationCollection : ConfigurationElementCollection
    {
        protected PluginHandlerConfigurationCollection()
        {
            var details = (PluginHandlerConfigurationElement)CreateNewElement();
            if (details.PluginName != "")
            {
                Add(details);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected sealed override ConfigurationElement CreateNewElement()
        {
            return new PluginHandlerConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginHandlerConfigurationElement)element).PluginName;
        }

        public PluginHandlerConfigurationElement this[int index]
        {
            get
            {
                return (PluginHandlerConfigurationElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public PluginHandlerConfigurationElement GetByName(string name)
        {
            return (PluginHandlerConfigurationElement)BaseGet(name);
        }

        public int IndexOf(PluginHandlerConfigurationElement details)
        {
            return BaseIndexOf(details);
        }

        private void Add(ConfigurationElement details)
        {
            BaseAdd(details);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(PluginHandlerConfigurationElement details)
        {
            if (BaseIndexOf(details) >= 0)
                BaseRemove(details.PluginName);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override string ElementName
        {
            get { return "plugin-handler"; }
        }
    }
}

