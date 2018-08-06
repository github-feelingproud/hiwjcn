using System;

namespace Lib.core
{
    [Serializable]
    public class MsgException : Exception
    {
        public virtual string ErrorCode { get; set; }

        public MsgException(string msg) : base(msg)
        { }
    }

    [Serializable]
    public class SourceNotExistException : Exception
    {
        public SourceNotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class AccessDenyException : Exception
    {
        public AccessDenyException(string msg) : base(msg) { }
    }

    [Serializable]
    public class DeleteNodeException : Exception
    {
        public DeleteNodeException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NodeNotExistException : Exception
    {
        public NodeNotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NodeTooMuchException : Exception
    {
        public NodeTooMuchException(string msg) : base(msg) { }
    }

    [Serializable]
    public class ExceptionLogException : Exception
    {
        public ExceptionLogException(string msg) : base(msg) { }
    }
}
