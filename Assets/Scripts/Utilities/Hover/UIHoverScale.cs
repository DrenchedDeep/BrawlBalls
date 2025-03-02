using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities.Hover
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Vector2 hoverSize = Vector2.zero;
        [SerializeField] private float transitionDuration;
        [SerializeField] private AnimationCurve transitionCurve;

        private Vector2 _originalScale;
        private RectTransform  _rectTransform;
        private Coroutine _hoverCoroutine;

        private float _currentTransitionTime;
        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            if(_rectTransform == null) throw new UnityException("Missing component of RectTransform");
            _originalScale = _rectTransform.sizeDelta;
        
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter");
            if (_hoverCoroutine != null)
            {
                _currentTransitionTime = transitionDuration - _currentTransitionTime;
                StopCoroutine(_hoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(Transition(_rectTransform.sizeDelta, hoverSize));
        }

  
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hoverCoroutine != null)
            {
                _currentTransitionTime = transitionDuration - _currentTransitionTime;
                StopCoroutine(_hoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(Transition(_rectTransform.sizeDelta, _originalScale));
        }
        
        private IEnumerator Transition(Vector2 start, Vector2 end)
        {
            while (_currentTransitionTime < transitionDuration)
            {
                _rectTransform.sizeDelta = Vector2.Lerp(start, end, transitionCurve.Evaluate(_currentTransitionTime / transitionDuration));
                _currentTransitionTime += Time.deltaTime;
                yield return null;
            }
            _currentTransitionTime = 0;
            _hoverCoroutine = null;
        }

    }
}
