using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeUi : MonoBehaviour
{
    [Tooltip("Text field displaying the current episode number.")]
    [SerializeField] private TextMeshProUGUI episodeCountText;

    [Tooltip("Scoring settings used to determine how many boxes make up one episode.")]
    [SerializeField] private ScoringSettings scoringParameters;

    [Tooltip("Scrollbar used as a progress bar showing how far through the current episode we are.")]
    [SerializeField] private Scrollbar episodeProgressionScroll;

    private float episodeCount = 0;
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

    /// <summary>
    /// Called whenever any agent delivers a box. Increments the score, checks whether
    /// the episode target has been reached and updates both UI elements.
    /// </summary>
    private void MLAgentScript_onBoxPassed()
    {
        score++;
        if (CalculateEpisodeProgression() >= 1f)
        {
            episodeCount++;
            ShowEpisode();
            score = 0;
        }
        ShowEpisodeProgression();
    }

    /// <summary>
    /// Updates the episode count text field with the current episode number.
    /// </summary>
    private void ShowEpisode()
    {
        episodeCountText.text = "Episode " + episodeCount;
    }

    /// <summary>
    /// Updates the scrollbar size to reflect the current episode progression.
    /// </summary>
    private void ShowEpisodeProgression()
    {
        episodeProgressionScroll.size = CalculateEpisodeProgression();
    }

    /// <summary>
    /// Calculates progress through the current episode as a 0 to 1 value,
    /// based on boxes delivered across all agents against the episode target.
    /// Returns 0 if required settings or agent counts are not yet available.
    /// </summary>
    /// <returns>A normalised float between 0 and 1 representing episode completion.</returns>
    private float CalculateEpisodeProgression()
    {
        if (scoringParameters == null || scoringParameters.BoxesPerEpisode == 0 || GameManager.totalDelagets == 0)
        {
            return 0f;
        }
        return (float)score / (scoringParameters.BoxesPerEpisode * GameManager.totalDelagets);
    }
}