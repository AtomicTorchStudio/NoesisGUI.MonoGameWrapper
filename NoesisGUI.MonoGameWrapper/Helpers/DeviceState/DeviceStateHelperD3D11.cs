namespace NoesisGUI.MonoGameWrapper.Helpers.DeviceState
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using SharpDX.Mathematics.Interop;
    using Buffer = SharpDX.Direct3D11.Buffer;
    using Device = SharpDX.Direct3D11.Device;

    /// <summary>
    /// This helper provide methods for saving and restoring D3D11 graphics device state
    /// with MonoGame. Provided by NoesisGUI team.
    /// </summary>
    internal class DeviceStateHelperD3D11 : DeviceStateHelper
    {
        /// <summary>
        /// Cached delegate instance to get the samplers from the shader stage.
        /// </summary>
        private static readonly ShaderStageGetStuff<SamplerState> ShaderStageGetSamplers
            = GetLambda<ShaderStageGetStuff<SamplerState>>("GetSamplers", typeof(SamplerState[]));

        /// <summary>
        /// Cached delegate instance to get the resources from the shader stage.
        /// </summary>
        private static readonly ShaderStageGetStuff<ShaderResourceView> ShaderStageGetResources
            = GetLambda<ShaderStageGetStuff<ShaderResourceView>>("GetShaderResources", typeof(ShaderResourceView[]));

        /// <summary>
        /// Cached delegate instance to get the constant buffers from the shader stage.
        /// </summary>
        private static readonly ShaderStageGetStuff<Buffer> ShaderStageGetConstantBuffers
            = GetLambda<ShaderStageGetStuff<Buffer>>("GetConstantBuffers", typeof(Buffer[]));

        /// <summary>
        /// Cached delegate instance to get the viewports count from the rasterizer state.
        /// </summary>
        private static readonly RasterizerStateGetViewportsCountDelegate RasterizerGetViewportsCount =
            CreateRasterizerGetViewportsCountLambda();

        private readonly Device device;

        private readonly Buffer[] psConstantBuffers = new Buffer[4];

        private readonly ShaderResourceView[] psResources = new ShaderResourceView[4];

        private readonly SamplerState[] psSamplers = new SamplerState[4];

        private readonly Buffer[] vb = new Buffer[1];

        private readonly int[] vbOffset = new int[1];

        private readonly int[] vbStride = new int[1];

        private readonly Buffer[] vsConstantBuffers = new Buffer[4];

        private readonly ShaderResourceView[] vsResources = new ShaderResourceView[4];

        private readonly SamplerState[] vsSamplers = new SamplerState[4];

        private RawColor4 blendFactor;

        private BlendState blendState;

        private DepthStencilState depthState;

        private DepthStencilView depthStencilView;

        private Buffer ib;

        private Format ibFormat;

        private int ibOffset;

        private InputLayout layout;

        private PixelShader ps;

        private RasterizerState rasterizerState;

        private RenderTargetView[] renderTargetView;

        private int sampleMaskRef;

        //private RawRectangle[] scissorRectangles;

        private int stencilRefRef;

        private PrimitiveTopology topology;

        private RawViewportF[] viewports = new RawViewportF[0];

        private VertexShader vs;

        public DeviceStateHelperD3D11(Device device)
        {
            this.device = device;
        }

        private delegate int RasterizerStateGetViewportsCountDelegate(RasterizerStage rasterizerStage);

        private delegate void ShaderStageGetStuff<T>(
            CommonShaderStage shaderStage,
            int startSlot,
            int count,
            T[] result);

        protected override void Restore()
        {
            var context = this.device.ImmediateContext;
            context.InputAssembler.PrimitiveTopology = this.topology;
            context.InputAssembler.InputLayout = this.layout;
            this.layout?.Dispose();
            context.Rasterizer.SetViewports(this.viewports);

            //context.Rasterizer.SetScissorRectangles(this.scissorRectangles);

            context.Rasterizer.State = this.rasterizerState;
            this.rasterizerState?.Dispose();

            context.OutputMerger.SetBlendState(this.blendState, this.blendFactor, this.sampleMaskRef);
            this.blendState?.Dispose();

            context.OutputMerger.SetDepthStencilState(this.depthState, this.stencilRefRef);
            this.depthState?.Dispose();

            context.OutputMerger.SetRenderTargets(this.depthStencilView, this.renderTargetView[0]);
            this.depthStencilView?.Dispose();
            this.renderTargetView[0]?.Dispose();

            context.PixelShader.Set(this.ps);
            context.PixelShader.SetConstantBuffers(0, this.psConstantBuffers);
            context.PixelShader.SetSamplers(0, this.psSamplers);
            context.PixelShader.SetShaderResources(0, this.psResources);
            this.ps?.Dispose();
            DisposeArray(this.psConstantBuffers);
            DisposeArray(this.psSamplers);
            DisposeArray(this.psResources);

            context.VertexShader.Set(this.vs);
            context.VertexShader.SetConstantBuffers(0, this.vsConstantBuffers);
            context.VertexShader.SetSamplers(0, this.vsSamplers);
            context.VertexShader.SetShaderResources(0, this.vsResources);
            this.vs?.Dispose();
            DisposeArray(this.vsConstantBuffers);
            DisposeArray(this.vsSamplers);
            DisposeArray(this.vsResources);

            context.InputAssembler.SetIndexBuffer(this.ib, this.ibFormat, this.ibOffset);
            this.ib?.Dispose();

            context.InputAssembler.SetVertexBuffers(0, this.vb, this.vbStride, this.vbOffset);
            DisposeArray(this.vb);
        }

        protected override void Save()
        {
            var context = this.device.ImmediateContext;
            this.topology = context.InputAssembler.PrimitiveTopology;
            this.layout = context.InputAssembler.InputLayout;

            var rasterizer = context.Rasterizer;
            this.SaveViewports(rasterizer);
            //this.scissorRectangles = rasterizer.GetScissorRectangles<RawRectangle>();
            this.rasterizerState = rasterizer.State;
            this.blendState = context.OutputMerger.GetBlendState(out this.blendFactor, out this.sampleMaskRef);
            this.depthState = context.OutputMerger.GetDepthStencilState(out this.stencilRefRef);
            this.renderTargetView = context.OutputMerger.GetRenderTargets(1, out this.depthStencilView);

            var pixelShaderStage = context.PixelShader;
            this.ps = pixelShaderStage.Get();
            ShaderStageGetConstantBuffers(pixelShaderStage, 0, 4, this.psConstantBuffers);
            ShaderStageGetSamplers(pixelShaderStage, 0, 4, this.psSamplers);
            ShaderStageGetResources(pixelShaderStage, 0, 4, this.psResources);

            var vertexShaderStage = context.VertexShader;
            this.vs = vertexShaderStage.Get();
            ShaderStageGetConstantBuffers(vertexShaderStage, 0, 4, this.vsConstantBuffers);
            ShaderStageGetSamplers(vertexShaderStage, 0, 4, this.vsSamplers);
            ShaderStageGetResources(vertexShaderStage, 0, 4, this.vsResources);

            context.InputAssembler.GetIndexBuffer(out this.ib, out this.ibFormat, out this.ibOffset);
            context.InputAssembler.GetVertexBuffers(0, 1, this.vb, this.vbStride, this.vbOffset);
        }

        // Helper method to get the viewports count from the rasterizer state.
        // Because the method is internal, we have to use Expression to call it via lambda
        // (method signature internal unsafe void GetViewports(ref int numViewportsRef, IntPtr viewportsRef)).
        private static RasterizerStateGetViewportsCountDelegate CreateRasterizerGetViewportsCountLambda()
        {
            var methodInfo = typeof(RasterizerStage)
                .GetMethod("GetViewports", BindingFlags.NonPublic | BindingFlags.Instance);

            var argInstance = Expression.Parameter(typeof(RasterizerStage), "rasterizerStage");
            var varNumViewports = Expression.Variable(typeof(int), "numViewports");
            var expCall = Expression.Call(argInstance,
                                          // ReSharper disable once AssignNullToNotNullAttribute
                                          methodInfo,
                                          varNumViewports,
                                          /*(viewportsRef*/
                                          Expression.Constant(IntPtr.Zero));

            var lambda = Expression.Lambda(typeof(RasterizerStateGetViewportsCountDelegate),
                                           Expression.Block(
                                               // variables
                                               new[] { varNumViewports },
                                               // call method
                                               expCall,
                                               // return num viewports
                                               varNumViewports),
                                           argInstance);

            return (RasterizerStateGetViewportsCountDelegate)lambda.Compile();
        }

        private static void DisposeArray<T>(T[] array)
            where T : IDisposable
        {
            foreach (var entry in array)
            {
                entry?.Dispose();
            }
        }

        // Helper method to get stuff from the shader stage.
        // Because many method are internal, we have to use Expression to call them via lambda
        // (method signature internal abstract void MethodName(int startSlot, int numBuffers, * someStuff)).
        private static T GetLambda<T>(string methodName, Type resultType)
            where T : Delegate
        {
            var methodInfo = typeof(CommonShaderStage)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            var argInstance = Expression.Parameter(typeof(CommonShaderStage), "shaderStage");
            var argStartSlot = Expression.Parameter(typeof(int),              "startSlot");
            var argNumSamplers = Expression.Parameter(typeof(int),            "numSamplers");
            var argResult = Expression.Parameter(resultType,                  "result");

            var expCall = Expression.Call(argInstance,
                                          // ReSharper disable once AssignNullToNotNullAttribute
                                          methodInfo,
                                          argStartSlot,
                                          argNumSamplers,
                                          argResult);

            var lambda = Expression.Lambda(typeof(T),
                                           expCall,
                                           argInstance,
                                           argStartSlot,
                                           argNumSamplers,
                                           argResult);

            return (T)lambda.Compile();
        }

        private void SaveViewports(RasterizerStage rasterizer)
        {
            var count = RasterizerGetViewportsCount(rasterizer);
            if (this.viewports.Length != count)
            {
                this.viewports = new RawViewportF[count];
            }

            rasterizer.GetViewports(this.viewports);
        }
    }
}