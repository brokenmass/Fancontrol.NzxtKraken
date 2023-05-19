using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Xml.Linq;
using FanControl.Plugins;
using HidSharp;
using static FanControl.NzxtKraken.NzxtKrakenDevice;


namespace FanControl.NzxtKraken
{
    internal class KrakenControlV3 : KrakenControl
    {
        HidDevice _hidDevice;
        byte _channel;
        int _minValue;
        public KrakenControlV3(string id, string name, float resetValue, int minValue, HidDevice hidDevice, byte channel) : base(id, name, resetValue)
        {
            _hidDevice = hidDevice;
            _channel = channel;
            _minValue = minValue;
        }

        public override void Set(float val)
        {
            _hidDevice.TryOpen(out HidStream stream);
            var speed = Math.Min(Math.Max((int) val, _minValue), 100);
            var packet = new byte[44];
            packet[0] = 0x72;
            packet[1] = _channel;
            packet[2] = 0x0;
            packet[3] = 0x0;
            for (int i = 0; i < 40; i++) packet[i + 4] = (byte) speed;
            stream.Write(packet);
            stream.Close();
        }
    }

    internal class NzxtKrakenX3 : NzxtKrakenDevice
    {
        internal override string Name => "Kraken X3";
        public KrakenSensor liquidTemperature;
        public KrakenSensor pumpSpeed;
        public KrakenControlV3 pumpControl;
        public NzxtKrakenX3(HidDevice hidDevice, IPluginLogger pluginLogger, IPluginSensorsContainer container) : base(hidDevice, pluginLogger, container)
        {
            liquidTemperature = new KrakenSensor($"liquidtemp-{_serial}", $"Liquid - {Name}");
            _container.TempSensors.Add(liquidTemperature);

            pumpSpeed = new KrakenSensor($"pumprpm-{_serial}", $"Pump - {Name}");
            _container.FanSensors.Add(pumpSpeed);

            pumpControl = new KrakenControlV3($"pumpcontrol-{_serial}", $"Pump - {Name}", 60, 20, hidDevice, 0x1 );
            _container.ControlSensors.Add(pumpControl);
        }

        public override void Update()
        {
            _hidDevice.TryOpen(out HidStream stream);
            stream.Write(new byte[] { 0x74, 0x01 });
            var packet = stream.Read();
            SetValues(packet);
            stream.Close();
        }

        internal virtual void SetValues(byte[] packet)
        {
            liquidTemperature.SetValue(packet[15] + (float)packet[16] / 10);
            pumpSpeed.SetValue(packet[18] << 8 | packet[17]);
            pumpControl.SetValue((int)packet[19]);
        }

        public static new bool SupportsDevice(HidDevice hidDevice)
        {
            return Array.Exists(new int[] { 0x2007, 0x2014 }, i => i == hidDevice.ProductID);
        }
    }

    internal class NzxtKrakenZ3 : NzxtKrakenX3
    {
        internal override string Name => "Kraken Z3";
        public KrakenSensor fanSpeed;
        public KrakenControlV3 fanControl;
        public NzxtKrakenZ3(HidDevice hidDevice, IPluginLogger pluginLogger, IPluginSensorsContainer container) : base(hidDevice, pluginLogger, container)
        {
            fanSpeed = new KrakenSensor($"fanrpm-{_serial}", $"Fan - {Name}");
            _container.FanSensors.Add(fanSpeed);

            fanControl = new KrakenControlV3($"fancontrol-{_serial}", $"Fan - {Name}", 30, 0, hidDevice, 0x2);
            _container.ControlSensors.Add(fanControl);
        }

        internal override void SetValues(byte[] packet)
        {
            base.SetValues(packet);
            fanSpeed.SetValue(packet[24] << 8 | packet[23]);
            fanControl.SetValue((int)packet[25]);
        }

        public static new bool SupportsDevice(HidDevice hidDevice)
        {
            return Array.Exists(new int[] { 0x3008 }, i => i == hidDevice.ProductID);
        }
    }

    internal class NzxtKrakenElite : NzxtKrakenZ3
    {
        internal override string Name => "Kraken Elite";

        public NzxtKrakenElite(HidDevice hidDevice, IPluginLogger pluginLogger, IPluginSensorsContainer container) : base(hidDevice, pluginLogger, container)
        {}

        public static new bool SupportsDevice(HidDevice hidDevice)
        {
            return Array.Exists(new int[] { 0x300C }, i => i == hidDevice.ProductID);
        }
    }

    internal class NzxtKraken2023 : NzxtKrakenZ3
    {
        internal override string Name => "Kraken";

        public NzxtKraken2023(HidDevice hidDevice, IPluginLogger pluginLogger, IPluginSensorsContainer container) : base(hidDevice, pluginLogger, container)
        { }

        public static new bool SupportsDevice(HidDevice hidDevice)
        {
            return Array.Exists(new int[] { 0x300E }, i => i == hidDevice.ProductID);
        }
    }
}
