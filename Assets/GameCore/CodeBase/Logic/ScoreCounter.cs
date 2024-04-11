using System;
using System.Collections;
using CodeBase.Infrastructure.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class ScoreCounter
{
    private TextMeshProUGUI _textScore;
    private TextMeshProUGUI _textScoreTop;
    private SessionData _sessionData;
    private ICoroutineRunner _coroutineRunner;
    private int _totalScore;
    private float _maximumMultiplierTime;
    private Coroutine _checkingMultiplier;
    private UnityEvent<int> _multiplierWorked = new UnityEvent<int>();
    public int TotalScore { get => _totalScore; }
    public UnityEvent<int> MultiplierWorked { get => _multiplierWorked; }

    public ScoreCounter(ICoroutineRunner coroutineRunner, TextMeshProUGUI textScore, TextMeshProUGUI textScoreTop, SessionData sessionData) {
        _textScore = textScore;
        _textScoreTop = textScoreTop;
        _sessionData = sessionData;
        _coroutineRunner = coroutineRunner;
    }

    public void SetScore(int valueScore) { 
        _totalScore = valueScore;
        ScoreChange(_totalScore);
    }

    public void AddScore(int valueScore, bool isMerge = false) { 
        if(isMerge) {
            if(_maximumMultiplierTime > 0) {
                if(_maximumMultiplierTime < 0.5f) {
                    valueScore *= 3;
                    _multiplierWorked?.Invoke(3);
                }else if(_maximumMultiplierTime < 1f){
                    valueScore *= 2;
                    _multiplierWorked?.Invoke(2);
                }
            } 

            if(_checkingMultiplier != null) _coroutineRunner.StopCoroutine(_checkingMultiplier);
            _checkingMultiplier = _coroutineRunner.StartCoroutine(CheckingMultiplier());
        }

        _totalScore += valueScore;
        ScoreChange(_totalScore); 
    }

    private void ScoreChange(int score) {
        _textScore.text = Convert.ToString(score);
        _sessionData.currentScore = score;
        if(score > _sessionData.topScore){
            _sessionData.topScore = score;
            _textScoreTop.text = Convert.ToString(_sessionData.topScore);
        } 
    }
    
    private IEnumerator CheckingMultiplier() { 
        _maximumMultiplierTime = 1.1f;
        while(_maximumMultiplierTime >= -0.1f) { 
            _maximumMultiplierTime -= Time.deltaTime;
            yield return null;
        } 
    } 
}
