using UnityEngine;

namespace Player.Helpers
{
    /// <summary>
    /// Константы для анимаций (п. 3.1.4 лекции - константы и конфигурация)
    /// </summary>
    public static class AnimationConstants
    {
        public const string BLEND_PARAM = "Blend";
        public const string INPUT_X_PARAM = "InputX";
        public const string INPUT_Z_PARAM = "InputZ";
        public const string NORMAL_STATE = "normal";
        public const string ANGRY_STATE = "angry";
        public const string HAPPY_STATE = "happy";
        public const string DEAD_STATE = "dead";

        public static class EyeOffsets
        {
            public static readonly Vector2 NORMAL = new Vector2(0, 0);
            public static readonly Vector2 HAPPY = new Vector2(0.33f, 0);
            public static readonly Vector2 ANGRY = new Vector2(0.66f, 0);
            public static readonly Vector2 DEAD = new Vector2(0.33f, 0.66f);
        }
    }
}