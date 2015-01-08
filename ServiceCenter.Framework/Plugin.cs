using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Framework
{
    public class Plugin
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Classname { get; set; }
        public string PluginType { get; set; }

        public List<Parameter> Parameters { get; set; }

        public void Clone(Plugin plugin)
        {
            this.Title = plugin.Title;
            this.Description = plugin.Description;
            this.Parameters = plugin.Parameters;
        }

        public virtual void InitializeParameters()
        {
            if (Parameters != null && Parameters.Count > 0)
            {
                Parameters.ForEach(p =>
                {
                    var property = this.GetType().GetProperty(p.Key);
                    if (property != null)
                    {
                        property.SetValue(this, Convert.ChangeType(p.Value, property.PropertyType), null);
                    }
                });
            }
        }
    }
}
