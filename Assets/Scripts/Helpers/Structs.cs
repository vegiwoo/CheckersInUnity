using System;

namespace Checkers.Helpers
{
    [Serializable]
	/// <summary>Игровой ход.</summary>
	public struct PlayStep
	{
		#region Variables and constants

		/// <summary>Имя игрока.</summary>
		public int PlayerNumber { get; private set; }
		/// <summary>Имя актора (игрового объекта).</summary>
		public string ActorName { get; private set; }
		/// <summary>Начальное положение актора (игрового объекта).</summary>
		public string ActorSource { get; private set; }
		/// <summary>Тип активности актора (игрового объекта).</summary>
		public ActorActionType ActorAction { get; private set; }
		/// <summary>Конечное положение актора (игрового объекта).</summary>
		public string ActorTarget { get; private set; }

		#endregion

		#region Constructors

		// Select 
		public PlayStep(int playerNumber, string actorName, string actorSource, ActorActionType action)
		{
			this.PlayerNumber = playerNumber;
			this.ActorName = actorName;
			this.ActorSource = actorSource;
			this.ActorAction = action;
			this.ActorTarget = default;
		}

		// Move 
		public PlayStep(int playerNumber, string actorName, string actorSource, ActorActionType action, string actorTarget)
		{
			this.PlayerNumber = playerNumber;
			this.ActorName = actorName;
			this.ActorSource = actorSource;
			this.ActorAction = action;
			this.ActorTarget = actorTarget;
		}

		// Remove
		public PlayStep(int playerNumber, ActorActionType action, string actorTarget)
		{
			this.PlayerNumber = playerNumber;
			this.ActorName = default;
			this.ActorAction = action;
			this.ActorSource = default;
			this.ActorTarget = actorTarget;
		}

		#endregion

		#region Methods

		/// <summary>Преобразует игровой шаг в строку описания.</summary>
		/// <returns>Строка описания игрового шага.</returns>
		public string StepToDescription()
		{
			string actorActionDescr = MyExtensions.GetEnumDescription((ActorActionType)(int)ActorAction);
			string resultString = default;

			switch (ActorAction)
			{
				case ActorActionType.Select:
					// Player 2: checker F6 Select
					resultString =
						$"Player {this.PlayerNumber}: {this.ActorName} {this.ActorSource} {actorActionDescr}";
					break;
				case ActorActionType.Move:
					// Player 1: checker C3 Move D4
					resultString =
						$"Player {this.PlayerNumber}: {this.ActorName} {this.ActorSource} {actorActionDescr} {ActorTarget}";
					break;
				case ActorActionType.Remove:
					// Player 1: Remove E5
					resultString =
						$"Player {this.PlayerNumber}: {actorActionDescr} {ActorTarget}";
					break;
			}
			return resultString;
		}

		public PlayStep DesciptionToStep(string descr)
		{
			// Player 2: checker F6 Select
			// Player 1: checker C3 Move D4
			// Player 1: Remove E5

			// Dummy
			return new PlayStep();
		}

		public override bool Equals(object obj)
		{
			return obj is PlayStep
				&& ((PlayStep)obj).PlayerNumber == PlayerNumber
				&& ((PlayStep)obj).ActorName == ActorName
				&& ((PlayStep)obj).ActorSource == ActorSource
				&& ((PlayStep)obj).ActorAction == ActorAction
				&& ((PlayStep)obj).ActorTarget == ActorTarget;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

	}
}