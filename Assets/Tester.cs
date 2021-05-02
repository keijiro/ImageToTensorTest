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
        var (w, h) = (_sourceSize.x, _sourceSize.y);

        _source = new Texture2D(w, h, _original.format, false);
        Graphics.CopyTexture(_original, 0, 0, 0, 0, w, h, _source, 0, 0, 0, 0);

        _scaler = new ImageScaler(_tensorWidth, _scalerCompute);
        _scaler.ProcessImage(_source);

        _tensorView = new Material(_tensorViewShader);
        _tensorView.SetBuffer("_Tensor", _scaler.SquarifiedTensor);
        _tensorView.SetInt("_TensorWidth", _tensorWidth);

        _sourceUI.texture = _source;
        _tensorUI.material = _tensorView;

        var rectx = (RectTransform)_sourceUI.transform;
        rectx.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        rectx.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
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
    }

    #endregion
}
