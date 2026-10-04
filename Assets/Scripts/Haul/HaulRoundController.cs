using System;

using InteractionSystem;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

namespace HaulSystem
{
    [DisallowMultipleComponent]
    public sealed class HaulRoundController : MonoBehaviour
    {
        [SerializeField] private Inventory _inventory;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private PlayerInteractor _playerInteractor;
        [SerializeField] private MessagePanelView _messagePanel;
        [SerializeField, Min(1)] private int _bronzeScore = 800;
        [SerializeField, Min(1)] private int _silverScore = 1200;
        [SerializeField, Min(1)] private int _goldScore = 1450;

        private HaulRoundPresenter _presenter;
        private int _completionFrame = -1;
        private bool _inputLocked;
        private bool _inventoryWasEnabled;
        private bool _playerWasEnabled;
        private bool _cameraWasEnabled;
        private bool _interactorWasEnabled;

        public HaulRoundModel Model { get; private set; }
        public bool CanExtract => isActiveAndEnabled && Model != null && Model.State == HaulRoundState.Collecting;

        private void Awake()
        {
            if (_inventory == null || _playerController == null || _cameraController == null
                || _playerInteractor == null || _messagePanel == null || !_messagePanel.IsConfigured)
            {
                Debug.LogError("HaulRoundController requires inventory, player, camera, interactor and a configured MessagePanelView.", this);
                enabled = false;
                return;
            }

            if (transform.IsChildOf(_messagePanel.transform))
            {
                Debug.LogError("HaulRoundController must be outside the MessagePanel hierarchy.", this);
                enabled = false;
                return;
            }

            _messagePanel.Hide();
        }

        private void Start()
        {
            if (_inventory.Model == null)
            {
                Debug.LogError("HaulRoundController requires an initialized inventory.", this);
                enabled = false;
                return;
            }

            try
            {
                Model = new HaulRoundModel(_inventory.Model, _bronzeScore, _silverScore, _goldScore);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"Invalid haul round configuration: {exception.Message}", this);
                enabled = false;
                return;
            }

            ConnectPresentation();
        }

        private void OnEnable()
        {
            if (Model != null) ConnectPresentation();
        }

        private void OnDisable()
        {
            if (Model != null) Model.Completed -= OnRoundCompleted;
            _presenter?.Dispose();
            _presenter = null;
            if (_messagePanel != null) _messagePanel.Hide();
            RestorePlayerInput();
        }

        private void Update()
        {
            if (Model == null || Model.State != HaulRoundState.Results || !Application.isFocused
                || Time.frameCount <= _completionFrame) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                foreach (KeyControl key in keyboard.allKeys)
                {
                    if (!key.wasPressedThisFrame) continue;
                    RestartLevel();
                    return;
                }
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame
                || mouse.middleButton.wasPressedThisFrame || mouse.forwardButton.wasPressedThisFrame
                || mouse.backButton.wasPressedThisFrame))
            {
                RestartLevel();
            }
        }

        public bool TryExtract(Inventory inventory)
        {
            if (!CanExtract || inventory != _inventory || !_inventory.isActiveAndEnabled || _inventory.IsOpen) return false;

            try
            {
                return Model.TryComplete();
            }
            catch (OverflowException)
            {
                Debug.LogError("The haul score exceeds the supported range.", this);
                return false;
            }
        }

        private void ConnectPresentation()
        {
            if (_presenter != null) return;
            Model.Completed += OnRoundCompleted;
            if (Model.State != HaulRoundState.Collecting)
            {
                LockPlayerInput();
                _completionFrame = Time.frameCount;
            }

            _presenter = new HaulRoundPresenter(Model, _messagePanel);
        }

        private void OnRoundCompleted(HaulResult result)
        {
            _completionFrame = Time.frameCount;
            LockPlayerInput();
        }

        private void LockPlayerInput()
        {
            if (_inputLocked) return;
            _inventoryWasEnabled = _inventory.enabled;
            _playerWasEnabled = _playerController.enabled;
            _cameraWasEnabled = _cameraController.enabled;
            _interactorWasEnabled = _playerInteractor.enabled;
            _inputLocked = true;

            _playerInteractor.enabled = false;
            _inventory.enabled = false;
            _playerController.enabled = false;
            _cameraController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RestorePlayerInput()
        {
            if (!_inputLocked) return;
            _inputLocked = false;
            if (_playerController != null) _playerController.enabled = _playerWasEnabled;
            if (_playerInteractor != null) _playerInteractor.enabled = _interactorWasEnabled;
            if (_cameraController != null) _cameraController.enabled = _cameraWasEnabled;
            if (_inventory != null) _inventory.enabled = _inventoryWasEnabled;
        }

        private void RestartLevel()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError("The active scene must be included in Build Settings to restart.", this);
                return;
            }

            if (!Model.TryBeginRestart()) return;
            SceneManager.LoadSceneAsync(scene.buildIndex);
        }
    }
}
