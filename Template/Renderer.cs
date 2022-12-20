using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;

namespace Template;

using SharpDX.Direct3D11;

interface IRenderer
{

    public void Resize(int width, int height);
    public void Render();

}

internal class ConstantBuffer<T> : Buffer where T : unmanaged
{

    public unsafe ConstantBuffer(Device device) : base(device, new BufferDescription
    {
        SizeInBytes = ((sizeof(T) - 1) | 15) + 1, // Nearest multiple of 16 that is larger
        BindFlags = BindFlags.ConstantBuffer
    })
    { }

}

internal abstract class Renderer : IRenderer, IDisposable
{

    private const int SAMPLE_COUNT = 4;


    protected readonly Device device = new(DriverType.Hardware, DeviceCreationFlags.None);

    protected readonly DeviceContext context;
    protected readonly SwapChain swapChain;

    protected RenderTargetView renderView = null!;
    protected DepthStencilView depthView = null!;

    protected int width;
    protected int height;


    public unsafe Renderer(Window window)
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
            Usage = Usage.RenderTargetOutput,
            OutputHandle = new WindowInteropHelper(window).Handle,
            IsWindowed = true,
            SampleDescription = new SampleDescription(SAMPLE_COUNT, 0)
        });

        BindViews();
    }

    private void BindViews()
    {
        using Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        using Texture2D depthBuffer = new(device, new Texture2DDescription
        {
            Format = Format.D32_Float_S8X24_UInt,
            ArraySize = 1, MipLevels = 1,
            Width = width, Height = height,
            SampleDescription = new SampleDescription(SAMPLE_COUNT, 0),
            BindFlags = BindFlags.DepthStencil
        });

        // Create render target and depth buffer
        renderView = new RenderTargetView(device, backBuffer);
        depthView = new DepthStencilView(device, depthBuffer);

        // Set viewport
        context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0, 1));
        context.OutputMerger.SetRenderTargets(depthView, renderView);
    }

    public void Resize(int width, int height)
    {
        // Release previously allocated resources
        renderView.Dispose();
        depthView.Dispose();

        this.width = width;
        this.height = height;

        swapChain.ResizeBuffers(1, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
        BindViews();
    }
    public virtual void Dispose()
    {
        device.Dispose();

        context.Dispose();
        swapChain.Dispose();

        renderView.Dispose();
        depthView.Dispose();
    }


    public abstract void Render();

}
