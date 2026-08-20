using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ui_skill : MonoBehaviour
{
    public GameObject player;
    public GameObject timerText;

    private PlayerHealth _playerHealth;
    private Animator _animator;
    private TextMeshProUGUI _timerTextMesh;
    private Image _skillImage;

    private static readonly int UseHash = Animator.StringToHash("use");

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (_playerHealth == null)
        {
            if (player != null)
                player.TryGetComponent<PlayerHealth>(out _playerHealth);
            if (_playerHealth == null)
                _playerHealth = PlayerHealth.Instance;
        }

        if (_animator == null)
            TryGetComponent<Animator>(out _animator);

        if (_skillImage == null)
            TryGetComponent<Image>(out _skillImage);

        if (_timerTextMesh == null && timerText != null)
            timerText.TryGetComponent<TextMeshProUGUI>(out _timerTextMesh);
    }

    public void TriggerSkill(Sprite sprite = null)
    {
        InitializeComponents();

        if (sprite != null && _skillImage != null)
        {
            _skillImage.sprite = sprite;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (_animator != null)
        {
            _animator.SetBool(UseHash, true);
            _animator.Play("skill", 0, 0f);
        }
    }

    public void animateSkill()
    {
        if (_playerHealth != null)
            _playerHealth.activeDuration = 20f;
    }

    public void updateTimer()
    {
        if (timerText != null)
            timerText.SetActive(true);
    }

    public void UpdateTimer() => updateTimer();

    private void Update()
    {
        if (_playerHealth == null)
        {
            if (player != null)
                player.TryGetComponent<PlayerHealth>(out _playerHealth);
            if (_playerHealth == null)
                _playerHealth = PlayerHealth.Instance;
        }

        if (_playerHealth != null)
        {
            if (_playerHealth.activeDuration > 0f)
            {
                if (_animator != null)
                    _animator.SetBool(UseHash, true);

                if (_timerTextMesh != null)
                    _timerTextMesh.text = _playerHealth.activeDuration.ToString("F2");
            }
            else
            {
                if (_animator != null)
                    _animator.SetBool(UseHash, false);

                if (timerText != null && timerText.activeSelf)
                    timerText.SetActive(false);
            }
        }
    }
}
