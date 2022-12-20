using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;

namespace Template;

using SharpDX.Direct3D11;

internal class ComputeRenderer : IRenderer, IDisposable
{

    protected readonly Device device = new(DriverType.Hardware, DeviceCreationFlags.None);

    protected readonly DeviceContext context;
    private readonly SwapChain swapChain;

    protected int width;
    protected int height;


    public unsafe ComputeRenderer(Window window)
    {
        FrameworkElement content = (FrameworkElement) window.Content;
        width = (int) content.ActualWidth;
        height = (int) content.ActualHeight;

        context = device.ImmediateContext;

        // Initialize swap chain
        using Factory1 factory = new();
        swapChain = new SwapChain(factory, device, new SwapChainDescription
        {
            ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            BufferCount = 1,
            Usage = Usage.UnorderedAccess,
            OutputHandle = new WindowInteropHelper(window).Handle,
            IsWindowed = true,
            SampleDescription = new SampleDescription(1, 0)
        });

        using CompilationResult result = ShaderBytecode.CompileFromFile("Shaders/Compute.hlsl", "Main", "cs_5_0");
        using ComputeShader shader = new(device, result);

        context.ComputeShader.Set(shader);
        BindViews();
    }

    private void BindViews()
    {
        using Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        using UnorderedAccessView output = new(device, backBuffer);

        context.ComputeShader.SetUnorderedAccessView(0, output);
    }

    public void Resize(int width, int height)
    {
        this.width = width;
        this.height = height;

        swapChain.ResizeBuffers(1, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
        BindViews();
    }

    public void Dispose()
    {
        device.Dispose();

        context.Dispose();
        swapChain.Dispose();
    }


    public virtual void Render()
    {
        context.Dispatch((width + 7) / 8, (height + 7) / 8, 1);
        swapChain.Present(0, PresentFlags.None);
    }

}
