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
   
    internal abstract class NzxtKrakenDevice
    {
        internal IPluginLogger _logger;
        internal HidDevice _hidDevice;
        internal IPluginSensorsContainer _container;
        internal readonly string _serial;

        internal abstract string Name { get; }

        public NzxtKrakenDevice(HidDevice hidDevice, IPluginLogger pluginLogger, IPluginSensorsContainer container)
        {
            _logger = pluginLogger;
            _hidDevice = hidDevice;
            _container = container;
            _serial = hidDevice.GetSerialNumber();
        }

        public abstract void Update();

        public static bool SupportsDevice(HidDevice hidDevice)
        {
            return false;
        }
    }
}
