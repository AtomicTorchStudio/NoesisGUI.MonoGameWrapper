namespace NoesisGUI.MonoGameWrapper.Helpers.DeviceState
{
    using System;

    internal abstract class DeviceStateHelper
    {
        public IDisposable Remember()
        {
            this.Save();
            return new DeviceStateRestorer(this);
        }

        protected abstract void Restore();

        protected abstract void Save();

        /// <summary>
        /// Disposable structure which calls the state.Restore() method during Dispose().
        /// </summary>
        private struct DeviceStateRestorer : IDisposable
        {
            private readonly DeviceStateHelper state;

            public DeviceStateRestorer(DeviceStateHelper state)
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