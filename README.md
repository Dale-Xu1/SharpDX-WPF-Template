# SharpDX Template for WPF

## Overview

This is a Visual Studio project template that sets up DirectX 11 in a WPF window using SharpDX. There are two examples in the project, one which sets up a basic standard rendering pipeline that draws a colorful spinning triangle. The other example sets up a simple compute shader, which currently just generates Sierpiński's triangle.

Unfortunately, WPF uses DirectX 9, so there's some weird interopping going on that seems to be the cause of a flickering issue when you resize the window. I probably shouldn't be using WPF in the first place, but if someone could figure out how to properly integrate D3D11 with WPF, I'd be very impressed.
