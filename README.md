# FanControl.NzxtKraken

Adds support for some Kraken AIOs to Fancontrol that otherwise would not be working (until LibreHardwareMonitor merge this PR https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/pull/1078)

## Installation

Grab latest release `FanControl.NzxtKraken.dll` and save it to the `Plugins` directory of your FanControl installation

## Setting up the developer environment

The project, after being imported to Visual Studio needs to have it reference to `FanControl.Plugins.dll` and `HidSharp.dll` from Fancontrol package.

## Supported devices

- NZXT Kraken 2023 (Standard / Elite)
- NZXT Kraken X3 (X73, X63, X53)
- NZXT Kraken Z3 (Z73, Z63, Z53)
- NZXT Kraken X2 (X72, X62, X52, X42)

## License

```
MIT License

Copyright (c) 2023 Marco Massarotto

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
