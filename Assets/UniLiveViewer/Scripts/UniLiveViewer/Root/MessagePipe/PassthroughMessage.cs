namespace UniLiveViewer.MessagePipe
{
    public class PassthroughMessage
    {
        public bool IsEnable => _isEnable;
        bool _isEnable;

        public PassthroughMessage(bool isEnable)
        {
            _isEnable = isEnable;
        }
    }
}