namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using System.Windows.Forms;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers.DeviceState;
    using NoesisGUI.MonoGameWrapper.Input;
    using SharpDX.Direct3D11;
    using View = Noesis.View;

    public class NoesisViewWrapper
    {
        private readonly Device deviceD3D11;

        private readonly DeviceStateHelper deviceState;

        private readonly GraphicsDevice graphicsDevice;

        private readonly FrameworkElement rootElement;

        private readonly TimeSpan startupTotalGameTime;

        private uint antiAliasingOffscreenSampleCount;

        private bool isPPAAEnabled;

        private TimeSpan lastUpdateTotalGameTime;

        private TessellationMaxPixelError quality = TessellationMaxPixelError.HighQuality;

        private RenderDeviceD3D11 renderDeviceD3D11;

        private Renderer renderer;

        private RenderFlags renderFlags;

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
            this.rootElement = rootElement;
            this.graphicsDevice = graphicsDevice;
            this.deviceD3D11 = (Device)this.graphicsDevice.Handle;
            this.deviceState = new DeviceStateHelperD3D11(this.deviceD3D11);
            this.startupTotalGameTime = this.lastUpdateTotalGameTime = currentTotalGameTime;

            this.CreateView();
        }

        public uint AntiAliasingOffscreenSampleCount
        {
            get => this.antiAliasingOffscreenSampleCount;
            set
            {
                if (this.antiAliasingOffscreenSampleCount == value)
                {
                    return;
                }

                this.antiAliasingOffscreenSampleCount = value;
                // TODO: waiting for a fix https://www.noesisengine.com/bugs/view.php?id=1686
                // then we can use a new method to apply the change
            }
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
        public TessellationMaxPixelError Quality
        {
            get => this.quality;
            set
            {
                if (this.quality.Equals(value))
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
        public RenderFlags RenderFlags
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

        /// <summary>
        /// Please note - it could change if the view is recreated.
        /// </summary>
        public View View => this.view;

        public void ApplyAntiAliasingSetting()
        {
            var content = this.view?.Content;
            if (content is null)
            {
                return;
            }

            content.PPAAMode = this.IsPPAAEnabled
                                   ? PPAAMode.Default
                                   : PPAAMode.Disabled;
            this.ApplyRenderingFlagsSetting();
        }

        public InputManager CreateInputManager(NoesisConfig config, Form form)
        {
            return new(this, config, form);
        }

        public void PreRender()
        {
            using (this.deviceState.Remember())
            {
                // TODO: consider not restoring device state if result was off (however we need to dispose temporary DX objects)
                this.renderer.RenderOffscreen();
            }
        }

        public void Render()
        {
            using (this.deviceState.Remember())
            {
                this.renderDeviceD3D11.DisableScissorRect();
                this.renderer.Render();
            }
        }

        public void SetSize(ushort width, ushort height)
        {
            this.view.SetSize(width, height);
            this.view.Update(this.lastUpdateTotalGameTime.TotalSeconds);
            // required in NoesisGUI 3.0, even if we don't render anything
            this.renderer.UpdateRenderTree();
        }

        public void Shutdown()
        {
            this.DestroyViewAndRenderer();
        }

        public void Update(GameTime gameTime)
        {
            this.lastUpdateTotalGameTime = gameTime.TotalGameTime;

            gameTime = this.CalculateRelativeGameTime(gameTime);
            this.view.Update(gameTime.TotalGameTime.TotalSeconds);
            // required in NoesisGUI 3.0, even if we don't render anything
            this.renderer.UpdateRenderTree();
        }

        /// <summary>
        /// Calculate game time since time of construction of this wrapper object (startup time).
        /// </summary>
        /// <param name="gameTime">MonoGame game time.</param>
        /// <returns>Time since startup of this wrapper object.</returns>
        internal GameTime CalculateRelativeGameTime(GameTime gameTime)
        {
            return new(gameTime.TotalGameTime - this.startupTotalGameTime,
                       gameTime.ElapsedGameTime);
        }

        private void ApplyQualitySetting()
        {
            this.view?.SetTessellationMaxPixelError(this.quality);
        }

        private void ApplyRenderingFlagsSetting()
        {
            var flags = this.renderFlags;
            flags |= RenderFlags.LCD;

            if (this.isPPAAEnabled)
            {
                flags |= RenderFlags.PPAA;
            }

            this.view?.SetFlags(flags);
        }

        private void CreateView()
        {
            if (this.view is not null)
            {
                return;
            }

            using (this.deviceState.Remember())
            {
                this.view = GUI.CreateView(this.rootElement);
                this.renderDeviceD3D11 = new RenderDeviceD3D11(this.deviceD3D11.ImmediateContext.NativePointer,
                                                               sRGB: false);

                // TODO: increased to deal with the glyph cache crash - refactor to move to NoesisConfig
                this.renderDeviceD3D11.GlyphCacheWidth = this.renderDeviceD3D11.GlyphCacheHeight = 2048;
                this.renderDeviceD3D11.OffscreenSampleCount = this.antiAliasingOffscreenSampleCount;
                this.renderer = this.view.Renderer;
                this.renderer.Init(this.renderDeviceD3D11);

                this.ApplyQualitySetting();
                this.ApplyAntiAliasingSetting();
                this.ApplyRenderingFlagsSetting();
            }
        }

        private void DestroyViewAndRenderer()
        {
            using (this.deviceState.Remember())
            {
                this.renderer.Shutdown();
            }

            this.view = null;
            this.renderer = null;
        }
    }
}