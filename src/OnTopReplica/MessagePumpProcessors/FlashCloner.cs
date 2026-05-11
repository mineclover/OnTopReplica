namespace OnTopReplica.MessagePumpProcessors {

    /// <summary>
    /// Automatically clones windows that are flashing.
    /// </summary>
    class FlashCloner : BaseMessagePumpProcessor {

        public override bool Process(ref System.Windows.Forms.Message msg) {
#if FLASH_CLONER_ENABLED
            if (msg.Msg == Native.HookMethods.WM_SHELLHOOKMESSAGE) {
                int hookCode = msg.WParam.ToInt32();

                if (hookCode == Native.HookMethods.HSHELL_FLASH) {
                    System.IntPtr flashHandle = msg.LParam;

                    Form.SetThumbnail(new WindowHandle(flashHandle), null);
                }
            }
#endif

            return false;
        }

        protected override void Shutdown() {
        }

    }

}
