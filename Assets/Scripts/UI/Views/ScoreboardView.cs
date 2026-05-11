using TMPro;
using UnityEngine;

namespace Player.UI
{
    public sealed class ScoreboardView : MonoBehaviour, IScoreboardView
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private string _prefix = "Score";

        public void SetScore(int score)
        {
            if (_scoreText == null)
                return;

            _scoreText.text = $"{_prefix}: {score}";
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
