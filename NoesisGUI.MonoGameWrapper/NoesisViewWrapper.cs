namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers.DeviceState;
    using NoesisGUI.MonoGameWrapper.Input;
    using SharpDX.Direct3D11;

    public class NoesisViewWrapper
    {
        private readonly DeviceStateHelper deviceState;

        private readonly GraphicsDevice graphicsDevice;

        private readonly Renderer renderer;

        private readonly TimeSpan startupTotalGameTime;

        private bool isPPAAEnabled;

        private TimeSpan lastUpdateTotalGameTime;

        private View.TessellationQuality quality = View.TessellationQuality.High;

        private View.RenderFlags renderFlags;

        private Sizei size;

        private View view;

        /// <summary>
        /// Create view wrapper.
        /// <param name="currentTotalGameTime">Current game time (needed to do proper Update() calls).</param>
        /// <param name="rootElement">Root UI element.</param>
        /// <param name="graphicsDevice">MonoGame Graphics Device instance.</param>
        /// </summary>
        public NoesisViewWrapper(
            FrameworkElement rootElement,
            GraphicsDevice graphicsDevice,
            TimeSpan currentTotalGameTime)
        {
            this.graphicsDevice = graphicsDevice;
            var deviceD3D11 = (Device)this.graphicsDevice.Handle;

            this.deviceState = new DeviceStateHelperD3D11(deviceD3D11);

            this.view = this.CreateView(rootElement, deviceD3D11);
            this.renderer = this.view.Renderer;

            this.ApplyQualitySetting();
            this.ApplyAntiAliasingSetting();
            this.ApplyRenderingFlagsSetting();

            this.startupTotalGameTime = this.lastUpdateTotalGameTime = currentTotalGameTime;
        }

        /// <summary>
        /// Gets or sets the anti-aliasing mode.
        /// </summary>
        public bool IsPPAAEnabled
        {
            get => this.isPPAAEnabled;
            set
            {
                if (this.isPPAAEnabled == value)
                {
                    return;
                }

                this.isPPAAEnabled = value;
                this.ApplyAntiAliasingSetting();
            }
        }

        /// <summary>
        /// Gets or sets the tesselation quality.
        /// </summary>
        public View.TessellationQuality Quality
        {
            get => this.quality;
            set
            {
                if (this.quality == value)
                {
                    return;
                }

                this.quality = value;
                this.ApplyQualitySetting();
            }
        }

        /// <summary>
        /// Gets or sets the render flags.
        /// </summary>
        public View.RenderFlags RenderFlags
        {
            get => this.renderFlags;
            set
            {
                if (this.renderFlags == value)
                {
                    return;
                }

                this.renderFlags = value;
                this.ApplyRenderingFlagsSetting();
            }
        }

        public void ApplyAntiAliasingSetting()
        {
            this.view?.SetIsPPAAEnabled(this.IsPPAAEnabled);
        }

        public InputManager CreateInputManager(NoesisConfig config)
        {
            return new InputManager(this, config);
        }

        public View GetView()
        {
            return this.view;
        }

        public void PreRender()
        {
            this.renderer.UpdateRenderTree();
            if (!this.renderer.NeedsOffscreen())
            {
                return;
            }

            using (this.deviceState.Remember())
            {
                this.renderer.RenderOffscreen();
            }
        }

        public void Render()
        {
            using (this.deviceState.Remember())
            {
                this.renderer.Render();
            }
        }

        public void SetSize(ushort width, ushort height)
        {
            this.size = new Sizei(width, height);
            this.view.SetSize(width, height);
            this.view.Update(this.lastUpdateTotalGameTime.TotalSeconds);
        }

        public void Shutdown()
        {
            using (this.deviceState.Remember())
            {
                this.renderer.Shutdown();
            }

            this.view = null;
        }

        public void Update(GameTime gameTime)
        {
            this.lastUpdateTotalGameTime = gameTime.TotalGameTime;

            gameTime = this.CalculateRelativeGameTime(gameTime);
            this.view.Update(gameTime.TotalGameTime.TotalSeconds);
        }

        /// <summary>
        /// Calculate game time since time of construction of this wrapper object (startup time).
        /// </summary>
        /// <param name="gameTime">MonoGame game time.</param>
        /// <returns>Time since startup of this wrapper object.</returns>
        internal GameTime CalculateRelativeGameTime(GameTime gameTime)
        {
            return new GameTime(gameTime.TotalGameTime - this.startupTotalGameTime,
                                gameTime.ElapsedGameTime);
        }

        private void ApplyQualitySetting()
        {
            this.view?.SetTessellationQuality(this.quality);
        }

        private void ApplyRenderingFlagsSetting()
        {
            if (this.renderFlags != 0)
            {
                this.view?.SetFlags(this.renderFlags);
            }
        }

        private View CreateView(FrameworkElement rootElement, Device deviceD3D11)
        {
            using (this.deviceState.Remember())
            {
                var view = GUI.CreateView(rootElement);
                var renderDeviceD3D11 = new RenderDeviceD3D11(deviceD3D11.ImmediateContext.NativePointer,
                                                              sRGB: false);
                view.Renderer.Init(renderDeviceD3D11);
                return view;
            }
        }
    }
}