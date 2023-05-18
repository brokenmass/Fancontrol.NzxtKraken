using FanControl.Plugins;
using HidSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanControl.NzxtKraken
{
    internal class KrakenSensor : IPluginSensor
    {
        public KrakenSensor(string id, string name)
        {
            _name = name;
            _id = id;
            
        }
        public void SetValue(float value)
        {
            _value = value;
        }

        public string Id => _id;
        string _id;

        public string Name => _name;
        string _name;

        public float? Value => _value;
        float _value;

        public void Update() { }
    }
   
    internal abstract class KrakenControl : KrakenSensor, IPluginControlSensor
    {
        const float _resetValue = 60.0f;

        public KrakenControl(string id, string name) : base(id, name) {}

        public void Reset()
        {
            Set(_resetValue);
        }

        public abstract void Set(float val);
    }

}
