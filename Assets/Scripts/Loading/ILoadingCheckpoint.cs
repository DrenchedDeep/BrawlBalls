using System;
using Cysharp.Threading.Tasks;

namespace Loading
{
    public interface ILoadingCheckpoint
    {
        
        Action OnComplete { get; set; }
        Action OnFailed { get; set; }
        
        public UniTask Execute();

    }
}
