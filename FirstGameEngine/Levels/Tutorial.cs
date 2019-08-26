using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FirstGameEngine.Levels
{
	public static class Tutorial
	{
		private static int _stepIndex = 0;


		private static List<TutorialStep> TutorialSteps = new List<TutorialStep>()
		{
			new TutorialStep
			(
				"Welcome to NameThisLater.",
				new List<ActionTypes>(),
				new List<ActionTypes>()
			),
			new TutorialStep
			(
				"NameThisLater is a 2D pixel platformer. Let me teach you the basic controls for this game.",
				new List<ActionTypes>(),
				new List<ActionTypes>()
			),
			new TutorialStep
			(
				"Press the left and right arrow buttons to move to the left or right",
				new List<ActionTypes>() { ActionTypes.GoLeft, ActionTypes.GoRight },
				new List<ActionTypes>() { ActionTypes.GoLeft, ActionTypes.GoRight }
			),
			new TutorialStep
			(
				"Great! Now try to jump by pressing the up arrow",
				new List<ActionTypes>() { ActionTypes.Jump },
				new List<ActionTypes>() { ActionTypes.GoLeft, ActionTypes.GoRight, ActionTypes.Jump }
			),
			new TutorialStep
			(
				"Congrats, you have finished the tutorial. Press the next button to go back to main menu.",
				new List<ActionTypes>() { },
				new List<ActionTypes>() { ActionTypes.Fire, ActionTypes.AxeHit, ActionTypes.GoLeft, ActionTypes.GoRight, ActionTypes.Jump}
			)
		};

		private static TutorialStep CurrentStep => _stepIndex < TutorialSteps.Count ? TutorialSteps[_stepIndex] : TutorialSteps[0];

		public static string CurrentText => CurrentStep.Message;
		public static bool Finished => _stepIndex >= TutorialSteps.Count;


		public static void Update(List<ActionTypes> actions, TutorialBox tutorialBox, Matrix transform)
		{
			//Clean the actions so the game update doesn't allow unknown actions  
			actions.RemoveAll(x => !CurrentStep.AllowedActions.Contains(x));

			//Remove the required actions as they were used at this step index
			CurrentStep.RequiredActions.RemoveAll(x => actions.Contains(x));

			if (CurrentStep.RequiredActions.Count == 0 && tutorialBox.PassPressed(transform))
			{
				_stepIndex++;
			}
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
		private const int BtnWidth = 48;
		private const int BtnHeight = 24;

		private Button PassButton;
		private Texture2D BoxBG;

		public int X { get; private set; }
		public int Y { get; private set; }

		private int DrawX { get; set; }
		private int DrawY { get; set; }

		public bool PassPressed(Matrix transform) => PassButton.IsPressed(transform3: transform);

		public TutorialBox(int x, int y)
		{
			DrawX = X = x;
			DrawY = Y = y;
		}

		public void SetDrawPos(int newX, int newY)
		{
			DrawX = newX;
			DrawY = newY;

			PassButton.X = DrawX + BoxBG.Width - BtnWidth - (int)BtnOffset.X;
			PassButton.Y = DrawY + BoxBG.Height - BtnHeight - (int)BtnOffset.Y;
		}

		[Obsolete]
		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw
			(
				texture: BoxBG,
				destinationRectangle: new Rectangle(DrawX, DrawY, BoxBG.Width, BoxBG.Height),
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
