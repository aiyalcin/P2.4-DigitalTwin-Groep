using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI episodeCountText;
    private int episodeCount;

    [SerializeField] private ScoringParameters scoringParameters;
    [SerializeField] private Scrollbar episodeProgressionScroll;
    private int score;

    private void OnEnable()
    {
        MLAgentScript.onEpisodeBegan += MLAgentScript_onEpisodeBegan;
        MLAgentScript.onBoxPassed += MLAgentScript_onBoxPassed;
    }

    private void OnDisable()
    {
        MLAgentScript.onEpisodeBegan -= MLAgentScript_onEpisodeBegan;
        MLAgentScript.onBoxPassed -= MLAgentScript_onBoxPassed;
    }

    private void MLAgentScript_onEpisodeBegan()
    {
        episodeCount++;
        episodeCountText.text = "Episode " + episodeCount.ToString();

        score = 0;
        ShowScore();
    }

    private void MLAgentScript_onBoxPassed()
    {
        score++;

        ShowScore();
    }

    private void ShowScore()
    {
        episodeProgressionScroll.size = CalculateScore();
    }

    private float CalculateScore()
    {
        if (scoringParameters == null || scoringParameters.BoxesPerEpisode == 0)
        {
            return 0f;
        }

        return (float)score / scoringParameters.BoxesPerEpisode;
    }
}
