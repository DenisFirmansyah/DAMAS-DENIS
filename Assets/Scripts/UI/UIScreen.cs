using System.Collections;
using UnityEngine;

public abstract class UIScreen : MonoBehaviour
{
    [Header("Screen Animation")]
    [SerializeField, Min(0f)] protected float showDuration = 0.25f;
    [SerializeField, Min(0f)] protected float hideDuration = 0.2f;

    public bool IsVisible { get; private set; }

    private Coroutine transitionCoroutine;

    protected virtual void Awake()
    {
        IsVisible = gameObject.activeSelf;
    }

    protected virtual void OnEnable()
    {
        IsVisible = true;
    }

    protected virtual void OnDisable()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }

    public virtual void Show(bool animate = true)
    {
        CancelTransition();
        gameObject.SetActive(true);
        IsVisible = true;
        OnBeforeShow();

        if (animate)
            transitionCoroutine = StartCoroutine(ShowRoutine());
        else
            CompleteShow();
    }

    public virtual void Hide(bool animate = true)
    {
        CancelTransition();
        OnBeforeHide();

        if (animate)
            transitionCoroutine = StartCoroutine(HideRoutine());
        else
            CompleteHide();
    }

    private IEnumerator ShowRoutine()
    {
        yield return AnimateShow();
        CompleteShow();
    }

    private IEnumerator HideRoutine()
    {
        yield return AnimateHide();
        CompleteHide();
    }

    private void CompleteShow()
    {
        IsVisible = true;
        OnShowComplete();
        transitionCoroutine = null;
    }

    private void CompleteHide()
    {
        IsVisible = false;
        gameObject.SetActive(false);
        OnHideComplete();
        transitionCoroutine = null;
    }

    private void CancelTransition()
    {
        if (transitionCoroutine == null)
            return;

        StopCoroutine(transitionCoroutine);
        transitionCoroutine = null;
    }

    protected virtual void OnBeforeShow() { }
    protected virtual void OnShowComplete() { }
    protected virtual void OnBeforeHide() { }
    protected virtual void OnHideComplete() { }

    protected virtual IEnumerator AnimateShow()
    {
        if (showDuration > 0f)
            yield return null;
    }

    protected virtual IEnumerator AnimateHide()
    {
        if (hideDuration > 0f)
            yield return null;
    }
}
