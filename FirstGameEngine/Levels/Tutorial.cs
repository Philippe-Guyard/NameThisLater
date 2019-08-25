using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FirstGameEngine.Levels
{
	public static class Tutorial
	{
		private static int _stepIndex = 0;


		private static List<TutorialStep> TutorialSteps = new List<TutorialStep>()
		{

		};

		private static TutorialStep CurrentStep => TutorialSteps[_stepIndex];


		public static void Update(List<ActionTypes> actions, TutorialBox tutorialBox)
		{
			//Clean the actions so the game update doesn't allow unknown actions  
			actions.RemoveAll(x => !CurrentStep.AllowedActions.Contains(x));

			//Remove the required actions as they were used at this step index
			CurrentStep.RequiredActions.RemoveAll(x => actions.Contains(x));

			if (CurrentStep.RequiredActions.Count == 0 && tutorialBox.PassPressed())
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
	}

	//UI element: label with button to pass
	public class TutorialBox
	{
		private static readonly Vector2 BtnOffset = new Vector2(1, 1);
		private const int BtnWidth = 16;
		private const int BtnHeight = 8;

		private Button PassButton;
		private Texture2D BoxBG;

		public int X { get; private set; }
		public int Y { get; private set; }

		public bool PassPressed() => PassButton.IsPressed();

		public TutorialBox(int x, int y)
		{
			X = x;
			Y = y;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw
			(
				texture: BoxBG,
				destinationRectangle: new Rectangle(X, Y, BoxBG.Width, BoxBG.Height),
				layerDepth: Constants.HeroLayerDepth - 0.0001f
			);

			PassButton.Draw(spriteBatch);
		}

		//TODO: Remove graphics device after we find a texture
		public void LoadContent(GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Content.ContentManager contentManager, string boxBG)
		{
			BoxBG = contentManager.Load<Texture2D>(boxBG);

			PassButton = new Button
			(
				X + BoxBG.Width - BtnWidth - BtnOffset.X,
				Y + BoxBG.Height - BtnOffset.Y - BtnHeight,
				BtnWidth,
				BtnHeight
			);
			PassButton.GraphicsDevice = graphicsDevice;
		}
	}
}
