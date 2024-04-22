using HandyFSM;
using UnityEngine;
using UnityEngine.UI;

public class TriggerTest : MonoBehaviour
{
    #region Inspector

    [SerializeField]
    private Button _button;

    [SerializeField]
    private HandyMachine _fsm;

    #endregion

    #region Behaviour

    private void OnEnable()
    {
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnButtonClick);
    }

    #endregion

    private void OnButtonClick()
    {
        _fsm.SqueezeTrigger("tap");
    }

}