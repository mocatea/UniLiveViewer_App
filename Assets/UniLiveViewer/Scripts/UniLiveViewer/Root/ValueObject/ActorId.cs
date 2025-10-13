using System;
using UniLiveViewer.Actor;

namespace UniLiveViewer.ValueObject
{
    /// <summary>
    /// MEMO: null使いたいのでstructにしない
    /// </summary>
    public sealed class ActorId : IEquatable<ActorId>
    {
        /// <summary> アクター種類 </summary>
        public ActorType Type { get; }

        /// <summary>
        /// 登録順なIndexと同義
        /// NOTE: 同じVRMをロードした場合は異なるIDとする
        /// </summary>
        public int Id { get; }

        public ActorId(ActorType type, int id)
        {
            Type = type;
            Id = id;
        }

        public bool Equals(ActorId other)
            => other is not null && Type == other.Type && Id == other.Id;

        public override bool Equals(object obj)
            => obj is ActorId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Type, Id);

        public static bool operator ==(ActorId left, ActorId right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(ActorId left, ActorId right)
            => !(left == right);

        public override string ToString() => $"{Type}:{Id}";
    }
}
