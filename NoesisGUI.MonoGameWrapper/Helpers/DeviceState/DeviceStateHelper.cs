namespace NoesisGUI.MonoGameWrapper.Helpers.DeviceState
{
    using System;

    internal abstract class DeviceStateHelper
    {
        public DeviceStateRestorer Remember()
        {
            this.Save();
            return new DeviceStateRestorer(this);
        }

        protected abstract void Restore();

        protected abstract void Save();

        /// <summary>
        /// Disposable structure which calls the state.Restore() method during Dispose().
        /// </summary>
        internal struct DeviceStateRestorer : IDisposable
        {
            private readonly DeviceStateHelper state;

            internal DeviceStateRestorer(DeviceStateHelper state)
            {
                this.state = state;
            }

            public void Dispose()
            {
                this.state.Restore();
            }
        }
    }
}