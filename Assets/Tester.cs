using UnityEngine;
using UI = UnityEngine.UI;

class Tester : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Texture2D _original = null;
    [SerializeField] Vector2Int _sourceSize = new Vector2Int(1920, 1080);
    [SerializeField] int _tensorWidth = 224;
    [Space]
    [SerializeField] UI.RawImage _sourceUI = null;
    [SerializeField] UI.RawImage _tensorUI = null;
    [SerializeField] RectTransform _mouseArea = null;
    [SerializeField] RectTransform _cursorUI = null;
    [Space]
    [SerializeField] ComputeShader _scalerCompute = null;
    [SerializeField] Shader _tensorViewShader = null;

    #endregion

    #region Private objects

    Texture2D _source;
    ImageScaler _scaler;
    Material _tensorView;
    bool _shouldReset;

    #endregion

    #region Scaler object management

    void InitializeScalerObjects()
    {
        // Crop the original texture and copy it into the source texture.
        var (w, h) = (_sourceSize.x, _sourceSize.y);
        var offs_x = (_original.width - w) / 2;
        var offs_y = (_original.height - h) / 2;

        _source = new Texture2D(w, h, _original.format, false);
        Graphics.CopyTexture
          (_original, 0, 0, offs_x, offs_y, w, h, _source, 0, 0, 0, 0);

        // Create and run the image scaler.
        _scaler = new ImageScaler(_tensorWidth, _scalerCompute);
        _scaler.ProcessImage(_source);

        // Show the result on the tensor view.
        _tensorView = new Material(_tensorViewShader);
        _tensorView.SetBuffer("_Tensor", _scaler.SquarifiedTensor);
        _tensorView.SetInt("_TensorWidth", _tensorWidth);

        // Update UI elements.
        _sourceUI.texture = _source;
        _tensorUI.material = _tensorView;

        // Resize the source view to fit the parent rectangle.
        var src_xform = (RectTransform)_sourceUI.transform;
        var parent = ((RectTransform)src_xform.parent).rect;

        var h_axis = RectTransform.Axis.Horizontal;
        var v_axis = RectTransform.Axis.Vertical;

        if (w * parent.height > parent.width * h)
        {
            src_xform.SetSizeWithCurrentAnchors(h_axis, parent.width);
            src_xform.SetSizeWithCurrentAnchors(v_axis, h * parent.width / w);
        }
        else
        {
            src_xform.SetSizeWithCurrentAnchors(h_axis, w * parent.height / h);
            src_xform.SetSizeWithCurrentAnchors(v_axis, parent.height);
        }
    }

    void ReleaseScalerObjects()
    {
        Destroy(_source);
        _source = null;

        _scaler.Dispose();
        _scaler = null;

        Destroy(_tensorView);
        _tensorView = null;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnValidate()
    {
        var originalSize = new Vector2Int(_original.width, _original.height);
        _sourceSize = Vector2Int.Min(_sourceSize, originalSize);
        _sourceSize = Vector2Int.Max(_sourceSize, Vector2Int.one * 64);
        _tensorWidth = Mathf.Clamp(_tensorWidth, 8, 1024);
        _shouldReset = true;
    }

    void Start()
      => InitializeScalerObjects();

    void OnDestroy()
      => ReleaseScalerObjects();

    void Update()
    {
        if (_shouldReset)
        {
            ReleaseScalerObjects();
            _shouldReset = false;
        }

        if (_source == null) InitializeScalerObjects();

        // Cursor movement
        var mp = _mouseArea.InverseTransformPoint(Input.mousePosition);
        var area = ((RectTransform)_cursorUI.transform.parent).rect;
        _cursorUI.anchoredPosition = mp * area.size / _mouseArea.rect.size;
    }

    #endregion
}
