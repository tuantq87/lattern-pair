using UnityEngine;

namespace LanternPair.UI.Views
{
    /// <summary>
    /// Base class for every full-screen view. <see cref="Manager.ViewManager"/> shows one at a time and fades it in.
    /// Override <see cref="OnShow"/> / <see cref="OnHide"/> to refresh data and (un)subscribe from events.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour
    {
        [SerializeField] private ViewType viewType;

        private CanvasGroup canvasGroup;

        public ViewType ViewType => viewType;

        public bool IsVisible { get; private set; }

        // Looked up lazily: views start inactive, so Awake may not have run when ViewManager first needs it.
        public CanvasGroup CanvasGroup => canvasGroup != null ? canvasGroup : canvasGroup = GetComponent<CanvasGroup>();

        public void Show(object context = null)
        {
            gameObject.SetActive(true);
            IsVisible = true;
            OnShow(context);
        }

        public void Hide()
        {
            IsVisible = false;
            OnHide();
            gameObject.SetActive(false);
        }

        /// <param name="context">Optional data from whoever opened the view, e.g. the finished run for GameOver.</param>
        protected virtual void OnShow(object context) { }

        protected virtual void OnHide() { }
    }
}
