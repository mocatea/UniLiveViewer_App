namespace UniLiveViewer.MessagePipe
{
    public class PlayerInputOperationMessage
    {
        public bool IsOperable => _isOperable;
        bool _isOperable;

        public PlayerInputOperationMessage(bool isOperable)
        {
            _isOperable = isOperable;
        }
    }
}