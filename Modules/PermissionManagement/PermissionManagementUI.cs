using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Modules.PermissionManagement
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class PermissionManagementUI : YamaPlayerListener
  {
    [SerializeField] private PermissionManagement _permissionManagement;
    [SerializeField] private LoopScroll _permissionScroll;
    [SerializeField] private Toggle _permissionToggle;
    [SerializeField] private GameObject _permissionPage;
    [SerializeField] private Color _ownerColor = new Color(0.39f, 0.71f, 0.96f, 1f);
    [SerializeField] private Color _adminColor = new Color(0.73f, 0.41f, 0.78f, 1f);
    [SerializeField] private Color _editorColor = new Color(0.51f, 0.78f, 0.52f, 1f);
    [SerializeField] private Color _viewerColor = new Color(1f, 0.72f, 0.30f, 1f);

    [Header("Localization")]
    [SerializeField] private Text _toggleLabelText;
    [SerializeField] private Text _descriptionText;

    private UIController _uiController;
    private int _permissionIndex = -1;

    private void Start()
    {
      _uiController = GetComponentInParent<UIController>();
      if (!Utilities.IsValid(_uiController) || !Utilities.IsValid(_permissionManagement)) return;
      _permissionManagement.AddListener(this);
      _uiController.AddListener(this);
      UpdateTranslation();
      GeneratePermissionView();
    }

    public void BeforeUserChangePlayerHandler() => CheckPermission();
    public void BeforeUserPlayTrack() => CheckPermission();
    public void BeforeUserPlayVideo() => CheckPermission();
    public void BeforeUserPauseVideo() => CheckPermission();
    public void BeforeUserStopVideo() => CheckPermission();
    public void BeforeUserSetTime() => CheckPermission();
    public void BeforeUserBackward() => CheckPermission();
    public void BeforeUserForward() => CheckPermission();
    public void BeforeUserReloadVideo() => CheckPermission();
    public void BeforeUserChangeLoop() => CheckPermission();
    public void BeforeUserChangeShufflePlay() => CheckPermission();
    public void BeforeUserChangeSpeed() => CheckPermission();
    public void BeforeUserChangeRepeat() => CheckPermission();
    public void BeforeUserAddTrackToQueue() => CheckPermission();
    public void BeforeUserRemoveTrackFromQueue() => CheckPermission();
    public void BeforeUserMoveTrackUp() => CheckPermission();
    public void BeforeUserMoveTrackDown() => CheckPermission();

    private void CheckPermission()
    {
      if (!Utilities.IsValid(_uiController) || !Utilities.IsValid(_permissionManagement)) return;
      if ((int)_permissionManagement.PlayerPermission >= (int)PlayerPermission.Editor) return;

      _uiController.CancelCurrentAction();
      _uiController.ShowMessage(
        _uiController.GetTranslation("module.permissionManagement.noPermission"),
        _uiController.GetTranslation("module.permissionManagement.noPermissionMessage")
      );
    }

    public void GeneratePermissionView()
    {
      if (!Utilities.IsValid(_permissionManagement) || !Utilities.IsValid(_permissionScroll)) return;

      _permissionScroll.SetUp(_permissionManagement.PermissionData.Count, this, nameof(UpdatePermissionView));

      bool showPage = (int)_permissionManagement.PlayerPermission >= (int)PlayerPermission.Admin;

      if (Utilities.IsValid(_permissionToggle)) _permissionToggle.gameObject.SetActive(showPage);
      if (Utilities.IsValid(_permissionPage)) _permissionPage.SetActive(showPage && _permissionToggle.isOn);
    }

    public void UpdatePermissionView()
    {
      if (!Utilities.IsValid(_permissionScroll)) return;

      ScrollRect scrollRect = _permissionScroll.GetComponent<ScrollRect>();
      if (!Utilities.IsValid(scrollRect)) return;

      for (int i = 0; i < _permissionScroll.LineCount; i++)
      {
        int index = _permissionScroll.Indexes[i];
        if (index == _permissionScroll.LastIndexes[i] || index == -1) continue;

        Transform cell = scrollRect.content.GetChild(i);
        DataToken value = _permissionManagement.PermissionData.GetValues()[index];

        if (value.DataDictionary.TryGetValue("displayName", TokenType.String, out DataToken displayName))
        {
          var name = cell.Find("Name");
          if (Utilities.IsValid(name))
          {
            var nameText = name.GetComponent<Text>();
            if (Utilities.IsValid(nameText))
              nameText.text = displayName.String;
          }
        }

        PlayerPermission permission = (PlayerPermission)value.DataDictionary["permission"].Int;
        bool couldControl = (int)_permissionManagement.PlayerPermission > (int)permission;

        var label = cell.Find("Label");
        if (Utilities.IsValid(label))
        {
          var labelText = label.GetComponent<Text>();
          if (Utilities.IsValid(labelText))
            labelText.text = permission == PlayerPermission.Owner ? "Owner" : "Admin";
        }

        var dropdown = cell.Find("Dropdown");
        if (Utilities.IsValid(dropdown))
        {
          dropdown.gameObject.SetActive(couldControl);
        }

        var mark = cell.Find("Mark");
        if (Utilities.IsValid(mark))
        {
          var markImage = mark.GetComponent<Image>();
          if (Utilities.IsValid(markImage))
          {
            switch (permission)
            {
              case PlayerPermission.Owner:
                markImage.color = _ownerColor;
                break;
              case PlayerPermission.Admin:
                markImage.color = _adminColor;
                if (Utilities.IsValid(dropdown)) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(0);
                break;
              case PlayerPermission.Editor:
                markImage.color = _editorColor;
                if (Utilities.IsValid(dropdown)) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(1);
                break;
              case PlayerPermission.Viewer:
                markImage.color = _viewerColor;
                if (Utilities.IsValid(dropdown)) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(2);
                break;
              default:
                markImage.color = _viewerColor;
                break;
            }
          }
        }

        var trigger = cell.GetComponent<IndexTrigger>();
        if (Utilities.IsValid(trigger))
        {
          trigger.SetProgramVariable("_variableObject", index);
        }

        cell.gameObject.SetActive(true);
      }
    }

    public void SetPermission()
    {
      if (!Utilities.IsValid(_permissionManagement) || !Utilities.IsValid(_permissionScroll) || _permissionIndex < 0) return;

      int cellIndex = Array.IndexOf(_permissionScroll.Indexes, _permissionIndex);
      if (cellIndex < 0) return;

      ScrollRect scrollRect = _permissionScroll.GetComponent<ScrollRect>();
      if (!Utilities.IsValid(scrollRect)) return;

      Transform cell = scrollRect.content.GetChild(cellIndex);
      var dropdownTransform = cell.Find("Dropdown");
      if (!Utilities.IsValid(dropdownTransform)) return;

      Dropdown dropdown = dropdownTransform.GetComponent<Dropdown>();
      if (!Utilities.IsValid(dropdown)) return;

      PlayerPermission playerPermission = PlayerPermission.Viewer;
      switch (dropdown.value)
      {
        case 0:
          playerPermission = PlayerPermission.Admin;
          break;
        case 1:
          playerPermission = PlayerPermission.Editor;
          break;
        case 2:
          playerPermission = PlayerPermission.Viewer;
          break;
      }

      _permissionManagement.TakeOwnership();
      _permissionManagement.SetPermission(_permissionIndex, playerPermission);
    }

    private void UpdateTranslation()
    {
      if (!Utilities.IsValid(_uiController)) return;
      if (Utilities.IsValid(_toggleLabelText)) _toggleLabelText.text = _uiController.GetTranslation("module.permissionManagement.toggleLabelText");
      if (Utilities.IsValid(_descriptionText))
      {
        _descriptionText.text = $"<color=#64B5F6>{_uiController.GetTranslation("module.permissionManagement.owner")}</color>\t\t\t{_uiController.GetTranslation("module.permissionManagement.ownerDesc")}\r\n" +
          $"<color=#BA68C8>{_uiController.GetTranslation("module.permissionManagement.admin")}</color>\t\t\t{_uiController.GetTranslation("module.permissionManagement.adminDesc")}\r\n" +
          $"<color=#81C784>{_uiController.GetTranslation("module.permissionManagement.editor")}</color>\t\t\t{_uiController.GetTranslation("module.permissionManagement.editorDesc")}\r\n" +
          $"<color=#FFB74D>{_uiController.GetTranslation("module.permissionManagement.viewer")}</color>\t\t\t{_uiController.GetTranslation("module.permissionManagement.viewerDesc")}";
      }
    }

    public void AfterLanguageChanged() => UpdateTranslation();
    public void AfterPermissionChanged() => GeneratePermissionView();
  }
}
