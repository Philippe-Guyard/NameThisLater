using System;
using System.Collections.Generic;
using System.Text;

namespace FirstGameEngine.Levels
{
	public static class Tutorial
	{
		private static int _stepIndex = 0;

		private static List<TutorialStep> TutorialSteps = new List<TutorialStep>()
		{

		};
		private static TutorialBox CurrentBox = new TutorialBox();

		private static TutorialStep CurrentStep => TutorialSteps[_stepIndex];


		public static void Update(List<ActionTypes> actions)
		{
			//Clean the actions so the game update doesn't allow unknown actions  
			actions.RemoveAll(x => !CurrentStep.AllowedActions.Contains(x));

			//Remove the required actions as they were used at this step index
			CurrentStep.RequiredActions.RemoveAll(x => actions.Contains(x));

			if (CurrentStep.RequiredActions.Count == 0 && CurrentBox.PassPressed)
				_stepIndex++;
		}

		private struct TutorialStep
		{
			public string Message { get; set; }
			public List<ActionTypes> RequiredActions { get; set; }
			public List<ActionTypes> AllowedActions { get; set; }

			public TutorialStep(string msg, List<ActionTypes> required, List<ActionTypes> allowed)
			{
				Message = msg;
				RequiredActions = required;
				AllowedActions = allowed;
			}
		}

		//UI element: label with button to pass
		private class TutorialBox
		{
			private Button PassButton;

			public bool PassPressed => PassButton.IsPressed();

			public TutorialBox()
			{

			}
		}
	}
}
