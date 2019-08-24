using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirstGameEngine
{
    public enum MovementDirection
    {
        Right,
        Left,
        Up,
        Down,
        NoMovement,
    }

    public enum CollisionType
    {
        NoCollision,
        Top,
        Right,
        Left,
        Bottom,
    }

    public enum ActionTypes
    {
        GoLeft,
        GoRight,
        ClimbLader,
        Jump,
        GoDownLader,
        Fire,
        StopClimb,
        AxeHit,
    }

    public enum ObstacleType
    {
        Chest,
        EnhancedChest,
        Lader,
        Tramway,
        Floor,
        Haystack,
    }

    public enum MonsterType
    {
        Farmer,
        Guard,
    }

    public enum MonsterBehaviorType
    {
        Patrol,
        Pursue,
    }

    public enum GameState
    {
        InLevel,
        StartMenu,
        Levels,
		Tutorial,
        Settings,
        Died,
    }
}