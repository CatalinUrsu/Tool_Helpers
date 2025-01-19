using TMPro;
using System;
using Helpers.UI;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

namespace Helpers.Editor
{
public class ToolsUI
{
#region Fields

    const string UI_LAYER_NAME = "UI";
    const string PATH_SPRITE = "UI/Skin/UISprite.psd";
    const string PATH_SPRITE_BG = "UI/Skin/Background.psd";
    const string PATH_INPUT_FIELD_BG = "UI/Skin/InputFieldBackground.psd";
    const string PATH_KNOB = "UI/Skin/Knob.psd";
    const string PATH_CHECKMARK = "UI/Skin/Checkmark.psd";
    const string PATH_DROPDOWN_ARROW = "UI/Skin/DropdownArrow.psd";
    const string PATH_MASK = "UI/Skin/UIMask.psd";

    static Color _colGray = new Color(.8f, .8f, .8f, 1f);
    static Color _txtColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

    static Vector2 _dropdownArrowSize = new Vector2(30, 30);
    static Vector2 _squareSize = new Vector2(50, 50);
    static Vector2 _buttonSize = new Vector2(150, 50);
    static Vector2 _dropdownSize = new Vector2(150, 30);

    static TMP_DefaultControls.Resources uiResources;

    static TMP_DefaultControls.Resources _uiResources
    {
        get
        {
            if (uiResources.standard != null)
                return uiResources;

            uiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_SPRITE);
            uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_SPRITE_BG);
            uiResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_INPUT_FIELD_BG);
            uiResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_KNOB);
            uiResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_CHECKMARK);
            uiResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_DROPDOWN_ARROW);
            uiResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(PATH_MASK);

            return uiResources;
        }
    }

#endregion

#region UI Commands

    [MenuItem("GameObject/UI/Helpers/DummyImage", false)]
    public static void CreateDummyImage(MenuCommand menuCommand)
    {
        var go = CreateUIElementRoot("Img", _squareSize, typeof(DummyImage));
        PlaceUIElement(go.gameObject, menuCommand);
    }

    [MenuItem("GameObject/UI/Helpers/Button", false)]
    public static void CreateButton(MenuCommand menuCommand)
    {
        var btnGO = CreateUIElementRoot("Btn", _buttonSize, typeof(Image), typeof(ButtonHelper));
        var childTxtGO = CreateUIElementRoot("Txt", _squareSize, typeof(TextMeshProUGUI));

        SetParentAndAlign(childTxtGO, btnGO);
        
        SetButton();
        SetText(childTxtGO, TextAlignmentOptions.Center, "Button");
        SetImage(btnGO, _uiResources.standard, Color.white);
        
        PlaceUIElement(btnGO, menuCommand);

        void SetButton()
        {
            var btn = btnGO.GetComponent<ButtonHelper>();
            ColorBlock colors = btn.colors;

            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }
    }

    [MenuItem("GameObject/UI/Helpers/Dropdown", false)]
    public static void CreateDropDown(MenuCommand menuCommand)
    {
        var dropdownGO = CreateUIElementRoot("Dropdown", _dropdownSize, typeof(Image), typeof(DropdownHelper));
        var captions = GetDropdownCaptions();
        var panel = GetDropdownPanel();
        var itemParent = panel.transform.GetChild(0).GetChild(0);
        var itemTemplate = GetDropdownItemTemplate();

        SetDropdownProperties();
        PlaceUIElement(dropdownGO, menuCommand);

        DropdownCaption GetDropdownCaptions()
        {
            var caption = CreateUIElementRoot("Captions", _dropdownSize, typeof(DropdownCaption)).GetComponent<DropdownCaption>();
            var txtGO = CreateUIElementRoot("Txt", _dropdownSize, typeof(TextMeshProUGUI));
            var imgArrow = CreateUIElementRoot("Img_Arrow", _dropdownArrowSize, typeof(Image));

            SetParentAndAlign(caption.gameObject, dropdownGO);
            SetParentAndAlign(txtGO, caption.gameObject);
            SetParentAndAlign(imgArrow, caption.gameObject);

            SetText(txtGO, TextAlignmentOptions.MidlineLeft, "Caption");
            SetDropdownImageRT(imgArrow, _uiResources.dropdown);

            caption.ImgArrow = imgArrow.GetComponent<Image>();
            caption.Text = txtGO.GetComponent<TextMeshProUGUI>();
            return caption;
        }

        DropdownPanel GetDropdownPanel()
        {
            var panelGO = CreateUIElementRoot("Panel", _dropdownSize, typeof(Image), typeof(DropdownPanel));
            var viewport = CreateUIElementRoot("Viewport", _dropdownSize, typeof(RectMask2D));
            var content = CreateUIElementRoot("Content", _dropdownSize, typeof(VerticalLayoutGroup));
            var scrollRect = panelGO.GetComponent<ScrollRect>();
            var scrollRectRT = panelGO.GetComponent<RectTransform>();
            var vieportRT = viewport.GetComponent<RectTransform>();
            var contentRT = content.GetComponent<RectTransform>();
            var scrollSize = new Vector2(0, 80);

            SetParentAndAlign(panelGO, dropdownGO);
            SetParentAndAlign(viewport, panelGO);
            SetParentAndAlign(content, viewport);

            //Set Rect Transform properties
            scrollRectRT.anchorMin = Vector2.zero;
            scrollRectRT.anchorMax = new Vector2(1, 0);
            scrollRectRT.pivot = new Vector2(.5f, 1);
            scrollRectRT.anchoredPosition = new Vector2(0, -5);
            scrollRectRT.sizeDelta = scrollSize;

            //Set ScrollRect properties
            scrollRect.horizontal = false;
            scrollRect.viewport = vieportRT;
            scrollRect.content = contentRT;

            //Set Viewport properties
            vieportRT.anchorMin = Vector2.zero;
            vieportRT.anchorMax = Vector2.one;
            vieportRT.sizeDelta = Vector2.zero;

            //Set Content properties
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = Vector2.one;
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = scrollSize;

            SetImage(panelGO, _uiResources.background, Color.white, Image.Type.Sliced);

            return panelGO.GetComponent<DropdownPanel>();
        }

        DropdownItem GetDropdownItemTemplate()
        {
            var item = CreateUIElementRoot("Item", _dropdownSize, typeof(Toggle), typeof(DropdownItem)).GetComponent<DropdownItem>();
            var imgBg = CreateUIElementRoot("Img_Bg", _dropdownSize, typeof(Image));
            var txtGO = CreateUIElementRoot("Txt", _dropdownSize, typeof(TextMeshProUGUI));
            var imgCheckmark = CreateUIElementRoot("Img_Checkmark", _dropdownArrowSize, typeof(Image));
            var toggle = item.GetComponent<Toggle>();

            SetParentAndAlign(item.gameObject, itemParent.gameObject);
            SetParentAndAlign(imgBg, item.gameObject);
            SetParentAndAlign(txtGO, item.gameObject);
            SetParentAndAlign(imgCheckmark, item.gameObject);

            SetImgBg();
            SetText(txtGO, TextAlignmentOptions.MidlineLeft, "Option A");
            SetDropdownImageRT(imgCheckmark, _uiResources.checkmark);
            SetToggle();

            item.Text = txtGO.GetComponent<TextMeshProUGUI>();
            item.Toggle = toggle;
            return item;

            void SetToggle()
            {
                toggle.targetGraphic = imgBg.GetComponent<Image>();
                toggle.graphic = imgCheckmark.GetComponent<Image>();
            }

            void SetImgBg()
            {
                var imgBgRT = imgBg.GetComponent<RectTransform>();
                imgBgRT.anchorMin = Vector2.zero;
                imgBgRT.anchorMax = Vector2.one;
                imgBgRT.sizeDelta = Vector2.zero;

                SetImage(imgBg, _uiResources.background, Color.white, Image.Type.Sliced);
            }
        }

        void SetDropdownImageRT(GameObject imgGo, Sprite sprite)
        {
            var imgArrowRT = imgGo.GetComponent<RectTransform>();

            imgGo.GetComponent<Image>().raycastTarget = false;
            imgArrowRT.anchorMin = imgArrowRT.anchorMax = imgArrowRT.pivot = new Vector2(1, .5f);
            imgArrowRT.anchoredPosition = new Vector2(-5, imgArrowRT.anchoredPosition.y);

            SetImage(imgGo, sprite, color: _colGray);
        }

        void SetDropdownProperties()
        {
            var dropdown = dropdownGO.GetComponent<DropdownHelper>();
            dropdown.Caption = captions;
            dropdown.Panel = panel;
            dropdown.ItemParent = itemParent;
            dropdown.ItemTemplate = itemTemplate;
        }
    }

#endregion

#region Canvas stuff for creating UI element

    static void SetImage(GameObject obj, Sprite sprite, Color color, Image.Type imgType = Image.Type.Simple)
    {
        var img = obj.GetComponent<Image>();

        img.sprite = sprite;
        img.type = imgType;
        img.color = color;
    }

    static void SetText(GameObject obj, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft, string text = "")
    {
        var txt = obj.GetComponent<TextMeshProUGUI>();
        var txtRT = obj.GetComponent<RectTransform>();

        txt.text = text;
        txt.fontSize = 20;
        txt.color = _txtColor;
        txt.raycastTarget = false;
        txt.alignment = alignment;

        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
    }

    static GameObject CreateUIElementRoot(string name, Vector2 size, params Type[] components)
    {
        GameObject element = ObjectFactory.CreateGameObject(name, components);
        RectTransform elementRT = element.GetOrAdd<RectTransform>();
        elementRT.sizeDelta = size;
        return element;
    }

    static void PlaceUIElement(GameObject element, MenuCommand menuCommand)
    {
        var explicitParentChoice = true;
        var parent = GetParentForGo();

        GameObjectUtility.EnsureUniqueNameForSibling(element);

        SetParentAndAlign(element, parent);
        if (!explicitParentChoice)
            SetPositionVisibleinSceneView(element.GetComponent<RectTransform>(), parent.GetComponent<RectTransform>());

        Undo.RegisterFullObjectHierarchyUndo(parent == null ? element : parent, "");
        Undo.SetCurrentGroupName("Create " + element.name);
        Selection.activeGameObject = element;

        GameObject GetParentForGo()
        {
            parent = menuCommand.context as GameObject;

            //If there's no rood GO
            if (parent == null)
            {
                parent = GetOrAddCanvasGO();
                explicitParentChoice = false;

                // If in Prefab Mode, Canvas has to be part of Prefab contents, otherwise use Prefab root instead.
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            return parent;
        }
    }

    static GameObject GetOrAddCanvasGO()
    {
        var selectedGo = Selection.activeGameObject;
        var canvasGo = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
        if (IsValidCanvas(canvasGo))
            return canvasGo.gameObject;

        // If no cnavas in selection or its parents, find all loaded Canvases, not just the ones in main scenes.
        var canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
        for (int i = 0; i < canvasArray.Length; i++)
            if (IsValidCanvas(canvasArray[i]))
                return canvasArray[i].gameObject;

        //If no canvases anywhere, create one
        return GetNewCanvasGO();

        bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            return StageUtility.GetStageHandle(canvas.gameObject) == StageUtility.GetCurrentStageHandle();
        }
        
        GameObject GetNewCanvasGO()
        {
            var root = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = LayerMask.NameToLayer(UI_LAYER_NAME);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");

            Undo.SetCurrentGroupName("Create " + root.name);
            return root;
        }
    }

    static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

        Undo.SetTransformParent(child.transform, parent.transform, "");

        RectTransform rectTransform = child.transform as RectTransform;
        if (rectTransform)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            Vector3 localPosition = rectTransform.localPosition;
            localPosition.z = 0;
            rectTransform.localPosition = localPosition;
        }
        else
        {
            child.transform.localPosition = Vector3.zero;
        }

        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        SetLayerRecursively(child, parent.layer);

        void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }

    static void SetPositionVisibleinSceneView(RectTransform itemRT, RectTransform canvasRT)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null || sceneView.camera == null)
            return;

        // Create world space Plane from canvas position.
        Vector2 localPlanePosition;
        Camera camera = sceneView.camera;
        Vector3 position = Vector3.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
        {
            // Adjust for canvas pivot
            localPlanePosition.x = localPlanePosition.x + canvasRT.sizeDelta.x * canvasRT.pivot.x;
            localPlanePosition.y = localPlanePosition.y + canvasRT.sizeDelta.y * canvasRT.pivot.y;

            localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRT.sizeDelta.x);
            localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRT.sizeDelta.y);

            // Adjust for anchoring
            position.x = localPlanePosition.x - canvasRT.sizeDelta.x * itemRT.anchorMin.x;
            position.y = localPlanePosition.y - canvasRT.sizeDelta.y * itemRT.anchorMin.y;

            Vector3 minLocalPosition;
            minLocalPosition.x = canvasRT.sizeDelta.x * (0 - canvasRT.pivot.x) + itemRT.sizeDelta.x * itemRT.pivot.x;
            minLocalPosition.y = canvasRT.sizeDelta.y * (0 - canvasRT.pivot.y) + itemRT.sizeDelta.y * itemRT.pivot.y;

            Vector3 maxLocalPosition;
            maxLocalPosition.x = canvasRT.sizeDelta.x * (1 - canvasRT.pivot.x) - itemRT.sizeDelta.x * itemRT.pivot.x;
            maxLocalPosition.y = canvasRT.sizeDelta.y * (1 - canvasRT.pivot.y) - itemRT.sizeDelta.y * itemRT.pivot.y;

            position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
            position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
        }

        itemRT.anchoredPosition = position;
        itemRT.localRotation = Quaternion.identity;
        itemRT.localScale = Vector3.one;
    }

#endregion
}
}