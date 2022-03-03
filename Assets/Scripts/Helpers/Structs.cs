namespace Checkers.Helpers
{
	/// <summary>Игровой ход.</summary>
	public struct PlayStep
	{
		/// <summary>Имя игрока.</summary>
		public string PlayerName { get; set; }
		/// <summary>Имя актора (игрового объекта).</summary>
		public string ActorName { get; set; }
		/// <summary>Начальное положение актора (игрового объекта).</summary>
		public string ActorSource { get; set; }
		/// <summary>Тип активности актора (игрового объекта).</summary>
		public ActorActionType ActorAction { get; set; }
		/// <summary>Коненое положение актора (игрового объекта).</summary>
		public string ActorTarget { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PlayStep
				&& ((PlayStep)obj).PlayerName == PlayerName
				&& ((PlayStep)obj).ActorName == ActorName
				&& ((PlayStep)obj).ActorSource == ActorSource
				&& ((PlayStep)obj).ActorAction == ActorAction
				&& ((PlayStep)obj).ActorTarget == ActorTarget;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}