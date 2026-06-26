using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI episodeCountText;
    private float episodeCount = 0;

    [SerializeField] private ScoringSettings scoringParameters;
    [SerializeField] private Scrollbar episodeProgressionScroll;
    private int score = 0;

    private void Start()
    {
        ShowEpisodeProgression();
        ShowEpisode();
    }

    private void OnEnable()
    {
        MLAgentScript.onBoxPassed += MLAgentScript_onBoxPassed;
    }

    private void OnDisable()
    {
        MLAgentScript.onBoxPassed -= MLAgentScript_onBoxPassed;
    }

    private void MLAgentScript_onBoxPassed()
    {
        score++;

        if (CalculateEpisodeProgression() >= 1f)
        {
            episodeCount++;
            ShowEpisode();
            score = 0;
            CalculateEpisodeProgression();
        }

        ShowEpisodeProgression();
    }

    private void ShowEpisode()
    {
        episodeCountText.text = "Episode " + episodeCount;
    }

    private void ShowEpisodeProgression()
    {
        episodeProgressionScroll.size = CalculateEpisodeProgression();
    }

    private float CalculateEpisodeProgression()
    {
        if (scoringParameters == null || scoringParameters.BoxesPerEpisode == 0 || GameManager.totalDelagets == 0)
        {
            return 0f;
        }

        return (float)score / (scoringParameters.BoxesPerEpisode * GameManager.totalDelagets);
    }
}
