using System;
using DragonsAndMachines.Integration;
using DragonsAndMachines.Unity.Presentation;
using DragonsAndMachines.Unity.UI;
using DragonsAndMachines.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace DragonsAndMachines.Unity.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private GameSession _session;
        private IsoHexMapView _map;
        private IsoCameraController _camera;
        private UIDocument _ui;
        private GameHud _hud;
        private WorldState _focusedWorld;
        private float _tickAccum;
        private bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null)
                return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Start()
        {
            try
            {
                Initialize();
                _initialized = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                enabled = false;
            }
        }

        private void Initialize()
        {
            EnsureEventSystem();
            _session = new GameSession();
            _session.GenerateWorld();

            var mapGo = new GameObject("HexMap");
            _map = mapGo.AddComponent<IsoHexMapView>();
            _map.Bind(_session);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            _camera = cam.GetComponent<IsoCameraController>();
            if (_camera == null)
                _camera = cam.gameObject.AddComponent<IsoCameraController>();
            _camera.Focus(HexLayout.MapCenter(_session.World.Width, _session.World.Height) + Vector3.up * 0.2f);
            _focusedWorld = _session.World;
            _session.WorldChanged += OnWorldChanged;

            var uiGo = new GameObject("HUD");
            _ui = uiGo.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.sortingOrder = 10;
            var runtimeTheme = Resources.Load<ThemeStyleSheet>("UnityDefaultRuntimeTheme");
            if (runtimeTheme == null)
                throw new InvalidOperationException("Не найдена стандартная Runtime Theme Style Sheet UnityDefaultRuntimeTheme.");
            settings.themeStyleSheet = runtimeTheme;
            _ui.panelSettings = settings;
            _ui.rootVisualElement.Clear();
            _hud = new GameHud(_session, _ui.rootVisualElement);
        }

        private void Update()
        {
            if (!_initialized)
                return;

            _tickAccum += Time.deltaTime;
            if (_tickAccum >= 1.25f)
            {
                _tickAccum = 0f;
                _session.Tick(1);
            }

            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;
            if (IsPointerOverUi(mouse.position.ReadValue()))
                return;
            var cam = Camera.main;
            if (cam == null)
                return;
            var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (_map.TryPick(ray, out var coord))
            {
                _session.SelectedCell = coord;
                _map.SetSelected(coord);
                _hud.Refresh();
            }
        }

        private bool IsPointerOverUi(Vector2 screenPos)
        {
            if (_ui?.rootVisualElement?.panel == null)
                return false;
            var panelPos = RuntimePanelUtils.ScreenToPanel(_ui.rootVisualElement.panel, screenPos);
            var picked = _ui.rootVisualElement.panel.Pick(panelPos);
            return picked != null && picked != _ui.rootVisualElement;
        }

        private void OnWorldChanged()
        {
            _map.SetSelected(_session.SelectedCell);
            if (_focusedWorld == _session.World)
                return;

            _focusedWorld = _session.World;
            _camera.Focus(HexLayout.MapCenter(_focusedWorld.Width, _focusedWorld.Height) + Vector3.up * 0.2f);
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }
    }
}
