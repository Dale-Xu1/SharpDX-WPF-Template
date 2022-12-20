using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;

namespace Template;

using SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential)]
internal struct Vertex
{

    public Vector3 Position { get; set; }
    public Color4 Color { get; set; }

}

[StructLayout(LayoutKind.Sequential)]
internal struct Constants
{

    public Matrix Transform { get; set; }

}

internal class ExampleRenderer : Renderer
{

    private readonly ConstantBuffer<Constants> constantBuffer;

    public unsafe ExampleRenderer(Window window) : base(window)
    {
        // Compile vertex and pixel shaders
        string path = "Shaders/Shader.hlsl";

        using CompilationResult vertexBytecode = ShaderBytecode.CompileFromFile(path, "VSMain", "vs_5_0");
        using CompilationResult pixelBytecode = ShaderBytecode.CompileFromFile(path, "PSMain", "ps_5_0");

        using VertexShader vertex = new(device, vertexBytecode);
        using PixelShader pixel = new(device, pixelBytecode);

        context.VertexShader.Set(vertex);
        context.PixelShader.Set(pixel);

        // Specify input layout
        using InputLayout layout = new(device, vertexBytecode, new InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
        });
        context.InputAssembler.InputLayout = layout;

        // Create constant buffer
        constantBuffer = new ConstantBuffer<Constants>(device);

        context.VertexShader.SetConstantBuffer(0, constantBuffer);
        context.PixelShader.SetConstantBuffer(0, constantBuffer);

        LoadVertexData();
        Init();
    }

    public override void Dispose()
    {
        base.Dispose();
        constantBuffer.Dispose();
    }

    private unsafe void LoadVertexData()
    {
        // Load vertex data into input assembler
        using Buffer vertices = Buffer.Create(device, BindFlags.VertexBuffer, new Vertex[]
        {
            new() { Position = new Vector3( 0,  1, 0), Color = new Color4(1, 0, 0, 1) },
            new() { Position = new Vector3( 1, -1, 0), Color = new Color4(0, 1, 0, 1) },
            new() { Position = new Vector3(-1, -1, 0), Color = new Color4(0, 0, 1, 1) }
        });
        using Buffer indices = Buffer.Create(device, BindFlags.IndexBuffer, new uint[] { 0, 1, 2,  0, 2, 1 });

        context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, sizeof(Vertex), 0));
        context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);

        context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    }


    private Matrix camera;
    private float a = 0;

    private void Init()
    {
        Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -5), Vector3.Zero, Vector3.Up);
        Matrix projection = Matrix.PerspectiveFovLH((float) Math.PI / 4, (float) width / height, 0.1f, 100);

        camera = view * projection;
    }

    public override void Render()
    {
        // Load constants
        Matrix world = Matrix.RotationY(a);
        a += 0.05f;

        Constants constants = new() { Transform = world * camera };
        context.UpdateSubresource(ref constants, constantBuffer);

        // Render scene
        context.ClearRenderTargetView(renderView, Color.Black);
        context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

        context.DrawIndexed(6, 0, 0);
        swapChain.Present(0, PresentFlags.None);
    }

}
