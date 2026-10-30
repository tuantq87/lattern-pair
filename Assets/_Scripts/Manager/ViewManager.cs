using System.Collections.Generic;
using LanternPair.UI.Views;
using PrimeTween;
using UnityEngine;

namespace LanternPair.Manager
{
    /// <summary>
    /// Shows one <see cref="BaseView"/> at a time in the main scene, with a short fade-in.
    /// Scene-scoped (no DontDestroyOnLoad): the views it manages live in the same scene.
    /// </summary>
    public class ViewManager : MonoBehaviour
    {
        public static ViewManager Instance { get; private set; }

        [Header("Views")]
        [SerializeField] private List<BaseView> views = new List<BaseView>();
        [SerializeField] private ViewType firstView = ViewType.Title;

        [Header("Transition")]
        [SerializeField] private float fadeInDuration = 0.25f;

        private readonly Dictionary<ViewType, BaseView> viewsByType = new Dictionary<ViewType, BaseView>();

        public BaseView Current { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            foreach (BaseView view in views)
            {
                if (view == null)
                {
                    Debug.LogWarning("ViewManager: empty slot in the views list");
                    continue;
                }

                if (viewsByType.ContainsKey(view.ViewType))
                {
                    Debug.LogWarning($"ViewManager: more than one view registered as {view.ViewType}");
                    continue;
                }

                viewsByType.Add(view.ViewType, view);
                view.Hide();
            }
        }

        void Start()
        {
            if (viewsByType.Count > 0) Show(firstView);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(ViewType type, object context = null)
        {
            if (!viewsByType.TryGetValue(type, out BaseView next))
            {
                Debug.LogWarning($"ViewManager: no view registered as {type}");
                return;
            }

            if (next == Current) return;

            if (Current != null) Current.Hide();
            Current = next;

            next.CanvasGroup.alpha = 0f;
            next.Show(context);
            Tween.Alpha(next.CanvasGroup, 1f, fadeInDuration);
        }
    }
}
