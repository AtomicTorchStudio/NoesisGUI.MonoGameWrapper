namespace NoesisGUI.MonoGameWrapper.Helpers.DeviceState
{
    #region

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using Device = SharpDX.Direct3D11.Device;

    #endregion

    /// <summary>
    /// This helper provide methods for saving and restoring D3D11 graphics device state
    /// with MonoGame. Provided by NoesisGUI team.
    /// </summary>
    internal class DeviceStateHelperD3D11 : DeviceStateHelper
    {
        private readonly Device device;

        private readonly Buffer[] vb = new Buffer[1];

        private readonly int[] vbOffset = new int[1];

        private readonly int[] vbStride = new int[1];

        private Color4 blendFactor;

        private BlendState blendState;

        private DepthStencilState depthState;

        private DepthStencilView depthStencilView;

        private Buffer ib;

        private Format ibFormat;

        private int ibOffset;

        private InputLayout layout;

        private PixelShader ps;

        private Buffer[] psConstantBuffers;

        private ShaderResourceView[] psResources;

        private SamplerState[] psSamplers;

        private RasterizerState rasterizerState;

        private RenderTargetView[] renderTargetView;

        private int sampleMaskRef;

        private Rectangle[] scissorRectangles;

        private int stencilRefRef;

        private PrimitiveTopology topology;

        private ViewportF[] viewports;

        private VertexShader vs;

        private Buffer[] vsConstantBuffers;

        private ShaderResourceView[] vsResources;

        private SamplerState[] vsSamplers;

        public DeviceStateHelperD3D11(Device device)
        {
            this.device = device;
        }

        protected override void Restore()
        {
            var context = this.device.ImmediateContext;
            context.InputAssembler.PrimitiveTopology = this.topology;
            context.InputAssembler.InputLayout = this.layout;
            context.Rasterizer.SetViewports(this.viewports);
            context.Rasterizer.SetScissorRectangles(this.scissorRectangles);
            context.Rasterizer.State = this.rasterizerState;
            context.OutputMerger.SetBlendState(this.blendState, this.blendFactor, this.sampleMaskRef);
            context.OutputMerger.SetDepthStencilState(this.depthState, this.stencilRefRef);
            context.OutputMerger.SetRenderTargets(this.depthStencilView, this.renderTargetView[0]);

            context.PixelShader.Set(this.ps);
            context.PixelShader.SetConstantBuffers(0, this.psConstantBuffers);
            context.PixelShader.SetSamplers(0, this.psSamplers);
            context.PixelShader.SetShaderResources(0, this.psResources);

            context.VertexShader.Set(this.vs);
            context.VertexShader.SetConstantBuffers(0, this.vsConstantBuffers);
            context.VertexShader.SetSamplers(0, this.vsSamplers);
            context.VertexShader.SetShaderResources(0, this.vsResources);

            context.InputAssembler.SetIndexBuffer(this.ib, this.ibFormat, this.ibOffset);
            context.InputAssembler.SetVertexBuffers(0, this.vb, this.vbStride, this.vbOffset);

            this.renderTargetView[0]?.Dispose();

            this.depthStencilView?.Dispose();
        }

        protected override void Save()
        {
            var context = this.device.ImmediateContext;
            this.topology = context.InputAssembler.PrimitiveTopology;
            this.layout = context.InputAssembler.InputLayout;
            this.viewports = context.Rasterizer.GetViewports();
            this.scissorRectangles = context.Rasterizer.GetScissorRectangles();
            this.rasterizerState = context.Rasterizer.State;
            this.blendState = context.OutputMerger.GetBlendState(out this.blendFactor, out this.sampleMaskRef);
            this.depthState = context.OutputMerger.GetDepthStencilState(out this.stencilRefRef);
            this.renderTargetView = context.OutputMerger.GetRenderTargets(1, out this.depthStencilView);

            this.ps = context.PixelShader.Get();
            this.psConstantBuffers = context.PixelShader.GetConstantBuffers(0, 4);
            this.psSamplers = context.PixelShader.GetSamplers(0, 4);
            this.psResources = context.PixelShader.GetShaderResources(0, 4);

            this.vs = context.VertexShader.Get();
            this.vsConstantBuffers = context.VertexShader.GetConstantBuffers(0, 4);
            this.vsSamplers = context.VertexShader.GetSamplers(0, 4);
            this.vsResources = context.VertexShader.GetShaderResources(0, 4);

            context.InputAssembler.GetIndexBuffer(out this.ib, out this.ibFormat, out this.ibOffset);
            context.InputAssembler.GetVertexBuffers(0, 1, this.vb, this.vbStride, this.vbOffset);
        }
    }
}