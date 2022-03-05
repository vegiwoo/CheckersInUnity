using System;
using System.Linq;
using Checkers.Interfaces;

namespace Checkers.Helpers
{
	/// <summary>Игровой ход.</summary>
	public struct PlayStep : IPlayStepable
	{
		#region Variables and constants

		public int PlayerNumber { get; set; }
		public string ActorName { get; set; }
		public string ActorSource { get; set; }
		public ActorActionType ActorAction { get; set; }
		public string ActorTarget { get; set; }

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

		/// <summary>Преобразует описание игрового шага в игровой шаг.</summary>
		/// <param name="description">Описание игрового шага.</param>
		/// <returns>Игровой шаг.</returns>
		public static IPlayStepable DesciptionToStep(string description)
		{
			PlayStep playStep = default;

			string[] subs01 = description.Split(':');

			for (int i = 0; i < subs01.Count(); i++)
			{
				subs01[i].Trim();
			}

            int playerNumber = Convert.ToInt32(subs01[0].Trim().Split(' ')[1]);

            string[] subs02 = subs01[1].Trim().Split(' ').Where(val => val != " ").ToArray();

            for (int i = 0; i < subs02.Count(); i++)
            {
                subs02[i].Trim();
            }

            switch (subs02.Count())
            {
				case 2:     // Remove case
					playStep = new PlayStep(playerNumber, ActorActionType.Remove, subs02[1]);
					break;
				case 3:     // Select case
					playStep = new PlayStep(playerNumber, subs02[0], subs02[1], ActorActionType.Select);
                    break;
                case 4:     // Move case
					playStep = new PlayStep(playerNumber, subs02[0], subs02[1], ActorActionType.Move, subs02[3]);
					break;
                default:
                    break;
            }

            return playStep;
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